// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Specialized;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.System.SystemServices;
using static Windows.Win32.ComExtensions;

namespace Windows;

[TestClass]
[DoNotParallelize]
public unsafe class WindowDropTargetTests
{
    [STATestMethod]
    public void DropTarget_CustomControl_RoutesOleCallbacks()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using CustomControl target = new(new Rectangle(0, 0, 200, 100), parentWindow: parent);

        AssertDropCallbacks(target);
    }

    [STATestMethod]
    public void DropTarget_EditControl_RoutesOleCallbacks()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using EditControl target = new(
            new Rectangle(0, 0, 200, 100),
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent);

        AssertDropCallbacks(target);
    }

    [STATestMethod]
    public void EnableDrop_EditControl_ReplacesSelectionAndInsertsAtCaret()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using EditControl target = new(
            new Rectangle(0, 0, 200, 100),
            editStyle: EditControl.Styles.AutoHorizontalScroll,
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent);

        AssertTextDrop(target);
    }

    [STATestMethod]
    public void EnableDrop_RichEditControl_ReplacesSelectionAndInsertsAtCaret()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using RichEditControl target = new(
            new Rectangle(0, 0, 200, 100),
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent);

        AssertTextDrop(target);
    }

    [STATestMethod]
    public void EnableDrag_EditControl_RequiresPressInsideNonemptySelection()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using EditControl source = new(
            new Rectangle(0, 0, 200, 100),
            text: "Before selected after",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent);

        AssertTextDragSource(source, "selected");
    }

    [STATestMethod]
    public void EnableDrag_RichEditControl_RequiresPressInsideNonemptySelection()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using RichEditControl source = new(
            new Rectangle(0, 0, 200, 100),
            text: "Before\r\nselected\r\nafter",
            editStyle: RichEditControl.Styles.Multiline,
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent);

        AssertTextDragSource(source, "selected", selectionStart: "Before\r".Length);
    }

    [STATestMethod]
    public void EnableDragDrop_EditControl_MovesSelectionWithinSameControl()
    {
        using Window parent = new(new Rectangle(0, 0, 400, 200));
        using EditControl target = new(
            new Rectangle(0, 0, 300, 100),
            text: "0123456789",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent)
        {
            EnableDrag = true,
            EnableDrop = true
        };

        AssertSameControlMove(target, 2, 5, 8, "0156723489", 5);

        target.Text = "0123456789";
        AssertSameControlMove(target, 2, 5, 1, "0234156789", 1);

        target.Text = "0123456789";
        AssertSameControlMoveRejected(target, 2, 5, 3);
    }

    [STATestMethod]
    public void EnableDragDrop_EditControl_MovesSelectionAcrossControls()
    {
        using Window parent = new(new Rectangle(0, 0, 600, 200));
        using EditControl source = new(
            new Rectangle(0, 0, 250, 100),
            text: "012345",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent)
        {
            EnableDrag = true
        };
        using EditControl target = new(
            new Rectangle(300, 0, 250, 100),
            text: "abcdef",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent)
        {
            EnableDrop = true
        };

        source.SetSelection(2, 4);
        BeginTextDragSession(source, 2, 4);
        try
        {
            DropText(target, GetScreenPointForCharacter(target, 3), "23", DROPEFFECT.DROPEFFECT_MOVE)
                .Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
            source.EndTextDragSession(DragDropEffects.Move);
        }
        catch
        {
            source.EndTextDragSession(DragDropEffects.None);
            throw;
        }

        source.Text.Should().Be("0145");
        source.GetSelection().Should().Be((2, 2));
        target.Text.Should().Be("abc23def");
        target.GetSelection().Should().Be((3, 5));
    }

    [STATestMethod]
    public void EnableDragDrop_EditControl_SourceChangedDuringMove_KeepsInsertedCopy()
    {
        using Window parent = new(new Rectangle(0, 0, 600, 200));
        using EditControl source = new(
            new Rectangle(0, 0, 250, 100),
            text: "012345",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent)
        {
            EnableDrag = true
        };
        using EditControl target = new(
            new Rectangle(300, 0, 250, 100),
            text: "abcdef",
            style: WindowStyles.Child | WindowStyles.Visible,
            parentWindow: parent)
        {
            EnableDrop = true
        };

        source.SetSelection(2, 4);
        BeginTextDragSession(source, 2, 4);
        DropText(target, GetScreenPointForCharacter(target, 3), "23", DROPEFFECT.DROPEFFECT_MOVE)
            .Should().Be(DROPEFFECT.DROPEFFECT_MOVE);

        source.Text = "source changed";
        source.Text = "source changed again";
        source.Text = "source changed during drag";
        source.EndTextDragSession(DragDropEffects.Move);

        source.Text.Should().Be("source changed during drag");
        target.Text.Should().Be("abc23def");
    }

    [STATestMethod]
    public void DropTarget_SecondAttachment_IsRejected()
    {
        using Window target = new(new Rectangle(0, 0, 200, 100));
        using DropTarget dropTarget = new(target);

        Action attachSecondTarget = () => _ = new DropTarget(target);

        attachSecondTarget.Should().Throw<InvalidOperationException>()
            .WithMessage("*already has an attached drop target*");
        object? attachedWindow = dropTarget.TestAccessor.Dynamic._attachedWindow;
        attachedWindow.Should().BeSameAs(target);
    }

    [STATestMethod]
    public void EnableDrop_InvalidInsertionIndex_RejectsDropBeforeMutation()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using InvalidInsertionControl target = new(parent);
        target.EnableInvalidDrop();
        object dropTargetObject = target.TestAccessor.Dynamic._attachedDropTarget;
        using ComScope<IDropTarget> dropTarget = new(dropTargetObject.GetComPointer<IDropTarget>());
        UnicodeTextDataObject source = new("text");
        using ComScope<IDataObject> dataObject = new(source.GetComPointer<IDataObject>());
        DROPEFFECT effect = DROPEFFECT.DROPEFFECT_COPY;

        dropTarget.Pointer->DragEnter(dataObject.Pointer, default, default, &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTarget.Pointer->Drop(dataObject.Pointer, default, default, &effect).Should().Be(HRESULT.S_OK);

        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);
        target.ReplacementCalled.Should().BeFalse();
    }

    [STATestMethod]
    public void EnableDrop_IncompleteInsertionProviders_RejectsBeforeAttachment()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using InvalidInsertionControl target = new(parent);

        Action enableDrop = target.EnableIncompleteDrop;

        enableDrop.Should().Throw<ArgumentException>();
        object? attachedDropTarget = target.TestAccessor.Dynamic._attachedDropTarget;
        attachedDropTarget.Should().BeNull();
    }

    [STATestMethod]
    public void DropTarget_ParentDestruction_RevokesChildRegistration()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using CustomControl target = new(new Rectangle(0, 0, 200, 100), parentWindow: parent);
        using DropTarget dropTarget = new(target);

        parent.Dispose();

        target.Handle.IsNull.Should().BeTrue();
        object? attachedWindow = dropTarget.TestAccessor.Dynamic._attachedWindow;
        attachedWindow.Should().BeNull();
    }

    [STATestMethod]
    public void DropTarget_WrongThreadDispose_IsRejected()
    {
        using Window parent = new(new Rectangle(0, 0, 300, 200));
        using CustomControl target = new(new Rectangle(0, 0, 200, 100), parentWindow: parent);
        using DropTarget dropTarget = new(target);
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                dropTarget.Dispose();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.Start();
        thread.Join();

        failure.Should().BeOfType<InvalidOperationException>();
        object? attachedWindow = dropTarget.TestAccessor.Dynamic._attachedWindow;
        attachedWindow.Should().BeSameAs(target);
        dropTarget.Dispose();
    }

    [TestMethod]
    public void DropTarget_MtaThread_IsRejected()
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            using Window target = new(new Rectangle(0, 0, 200, 100));
            try
            {
                using DropTarget dropTarget = new(target);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.MTA);

        thread.Start();
        thread.Join();

        failure.Should().BeOfType<InvalidOperationException>();
    }

    [STATestMethod]
    public void DropTarget_DispatcherShutdown_RevokesSurvivingWindowRegistration()
    {
        Window? survivingTarget = null;
        DropTarget? survivingDropTarget = null;
        try
        {
            Application.Run(() =>
            {
                Window root = new(new Rectangle(0, 0, 300, 200));
                survivingTarget = new(new Rectangle(350, 0, 200, 100));
                survivingDropTarget = new(survivingTarget);
                root.Dispatcher.TryPost(() => PInvoke.DestroyWindow(root.Handle)).Should().BeTrue();
                return root;
            });

            survivingTarget.Should().NotBeNull();
            survivingTarget!.Handle.IsNull.Should().BeFalse();
            object? attachedWindow = survivingDropTarget!.TestAccessor.Dynamic._attachedWindow;
            attachedWindow.Should().BeNull();
        }
        finally
        {
            survivingDropTarget?.Dispose();
            survivingTarget?.Dispose();
        }
    }

    [TestMethod]
    public void GetText_ByteLengthAtLimit_ReturnsCharacterLength()
    {
        Func<nuint, int, int> getUnicodeCharacterLength =
            typeof(DropDataObject).TestAccessor.CreateDelegate<Func<nuint, int, int>>("GetUnicodeCharacterLength");
        nuint maximumByteLength = checked((nuint)((ulong)int.MaxValue * sizeof(char)));

        getUnicodeCharacterLength(maximumByteLength, int.MaxValue).Should().Be(int.MaxValue);
    }

    [TestMethod]
    public void GetText_ByteLengthAboveIntRange_ThrowsInvalidDataException()
    {
        Func<nuint, int, int> getUnicodeCharacterLength =
            typeof(DropDataObject).TestAccessor.CreateDelegate<Func<nuint, int, int>>("GetUnicodeCharacterLength");
        ulong overLimitValue = ((ulong)int.MaxValue * sizeof(char)) + sizeof(char);
        nuint overLimit = checked((nuint)overLimitValue);

        Action getLength = () => getUnicodeCharacterLength(overLimit, int.MaxValue);

        getLength.Should().Throw<InvalidDataException>();
    }

    [TestMethod]
    public void GetText_OddByteLength_ThrowsInvalidDataException()
    {
        Func<nuint, int, int> getUnicodeCharacterLength =
            typeof(DropDataObject).TestAccessor.CreateDelegate<Func<nuint, int, int>>("GetUnicodeCharacterLength");

        Action getLength = () => getUnicodeCharacterLength(3, 2);

        getLength.Should().Throw<InvalidDataException>()
            .WithMessage("*odd byte length*");
    }

    [TestMethod]
    public void UnicodeTextDataObject_ProvidesTextAndEnumeratesFormat()
    {
        const string Text = "Source Unicode text: \u03bb";
        List<string> phases = [];
        UnicodeTextDataObject source = new(Text);
        ObservingDataObjectAdapter adapter = new(source, phases);
        using ComScope<IDataObject> dataObject = new(adapter.GetComPointer<IDataObject>());
        DropDataObject data = new(dataObject.Pointer);

        data.ContainsText.Should().BeTrue();
        data.GetText().Should().Be(Text);
        phases.Should().Equal(
            "query-get-data-entered",
            "query-get-data-returned",
            "query-get-data-entered",
            "query-get-data-returned",
            "get-data-entered",
            "get-data-succeeded",
            "get-data-returned");

        IEnumFORMATETC* enumeratorPointer;
        dataObject.Pointer->EnumFormatEtc(
            (uint)DATADIR.DATADIR_GET,
            &enumeratorPointer).Should().Be(HRESULT.S_OK);
        using ComScope<IEnumFORMATETC> enumerator = new(enumeratorPointer);
        FORMATETC format;
        uint fetched;
        enumerator.Pointer->Next(1, &format, &fetched).Should().Be(HRESULT.S_OK);
        fetched.Should().Be(1);
        format.cfFormat.Should().Be((ushort)CLIPBOARD_FORMAT.CF_UNICODETEXT);
        format.dwAspect.Should().Be((uint)DVASPECT.DVASPECT_CONTENT);
        format.lindex.Should().Be(-1);
        format.tymed.Should().Be((uint)TYMED.TYMED_HGLOBAL);
        enumerator.Pointer->Next(1, &format, &fetched).Should().Be(PInvoke.S_FALSE);
        fetched.Should().Be(0);
    }

    [TestMethod]
    public void TextDragSource_RequiresMovementBeyondSystemDragThreshold()
    {
        Func<Point, Point, int, int, bool> hasExceededDragThreshold =
            typeof(TextDragSource).TestAccessor.CreateDelegate<Func<Point, Point, int, int, bool>>(
                "HasExceededDragThreshold");
        Point start = new(10, 20);

        hasExceededDragThreshold(start, start, 4, 6).Should().BeFalse();
        hasExceededDragThreshold(start, new Point(11, 19), 4, 6).Should().BeFalse();
        hasExceededDragThreshold(start, new Point(14, 26), 4, 6).Should().BeFalse();
        hasExceededDragThreshold(start, new Point(6, 14), 4, 6).Should().BeFalse();
        hasExceededDragThreshold(start, new Point(15, 20), 4, 6).Should().BeTrue();
        hasExceededDragThreshold(start, new Point(5, 20), 4, 6).Should().BeTrue();
        hasExceededDragThreshold(start, new Point(10, 27), 4, 6).Should().BeTrue();
        hasExceededDragThreshold(start, new Point(10, 13), 4, 6).Should().BeTrue();
    }

    private static void AssertDropCallbacks(Window target)
    {
        using DropTarget dropTarget = new(target);
        const string DroppedText = "Text from another UI framework";
        string[] droppedFiles =
        [
            Path.Combine(Path.GetTempPath(), "thirtytwo-drop-one.txt"),
            Path.Combine(Path.GetTempPath(), "thirtytwo-drop-two.txt")
        ];
        global::System.Windows.Forms.DataObject source = new();
        source.SetText(DroppedText);
        StringCollection fileDropList = [.. droppedFiles];
        source.SetFileDropList(fileDropList);

        List<string> callbacks = [];
        DropDataObject? escapedData = null;
        dropTarget.DragEnter += (_, eventArgs) =>
        {
            callbacks.Add("enter");
            eventArgs.KeyState.Should().Be(DragDropKeyStates.ControlKey);
            eventArgs.ScreenLocation.Should().Be(new Point(17, 29));
            eventArgs.AllowedEffect.Should().Be(DragDropEffects.Copy | DragDropEffects.Move);
            eventArgs.Data.ContainsText.Should().BeTrue();
            eventArgs.Data.GetText().Should().Be(DroppedText);
            eventArgs.Data.GetText(DroppedText.Length).Should().Be(DroppedText);
            Action getOversizedText = () => eventArgs.Data.GetText(DroppedText.Length - 1);
            getOversizedText.Should().Throw<InvalidDataException>();
            eventArgs.Data.ContainsFilePaths.Should().BeTrue();
            eventArgs.Data.GetFilePaths().Should().Equal(droppedFiles);
            Action getTooManyFiles = () => eventArgs.Data.GetFilePaths(maximumFileCount: 1);
            getTooManyFiles.Should().Throw<InvalidDataException>();
            Action getLongFilePath = () => eventArgs.Data.GetFilePaths(maximumPathLength: 1);
            getLongFilePath.Should().Throw<InvalidDataException>();
            Action getLargeCombinedPaths = () => eventArgs.Data.GetFilePaths(maximumTotalPathLength: 1);
            getLargeCombinedPaths.Should().Throw<InvalidDataException>();
            Action disposeDuringCallback = dropTarget.Dispose;
            disposeDuringCallback.Should().Throw<InvalidOperationException>();
            object? attachedWindow = dropTarget.TestAccessor.Dynamic._attachedWindow;
            attachedWindow.Should().BeSameAs(target);
            escapedData = eventArgs.Data;
            eventArgs.Effect = DragDropEffects.Copy;
        };
        dropTarget.DragOver += (_, eventArgs) =>
        {
            callbacks.Add("over");
            eventArgs.Data.GetText().Should().Be(DroppedText);
            eventArgs.Effect = DragDropEffects.Move;
        };
        dropTarget.DragDrop += (_, eventArgs) =>
        {
            callbacks.Add("drop");
            eventArgs.Data.GetText().Should().Be(DroppedText);
            eventArgs.Effect = DragDropEffects.Copy;
        };
        dropTarget.DragLeave += (_, _) => callbacks.Add("leave");

        using ComScope<IDropTarget> dropTargetPointer = new(dropTarget.GetComPointer<IDropTarget>());
        using ComScope<IDataObject> dataObject = new(source.GetComPointer<IDataObject>());
        POINTL point = new() { x = 17, y = 29 };
        DROPEFFECT effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;

        dropTargetPointer.Pointer->DragEnter(
            dataObject.Pointer,
            MODIFIERKEYS_FLAGS.MK_CONTROL,
            point,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        escapedData.Should().NotBeNull();
        Action useEscapedData = () => escapedData!.GetText();
        useEscapedData.Should().Throw<ObjectDisposedException>();

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTargetPointer.Pointer->DragOver(default, point, &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_MOVE);

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTargetPointer.Pointer->Drop(dataObject.Pointer, default, point, &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        callbacks.Should().Equal("enter", "over", "drop");

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTargetPointer.Pointer->DragEnter(
            dataObject.Pointer,
            MODIFIERKEYS_FLAGS.MK_CONTROL,
            point,
            &effect).Should().Be(HRESULT.S_OK);
        dropTargetPointer.Pointer->DragLeave().Should().Be(HRESULT.S_OK);
        callbacks.Should().EndWith(["enter", "leave"]);

        EventHandler<DragEventArgs> selectDisallowedEffect = (_, eventArgs) =>
            eventArgs.Effect = DragDropEffects.Link;
        dropTarget.DragEnter += selectDisallowedEffect;
        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        HRESULT invalidEffectResult = dropTargetPointer.Pointer->DragEnter(
            dataObject.Pointer,
            MODIFIERKEYS_FLAGS.MK_CONTROL,
            point,
            &effect);
        dropTarget.DragEnter -= selectDisallowedEffect;
        invalidEffectResult.Failed.Should().BeTrue();
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTargetPointer.Pointer->DragOver(default, point, &effect).Should().Be(PInvoke.E_UNEXPECTED);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTargetPointer.Pointer->DragEnter(
            dataObject.Pointer,
            MODIFIERKEYS_FLAGS.MK_CONTROL,
            point,
            &effect).Should().Be(HRESULT.S_OK);
        EventHandler<DragEventArgs> throwOnDragOver = (_, _) => throw new InvalidOperationException("Expected failure.");
        dropTarget.DragOver += throwOnDragOver;
        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTargetPointer.Pointer->DragOver(default, point, &effect).Failed.Should().BeTrue();
        dropTarget.DragOver -= throwOnDragOver;
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTargetPointer.Pointer->DragOver(default, point, &effect).Should().Be(PInvoke.E_UNEXPECTED);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);

        dropTarget.Dispose();
        object? detachedWindow = dropTarget.TestAccessor.Dynamic._attachedWindow;
        detachedWindow.Should().BeNull();
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTargetPointer.Pointer->DragOver(default, point, &effect).Should().Be(HRESULT.COR_E_OBJECTDISPOSED);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);
    }

    private static void AssertTextDrop(EditBase target)
    {
        const string InitialText = "Before selected after";
        const string DroppedText = "dropped \u03bb ";
        target.Text = InitialText;
        target.SetSelection(7, 15);
        target.EnableDrop.Should().BeFalse();

        target.EnableDrop = true;

        target.EnableDrop.Should().BeTrue();
        object dropTargetObject = target.TestAccessor.Dynamic._attachedDropTarget;
        dropTargetObject.Should().BeOfType<TextDropTarget>();
        using ComScope<IDropTarget> dropTarget = new(dropTargetObject.GetComPointer<IDropTarget>());
        global::System.Windows.Forms.DataObject source = new();
        source.SetText(DroppedText);
        using ComScope<IDataObject> dataObject = new(source.GetComPointer<IDataObject>());
        POINTL selectionStart = GetScreenPointForCharacter(target, 7);
        POINTL beforeAfter = GetScreenPointForCharacter(target, 16);
        DROPEFFECT effect = DROPEFFECT.DROPEFFECT_MOVE;

        dropTarget.Pointer->DragEnter(
            dataObject.Pointer,
            default,
            selectionStart,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
        target.GetSelection().Should().Be((7, 7));
        ((bool)target.TestAccessor.Dynamic._dropInsertionCaretVisible).Should().BeTrue();
        effect = DROPEFFECT.DROPEFFECT_MOVE;
        dropTarget.Pointer->Drop(
            dataObject.Pointer,
            default,
            selectionStart,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
        target.Text.Should().Be("Before dropped λ selected after");
        target.GetSelection().Should().Be((7, 7 + DroppedText.Length));

        target.Text = InitialText;
        target.SetSelection(7, 15);

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTarget.Pointer->DragEnter(
            dataObject.Pointer,
            MODIFIERKEYS_FLAGS.MK_CONTROL,
            selectionStart,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        dropTarget.Pointer->DragLeave().Should().Be(HRESULT.S_OK);
        target.GetSelection().Should().Be((7, 15));
        ((bool)target.TestAccessor.Dynamic._dropInsertionCaretVisible).Should().BeFalse();

        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTarget.Pointer->DragEnter(
            dataObject.Pointer,
            default,
            selectionStart,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
        target.GetSelection().Should().Be((7, 7));
        effect = DROPEFFECT.DROPEFFECT_COPY | DROPEFFECT.DROPEFFECT_MOVE;
        dropTarget.Pointer->DragOver(default, beforeAfter, &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
        target.GetSelection().Should().Be((16, 16));
        dropTarget.Pointer->DragLeave().Should().Be(HRESULT.S_OK);
        target.GetSelection().Should().Be((7, 15));

        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTarget.Pointer->DragEnter(
            dataObject.Pointer,
            default,
            selectionStart,
            &effect).Should().Be(HRESULT.S_OK);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTarget.Pointer->Drop(
            dataObject.Pointer,
            default,
            beforeAfter,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        target.Text.Should().Be("Before selected dropped \u03bb after");
        int caret = 16 + DroppedText.Length;
            target.GetSelection().Should().Be((16, caret));

        target.EnableDrop = false;
        target.EnableDrop.Should().BeFalse();
        effect = DROPEFFECT.DROPEFFECT_COPY;
        dropTarget.Pointer->DragOver(
            default,
            beforeAfter,
            &effect).Should().Be(HRESULT.COR_E_OBJECTDISPOSED);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_NONE);

        const string InsertedText = "inserted ";
        target.Text = "Before after";
        target.SetSelection(7, 7);
        target.EnableDrop = true;
        target.EnableDrop.Should().BeTrue();
        object replacementDropTargetObject = target.TestAccessor.Dynamic._attachedDropTarget;
        replacementDropTargetObject.Should().NotBeSameAs(dropTargetObject);
        using ComScope<IDropTarget> replacementDropTarget = new(
            replacementDropTargetObject.GetComPointer<IDropTarget>());
        global::System.Windows.Forms.DataObject insertionSource = new();
        insertionSource.SetText(InsertedText);
        using ComScope<IDataObject> insertionDataObject = new(insertionSource.GetComPointer<IDataObject>());
        POINTL insertionPoint = GetScreenPointForCharacter(target, 7);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        replacementDropTarget.Pointer->DragEnter(
            insertionDataObject.Pointer,
            default,
            insertionPoint,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        effect = DROPEFFECT.DROPEFFECT_COPY;
        replacementDropTarget.Pointer->Drop(
            insertionDataObject.Pointer,
            default,
            insertionPoint,
            &effect).Should().Be(HRESULT.S_OK);
        effect.Should().Be(DROPEFFECT.DROPEFFECT_COPY);
        target.Text.Should().Be("Before inserted after");
        target.GetSelection().Should().Be((7, 7 + InsertedText.Length));

        string textBeforeEmptyDrop = target.Text;
        DropText(
            target,
            insertionPoint,
            string.Empty,
            DROPEFFECT.DROPEFFECT_MOVE).Should().Be(DROPEFFECT.DROPEFFECT_NONE);
        target.Text.Should().Be(textBeforeEmptyDrop);

        target.EnableDrop = false;
    }

    private static void AssertSameControlMove(
        EditBase target,
        int sourceStart,
        int sourceEnd,
        int insertionIndex,
        string expectedText,
        int expectedSelectionStart)
    {
        target.SetSelection(sourceStart, sourceEnd);
        BeginTextDragSession(target, sourceStart, sourceEnd);
        try
        {
            int sourceLength = sourceEnd - sourceStart;
            DropText(
                target,
                GetScreenPointForCharacter(target, insertionIndex),
                target.Text[sourceStart..sourceEnd],
                DROPEFFECT.DROPEFFECT_MOVE).Should().Be(DROPEFFECT.DROPEFFECT_MOVE);
            target.GetSelection().Should().Be((insertionIndex, insertionIndex + sourceLength));
            target.EndTextDragSession(DragDropEffects.Move);
        }
        catch
        {
            target.EndTextDragSession(DragDropEffects.None);
            throw;
        }

        target.Text.Should().Be(expectedText);
        target.GetSelection().Should().Be((expectedSelectionStart, expectedSelectionStart + (sourceEnd - sourceStart)));
    }

    private static void AssertSameControlMoveRejected(
        EditBase target,
        int sourceStart,
        int sourceEnd,
        int insertionIndex)
    {
        string originalText = target.Text;
        target.SetSelection(sourceStart, sourceEnd);
        BeginTextDragSession(target, sourceStart, sourceEnd);
        try
        {
            DropText(
                target,
                GetScreenPointForCharacter(target, insertionIndex),
                target.Text[sourceStart..sourceEnd],
                DROPEFFECT.DROPEFFECT_MOVE).Should().Be(DROPEFFECT.DROPEFFECT_NONE);
            target.EndTextDragSession(DragDropEffects.None);
        }
        catch
        {
            target.EndTextDragSession(DragDropEffects.None);
            throw;
        }

        target.Text.Should().Be(originalText);
        target.GetSelection().Should().Be((sourceStart, sourceEnd));
    }

    private static void BeginTextDragSession(EditBase source, int start, int end)
        => source.BeginTextDragSession(
            start,
            end,
            () => (long)source.TestAccessor.Dynamic._textGeneration,
            source.GetSelection,
            source.SetSelection,
            text => source.ReplaceSelection(text));

    private static unsafe DROPEFFECT DropText(
        EditBase target,
        POINTL point,
        string text,
        DROPEFFECT allowedEffect)
    {
        object dropTargetObject = target.TestAccessor.Dynamic._attachedDropTarget;
        using ComScope<IDropTarget> dropTarget = new(dropTargetObject.GetComPointer<IDropTarget>());
        object source;
        if (text.Length == 0)
        {
            source = new UnicodeTextDataObject(text);
        }
        else
        {
            global::System.Windows.Forms.DataObject data = new();
            data.SetText(text);
            source = data;
        }

        using ComScope<IDataObject> dataObject = new(source.GetComPointer<IDataObject>());
        (int Start, int End) selectionBeforeDragEnter = target.GetSelection();
        DROPEFFECT effect = allowedEffect;
        dropTarget.Pointer->DragEnter(dataObject.Pointer, default, point, &effect).Should().Be(HRESULT.S_OK);
        if (Window.ActiveTextDragSession?.IsSource(target) == true)
        {
            target.GetSelection().Should().Be(
                selectionBeforeDragEnter,
                "the source selection should remain visible until the drop");
        }

        effect = allowedEffect;
        dropTarget.Pointer->Drop(dataObject.Pointer, default, point, &effect).Should().Be(HRESULT.S_OK);
        return effect;
    }

    private static POINTL GetScreenPointForCharacter(EditBase target, int index)
    {
        Point point = GetCharacterPosition(target, index);
        target.ClientToScreen(ref point).Should().BeTrue();
        return new POINTL { x = point.X, y = point.Y };
    }

    private static void AssertTextDragSource(
        EditBase source,
        string expectedSelectedText,
        int? selectionStart = null)
    {
        string text = source.Text;
        int start = selectionStart ?? text.IndexOf(expectedSelectedText, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        source.SetSelection(start, start + expectedSelectedText.Length);
        source.EnableDrag.Should().BeFalse();

        source.EnableDrag = true;

        source.EnableDrag.Should().BeTrue();
        object dragSourceObject = source.TestAccessor.Dynamic._attachedDragSource;
        dragSourceObject.Should().BeOfType<TextDragSource>();
        string selectedText = dragSourceObject.TestAccessor.Dynamic.GetText();
        selectedText.Should().Be(expectedSelectedText);
        using ComScope<IDropSource> dragSource = new(dragSourceObject.GetComPointer<IDropSource>());
        dragSource.Pointer->QueryContinueDrag(
            false,
            MODIFIERKEYS_FLAGS.MK_LBUTTON).Should().Be(HRESULT.S_OK);
        dragSource.Pointer->QueryContinueDrag(false, default).Should().Be(PInvoke.DRAGDROP_S_DROP);
        dragSource.Pointer->QueryContinueDrag(true, default).Should().Be(PInvoke.DRAGDROP_S_CANCEL);

        Point insideSelection = GetCharacterPosition(source, start + (expectedSelectedText.Length / 2));
        int hitCharacter = source.TestAccessor.Dynamic.GetCharacterIndexFromPoint(insideSelection);
        hitCharacter.Should().BeInRange(
            start,
            start + expectedSelectedText.Length - 1,
            $"character {start + (expectedSelectedText.Length / 2)} is displayed at {insideSelection}");
        Point insideSelectionScreen = insideSelection;
        source.ClientToScreen(ref insideSelectionScreen).Should().BeTrue();
        PInvoke.SetCursorPos(insideSelectionScreen.X, insideSelectionScreen.Y).Should().NotBe(default(BOOL));
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.SetCursor, insideSelection)
            .Should().Be((LRESULT)1);
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonDown, insideSelection)
            .Should().Be((LRESULT)0);
        string? pendingText = dragSourceObject.TestAccessor.Dynamic._pendingText;
        pendingText.Should().Be(expectedSelectedText);
        ((bool)dragSourceObject.TestAccessor.Dynamic._hasCapture).Should().BeTrue();
        source.GetSelection().Should().Be((start, start + expectedSelectedText.Length));
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.MouseMove, insideSelection).Should().BeNull();
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonUp, insideSelection)
            .Should().Be((LRESULT)0);
        ((bool)dragSourceObject.TestAccessor.Dynamic._hasCapture).Should().BeFalse();
        source.GetSelection().Should().Be((hitCharacter, hitCharacter));

        source.SetSelection(start, start + expectedSelectedText.Length);
        Point outsideSelection = GetCharacterPosition(source, 0);
        Point outsideSelectionScreen = outsideSelection;
        source.ClientToScreen(ref outsideSelectionScreen).Should().BeTrue();
        PInvoke.SetCursorPos(outsideSelectionScreen.X, outsideSelectionScreen.Y).Should().NotBe(default(BOOL));
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.SetCursor, outsideSelection).Should().BeNull();
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonDown, outsideSelection);
        pendingText = dragSourceObject.TestAccessor.Dynamic._pendingText;
        pendingText.Should().BeNull();
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonUp, outsideSelection);
        SendMouseMessage(source, MessageType.LeftButtonDown, outsideSelection);
        SendMouseMessage(source, MessageType.LeftButtonUp, outsideSelection);
        source.GetSelection().Should().NotBe((start, start + expectedSelectedText.Length));

        source.SetSelection(start, start);
        string emptySelectionText = dragSourceObject.TestAccessor.Dynamic.GetText();
        emptySelectionText.Should().BeEmpty();
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonDown, insideSelection);
        pendingText = dragSourceObject.TestAccessor.Dynamic._pendingText;
        pendingText.Should().BeNull();
        InvokeTextDragMouseHandler(dragSourceObject, source, MessageType.LeftButtonUp, insideSelection);

        source.EnableDrag = false;
        source.EnableDrag.Should().BeFalse();
        object? detachedSource = source.TestAccessor.Dynamic._attachedDragSource;
        detachedSource.Should().BeNull();
    }

    private static Point GetCharacterPosition(EditBase source, int index)
    {
        Point position = source.TestAccessor.Dynamic.GetCharacterPosition(index);
        return new Point(position.X + 1, position.Y + 1);
    }

    private static void SendMouseMessage(EditBase source, MessageType message, Point position)
        => _ = source.SendMessage(
            message,
            (WPARAM)(uint)MouseKey.LeftButton,
            PackPoint(position.X, position.Y));

    private static LRESULT? InvokeTextDragMouseHandler(
        object dragSource,
        EditBase source,
        MessageType message,
        Point position)
        => dragSource.TestAccessor.Dynamic.WindowMessageHandler(
            source,
            source.Handle,
            message,
            (WPARAM)(uint)MouseKey.LeftButton,
            PackPoint(position.X, position.Y));

    private static LPARAM PackPoint(int x, int y)
        => (LPARAM)(nint)((uint)(ushort)x | ((uint)(ushort)y << 16));

    private sealed unsafe class ObservingDataObjectAdapter
        : IDataObject.Interface, IManagedWrapper<IDataObject>
    {
        private readonly IDataObject.Interface _inner;
        private readonly List<string> _phases;

        internal ObservingDataObjectAdapter(IDataObject.Interface inner, List<string> phases)
        {
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentNullException.ThrowIfNull(phases);
            _inner = inner;
            _phases = phases;
        }

        HRESULT IDataObject.Interface.GetData(FORMATETC* format, STGMEDIUM* medium)
        {
            Record("get-data-entered");
            try
            {
                HRESULT result = _inner.GetData(format, medium);
                if (result.Succeeded)
                {
                    Record("get-data-succeeded");
                }

                return result;
            }
            finally
            {
                Record("get-data-returned");
            }
        }

        HRESULT IDataObject.Interface.GetDataHere(FORMATETC* format, STGMEDIUM* medium)
            => _inner.GetDataHere(format, medium);

        HRESULT IDataObject.Interface.QueryGetData(FORMATETC* format)
        {
            Record("query-get-data-entered");
            try
            {
                return _inner.QueryGetData(format);
            }
            finally
            {
                Record("query-get-data-returned");
            }
        }

        HRESULT IDataObject.Interface.GetCanonicalFormatEtc(FORMATETC* input, FORMATETC* output)
            => _inner.GetCanonicalFormatEtc(input, output);

        HRESULT IDataObject.Interface.SetData(FORMATETC* format, STGMEDIUM* medium, BOOL release)
            => _inner.SetData(format, medium, release);

        HRESULT IDataObject.Interface.EnumFormatEtc(uint direction, IEnumFORMATETC** formats)
            => _inner.EnumFormatEtc(direction, formats);

        HRESULT IDataObject.Interface.DAdvise(FORMATETC* format, uint flags, IAdviseSink* sink, uint* connection)
            => _inner.DAdvise(format, flags, sink, connection);

        HRESULT IDataObject.Interface.DUnadvise(uint connection)
            => _inner.DUnadvise(connection);

        HRESULT IDataObject.Interface.EnumDAdvise(IEnumSTATDATA** enumerator)
            => _inner.EnumDAdvise(enumerator);

        private void Record(string phase)
        {
            try
            {
                _phases.Add(phase);
            }
            catch
            {
            }
        }
    }

    private sealed class InvalidInsertionControl : CustomControl
    {
        internal InvalidInsertionControl(Window parent)
            : base(
                new Rectangle(0, 0, 200, 100),
                style: WindowStyles.Child | WindowStyles.Visible,
                parentWindow: parent)
        {
        }

        internal bool ReplacementCalled { get; private set; }

        internal void EnableInvalidDrop()
            => SetTextDropEnabled(
                value: true,
                _ => ReplacementCalled = true,
                () => (0, 0),
                (_, _) => { },
                _ => int.MaxValue,
                () => 1);

        internal void EnableIncompleteDrop()
            => SetTextDropEnabled(
                value: true,
                _ => ReplacementCalled = true,
                getInsertionIndex: _ => 0);
    }
}
