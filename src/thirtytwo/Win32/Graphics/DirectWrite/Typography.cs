// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Wraps <see cref="IDWriteTypography"/> and stores OpenType font feature selections.
/// </summary>
/// <remarks>
///  Instances own the underlying <see cref="IDWriteTypography"/> COM interface pointer and release it when disposed.
/// </remarks>
public unsafe class Typography : DirectDrawBase<IDWriteTypography>
{
    /// <summary>
    ///  Wraps an existing typography pointer.
    /// </summary>
    /// <param name="typography">The existing typography interface pointer to wrap.</param>
    /// <remarks>The wrapper takes responsibility for releasing this COM interface pointer when disposed.</remarks>
    public Typography(IDWriteTypography* typography) : base(typography)
    {
    }

    /// <summary>
    ///  Creates an empty typography object.
    /// </summary>
    /// <remarks>
    ///  This constructor calls <see cref="IDWriteFactory.CreateTypography(IDWriteTypography**)"/> and throws when
    ///  the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public Typography() : this(Create())
    {
    }

    private static IDWriteTypography* Create()
    {
        IDWriteTypography* typography;
        Application.DirectWriteFactory.Pointer->CreateTypography(&typography).ThrowOnFailure();
        return typography;
    }

    /// <summary>
    ///  Adds a font feature to this typography object.
    /// </summary>
    /// <param name="fontFeature">The feature to append.</param>
    /// <remarks>
    ///  This method calls <see cref="IDWriteTypography.AddFontFeature(DWRITE_FONT_FEATURE)"/> and throws when the
    ///  underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public void AddFontFeature(FontFeature fontFeature)
    {
        Pointer->AddFontFeature(Unsafe.As<FontFeature, DWRITE_FONT_FEATURE>(ref fontFeature)).ThrowOnFailure();
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Retrieves a font feature by index.
    /// </summary>
    /// <param name="fontFeatureIndex">The zero-based index of the feature to retrieve.</param>
    /// <param name="fontFeature">Receives the feature at the specified index.</param>
    /// <remarks>
    ///  This wrapper calls <see cref="IDWriteTypography.GetFontFeature(uint, DWRITE_FONT_FEATURE*)"/> directly and
    ///  does not check the returned <c>HRESULT</c>.
    /// </remarks>
    public void GetFontFeature(uint fontFeatureIndex, out FontFeature fontFeature)
    {
        fixed (void* f = &fontFeature)
        {
            Pointer->GetFontFeature(fontFeatureIndex, (DWRITE_FONT_FEATURE*)f);
            GC.KeepAlive(this);
        }
    }

    /// <summary>
    ///  Gets the number of features currently stored.
    /// </summary>
    /// <value>The number of stored font features.</value>
    public uint FontFeatureCount
    {
        get
        {
            uint result = Pointer->GetFontFeatureCount();
            GC.KeepAlive(this);
            return result;
        }
    }
}