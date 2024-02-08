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
    private List<EventDescriptor>? _events;

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
        HRESULT hr = typeInfo.Pointer->GetDocumentation(
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

        using (VARIANT value = dispatch.Pointer->GetPropertyValue("__id"))
        {
            if (value.vt == VARENUM.VT_BSTR)
            {
                return value.data.bstrVal.ToString();
            }
        }

        using (VARIANT value = dispatch.Pointer->GetPropertyValue(Interop.DISPID_Name))
        {
            if (value.vt == VARENUM.VT_BSTR)
            {
                return value.data.bstrVal.ToString();
            }
        }

        using (VARIANT value = dispatch.Pointer->GetPropertyValue("Name"))
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

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    {
        InitializeEventDescriptors();
        return new([.. _events]);
    }

    [MemberNotNull(nameof(_events))]
    private void InitializeEventDescriptors()
    {
        if (_events is not null)
        {
            return;
        }

        _events = [];

        using var typeInfo = GetObjectTypeInfo();
        if (typeInfo.IsNull)
        {
            return;
        }

        using ComScope<ITypeLib> typeLib = new(null);
        uint typeIndex;
        HRESULT hr = typeInfo.Pointer->GetContainingTypeLib(typeLib, &typeIndex);
        if (hr.Failed)
        {
            return;
        }

        using var container = _comObject.TryGetInterface<IConnectionPointContainer>(out hr);
        if (hr.Failed)
        {
            return;
        }

        using ComScope<IEnumConnectionPoints> enumerator = new(null);
        container.Pointer->EnumConnectionPoints(enumerator);
        if (hr.Failed)
        {
            return;
        }

        uint count;
        IConnectionPoint* connectionPoint = null;
        while (enumerator.Pointer->Next(1u, &connectionPoint, &count).Succeeded && count == 1)
        {
            using ComScope<IConnectionPoint> scope = new(connectionPoint);
            Guid connectionId;
            hr = connectionPoint->GetConnectionInterface(&connectionId);
            if (hr.Failed)
            {
                continue;
            }

            using ComScope<ITypeInfo> eventTypeInfo = new(null);
            hr = typeLib.Pointer->GetTypeInfoOfGuid(connectionId, eventTypeInfo);
            if (hr.Failed)
            {
                continue;
            }

            using var typeAttr = eventTypeInfo.Pointer->GetTypeAttr(out hr);
            if (hr.Failed
                || typeAttr.Value->typekind != TYPEKIND.TKIND_DISPATCH
                || ((TYPEFLAGS)typeAttr.Value->wTypeFlags).HasFlag(TYPEFLAGS.TYPEFLAG_FDUAL))
            {
                // We only handle IDispatch interfaces
                continue;
            }

            using BSTR name = default;
            hr = typeInfo.Pointer->GetDocumentation(Interop.MEMBERID_NIL, &name, null, null, null);
            if (hr.Failed)
            {
                continue;
            }

            string interfaceName = name.ToString();
            Guid interfaceGuid = connectionId;

            EnumerateFunctionDescriptions(eventTypeInfo, HandleFunction);

            void HandleFunction(ITypeInfo* typeInfo, FUNCDESC* description, ReadOnlySpan<BSTR> names)
            {
                if (ComEventDescriptor.GetDelegateType(typeInfo, description) is not Type delegateType)
                {
                    return;
                }

                using BSTR documentation = default;
                HRESULT hr = typeInfo->GetDocumentation(description->memid, null, &documentation, out _, null);

                _events.Add(new ComEventDescriptor(
                    names[0].ToString(),
                    description->memid,
                    interfaceGuid,
                    documentation.ToString(),
                    names[1..].ToStringArray(),
                    delegateType,
                    attrs: null));
            }
        }
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes) => throw new NotImplementedException();

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
        InitializePropertyDescriptors();
        return new([.. _properties]);
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes) =>
        ((ICustomTypeDescriptor)this).GetProperties();

    object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd) => _comObject;

    private ComScope<ITypeInfo> GetObjectTypeInfo(bool preferIProvideClassInfo = false)
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
            hr = dispatch.Pointer->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
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
            hr = classInfo.Pointer->GetClassInfo(typeInfo);
            return typeInfo;
        }
    }

    private delegate void EnumerateFunctionDescriptionDelegate(ITypeInfo* typeInfo, FUNCDESC* function, ReadOnlySpan<BSTR> names);

    private void EnumerateFunctionDescriptions(ITypeInfo* typeInfo, EnumerateFunctionDescriptionDelegate func)
    {
        if (typeInfo == null)
        {
            return;
        }

        TYPEATTR* ta;
        if (typeInfo->GetTypeAttr(&ta).Failed)
        {
            return;
        }

        TYPEATTR typeAttributes = *ta;
        typeInfo->ReleaseTypeAttr(ta);

        for (int i = 0; i < typeAttributes.cFuncs; i++)
        {
            FUNCDESC* function;
            HRESULT hr = typeInfo->GetFuncDesc((uint)i, &function);
            if (hr.Failed)
            {
                continue;
            }

            try
            {
                uint count = (uint)function->cParams + 1u;
                using BstrBuffer names = new((int)count);
                hr = typeInfo->GetNames(function->memid, names, count, &count);
                if (hr.Failed)
                {
                    return;
                }

                func(typeInfo, function, names[..(int)count]);
            }
            finally
            {
                typeInfo->ReleaseFuncDesc(function);
            }
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

        using var typeInfo = GetObjectTypeInfo();
        if (typeInfo.IsNull)
        {
            return;
        }

        Dictionary<int, PropertyInfo> propertyInfo = [];
        EnumerateFunctionDescriptions(typeInfo, ProcessFunction);

        foreach (PropertyInfo property in propertyInfo.Values)
        {
            _properties.Add(new ComPropertyDescriptor(
                property.Name ?? throw new InvalidOperationException(),
                property.DispatchId,
                !property.HasSetter,
                property.Type,
                attrs: null));
        }

        void ProcessFunction(ITypeInfo* typeInfo, FUNCDESC* function, ReadOnlySpan<BSTR> names)
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
                info.Name = names[0].ToString();
            }
            else if (!names[0].AsSpan().SequenceEqual(info.Name))
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