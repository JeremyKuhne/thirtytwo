// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static class WrapperEnumExtensions
{
    public static HCURSOR.SetScope SetCursorScope(this CursorId cursor) => ((HCURSOR)cursor).SetCursorScope();
}