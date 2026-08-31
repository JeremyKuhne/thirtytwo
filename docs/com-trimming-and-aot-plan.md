# COM trimming and Native AOT plan

## Objective

Provide fully trimmable and Native AOT-compatible paths for:

- consuming ActiveX properties and events;
- projecting managed properties through `IDispatchEx`; and
- exposing COM metadata through `ICustomTypeDescriptor`.

Keep the existing runtime-discovery paths for compatibility, but mark their actual trimming and dynamic-code requirements. New paths must make every managed type and callback visible to static analysis.

## Current state

The compatibility path discovers metadata at run time:

1. `ActiveXControl` creates a `ComTypeDescriptor`.
2. `ComTypeDescriptor` reads `ITypeInfo` and creates property and event descriptors.
3. Property access goes through `PropertyDescriptor` and boxed `VARIANT` values.
4. Event discovery maps COM parameter descriptions to closed `Action<T...>` types with `Type.MakeGenericType`.
5. `ReflectPropertiesDispatch` enumerates a managed object's properties and invokes them by name.

The annotations added before this plan deliberately expose these limitations:

- `ICustomTypeDescriptor` members carry the framework's matching `RequiresUnreferencedCode` contracts.
- ActiveX property descriptor access propagates `RequiresUnreferencedCode`.
- Runtime construction of event delegate types propagates `RequiresDynamicCode`.
- Reflective managed dispatch propagates `RequiresUnreferencedCode` and preserves all members on its runtime `Type` while that path remains in use.

These annotations are compatibility boundaries, not the intended final AOT design.

## Design constraints

1. Do not silence a warning merely to obtain a clean build. A suppression is acceptable only next to a statically verifiable invariant that explains why the warned operation is safe.
2. Do not root entire assemblies or all ActiveX-derived types as the primary solution.
3. Keep raw COM calls blittable and preserve existing pointer ownership, `Advise`/`Unadvise`, and CCW lifetime contracts.
4. Do not construct closed generic types from runtime `Type` values on the AOT path.
5. Do not use reflection by member name on the AOT path.
6. Permit COM metadata to remain dynamic; require the mapping from that metadata to managed code and managed types to be static.
7. Preserve the compatibility path so controls without generated or registered metadata continue to work in non-trimmed applications.

## Proposed architecture

### 1. Explicit ActiveX metadata

Introduce an immutable metadata object supplied when an `ActiveXControl` is constructed. It should describe only the members used by that control wrapper.

A property registration should contain:

- COM name and, when known, DISPID;
- expected `VARENUM` representation;
- statically created getter and setter callbacks; and
- the statically referenced managed property type.

An event registration should contain:

- source interface IID;
- event DISPID and name;
- expected COM argument representations; and
- a statically created callback that receives or converts the arguments.

Registrations should be created through generic factory methods. A factory invocation such as a string property or an `Action<int>` event makes the closed generic type and conversion code visible to the compiler. The runtime registry must not receive an arbitrary unannotated `Type` and claim that registration alone roots it.

The exact public shape needs prototyping. The expected usage is conceptually:

```csharp
private static readonly ActiveXMetadata s_metadata = ActiveXMetadata.CreateBuilder()
    .Property<MediaPlayer, string?>("URL", get: static control => control.URL, set: static (control, value) => control.URL = value)
    .Property<MediaPlayer, bool>("stretchToFit", get: static control => control.StretchToFit, set: static (control, value) => control.StretchToFit = value)
    .Event<MediaPlayer, int>(sourceInterfaceId, playStateChangeDispId, static (control, state) => control.OnPlayStateChange(state))
    .Build();
```

This sketch illustrates rooting through generic instantiations and static callbacks; it is not a committed API.

### 2. Typed ActiveX property access

Add a descriptor-free path for wrappers that know the property type:

- resolve and cache a DISPID from a COM name when a registration does not provide one;
- invoke `IDispatch` directly;
- convert `VARIANT` values through statically selected generic converters; and
- expose typed protected helpers used by generated or handwritten wrappers.

The intended call pattern is `GetComProperty<T>` and `SetComProperty<T>`, backed by a closed converter for `T`. The first supported set should match current behavior: `string`, `bool`, and `int`. Unsupported types must fail during metadata construction, not after a trimmed deployment.

Keep the current object-returning, descriptor-based helpers as the annotated compatibility path until migration is complete.

### 3. Callback-based ActiveX event sinks

Build on the existing connection-point ownership implementation rather than creating event delegate types from `ITypeInfo`.

Prototype a generic connection API where:

- the source interface is represented by a statically known unmanaged `IComIID` type;
- a reusable CCW exposes an `IDispatch` vtable for that source IID;
- `IDispatch.Invoke` forwards the DISPID and borrowed `VARIANT` arguments to a rooted callback;
- generated or handwritten callbacks perform typed conversion and raise ordinary managed events; and
- the returned registration owns the `Advise` cookie and always calls `Unadvise` before releasing the sink.

This removes `Type.MakeGenericType` from event subscription. Runtime `ITypeInfo` can still be used for diagnostics or compatibility discovery, but it is not authoritative for code generation on the AOT path.

Investigate whether one callback sink can safely alias multiple source IIDs to the same `IDispatch` vtable. If not, generate or register one statically known sink type per source interface.

### 4. Trimmable managed `IDispatchEx` projection

Add a non-reflective constructor or sibling implementation for `ClassPropertyDispatchAdapter`. It should accept explicit entries containing:

- name, DISPID, and `FDEX_PROP_FLAGS`;
- a getter callback; and
- an optional setter callback.

Invocation then selects an entry by DISPID and calls its callback directly. Generic entry factories root target/value types and perform `VARIANT` conversion without `Type.InvokeMember`.

Retain the current `ClassPropertyDispatchAdapter(object)` and parameterless `ReflectPropertiesDispatch` constructor as `RequiresUnreferencedCode` compatibility APIs. Add a constructor to the dispatch base that accepts explicit static metadata so derived CCWs can choose the trimmable path.

### 5. Split dynamic and static type-descriptor paths

Refactor `ComTypeDescriptor` so dynamic event delegate construction is isolated behind an explicitly annotated compatibility factory or strategy. The descriptor itself should be usable with explicit metadata without inheriting a class-wide `RequiresDynamicCode` contract.

For the static path:

- property descriptors receive a rooted managed type and direct COM converter;
- event descriptors receive an already rooted delegate type or callback descriptor;
- internal strongly typed methods avoid calling framework `ICustomTypeDescriptor` members from `ActiveXControl`; and
- explicit `ICustomTypeDescriptor` implementations retain the framework-required annotations.

A narrowly scoped suppression around a framework `ICustomTypeDescriptor` call is acceptable only if the explicit metadata object proves every returned `PropertyType` or `EventType` is rooted. Prefer an internal strongly typed method that avoids the annotated framework call.

## Investigation tasks

1. Catalog the type libraries and DISPIDs used by the ActiveX sample, including Windows Media Player event source IIDs and signatures.
2. Trace `CustomComWrappers` interface lookup to determine how a callback sink can advertise a caller-selected source IID without runtime-generated vtables.
3. Verify `IDispatch.Invoke` argument ordering, by-ref arguments, optional arguments, LCID handling, and ownership for every supported `VARIANT` conversion.
4. Decide whether metadata belongs in `thirtytwo`, generated source, or a small companion package.
5. Evaluate source generation from a type library versus handwritten registration. Generated code should be optional; the runtime API must remain directly usable.
6. Determine whether `PropertyDescriptor` and `EventDescriptor` remain necessary for the AOT path or are only compatibility/design-time surfaces.
7. Establish behavior for a runtime COM signature that disagrees with registered metadata.
8. Review callback exception translation to `HRESULT` and `EXCEPINFO`.
9. Security-review all new vtable, pointer, marshalling, and borrowed-span code before merging.

## Delivery phases

### Phase 1: Accurate contracts

- Add and validate trimming/AOT annotations.
- Keep warnings as errors in compatibility builds.
- Add an analyzer build using `IsTrimmable=true` and `IsAotCompatible=true`.

Exit criterion: the library analyzer build is clean because requirements propagate to callers, not because warnings are globally suppressed.

### Phase 2: Typed property path

- Implement generic `VARIANT` converters.
- Add typed property helpers and DISPID caching.
- Migrate the ActiveX media-player wrapper.
- Test missing properties, type mismatches, null strings, failed `IDispatch` calls, and cache behavior.

Exit criterion: media-player property get/set publishes and runs trimmed without `RequiresUnreferencedCode` at the call sites.

### Phase 3: Callback event path

- Implement the generic source-interface sink and connection registration.
- Add a typed Windows Media Player event to the sample.
- Test callback argument conversion and deterministic `Unadvise`.

Exit criterion: an AOT-published sample receives a real event without `MakeGenericType`, reflection, or generated code at run time.

### Phase 4: Managed dispatch projection

- Add explicit callback entries to `ClassPropertyDispatchAdapter` or a sibling type.
- Add the metadata-taking `ReflectPropertiesDispatch` path.
- Migrate existing tests to exercise both explicit and compatibility modes.

Exit criterion: explicit managed properties are callable through `IDispatchEx` in a trimmed AOT integration host.

### Phase 5: Descriptor separation and compatibility policy

- Split static and dynamic `ComTypeDescriptor` strategies.
- Narrow annotations to compatibility entry points.
- Document which APIs are AOT-safe and which require runtime discovery.
- Decide the long-term status of the object-returning ActiveX property helpers.

## Validation matrix

For each phase, validate:

- normal Release build with warnings as errors;
- `IsTrimmable=true` and `IsAotCompatible=true` analyzer build;
- trimmed framework-dependent publish where applicable;
- self-contained Native AOT publish;
- x64 runtime integration against Windows Media Player where installed;
- property get/set and at least one real event callback;
- disposal during normal shutdown, failed `Advise`, and callback exceptions; and
- no leaked COM references or stale callbacks after `Unadvise`.

Keep a small AOT integration application in package tests once the static path exists. Unit tests alone cannot prove that all required native code was generated.

## Open decisions

- Whether public metadata is builder-based, generated, or both.
- Whether event callbacks expose raw `VARIANT` spans as an advanced escape hatch.
- Whether DISPIDs are mandatory in static metadata or can be resolved once by name.
- Whether unsupported `VARIANT` shapes fail registration, subscription, or invocation.
- Whether the compatibility descriptor path remains enabled by default for non-trimmed applications.
