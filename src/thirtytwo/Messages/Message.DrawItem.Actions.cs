// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref partial struct DrawItem
    {
        [Flags]
        public enum Actions : uint
        {
            DrawEntire = ODA_FLAGS.ODA_DRAWENTIRE,
            Select = ODA_FLAGS.ODA_SELECT,
            Focus = ODA_FLAGS.ODA_FOCUS,
        }
    }
}