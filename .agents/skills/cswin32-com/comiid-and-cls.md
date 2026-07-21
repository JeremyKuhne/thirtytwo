# IComIID across target frameworks, and CLS compliance

Detail for [cswin32-com](SKILL.md). `IComIID` is the interface CsWin32 uses to
recover a COM type's IID generically. A support helper such as `IID.Get<T>()`
can expose a `Guid*` to the type's `IComIID.Guid`. Its shape differs by target
framework, and how much you author by hand depends on the CsWin32 version.

## CsWin32 0.3.287 and later

The generator emits `IComIID` and attaches it to **every** generated COM struct
on both supported runtime families - a static-abstract
`static ref readonly Guid Guid` on the .NET 10 leg, and an instance
`ref readonly Guid Guid` on .NET Framework. Nothing to hand-author: adding a
generated COM type to `NativeMethods.txt` carries `IComIID` through an
`IComIID`-constrained scope automatically on both legs. A support helper reads
it via `T.Guid` on .NET 10 and `default(T).Guid` on .NET Framework.

Only **manual** structs implement `IComIID` by hand, matching the per-TFM shape
(see [manual-structs.md](manual-structs.md)).

This both-family generation was added by
[microsoft/CsWin32#1705](https://github.com/microsoft/CsWin32/pull/1705).

## Older CsWin32 (before 0.3.287)

Within this skill's .NET 10 / .NET Framework target pair, older generators emit
the static-abstract `IComIID` and attach it to generated structs only on the
.NET 10 leg. On .NET Framework you supply the missing pieces by hand, included
only for that target with a Framework-only source folder or `#if NETFRAMEWORK`:

- The `IComIID` interface itself (an instance-based `ref readonly Guid Guid`).
- One `partial struct` per generated COM type used through `ComScope<T>`, adding
  `IComIID` to its base list and returning the CsWin32-emitted `IID_Guid` field:

```csharp
namespace Windows.Win32.System.Com;

internal partial struct IRunningObjectTable : IComIID
{
    readonly ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref Unsafe.AsRef(in IID_Guid);
    }
}
```

`IID_Guid` is the generator-emitted static field and is present on these older
versions even when `IComIID` is not.

Add a partial for each new generated COM type you scope. A **manual** struct is
not given the instance polyfill (it would force an interface dispatch on a hot
path); a manual struct that must work on .NET Framework spells out the instance
member itself, as in [manual-structs.md](manual-structs.md). Whether a repo uses
the 0.3.287+ both-family generation or the older Framework polyfill - and where
those polyfill files live - is repository-specific; **see the overlay.**
Upgrading the generator to 0.3.287 or later lets you delete the whole polyfill.

## CLS compliance

A CLS-compliant assembly (`[assembly: CLSCompliant(true)]`) trips `CS3016` on
generated COM wrappers that carry CCW thunks (the `[UnmanagedCallersOnly(...)]`
array argument). CsWin32 0.3.287+ auto-emits `[CLSCompliant(false)]` only on
**internal** wrappers that carry those thunks and adds `CS3019` / `CS3021` to
its generated-file `#pragma warning disable` list. Internal projections therefore
need no consumer workaround. The .NET Framework target does not emit the thunks
and receives no annotation.

CsWin32 deliberately does not annotate **public** wrappers: the consuming
library owns its public CLS contract. In a CLS-compliant assembly with a public
projection, decide whether each wrapper belongs in the public surface. Mark an
intentionally non-CLS wrapper with a hand-authored `[CLSCompliant(false)]`
partial, reduce its visibility, or otherwise address the public contract rather
than applying a blanket suppression. Before 0.3.287, internal CCW-bearing
wrappers also need the hand-authored partials and may need narrowly scoped
`CS3019` / `CS3021` suppression. See the overlay and
[microsoft/CsWin32#1706](https://github.com/microsoft/CsWin32/pull/1706).
The underlying array-argument diagnostic is tracked by
[dotnet/roslyn#68526](https://github.com/dotnet/roslyn/issues/68526).
