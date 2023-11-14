// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

internal unsafe sealed class ComTypeDescriptor : ICustomTypeDescriptor
{
    private string? _className;
    private readonly IComPointer _comObject;
    private List<PropertyDescriptor>? _properties;

    public ComTypeDescriptor(IComPointer comObject)
    {
        ArgumentNullException.ThrowIfNull(comObject);
        _comObject = comObject;
    }

    AttributeCollection ICustomTypeDescriptor.GetAttributes() => AttributeCollection.Empty;

    string? ICustomTypeDescriptor.GetClassName()
    {
        if (_className is not null)
        {
            return _className;
        }

        using var typeInfo = GetObjectTypeInfo(preferIProvideClassInfo: true);
        if (typeInfo.IsNull)
        {
            _className = string.Empty;
            return _className;
        }

        using BSTR name = default;
        HRESULT hr = typeInfo.Value->GetDocumentation(
            Interop.MEMBERID_NIL,
            &name,
            null,
            null,
            null);

        Debug.Assert(hr.Succeeded);
        _className = name.ToString();
        return _className;
    }

    string? ICustomTypeDescriptor.GetComponentName()
    {
        using var dispatch = _comObject.TryGetInterface<IDispatch>(out HRESULT hr);
        if (hr.Failed)
        {
            return string.Empty;
        }

        using (VARIANT value = dispatch.Value->GetPropertyValue("__id"))
        {
            if (value.vt == VARENUM.VT_BSTR)
            {
                return value.data.bstrVal.ToString();
            }
        }

        using (VARIANT value = dispatch.Value->GetPropertyValue(Interop.DISPID_Name))
        {
            if (value.vt == VARENUM.VT_BSTR)
            {
                return value.data.bstrVal.ToString();
            }
        }

        using (VARIANT value = dispatch.Value->GetPropertyValue("Name"))
        {
            if (value.vt == VARENUM.VT_BSTR)
            {
                return value.data.bstrVal.ToString();
            }
        }

        return string.Empty;
    }

    TypeConverter? ICustomTypeDescriptor.GetConverter() => null;
    EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent() => throw new NotImplementedException();
    PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty() => throw new NotImplementedException();
    object? ICustomTypeDescriptor.GetEditor(Type editorBaseType) => null;
    EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => throw new NotImplementedException();
    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes) => throw new NotImplementedException();

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
        InitializePropertyDescriptors();
        return new(_properties.ToArray());
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes) => throw new NotImplementedException();

    object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd) => _comObject;

    private ComScope<ITypeInfo> GetObjectTypeInfo(bool preferIProvideClassInfo)
    {
        if (preferIProvideClassInfo)
        {
            ComScope<ITypeInfo> typeInfo = FromIProvideClassInfo();
            if (typeInfo.IsNull)
            {
                typeInfo = FromIDispatch();
            }

            return typeInfo;
        }
        else
        {
            ComScope<ITypeInfo> typeInfo = FromIDispatch();
            if (typeInfo.IsNull)
            {
                typeInfo = FromIProvideClassInfo();
            }

            return typeInfo;
        }

        ComScope<ITypeInfo> FromIDispatch()
        {
            using var dispatch = _comObject.TryGetInterface<IDispatch>(out HRESULT hr);
            if (hr.Failed)
            {
                return default;
            }

            ComScope<ITypeInfo> typeInfo = new(null);
            hr = dispatch.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
            return typeInfo;
        }

        ComScope<ITypeInfo> FromIProvideClassInfo()
        {
            using var classInfo = _comObject.TryGetInterface<IProvideClassInfo>(out HRESULT hr);
            if (hr.Failed)
            {
                return default;
            }

            ComScope<ITypeInfo> typeInfo = new(null);
            hr = classInfo.Value->GetClassInfo(typeInfo);
            return typeInfo;
        }
    }

    [MemberNotNull(nameof(_properties))]
    private void InitializePropertyDescriptors()
    {
        if (_properties is not null)
        {
            return;
        }

        _properties = [];

        using var typeInfo = GetObjectTypeInfo(preferIProvideClassInfo: false);
        if (typeInfo.IsNull)
        {
            return;
        }

        TYPEATTR* ta;
        if (typeInfo.Value->GetTypeAttr(&ta).Failed)
        {
            return;
        }

        TYPEATTR typeAttributes = *ta;
        typeInfo.Value->ReleaseTypeAttr(ta);
        Dictionary<int, PropertyInfo> propertyInfo = [];

        for (int i = 0; i < typeAttributes.cFuncs; i++)
        {
            FUNCDESC* function;
            HRESULT hr = typeInfo.Value->GetFuncDesc((uint)i, &function);
            if (hr.Failed)
            {
                continue;
            }

            try
            {
                using BSTR name = default;
                uint count;
                hr = typeInfo.Value->GetNames(function->memid, &name, 1u, &count);
                ProcessFunction(function, name);
            }
            finally
            {
                typeInfo.Value->ReleaseFuncDesc(function);
            }
        }

        foreach (PropertyInfo property in propertyInfo.Values)
        {
            _properties.Add(new ComPropertyDescriptor(
                property.Name ?? throw new InvalidOperationException(),
                property.DispatchId,
                !property.HasSetter,
                property.Type,
                attrs: null));
        }

        void ProcessFunction(FUNCDESC* function, BSTR name)
        {
            propertyInfo.TryGetValue(function->memid, out PropertyInfo info);
            VARENUM type = VARENUM.VT_EMPTY;

            if (function->invkind.HasFlag(INVOKEKIND.INVOKE_PROPERTYGET) && function->cParams == 0)
            {
                type = function->elemdescFunc.tdesc.vt;
            }
            else if (function->invkind.HasFlag(INVOKEKIND.INVOKE_PROPERTYPUT) && function->cParams == 1)
            {
                type = function->lprgelemdescParam[0].tdesc.vt;
                info.HasSetter = true;
            }
            else
            {
                // Not a simple property
                return;
            }

            if (type == VARENUM.VT_EMPTY || (info.Type != VARENUM.VT_EMPTY && info.Type != type))
            {
                throw new NotSupportedException("Unexpected property type.");
            }

            if (!ComPropertyDescriptor.IsSupportedType(type))
            {
                return;
            }

            info.Type = type;
            if (info.Name is null)
            {
                info.Name = name.ToString();
            }
            else if (!name.AsSpan().SequenceEqual(info.Name))
            {
                throw new NotSupportedException("Mismatched put/get type.");
            }

            info.DispatchId = function->memid;

            propertyInfo[function->memid] = info;
        }
    }

    private struct PropertyInfo
    {
        public string? Name { get; set; }
        public int DispatchId { get; set; }
        public bool HasSetter { get; set; }
        public VARENUM Type { get; set; }
    }
}