// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class Typography : DisposableBase.Finalizable, IPointer<IDWriteTypography>
{
    private readonly AgileComPointer<IDWriteTypography> _typography;

    public unsafe IDWriteTypography* Pointer { get; private set; }

    public Typography(IDWriteTypography* typography)
    {
        Pointer = typography;

        // Ensure that this can be disposed on the finalizer thread by giving the "last" ref count
        // to an agile pointer.
        _typography = new AgileComPointer<IDWriteTypography>(typography, takeOwnership: true);
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

    protected override void Dispose(bool disposing)
    {
        Pointer = null;

        if (disposing)
        {
            _typography.Dispose();
        }
    }
}