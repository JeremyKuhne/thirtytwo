// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Maps COM connection-point metadata to a managed <see cref="EventDescriptor"/>.
/// </summary>
internal unsafe class ComEventDescriptor : EventDescriptor
{
    private readonly Type _eventType;
    private readonly string _name;
    private readonly string _description;
    private readonly string[] _parameterNames;

    /// <summary>
    ///  Initializes a descriptor for a COM event source method.
    /// </summary>
    /// <param name="name">Display name for the event member.</param>
    /// <param name="dispatchId">DISPID of the event method.</param>
    /// <param name="interfaceId">IID of the source interface that declares the event method.</param>
    /// <param name="description">Documentation text for the event.</param>
    /// <param name="parameterNames">Ordered parameter names for the delegate signature.</param>
    /// <param name="eventType">Managed delegate type mapped from the COM signature.</param>
    /// <param name="attrs">Optional descriptor attributes.</param>
    public ComEventDescriptor(
        string name,
        int dispatchId,
        Guid interfaceId,
        string description,
        string[] parameterNames,
        Type eventType,
        Attribute[]? attrs) : base(name, attrs)
    {
        _eventType = eventType;
        _name = name;
        _description = description;
        _parameterNames = parameterNames;
        DispatchId = dispatchId;
        InterfaceId = interfaceId;
    }

    /// <summary>
    ///  Gets the ordered parameter names for the generated delegate signature.
    /// </summary>
    public ReadOnlySpan<string> ParameterNames => _parameterNames;

    /// <summary>
    ///  Gets the COM dispatch identifier for this event method.
    /// </summary>
    public int DispatchId { get; }

    /// <summary>
    ///  Gets the IID of the source interface that defines this event method.
    /// </summary>
    public Guid InterfaceId { get; }

    /// <inheritdoc/>
    public override Type ComponentType => typeof(IComPointer);
    /// <inheritdoc/>
    public override Type EventType => _eventType;
    /// <inheritdoc/>
    public override bool IsMulticast => false;
    /// <inheritdoc/>
    public override string DisplayName => _name;
    /// <inheritdoc/>
    public override string Description => _description;

    // In order for this to work without creating a separate connection point for every event we would need to
    // take a factory in the constructor that the ComTypeDescriptor would host to create the connection point.
    /// <inheritdoc/>
    public override void AddEventHandler(object component, Delegate value) => throw new NotImplementedException();
    /// <inheritdoc/>
    public override void RemoveEventHandler(object component, Delegate value) => throw new NotImplementedException();

    /// <summary>
    ///  Determines the managed delegate type for a COM function description.
    /// </summary>
    /// <param name="typeInfo">Type information that owns <paramref name="description"/>.</param>
    /// <param name="description">Function description to map.</param>
    /// <returns>
    ///  A managed delegate <see cref="Type"/> when the signature can be represented; otherwise
    ///  <see langword="null"/>.
    /// </returns>
    /// <remarks>
    ///  <para>
    ///   <paramref name="typeInfo"/> and <paramref name="description"/> are borrowed pointers and must remain valid
    ///   for the duration of this call.
    ///  </para>
    /// </remarks>
    [RequiresDynamicCode("COM event signatures may require constructing delegate types at run time.")]
    public static Type? GetDelegateType(ITypeInfo* typeInfo, FUNCDESC* description)
    {
        if (description->funckind != FUNCKIND.FUNC_DISPATCH
            || description->callconv != CALLCONV.CC_STDCALL
            || !description->invkind.HasFlag(INVOKEKIND.INVOKE_FUNC))
        {
            return null;
        }

        if (description->elemdescFunc.tdesc.vt == VARENUM.VT_VOID)
        {
            // Action
            switch (description->cParams)
            {
                case 0:
                    return typeof(Action);
                case 1:
                {
                    Type? parameter1 = VARIANT.GetManagedType(description->lprgelemdescParam[0].tdesc.vt);
                    return parameter1 is null ? null : typeof(Action<>).MakeGenericType(parameter1);
                }

                case 2:
                {
                    Type? parameter1 = VARIANT.GetManagedType(description->lprgelemdescParam[0].tdesc.vt);
                    Type? parameter2 = VARIANT.GetManagedType(description->lprgelemdescParam[1].tdesc.vt);
                    return parameter1 is null || parameter2 is null
                        ? null
                        : typeof(Action<,>).MakeGenericType(parameter1, parameter2);
                }

                case 3:
                {
                    Type? parameter1 = VARIANT.GetManagedType(description->lprgelemdescParam[0].tdesc.vt);
                    Type? parameter2 = VARIANT.GetManagedType(description->lprgelemdescParam[1].tdesc.vt);
                    Type? parameter3 = VARIANT.GetManagedType(description->lprgelemdescParam[2].tdesc.vt);
                    return parameter1 is null || parameter2 is null || parameter3 is null
                        ? null
                        : typeof(Action<,,>).MakeGenericType(parameter1, parameter2, parameter3);
                }

                case 4:
                {
                    Type? parameter1 = VARIANT.GetManagedType(description->lprgelemdescParam[0].tdesc.vt);
                    Type? parameter2 = VARIANT.GetManagedType(description->lprgelemdescParam[1].tdesc.vt);
                    Type? parameter3 = VARIANT.GetManagedType(description->lprgelemdescParam[2].tdesc.vt);
                    Type? parameter4 = VARIANT.GetManagedType(description->lprgelemdescParam[3].tdesc.vt);
                    return parameter1 is null || parameter2 is null || parameter3 is null || parameter4 is null
                        ? null
                        : typeof(Action<,,,>).MakeGenericType(parameter1, parameter2, parameter3, parameter4);
                }
            }
        }

        return null;
    }
}