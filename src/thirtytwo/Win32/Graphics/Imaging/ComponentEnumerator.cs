// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IEnumUnknown</c> that enumerates WIC components.
/// </summary>
/// <remarks>
///  Enumeration state is maintained by the underlying COM enumerator. Each successful call to <see cref="Next(out ComponentInfo?)"/>
///  advances that state by one element.
/// </remarks>
public unsafe class ComponentEnumerator : DirectDrawBase<IEnumUnknown>
{
    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IEnumUnknown*</c>.
    /// </summary>
    /// <param name="pointer">
    ///  Native enumerator pointer. The wrapper takes ownership of one existing COM reference.
    /// </param>
    public ComponentEnumerator(IEnumUnknown* pointer) : base(pointer) { }

    /// <summary>
    ///  Creates a WIC component enumerator for the specified component type.
    /// </summary>
    /// <param name="componentType">Component category flags to include in the enumeration.</param>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public ComponentEnumerator(WICComponentType componentType)
        : base(CreateComponentEnumerator(Application.ImagingFactory, componentType))
    {
    }

    /// <summary>
    ///  Creates an <c>IEnumUnknown*</c> that enumerates WIC components.
    /// </summary>
    /// <param name="factory">Imaging factory used to create the component enumerator.</param>
    /// <param name="componentType">Component category flags to include in the enumeration.</param>
    /// <returns>An enumerator pointer with one COM reference owned by the caller.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public static IEnumUnknown* CreateComponentEnumerator(ImagingFactory factory, WICComponentType componentType)
    {
        IEnumUnknown* enumerator;
        factory.Pointer->CreateComponentEnumerator(
            (uint)componentType,
            (uint)WICComponentEnumerateOptions.WICComponentEnumerateDefault,
            &enumerator).ThrowOnFailure();

        GC.KeepAlive(factory);
        return enumerator;
    }

    /// <summary>
    ///  Converts a <see cref="ComponentEnumerator"/> wrapper to its underlying <c>IEnumUnknown*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IEnumUnknown*</c> pointer.</returns>
    public static implicit operator IEnumUnknown*(ComponentEnumerator d) => d.Pointer;

    /// <summary>
    ///  Retrieves the next component in the native enumeration sequence.
    /// </summary>
    /// <param name="componentInfo">
    ///  When this method returns <see langword="true"/>, contains the next component wrapper.
    ///  Otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    ///  <see langword="true"/> when one component is returned; otherwise <see langword="false"/> when the sequence
    ///  is exhausted, the wrapped pointer is null, or the COM enumerator does not return exactly one item.
    /// </returns>
    public bool Next([NotNullWhen(true)] out ComponentInfo? componentInfo)
    {
        componentInfo = null;
        IEnumUnknown* enumerator = this;
        if (enumerator is null)
        {
            return false;
        }

        uint fetched;
        using ComScope<IUnknown> unknown = new(null);
        HRESULT result = enumerator->Next(1, unknown, &fetched);
        if (result != HRESULT.S_OK || fetched != 1)
        {
            return false;
        }

        componentInfo = new(unknown.Pointer->QueryInterface<IWICComponentInfo>());
        return true;
    }
}