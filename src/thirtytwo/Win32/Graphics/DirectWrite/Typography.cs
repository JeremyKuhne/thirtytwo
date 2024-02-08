// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class Typography : DirectDrawBase<IDWriteTypography>
{
    public Typography(IDWriteTypography* typography) : base(typography)
    {
    }

    public Typography() : this(Create())
    {
    }

    private static IDWriteTypography* Create()
    {
        IDWriteTypography* typography;
        Application.DirectWriteFactory.Pointer->CreateTypography(&typography).ThrowOnFailure();
        return typography;
    }

    /// <inheritdoc cref="IDWriteTypography.AddFontFeature(DWRITE_FONT_FEATURE)"/>
    public void AddFontFeature(FontFeature fontFeature)
    {
        Pointer->AddFontFeature(Unsafe.As<FontFeature, DWRITE_FONT_FEATURE>(ref fontFeature)).ThrowOnFailure();
        GC.KeepAlive(this);
    }

    /// <inheritdoc cref="IDWriteTypography.GetFontFeature(uint, DWRITE_FONT_FEATURE*)"/>
    public void GetFontFeature(uint fontFeatureIndex, out FontFeature fontFeature)
    {
        fixed (void* f = &fontFeature)
        {
            Pointer->GetFontFeature(fontFeatureIndex, (DWRITE_FONT_FEATURE*)f);
            GC.KeepAlive(this);
        }
    }

    /// <inheritdoc cref="IDWriteTypography.GetFontFeatureCount()"/>
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