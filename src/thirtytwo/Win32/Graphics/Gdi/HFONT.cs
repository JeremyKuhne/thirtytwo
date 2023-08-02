// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HFONT : IHandle<HFONT>, IDisposable
{
    HFONT IHandle<HFONT>.Handle => this;
    object? IHandle<HFONT>.Wrapper => null;

    public static implicit operator HFONT(StockFont font) => (HFONT)Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)font);

    public static explicit operator HFONT(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)Interop.GetObjectType(handle) == OBJ_TYPE.OBJ_FONT);
        return new(handle.Value);
    }

    public static explicit operator HFONT(LRESULT result) => new(result.Value);

    /// <summary>
    ///  Creates a logical font with the specified characteristics that can be selected into a <see cref="DeviceContext"/>.
    /// </summary>
    /// <param name="height">"em" height of the font in logical pixels.</param>
    /// <param name="width">Average character width in logical pixels.</param>
    /// <param name="escapement">Angle in tenths of degrees.</param>
    /// <param name="orientation">Angle in tenths of degrees.</param>
    public static HFONT CreateFont(
         int height = 0,
         int width = 0,
         int escapement = 0,
         int orientation = 0,
         FontWeight weight = FontWeight.DoNotCare,
         bool italic = false,
         bool underline = false,
         bool strikeout = false,
         CharacterSet characterSet = CharacterSet.Default,
         OutputPrecision outputPrecision = OutputPrecision.Default,
         ClippingPrecision clippingPrecision = ClippingPrecision.Default,
         FontQuality quality = FontQuality.Default,
         FontPitch pitch = FontPitch.Default,
         FontFamilyType family = FontFamilyType.DoNotCare,
         string? typeface = null)
    {
        fixed (char* t = typeface)
        {
            return Interop.CreateFont(
                height,
                width,
                escapement,
                orientation,
                (int)weight,
                (uint)(int)(BOOL)italic,
                (uint)(int)(BOOL)underline,
                (uint)(int)(BOOL)strikeout,
                (uint)characterSet,
                (uint)outputPrecision,
                (uint)clippingPrecision,
                (uint)quality,
                (uint)((byte)pitch | (byte)family),
                t);
        }
    }

    public LOGFONTW GetLogicalFont()
    {
        Unsafe.SkipInit(out LOGFONTW logfont);
        if (Interop.GetObject(this, sizeof(LOGFONTW), &logfont) == 0)
        {
            logfont = default;
        }

        return logfont;
    }

    public FontQuality GetQuality()
    {
        LOGFONTW logfont = GetLogicalFont();
        return (FontQuality)logfont.lfQuality;
    }

    public string GetFaceName()
    {
        LOGFONTW logfont = GetLogicalFont();
        return logfont.lfFaceName.AsReadOnlySpan().SliceAtNull().ToString();
    }

    public static int GetHeightForDpi(int pointSize, int dpi)
    {
        // A point is 1/72 of an inch (1/12 of a pica)
        return -Interop.MulDiv(
            pointSize,
            dpi,
            72);
    }

    public void Dispose()
    {
        if (!IsNull)
        {
            Interop.DeleteObject(this);
        }

        Unsafe.AsRef(this) = default;
    }
}