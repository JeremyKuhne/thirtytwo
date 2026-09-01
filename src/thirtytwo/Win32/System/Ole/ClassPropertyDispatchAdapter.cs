// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;
using InteropMarshal = System.Runtime.InteropServices.Marshal;

namespace Windows.Win32.System.Ole;

/// <summary>
///  Provides an <see cref="IDispatchEx"/> friendly view of a given class' public, non-indexed properties.
/// </summary>
/// <remarks>
///  <para>
///   This adapter projects managed properties as late-bound dispatch members and keeps an internal DISPID map.
///  </para>
/// </remarks>
public unsafe partial class ClassPropertyDispatchAdapter
{
    private const int StartingDispId = 0x00010000;
    private int _nextDispId = StartingDispId;

    private readonly WeakReference<object> _instance;

    private readonly Dictionary<int, DispatchEntry> _members = [];
    private readonly Dictionary<string, int> _reverseLookup = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  Initializes a new adapter for the specified managed <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">
    ///  Managed object whose public properties are exposed through dispatch metadata and invocation.
    /// </param>
    [RequiresUnreferencedCode("The target's public properties are discovered at run time.")]
    public ClassPropertyDispatchAdapter(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        _instance = new(instance);

        var properties = instance.GetType().GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            var (name, dispId, flags) = GetPropertyInfo(property);
            dispId = GetUnusedDispId(dispId);

            if (_reverseLookup.ContainsKey(name))
            {
                Debug.WriteLine($"Already found name {name}");
                continue;
            }

            _members.Add(
                dispId,
                new()
                {
                    DispId = dispId,
                    Flags = flags,
                    Name = name,
                    Property = property
                });

            _reverseLookup.Add(name, dispId);
        }
    }

    private int GetUnusedDispId(int desiredId)
    {
        if (desiredId != PInvoke.DISPID_UNKNOWN && !_members.ContainsKey(desiredId))
        {
            return desiredId;
        }

        do
        {
            desiredId = _nextDispId;
            _nextDispId++;
        } while (_members.ContainsKey(desiredId));

        return desiredId;
    }

    /// <summary>
    ///  Try to find the DISPID for a given <paramref name="name"/>. Searches are case insensitive.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Matches up to <see cref="IDispatchEx.GetDispID(BSTR, uint, int*)"/>
    ///  </para>
    /// </remarks>
    /// <param name="name">Member name to resolve.</param>
    /// <param name="dispId">The DISPID, if found.</param>
    /// <returns><see langword="true"/> if the given <paramref name="name"/> is found.</returns>
    public bool TryGetDispID(string name, out int dispId) => _reverseLookup.TryGetValue(name, out dispId);

    /// <summary>
    ///  Try to find the member name for a given <paramref name="dispId"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Matches up to <see cref="IDispatchEx.GetMemberName(IDispatchEx*, int, BSTR*)"/>
    ///  </para>
    /// </remarks>
    /// <param name="dispId">DISPID to resolve.</param>
    /// <param name="name">The name, if found.</param>
    /// <returns><see langword="true"/> if the given <paramref name="dispId"/> is found.</returns>
    public bool TryGetMemberName(int dispId, [NotNullWhen(true)] out string? name)
    {
        if (_members.TryGetValue(dispId, out DispatchEntry value))
        {
            name = value.Name;
            return true;
        }

        name = null;
        return false;
    }

    /// <summary>
    ///  Attempts to invoke the given <paramref name="dispId"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Matches up to <see cref="IDispatchEx.InvokeEx(IDispatchEx*, int, uint, ushort, DISPPARAMS*, VARIANT*, EXCEPINFO*, Com.IServiceProvider*)"/>
    ///  </para>
    ///  <para>
    ///   The call does not add references to values already present in <paramref name="parameters"/> and writes the
    ///   invocation result into <paramref name="result"/> for property gets.
    ///  </para>
    /// </remarks>
    /// <param name="dispId">DISPID to invoke.</param>
    /// <param name="lcid">Locale identifier used by the dispatch invocation.</param>
    /// <param name="flags">Dispatch operation flags.</param>
    /// <param name="parameters">Arguments in COM dispatch order (right-to-left in <c>rgvarg</c>).</param>
    /// <param name="result">Output location for the return value of get-style invocations.</param>
    /// <returns>
    ///  <see cref="HRESULT.S_OK"/> on success, or the corresponding COM error such as
    ///  <see cref="HRESULT.E_INVALIDARG"/>, <see cref="HRESULT.E_POINTER"/>,
    ///  <see cref="PInvoke.DISP_E_MEMBERNOTFOUND"/>, <see cref="PInvoke.DISP_E_BADPARAMCOUNT"/>,
    ///  <see cref="PInvoke.DISP_E_NONAMEDARGS"/>, or <see cref="PInvoke.DISP_E_PARAMNOTFOUND"/>.
    /// </returns>
    public HRESULT Invoke(
        int dispId,
        uint lcid,
        DISPATCH_FLAGS flags,
        DISPPARAMS* parameters,
        VARIANT* result)
    {
        if (!_members.TryGetValue(dispId, out var entry))
        {
            return PInvoke.DISP_E_MEMBERNOTFOUND;
        }

        if (!CanInvoke(entry, flags))
        {
            return HRESULT.E_INVALIDARG;
        }

        if (!_instance.TryGetTarget(out object? target))
        {
            return HRESULT.COR_E_OBJECTDISPOSED;
        }

        if (parameters is null)
        {
            return HRESULT.E_POINTER;
        }

        if ((parameters->cArgs > 0 && parameters->rgvarg is null)
            || (parameters->cNamedArgs > 0 && parameters->rgdispidNamedArgs is null)
            || parameters->cNamedArgs > parameters->cArgs)
        {
            return HRESULT.E_INVALIDARG;
        }

        if (flags == DISPATCH_FLAGS.DISPATCH_PROPERTYPUT)
        {
            if (parameters->cArgs != 1)
            {
                return PInvoke.DISP_E_BADPARAMCOUNT;
            }

            if (parameters->cNamedArgs != 1
                || *parameters->rgdispidNamedArgs != PInvoke.DISPID_PROPERTYPUT)
            {
                return PInvoke.DISP_E_PARAMNOTFOUND;
            }

            try
            {
                object? value = InteropMarshal.GetObjectForNativeVariant((nint)parameters->rgvarg);
                entry.Property.SetValue(target, value);
            }
            catch (Exception ex)
            {
                return (HRESULT)ex.HResult;
            }
        }
        else
        {
            if (parameters->cNamedArgs != 0)
            {
                return PInvoke.DISP_E_NONAMEDARGS;
            }

            if (parameters->cArgs != 0)
            {
                return PInvoke.DISP_E_BADPARAMCOUNT;
            }

            if (result is null)
            {
                return HRESULT.E_POINTER;
            }

            try
            {
                object? resultObject = entry.Property.GetValue(target);
                InteropMarshal.GetNativeVariantForObject(resultObject, (nint)result);
            }
            catch (Exception ex)
            {
                return (HRESULT)ex.HResult;
            }
        }

        return HRESULT.S_OK;
    }

    private static bool CanInvoke(DispatchEntry entry, DISPATCH_FLAGS flags) => flags switch
    {
        DISPATCH_FLAGS.DISPATCH_PROPERTYGET => entry.Flags.HasFlag(FDEX_PROP_FLAGS.fdexPropCanGet),
        DISPATCH_FLAGS.DISPATCH_PROPERTYPUT => entry.Flags.HasFlag(FDEX_PROP_FLAGS.fdexPropCanPut),
        _ => false
    };

    /// <summary>
    ///  Try to find the next logical DISPID after the given <paramref name="dispId"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Matches up to <see cref="IDispatchEx.GetNextDispID(IDispatchEx*, uint, int, int*)"/>
    ///  </para>
    /// </remarks>
    /// <param name="dispId">Current DISPID, or <see cref="PInvoke.DISPID_STARTENUM"/> to start enumeration.</param>
    /// <param name="nextDispId">The DISPID, if found.</param>
    /// <returns><see langword="true"/> if the next DISPID after <paramref name="dispId"/> is found.</returns>
    public bool TryGetNextDispId(int dispId, out int nextDispId)
    {
        bool foundLast = dispId == PInvoke.DISPID_STARTENUM;

        foreach (int currentId in _members.Keys)
        {
            if (foundLast)
            {
                nextDispId = currentId;
                return true;
            }

            foundLast = dispId == currentId;
        }

        nextDispId = PInvoke.DISPID_UNKNOWN;
        return false;
    }

    /// <summary>
    ///  Try to get the member properties for the given <paramref name="dispId"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Matches up to <see cref="IDispatchEx.GetMemberProperties(int, uint, FDEX_PROP_FLAGS*)"/>
    ///  </para>
    /// </remarks>
    /// <param name="dispId">DISPID to query.</param>
    /// <param name="flags">Member property flags, if the member exists.</param>
    /// <returns><see langword="true"/> if the <paramref name="dispId"/> is found.</returns>
    public bool TryGetMemberProperties(int dispId, out FDEX_PROP_FLAGS flags)
    {
        if (_members.TryGetValue(dispId, out DispatchEntry value))
        {
            flags = value.Flags;
            return true;
        }

        flags = default;
        return false;
    }

    // Somewhat surprisingly, IReflect doesn't map property names back to the original type, so Invokes back through
    // IDispatch/Ex would come in with a Get/SetProperty flags instead of Get/SetField. There is no way to specify
    // a field via IDispatch/Ex, so IReflect implementers would have to track this case.

    // private static (string Name, int Dispid, FDEX_PROP_FLAGS Flags) GetFieldInfo(FieldInfo info)
    // {
    //     int dispid = info.GetCustomAttribute<DispIdAttribute>()?.Value ?? PInvoke.DISPID_UNKNOWN;
    //     string name = info.Name;
    //     FDEX_PROP_FLAGS flags =
    //         FDEX_PROP_FLAGS.fdexPropCanGet
    //         | FDEX_PROP_FLAGS.fdexPropCanPut
    //         | FDEX_PROP_FLAGS.fdexPropCannotPutRef
    //         | FDEX_PROP_FLAGS.fdexPropCannotCall
    //         | FDEX_PROP_FLAGS.fdexPropCannotConstruct
    //         | FDEX_PROP_FLAGS.fdexPropCannotSourceEvents;
    //
    //     return (name, dispid, flags);
    // }

    private static (string Name, int DispId, FDEX_PROP_FLAGS Flags) GetPropertyInfo(PropertyInfo info)
    {
        int dispid = info.GetCustomAttribute<DispIdAttribute>()?.Value ?? PInvoke.DISPID_UNKNOWN;
        string name = info.Name;
        FDEX_PROP_FLAGS flags =
            (info.GetMethod?.IsPublic == true ? FDEX_PROP_FLAGS.fdexPropCanGet : FDEX_PROP_FLAGS.fdexPropCannotGet)
            | (info.SetMethod?.IsPublic == true ? FDEX_PROP_FLAGS.fdexPropCanPut : FDEX_PROP_FLAGS.fdexPropCannotPut)
            | FDEX_PROP_FLAGS.fdexPropCannotPutRef
            | FDEX_PROP_FLAGS.fdexPropCannotCall
            | FDEX_PROP_FLAGS.fdexPropCannotConstruct
            | FDEX_PROP_FLAGS.fdexPropCannotSourceEvents;

        return (name, dispid, flags);
    }
}
