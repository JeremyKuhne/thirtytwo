// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Maps a COM property to a managed property.
/// </summary>
internal unsafe class ComPropertyDescriptor : PropertyDescriptor
{
    private readonly bool _readOnly;
    private readonly int _dispatchId;
    private readonly VARENUM _variantType;
    private readonly Type _managedType;

    public ComPropertyDescriptor(
        string name,
        int dispatchId,
        bool readOnly,
        VARENUM variantType,
        Attribute[]? attrs) : base(name, attrs)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (GetManagedType(variantType) is not { } managedType)
            throw new ArgumentException("Unsupported variant type", nameof(variantType));

        _managedType = managedType;
        _dispatchId = dispatchId;
        _readOnly = readOnly;
        _variantType = variantType;
    }

    /// <summary>
    ///  Returns true if the given type is supported by this descriptor.
    /// </summary>
    internal static bool IsSupportedType(VARENUM type) => GetManagedType(type) is not null;

    private static Type? GetManagedType(VARENUM type) => type switch
    {
        VARENUM.VT_BSTR => typeof(string),
        VARENUM.VT_BOOL => typeof(bool),
        VARENUM.VT_INT or VARENUM.VT_I4 => typeof(int),
        _ => null,
    };

    public override Type ComponentType => typeof(IComPointer);
    public override bool IsReadOnly => _readOnly;
    public override Type PropertyType => _managedType;
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => false;

    public override object? GetValue(object? component)
    {
        if (component is not IComPointer comObject)
        {
            throw new ArgumentException(null, nameof(component));
        }

        using var dispatch = comObject.TryGetInterface<IDispatch>(out HRESULT hr);
        if (hr.Failed)
        {
            return null;
        }

        using VARIANT value = dispatch.Pointer->GetPropertyValue(_dispatchId);
        if (value.IsEmpty)
        {
            return null;
        }

        if (PropertyType == typeof(string))
        {
            return (string)value;
        }
        else if (PropertyType == typeof(int))
        {
            return (int)value;
        }
        else if (PropertyType == typeof(bool))
        {
            return (bool)value;
        }

        return null;
    }

    public override void SetValue(object? component, object? value)
    {
        if (component is not IComPointer comObject)
        {
            throw new ArgumentException(null, nameof(component));
        }

        if ((value is not null && !value.GetType().IsAssignableFrom(PropertyType))
            || (value is null && PropertyType.IsValueType))
        {
            throw new ArgumentException("Invalid value type.", nameof(value));
        }

        using var dispatch = comObject.TryGetInterface<IDispatch>(out HRESULT hr);
        if (hr.Failed)
        {
            return;
        }

        using VARIANT variant = new()
        {
            vt = _variantType
        };

        if (PropertyType == typeof(string))
        {
            switch (_variantType)
            {
                case VARENUM.VT_BSTR:
                    variant.data.bstrVal = value is null ? default : new((string)value);
                    break;
            }
        }
        else if (PropertyType == typeof(bool))
        {
            variant.data.boolVal = (bool)value! ? VARIANT_BOOL.VARIANT_TRUE : VARIANT_BOOL.VARIANT_FALSE;
        }

        dispatch.Pointer->SetPropertyValue(_dispatchId, variant).ThrowOnFailure();
    }
}