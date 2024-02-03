// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

internal unsafe class ComEventDescriptor : EventDescriptor
{
    private readonly Type _eventType;
    private readonly string _name;
    private readonly string _description;
    private readonly string[] _parameterNames;

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

    public ReadOnlySpan<string> ParameterNames => _parameterNames;
    public int DispatchId { get; }
    public Guid InterfaceId { get; }

    public override Type ComponentType => typeof(IComPointer);
    public override Type EventType => _eventType;
    public override bool IsMulticast => false;
    public override string DisplayName => _name;
    public override string Description => _description;

    // In order for this to work without creating a separate connection point for every event we would need to
    // take a factory in the constructor that the ComTypeDescriptor would host to create the connection point.
    public override void AddEventHandler(object component, Delegate value) => throw new NotImplementedException();
    public override void RemoveEventHandler(object component, Delegate value) => throw new NotImplementedException();

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