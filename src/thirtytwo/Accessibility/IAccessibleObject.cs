// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Variant;

namespace Windows.Accessibility;

public interface IAccessibleObject
{
    string? Name { get; }
    string? Description { get; }
    ObjectRoles Role { get; }
    ObjectState State { get; }
    string? Help { get; }
    string? KeyboardShortcut { get; }
    Rectangle Bounds { get; }
    IAccessibleObject HitTest(Point location);
    int ChildCount { get; }
    IAccessibleObject Parent { get; }
    string? DefaultAction { get; }
    bool DoDefaultAction();
    string? GetValue();
    bool SetValue(BSTR value);
    IAccessibleObject GetFocus();
    bool SupportsSelection { get; }
    VARIANT GetSelection();
    HRESULT SetSelection(SelectionFlags flags);
}