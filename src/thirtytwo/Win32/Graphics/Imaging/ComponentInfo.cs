// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICComponentInfo</c> that describes one WIC component class.
/// </summary>
/// <remarks>This type owns one COM reference to the wrapped interface and releases it when disposed.</remarks>
public unsafe class ComponentInfo : DirectDrawBase<IWICComponentInfo>
{
    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICComponentInfo*</c>.
    /// </summary>
    /// <param name="pointer">
    ///  Native component-info pointer. The wrapper takes ownership of one existing COM reference.
    /// </param>
    public ComponentInfo(IWICComponentInfo* pointer) : base(pointer) { }

    /// <summary>
    ///  Creates component information for a registered WIC component class identifier.
    /// </summary>
    /// <param name="componentClassId">CLSID of the WIC component to query.</param>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public ComponentInfo(Guid componentClassId)
        : base(CreateComponentInfo(Application.ImagingFactory, componentClassId))
    {
    }

    /// <summary>
    ///  Creates an <c>IWICComponentInfo*</c> for the specified WIC component class identifier.
    /// </summary>
    /// <param name="factory">Imaging factory used to query component metadata.</param>
    /// <param name="componentClassId">CLSID of the WIC component to query.</param>
    /// <returns>A component-info pointer with one COM reference owned by the caller.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public static IWICComponentInfo* CreateComponentInfo(ImagingFactory factory, Guid componentClassId)
    {
        IWICComponentInfo* info;
        factory.Pointer->CreateComponentInfo(&componentClassId, &info).ThrowOnFailure();
        GC.KeepAlive(factory);
        return info;
    }

    /// <summary>
    ///  Gets the localized friendly name provided by the WIC component.
    /// </summary>
    /// <value>Friendly display name text without the terminating null character.</value>
    /// <exception cref="Exception">
    ///  A native <c>GetFriendlyName</c> call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    /// <exception cref="InvalidDataException">
    ///  The returned text length is zero or the final character is not a null terminator.
    /// </exception>
    public string FriendlyName
    {
        get
        {
            uint length;
            Pointer->GetFriendlyName(0, null, &length).ThrowOnFailure();
            using BufferScope<char> name = new(stackalloc char[256]);
            name.EnsureCapacity(checked((int)length));
            fixed (char* namePointer = name)
            {
                Pointer->GetFriendlyName((uint)name.Length, namePointer, &length).ThrowOnFailure();
                int characterCount = checked((int)length);
                if (characterCount == 0 || name[characterCount - 1] != '\0')
                {
                    throw new InvalidDataException("The WIC component returned an invalid friendly name.");
                }

                return name[..(characterCount - 1)].ToString();
            }
        }
    }

    /// <summary>
    ///  Converts a <see cref="ComponentInfo"/> wrapper to its underlying <c>IWICComponentInfo*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICComponentInfo*</c> pointer.</returns>
    public static implicit operator IWICComponentInfo*(ComponentInfo d) => d.Pointer;
}