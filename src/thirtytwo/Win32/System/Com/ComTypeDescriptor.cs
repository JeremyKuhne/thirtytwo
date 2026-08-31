// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Provides managed type descriptor metadata for COM objects.
/// </summary>
[RequiresDynamicCode("COM event signatures may require constructing delegate types at run time.")]
internal unsafe sealed partial class ComTypeDescriptor : ICustomTypeDescriptor
{
    private string? _className;
    private readonly IComPointer _comObject;
    private List<PropertyDescriptor>? _properties;
    private List<EventDescriptor>? _events;

    /// <summary>
    ///  Initializes a new descriptor for the given COM-backed object.
    /// </summary>
    /// <param name="comObject">Object that can provide COM interface pointers.</param>
    public ComTypeDescriptor(IComPointer comObject)
    {
        ArgumentNullException.ThrowIfNull(comObject);
        _comObject = comObject;
    }

    /// <inheritdoc cref="ICustomTypeDescriptor.GetAttributes"/>
    AttributeCollection ICustomTypeDescriptor.GetAttributes() => AttributeCollection.Empty;

    /// <inheritdoc cref="ICustomTypeDescriptor.GetClassName"/>
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
            PInvoke.MEMBERID_NIL,
            &name,
            null,
            null,
            null);

        Debug.Assert(hr.Succeeded);
        _className = name.ToString();
        return _className;
    }

    /// <inheritdoc cref="ICustomTypeDescriptor.GetComponentName"/>
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

        using (VARIANT value = dispatch.Pointer->GetPropertyValue(PInvoke.DISPID_Name))
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

    /// <inheritdoc cref="ICustomTypeDescriptor.GetConverter"/>
    [RequiresUnreferencedCode("PropertyDescriptor's PropertyType cannot be statically discovered.")]
    TypeConverter? ICustomTypeDescriptor.GetConverter() => null;
    /// <inheritdoc cref="ICustomTypeDescriptor.GetDefaultEvent"/>
    [RequiresUnreferencedCode("PropertyDescriptor's PropertyType cannot be statically discovered.")]
    EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent() => throw new NotImplementedException();
    /// <inheritdoc cref="ICustomTypeDescriptor.GetDefaultProperty"/>
    [RequiresUnreferencedCode("PropertyDescriptor's PropertyType cannot be statically discovered.")]
    PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty() => throw new NotImplementedException();
    /// <inheritdoc cref="ICustomTypeDescriptor.GetEditor"/>
    [RequiresUnreferencedCode("Editors registered in the type's metadata may be trimmed.")]
    object? ICustomTypeDescriptor.GetEditor(Type editorBaseType) => null;

    /// <inheritdoc cref="ICustomTypeDescriptor.GetEvents()"/>
    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    {
        InitializeEventDescriptors();
        return new([.. _events]);
    }

    /// <summary>
    ///  Initializes cached event descriptors from COM type information.
    /// </summary>
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
            hr = typeInfo.Pointer->GetDocumentation(PInvoke.MEMBERID_NIL, &name, null, null, null);
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
                uint helpContext;
                HRESULT hr = typeInfo->GetDocumentation(
                    description->memid,
                    null,
                    &documentation,
                    &helpContext,
                    null);

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

    /// <inheritdoc cref="ICustomTypeDescriptor.GetEvents(Attribute[])"/>
    [RequiresUnreferencedCode("EventDescriptor's EventType cannot be statically discovered.")]
    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes) => throw new NotImplementedException();

    /// <inheritdoc cref="ICustomTypeDescriptor.GetProperties()"/>
    [RequiresUnreferencedCode("PropertyDescriptor's PropertyType cannot be statically discovered.")]
    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
        InitializePropertyDescriptors();
        return new([.. _properties]);
    }

    /// <inheritdoc cref="ICustomTypeDescriptor.GetProperties(Attribute[])"/>
    [RequiresUnreferencedCode("PropertyDescriptor's PropertyType cannot be statically discovered.")]
    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes) =>
        ((ICustomTypeDescriptor)this).GetProperties();

    /// <inheritdoc cref="ICustomTypeDescriptor.GetPropertyOwner"/>
    object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd) => _comObject;

    /// <summary>
    ///  Gets type information for the wrapped COM object.
    /// </summary>
    /// <param name="preferIProvideClassInfo">
    ///  <see langword="true"/> to try <see cref="IProvideClassInfo"/> first; otherwise <see cref="IDispatch"/>
    ///  is preferred.
    /// </param>
    /// <returns>
    ///  A <see cref="ComScope{T}"/> that owns one AddRef'd <see cref="ITypeInfo"/> reference when available;
    ///  otherwise a null scope.
    /// </returns>
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
            hr = dispatch.Pointer->GetTypeInfo(0, PInvoke.GetThreadLocale(), typeInfo);
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

    /// <summary>
    ///  Enumerates function descriptions for a COM type and invokes <paramref name="func"/> for each function.
    /// </summary>
    /// <param name="typeInfo">Type information to enumerate.</param>
    /// <param name="func">Callback invoked with borrowed pointers valid only during the callback.</param>
    private void EnumerateFunctionDescriptions(ITypeInfo* typeInfo, EnumerateFunctionDescriptionDelegate func)
    {
        if (typeInfo is null)
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

    /// <summary>
    ///  Initializes cached property descriptors from COM type information.
    /// </summary>
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
}