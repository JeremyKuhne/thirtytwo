// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI font handle (<c>HFONT</c>).
/// </summary>
public unsafe partial struct HFONT : IHandle<HFONT>, IDisposable
{
    /// <inheritdoc cref="IHandle{T}.Handle"/>
    HFONT IHandle<HFONT>.Handle => this;

    /// <inheritdoc cref="IHandle{T}.Wrapper"/>
    object? IHandle<HFONT>.Wrapper => null;

    /// <summary>
    ///  Gets a borrowed stock font.
    /// </summary>
    /// <param name="font">Stock font selector.</param>
    /// <returns>A stock-object font handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HFONT(StockFont font) => (HFONT)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)font);

    /// <summary>
    ///  Reinterprets a generic GDI object handle as a font handle.
    /// </summary>
    /// <param name="handle">Source handle expected to be <c>OBJ_FONT</c> or null.</param>
    /// <returns>The same native value typed as <see cref="HFONT"/>.</returns>
    public static explicit operator HFONT(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)PInvoke.GetObjectType(handle) == OBJ_TYPE.OBJ_FONT);
        return new(handle.Value);
    }

    /// <summary>
    ///  Reinterprets a window-message result value as a font handle.
    /// </summary>
    /// <param name="result">Message result containing an <c>HFONT</c> value.</param>
    /// <returns>The same native value typed as <see cref="HFONT"/>.</returns>
    public static explicit operator HFONT(LRESULT result) => new(result.Value);

    /// <summary>
    ///  <para>
    ///   Creates a logical font with the specified characteristics.
    ///  </para>
    ///  <para>
    ///   The returned handle can be selected into a <see cref="DeviceContext"/>.
    ///  </para>
    /// </summary>
    /// <param name="height">
    ///  <para>
    ///   Requested cell height in logical units.
    ///  </para>
    ///  <para>
    ///   Positive values match character-cell height; negative values request character height
    ///   (commonly computed from point size and DPI).
    ///  </para>
    /// </param>
    /// <param name="width">
    ///  <para>
    ///   Average character width in logical units, or 0 for default matching.
    ///  </para>
    /// </param>
    /// <param name="escapement">
    ///  <para>
    ///   Text escapement angle in tenths of degrees.
    ///  </para>
    /// </param>
    /// <param name="orientation">
    ///  <para>
    ///   Character-orientation angle in tenths of degrees.
    ///  </para>
    /// </param>
    /// <param name="weight">
    ///  <para>
    ///   Font weight.
    ///  </para>
    /// </param>
    /// <param name="italic">
    ///  <para>
    ///   <see langword="true"/> for italic style; otherwise, <see langword="false"/>.
    ///  </para>
    /// </param>
    /// <param name="underline">
    ///  <para>
    ///   <see langword="true"/> to underline text; otherwise, <see langword="false"/>.
    ///  </para>
    /// </param>
    /// <param name="strikeout">
    ///  <para>
    ///   <see langword="true"/> to strike through text; otherwise, <see langword="false"/>.
    ///  </para>
    /// </param>
    /// <param name="characterSet">
    ///  <para>
    ///   Character set identifier.
    ///  </para>
    /// </param>
    /// <param name="outputPrecision">
    ///  <para>
    ///   Output precision hint.
    ///  </para>
    /// </param>
    /// <param name="clippingPrecision">
    ///  <para>
    ///   Clipping precision hint.
    ///  </para>
    /// </param>
    /// <param name="quality">
    ///  <para>
    ///   Rendering quality hint.
    ///  </para>
    /// </param>
    /// <param name="pitch">
    ///  <para>
    ///   Pitch and family low-byte pitch value.
    ///  </para>
    /// </param>
    /// <param name="family">
    ///  <para>
    ///   Pitch and family low-byte family value.
    ///  </para>
    /// </param>
    /// <param name="typeface">
    ///  <para>
    ///   Typeface face name, or <see langword="null"/> for default matching.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   The created font handle.
    ///  </para>
    ///  <para>
    ///   A null handle indicates failure from <c>CreateFont</c>.
    ///  </para>
    /// </returns>
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
            return PInvoke.CreateFont(
                height,
                width,
                escapement,
                orientation,
                (int)weight,
                (uint)(int)(BOOL)italic,
                (uint)(int)(BOOL)underline,
                (uint)(int)(BOOL)strikeout,
                (FONT_CHARSET)characterSet,
                (FONT_OUTPUT_PRECISION)outputPrecision,
                (FONT_CLIP_PRECISION)clippingPrecision,
                (FONT_QUALITY)quality,
                (uint)((byte)pitch | (byte)family),
                t);
        }
    }

    /// <summary>
    ///  Gets the logical-font description for this handle.
    /// </summary>
    /// <returns>
    ///  The <see cref="LOGFONTW"/> reported by <c>GetObject</c>; returns <see langword="default"/> when
    ///  <c>GetObject</c> returns zero.
    /// </returns>
    public LOGFONTW GetLogicalFont()
    {
        Unsafe.SkipInit(out LOGFONTW logfont);
        if (PInvoke.GetObject(this, sizeof(LOGFONTW), &logfont) == 0)
        {
            logfont = default;
        }

        return logfont;
    }

    /// <summary>
    ///  Gets the quality field from the logical font.
    /// </summary>
    /// <returns>The <see cref="FontQuality"/> value in <see cref="LOGFONTW.lfQuality"/>.</returns>
    public FontQuality GetQuality()
    {
        LOGFONTW logfont = GetLogicalFont();
        return (FontQuality)logfont.lfQuality;
    }

    /// <summary>
    ///  Gets the font face name from the logical font.
    /// </summary>
    /// <returns>
    ///  The null-terminated face name from <see cref="LOGFONTW.lfFaceName"/>, or an empty string when unavailable.
    /// </returns>
    public string GetFaceName()
    {
        LOGFONTW logfont = GetLogicalFont();
        return logfont.lfFaceName.AsReadOnlySpan().SliceAtNull().ToString();
    }

    /// <summary>
    ///  Converts a point size to a logical font height for a given DPI.
    /// </summary>
    /// <param name="pointSize">Point size, where one point is 1/72 inch.</param>
    /// <param name="dpi">Device DPI for the vertical axis.</param>
    /// <returns>
    ///  A negative logical height suitable for the <c>height</c> argument in <see cref="CreateFont"/>,
    ///  computed with <c>MulDiv(pointSize, dpi, 72)</c>.
    /// </returns>
    public static int GetHeightForDpi(int pointSize, int dpi)
    {
        // A point is 1/72 of an inch (1/12 of a pica)
        return -PInvoke.MulDiv(
            pointSize,
            dpi,
            72);
    }

    /// <summary>
    ///  Deletes the font with <c>DeleteObject</c> when the handle is not null, then clears this wrapper.
    /// </summary>
    /// <remarks>
    ///  This method does not validate ownership. Handles borrowed from stock-font APIs are not owned by the caller
    ///  and should generally not be disposed through this wrapper.
    /// </remarks>
    public void Dispose()
    {
        if (!IsNull)
        {
            PInvoke.DeleteObject(this);
        }

        Unsafe.AsRef(in this) = default;
    }
}