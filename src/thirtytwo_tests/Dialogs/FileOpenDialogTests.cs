// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows;
using Windows.Dialogs;
using Windows.Messages;
using Windows.Win32.Foundation;

namespace Tests.Windows.Dialogs;

public class FileOpenDialogTests
{
    [StaFact]
    public void FileOpenDialog_Create()
    {
        using FileOpenDialog dialog = new();
    }

    [StaFact]
    public void FileOpenDialog_GetResults_AfterCreation()
    {
        using FileOpenDialog dialog = new();
        Assert.Throws<COMException>(() => dialog.GetResults());
    }

    [StaFact]
    public void FileOpenDialog_GetResults_AfterShow()
    {
        using Window window = new(Window.DefaultBounds);
        using FileOpenDialog dialog = new(window);

        EnterIdleHandler.Attach(window, (bool isDialog, HWND hwnd) =>
        {
            hwnd.SendMessage(MessageType.Close);
        });

        dialog.ShowDialog().Should().BeFalse();

        // Can't get results if none were selected
        Assert.Throws<COMException>(() => dialog.GetResults());
    }

    [StaFact]
    public void FileOpenDialog_GetHWND_WithoutParent()
    {
        using FileOpenDialog dialog = new();
        HWND hwnd = dialog.Handle;
        hwnd.Should().NotBeNull();
    }

    [StaFact]
    public void FileOpenDialog_ShowAndClose()
    {
        using Window window = new(Window.DefaultBounds);
        using FileOpenDialog dialog = new(window);

        EnterIdleHandler.Attach(window, (bool isDialog, HWND hwnd) =>
        {
            isDialog.Should().BeTrue();
            hwnd.SendMessage(MessageType.Close);
        });

        dialog.ShowDialog().Should().BeFalse();
    }

    [StaFact]
    public void FileOpenDialog_DialogOptions_Get()
    {
        using FileOpenDialog dialog = new();
        FileDialog.Options options = dialog.DialogOptions;
        options.Should().Be(FileDialog.Options.NoChangeDirectory | FileDialog.Options.PathMustExist | FileDialog.Options.FileMustExist);
    }

    [StaFact]
    public void FileOpenDialog_CurrentSelection_AfterCreate()
    {
        using FileOpenDialog dialog = new();
        dialog.CurrentSelection.Should().BeNull();
    }

    [StaFact]
    public void FileOpenDialog_CurrentSelection_AfterShow()
    {
        using FileOpenDialog dialog = new();
        dialog.SelectionChanged += (object? sender, EventArgs e) =>
        {
            dialog.CurrentSelection.Should().BeNull();
            dialog.Close();
        };

        dialog.ShowDialog().Should().BeFalse();
    }

    [StaFact(Skip = "For manual testing.")]
    public void FileOpenDialog_Manual()
    {
        using FileOpenDialog dialog = new();

        dialog.SelectionChanged += (object? sender, EventArgs e) =>
        {
            string? selection = dialog.CurrentSelection;
        };

        dialog.ShowDialog();
    }
}
