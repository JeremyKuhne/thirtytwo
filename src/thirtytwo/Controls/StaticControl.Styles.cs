// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.SystemServices;

namespace Windows;

public partial class StaticControl
{
    [Flags]
    public enum Styles : uint
    {
        /// <summary>
        ///  Simple rectangle with left aligned text.
        /// </summary>
        Left = STATIC_STYLES.SS_LEFT,

        /// <summary>
        ///  Simple rectangle with centered text.
        /// </summary>
        Center = STATIC_STYLES.SS_CENTER,

        /// <summary>
        ///  Simple rectangle with right aligned text.
        /// </summary>
        Right = STATIC_STYLES.SS_RIGHT,

        /// <summary>
        ///  Icon resource named by text is displayed.
        /// </summary>
        Icon = STATIC_STYLES.SS_ICON,

        /// <summary>
        ///  Rectangle filled with the window frame color.
        /// </summary>
        BlackRectangle = STATIC_STYLES.SS_BLACKRECT,

        /// <summary>
        ///  Rectangle filled with the screen background color.
        /// </summary>
        GrayRectangle = STATIC_STYLES.SS_GRAYRECT,

        /// <summary>
        ///  Rectangle filled with the window background color.
        /// </summary>
        WhiteRectangle = STATIC_STYLES.SS_WHITERECT,

        /// <summary>
        ///  Box with a frame drawn in the current window frame color.
        /// </summary>
        BlackFrame = STATIC_STYLES.SS_BLACKFRAME,

        /// <summary>
        ///  Box with a frame drawn in the screen background [desktop] color.
        /// </summary>
        GrayFrame = STATIC_STYLES.SS_GRAYFRAME,

        /// <summary>
        ///  Box with a frame drawn with the window background color.
        /// </summary>
        WhiteFrame = STATIC_STYLES.SS_WHITEFRAME,

         UserItem = STATIC_STYLES.SS_USERITEM,

        /// <summary>
        ///  Simple rectangle single-line left aligned text.
        /// </summary>
        Simple = STATIC_STYLES.SS_SIMPLE,

        /// <summary>
        ///  Left with no word wrapping.
        /// </summary>
        LeftNoWordWrap = STATIC_STYLES.SS_LEFTNOWORDWRAP,

        /// <summary>
        ///  Owner of the static control is responsible for drawing.
        /// </summary>
        OwnerDraw = STATIC_STYLES.SS_OWNERDRAW,

        /// <summary>
        ///  Bitmap is to be displayed. Text is the name of a bitmap resource.
        ///  Control ignores width and height and sizes to fit the bitmap.
        /// </summary>
        Bitmap = STATIC_STYLES.SS_BITMAP,

        /// <summary>
        ///  Enhanced metafile is to be displayed.
        /// </summary>
        EnhancedMetafile = STATIC_STYLES.SS_ENHMETAFILE,

        /// <summary>
        ///  Draws the top and bottom edges with the etched style.
        /// </summary>
        EtchedHorizontal = STATIC_STYLES.SS_ETCHEDHORZ,

        /// <summary>
        ///  Draws the left and right edges with the etched style.
        /// </summary>
        EtchedVertical = STATIC_STYLES.SS_ETCHEDVERT,

        /// <summary>
        ///  Draws the frame with the etched style.
        /// </summary>
        EtchedFrame = STATIC_STYLES.SS_ETCHEDFRAME,

        // SS_TYPEMASK          = 0x0000001F,

        /// <summary>
        ///  Adjust the size of bitmaps to fit the control.
        /// </summary>
        RealSizeControl = STATIC_STYLES.SS_REALSIZECONTROL,

        /// <summary>
        ///  Prevents interpretation of &amp; characters as accelerator prefixes.
        /// </summary>
        NoPrefix = STATIC_STYLES.SS_NOPREFIX,

        /// <summary>
        ///  Sends parent window click and enable notifications.
        /// </summary>
        Notify = STATIC_STYLES.SS_NOTIFY,

        /// <summary>
        ///  Centers bitmap in the control.
        /// </summary>
        CenterImage = STATIC_STYLES.SS_CENTERIMAGE,

        /// <summary>
        ///  Lower right corner stays fixed when control is resized.
        /// </summary>
        RightJustify = STATIC_STYLES.SS_RIGHTJUST,

        /// <summary>
        ///  Uses actual resource widtth for icons.
        /// </summary>
        RealSizeImage = STATIC_STYLES.SS_REALSIZEIMAGE,

        /// <summary>
        ///  Draws half-sunken border around the control.
        /// </summary>
        Sunken = STATIC_STYLES.SS_SUNKEN,

        /// <summary>
        ///  Displays text as an edit control would.
        /// </summary>
        EditControl = STATIC_STYLES.SS_EDITCONTROL,

        /// <summary>
        ///  If the end of the string doesn't fit, ellipsis are added to the end.
        ///  If a word doesn't fit into the control rectangle, it is truncated.
        ///  Forces one line with no word wrap.
        /// </summary>
        EndEllipsis = STATIC_STYLES.SS_ENDELLIPSIS,

        /// <summary>
        ///  Replaces characters in the middle of the string with ellipses.
        ///  Forces one line with no word wrap.
        /// </summary>
        PathEllipsis = STATIC_STYLES.SS_PATHELLIPSIS,

        /// <summary>
        ///  Truncates any word that does not fit in the rectangle and adds ellipses.
        ///  Forces one line with no word wrap.
        /// </summary>
        WordEllipsis = STATIC_STYLES.SS_WORDELLIPSIS,

        // SS_ELLIPSISMASK      = 0x0000C000
    }
}