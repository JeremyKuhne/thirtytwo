// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Variant;

namespace Windows.Accessibility;

/// <summary>
///  Defines the managed accessibility contract used to supply Microsoft Active Accessibility (MSAA)
///  information for an object and its children.
/// </summary>
public interface IAccessibleObject
{
    /// <summary>
    ///  Gets the localized name announced for the object.
    /// </summary>
    /// <returns>The object name, or <see langword="null"/> when the object does not expose a name.</returns>
    string? Name { get; }

    /// <summary>
    ///  Gets a localized description that provides supplemental details about the object.
    /// </summary>
    /// <returns>The description text, or <see langword="null"/> when no description is available.</returns>
    string? Description { get; }

    /// <summary>
    ///  Gets the MSAA role that classifies the type of user interface element.
    /// </summary>
    /// <returns>An <see cref="ObjectRoles"/> value describing the control type semantics.</returns>
    ObjectRoles Role { get; }

    /// <summary>
    ///  Gets the current MSAA state flags for the object.
    /// </summary>
    /// <returns>A bitwise combination of <see cref="ObjectState"/> flags.</returns>
    ObjectState State { get; }

    /// <summary>
    ///  Gets localized help text associated with the object.
    /// </summary>
    /// <returns>Help text, or <see langword="null"/> when no help text is provided.</returns>
    string? Help { get; }

    /// <summary>
    ///  Gets the keyboard shortcut used to activate or focus the object.
    /// </summary>
    /// <returns>A shortcut string such as ALT+key, or <see langword="null"/> when none is defined.</returns>
    string? KeyboardShortcut { get; }

    /// <summary>
    ///  Gets the object bounds in screen coordinates.
    /// </summary>
    /// <returns>The screen rectangle occupied by the object.</returns>
    Rectangle Bounds { get; }

    /// <summary>
    ///  Performs hit testing and returns the accessible object at the specified screen point.
    /// </summary>
    /// <param name="location">The hit test location in screen coordinates.</param>
    /// <returns>The matching accessible object for <paramref name="location"/>.</returns>
    IAccessibleObject HitTest(Point location);

    /// <summary>
    ///  Gets the number of immediate accessible children.
    /// </summary>
    /// <returns>The number of children directly contained by this object.</returns>
    int ChildCount { get; }

    /// <summary>
    ///  Gets the parent accessible object.
    /// </summary>
    /// <returns>The parent object in the accessible tree.</returns>
    IAccessibleObject Parent { get; }

    /// <summary>
    ///  Gets the localized description of the action that occurs when the object is activated.
    /// </summary>
    /// <returns>Default action text, or <see langword="null"/> if the object has no default action.</returns>
    string? DefaultAction { get; }

    /// <summary>
    ///  Executes the object's default action.
    /// </summary>
    /// <returns>
    ///  <see langword="true"/> if the object supports and performed a default action; otherwise,
    ///  <see langword="false"/>.
    /// </returns>
    bool DoDefaultAction();

    /// <summary>
    ///  Gets the value exposed by the object.
    /// </summary>
    /// <returns>The current value, or <see langword="null"/> if the object does not expose a value.</returns>
    string? GetValue();

    /// <summary>
    ///  Sets the value exposed by the object.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns><see langword="true"/> if the value was accepted; otherwise, <see langword="false"/>.</returns>
    bool SetValue(BSTR value);

    /// <summary>
    ///  Gets the object that currently has keyboard focus within this object's scope.
    /// </summary>
    /// <returns>The focused object.</returns>
    IAccessibleObject GetFocus();

    /// <summary>
    ///  Gets a value indicating whether this object supports selection operations.
    /// </summary>
    /// <returns>
    ///  <see langword="true"/> if selection retrieval and updates are supported; otherwise,
    ///  <see langword="false"/>.
    /// </returns>
    bool SupportsSelection { get; }

    /// <summary>
    ///  Gets the current selection payload represented as an MSAA variant.
    /// </summary>
    /// <returns>
    ///  A <see cref="VARIANT"/> containing no selection, one selected child, or a COM enumerator for multiple
    ///  selected children.
    /// </returns>
    VARIANT GetSelection();

    /// <summary>
    ///  Applies a selection command to this object.
    /// </summary>
    /// <param name="flags">The selection behavior flags to apply.</param>
    /// <returns>An HRESULT indicating success or failure of the requested selection operation.</returns>
    HRESULT SetSelection(SelectionFlags flags);
}