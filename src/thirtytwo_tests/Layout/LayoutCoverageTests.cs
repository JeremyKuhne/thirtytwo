// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows;

[TestClass]
public class LayoutCoverageTests
{
    [TestMethod]
    public void FixedPercentLayout_AllAlignments_UseNonzeroBoundsOrigin()
    {
        Rectangle bounds = new(10, 20, 100, 60);
        (VerticalAlignment Vertical, HorizontalAlignment Horizontal, Point Expected)[] cases =
        [
            (VerticalAlignment.Top, HorizontalAlignment.Left, new(10, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Center, new(40, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Right, new(70, 20)),
            (VerticalAlignment.Center, HorizontalAlignment.Left, new(10, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Center, new(40, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Right, new(70, 35)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Left, new(10, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Center, new(40, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Right, new(70, 50))
        ];

        foreach ((VerticalAlignment vertical, HorizontalAlignment horizontal, Point expected) in cases)
        {
            RecordingLayoutHandler handler = new();
            FixedPercentLayout layout = new(
                handler,
                heightPercent: 0.5f,
                widthPercent: 0.4f,
                vertical,
                horizontal);

            layout.Layout(bounds, 1.25f);

            handler.LastBounds.Should().Be(new Rectangle(expected, new Size(40, 30)));
            handler.LastScale.Should().Be(1.25f);
        }
    }

    [TestMethod]
    public void FixedPercent_UniformPercent_AppliesBothDimensions()
    {
        RecordingLayoutHandler handler = new();
        ILayoutHandler layout = Layout.FixedPercent(0.5f, handler);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(35, 35, 50, 30));
    }

    [TestMethod]
    public void FixedPercentLayout_InvalidAlignments_Throw()
    {
        RecordingLayoutHandler handler = new();

        Action invalidVertical = () => _ = new FixedPercentLayout(
            handler,
            heightPercent: 0.5f,
            widthPercent: 0.4f,
            (VerticalAlignment)int.MaxValue,
            HorizontalAlignment.Left);
        Action invalidHorizontal = () => _ = new FixedPercentLayout(
            handler,
            heightPercent: 0.5f,
            widthPercent: 0.4f,
            VerticalAlignment.Top,
            (HorizontalAlignment)int.MaxValue);

        invalidVertical.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("verticalAlignment");
        invalidHorizontal.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("horizontalAlignment");
    }

    [TestMethod]
    public void FixedSizeLayout_AllAlignments_UseNonzeroBoundsOrigin()
    {
        Rectangle bounds = new(10, 20, 100, 60);
        (VerticalAlignment Vertical, HorizontalAlignment Horizontal, Point Expected)[] cases =
        [
            (VerticalAlignment.Top, HorizontalAlignment.Left, new(10, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Center, new(40, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Right, new(70, 20)),
            (VerticalAlignment.Center, HorizontalAlignment.Left, new(10, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Center, new(40, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Right, new(70, 35)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Left, new(10, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Center, new(40, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Right, new(70, 50))
        ];

        foreach ((VerticalAlignment vertical, HorizontalAlignment horizontal, Point expected) in cases)
        {
            RecordingLayoutHandler handler = new();
            FixedSizeLayout layout = new(handler, new Size(40, 30), vertical, horizontal);

            layout.Layout(bounds, 1.0f);

            handler.LastBounds.Should().Be(new Rectangle(expected, new Size(40, 30)));
        }
    }

    [TestMethod]
    public void FixedSizeLayout_FractionalScale_RoundsDimensions()
    {
        RecordingLayoutHandler handler = new();
        FixedSizeLayout layout = new(handler, new Size(3, 5));

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.5f);

        handler.LastBounds.Should().Be(new Rectangle(58, 46, 4, 8));
        handler.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void FixedSize_ForwardsSizeAndAlignments()
    {
        RecordingLayoutHandler handler = new();
        ILayoutHandler layout = Layout.FixedSize(
            new Size(40, 30),
            handler,
            VerticalAlignment.Bottom,
            HorizontalAlignment.Right);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(70, 50, 40, 30));
    }

    [TestMethod]
    public void FixedSizeLayout_InvalidAlignments_Throw()
    {
        RecordingLayoutHandler handler = new();

        Action invalidVertical = () => _ = new FixedSizeLayout(
            handler,
            new Size(40, 30),
            (VerticalAlignment)int.MaxValue,
            HorizontalAlignment.Left);
        Action invalidHorizontal = () => _ = new FixedSizeLayout(
            handler,
            new Size(40, 30),
            VerticalAlignment.Top,
            (HorizontalAlignment)int.MaxValue);

        invalidVertical.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("verticalAlignment");
        invalidHorizontal.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("horizontalAlignment");
    }

    [TestMethod]
    public void FixedPercentLayout_InvalidPercentages_Throw()
    {
        RecordingLayoutHandler handler = new();

        Action negative = () => _ = new FixedPercentLayout(handler, -0.1f, 1.0f);
        Action notFinite = () => _ = new FixedPercentLayout(handler, 1.0f, float.PositiveInfinity);

        negative.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("heightPercent");
        notFinite.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("widthPercent");
    }

    [TestMethod]
    public void FixedPercentLayout_OverOnePercentage_AllowsOversizing()
    {
        RecordingLayoutHandler handler = new();
        FixedPercentLayout layout = new(handler, heightPercent: 1.5f, widthPercent: 2.0f);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(-40, 5, 200, 90));
    }

    [TestMethod]
    public void FixedSizeLayout_NegativeDimension_Throws()
    {
        RecordingLayoutHandler handler = new();

        Action create = () => _ = new FixedSizeLayout(handler, new Size(-1, 10));

        create.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("size");
    }

    [TestMethod]
    public void LayoutHandlers_NullHandler_Throw()
    {
        FluentActions.Invoking(() => _ = new FillLayout(null!)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => _ = new FixedPercentLayout(null!, 1.0f, 1.0f))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => _ = new FixedSizeLayout(null!, new Size(10, 10)))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => _ = new PaddedLayout(0, null!)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => _ = new ReplaceableLayout(null!)).Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ScaledLayouts_NonpositiveOrNonfiniteScale_Throw()
    {
        RecordingLayoutHandler handler = new();
        FixedSizeLayout fixedSize = new(handler, new Size(10, 10));
        PaddedLayout padded = new(10, handler);

        FluentActions.Invoking(() => fixedSize.Layout(Rectangle.Empty, 0))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => fixedSize.Layout(Rectangle.Empty, -1))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => padded.Layout(Rectangle.Empty, float.NaN))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => padded.Layout(Rectangle.Empty, float.PositiveInfinity))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void LayoutDimensions_UnitFactor_PreservesMaximumValue()
    {
        RecordingLayoutHandler fixedPercentHandler = new();
        RecordingLayoutHandler fixedSizeHandler = new();
        FixedPercentLayout fixedPercent = new(
            fixedPercentHandler,
            heightPercent: 1.0f,
            widthPercent: 1.0f,
            VerticalAlignment.Top,
            HorizontalAlignment.Left);
        FixedSizeLayout fixedSize = new(
            fixedSizeHandler,
            new Size(int.MaxValue, int.MaxValue),
            VerticalAlignment.Top,
            HorizontalAlignment.Left);
        Rectangle maximumBounds = new(0, 0, int.MaxValue, int.MaxValue);

        fixedPercent.Layout(maximumBounds, 1.0f);
        fixedSize.Layout(maximumBounds, 1.0f);

        fixedPercentHandler.LastBounds.Should().Be(maximumBounds);
        fixedSizeHandler.LastBounds.Should().Be(maximumBounds);
    }

    [TestMethod]
    public void SplitLayouts_UnitPercentage_PreservesMaximumExtent()
    {
        RecordingLayoutHandler firstRow = new();
        RecordingLayoutHandler lastRow = new();
        RecordingLayoutHandler firstColumn = new();
        RecordingLayoutHandler lastColumn = new();
        RowsLayout rows = new((1.0f, firstRow), (0.0f, lastRow));
        ColumnsLayout columns = new((1.0f, firstColumn), (0.0f, lastColumn));

        rows.Layout(new Rectangle(0, 0, 1, int.MaxValue), 1.0f);
        columns.Layout(new Rectangle(0, 0, int.MaxValue, 1), 1.0f);

        firstRow.LastBounds.Height.Should().Be(int.MaxValue);
        lastRow.LastBounds.Height.Should().Be(0);
        firstColumn.LastBounds.Width.Should().Be(int.MaxValue);
        lastColumn.LastBounds.Width.Should().Be(0);
    }

    [TestMethod]
    public void ScaledGeometry_ResultOutsideIntegerRange_Throws()
    {
        RecordingLayoutHandler handler = new();
        FixedPercentLayout fixedPercent = new(handler, heightPercent: 1.0f, widthPercent: 2.0f);
        FixedSizeLayout fixedSize = new(handler, new Size(int.MaxValue, 1));
        PaddedLayout padded = new((int.MaxValue, 0, 0, 0), handler);

        FluentActions.Invoking(() => fixedPercent.Layout(new Rectangle(0, 0, int.MaxValue, 1), 1.0f))
            .Should().Throw<OverflowException>();
        FluentActions.Invoking(() => fixedSize.Layout(Rectangle.Empty, 2.0f))
            .Should().Throw<OverflowException>();
        FluentActions.Invoking(() => padded.Layout(new Rectangle(0, 0, 10, 10), 2.0f))
            .Should().Throw<OverflowException>();
    }

    [TestMethod]
    public void RowsLayout_ThreeChildren_AssignsRoundingRemainderToLast()
    {
        RecordingLayoutHandler first = new();
        RecordingLayoutHandler second = new();
        RecordingLayoutHandler third = new();
        RowsLayout layout = new((0.333f, first), (0.333f, second), (0.334f, third));

        layout.Layout(new Rectangle(10, 20, 101, 101), 1.5f);

        first.LastBounds.Should().Be(new Rectangle(10, 20, 101, 33));
        second.LastBounds.Should().Be(new Rectangle(10, 53, 101, 33));
        third.LastBounds.Should().Be(new Rectangle(10, 86, 101, 35));
        first.LastScale.Should().Be(1.5f);
        second.LastScale.Should().Be(1.5f);
        third.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void ColumnsLayout_ThreeChildren_AssignsRoundingRemainderToLast()
    {
        RecordingLayoutHandler first = new();
        RecordingLayoutHandler second = new();
        RecordingLayoutHandler third = new();
        ColumnsLayout layout = new((0.333f, first), (0.333f, second), (0.334f, third));

        layout.Layout(new Rectangle(10, 20, 101, 101), 1.5f);

        first.LastBounds.Should().Be(new Rectangle(10, 20, 33, 101));
        second.LastBounds.Should().Be(new Rectangle(43, 20, 33, 101));
        third.LastBounds.Should().Be(new Rectangle(76, 20, 35, 101));
        first.LastScale.Should().Be(1.5f);
        second.LastScale.Should().Be(1.5f);
        third.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void RowsAndColumnsLayout_SingleChild_ReceivesAllBounds()
    {
        Rectangle bounds = new(10, 20, 101, 61);
        RecordingLayoutHandler rowHandler = new();
        RecordingLayoutHandler columnHandler = new();

        new RowsLayout((1.0f, rowHandler)).Layout(bounds, 1.25f);
        new ColumnsLayout((1.0f, columnHandler)).Layout(bounds, 1.25f);

        rowHandler.LastBounds.Should().Be(bounds);
        columnHandler.LastBounds.Should().Be(bounds);
        rowHandler.LastScale.Should().Be(1.25f);
        columnHandler.LastScale.Should().Be(1.25f);
    }

    [TestMethod]
    public void RowsAndColumnsLayout_CommonDecimalPercentages_AreAccepted()
    {
        RecordingLayoutHandler handler = new();
        (float Percent, ILayoutHandler Handler)[] handlers =
        [
            (0.07f, handler),
            (0.42f, handler),
            (0.32f, handler),
            (0.12f, handler),
            (0.07f, handler)
        ];

        Action createRows = () => _ = new RowsLayout(handlers);
        Action createColumns = () => _ = new ColumnsLayout(handlers);

        createRows.Should().NotThrow();
        createColumns.Should().NotThrow();
    }

    [TestMethod]
    public void RowsAndColumnsLayout_NegativeOrOversizedIndividualPercentage_Throws()
    {
        RecordingLayoutHandler handler = new();

        Action createRows = () => _ = new RowsLayout((-0.5f, handler), (1.5f, handler));
        Action createColumns = () => _ = new ColumnsLayout((1.5f, handler), (-0.5f, handler));

        createRows.Should().Throw<ArgumentOutOfRangeException>();
        createColumns.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void RowsAndColumnsLayout_NullHandler_Throws()
    {
        Action createRows = () => _ = new RowsLayout((1.0f, null!));
        Action createColumns = () => _ = new ColumnsLayout((1.0f, null!));

        createRows.Should().Throw<ArgumentNullException>();
        createColumns.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void RowsAndColumnsLayout_NonfinitePercentage_Throws()
    {
        RecordingLayoutHandler handler = new();

        Action createRows = () => _ = new RowsLayout((float.NaN, handler), (1.0f, handler));
        Action createColumns = () => _ = new ColumnsLayout((float.PositiveInfinity, handler), (0.0f, handler));

        createRows.Should().Throw<ArgumentOutOfRangeException>();
        createColumns.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void RowsAndColumnsLayout_CopyHandlerDefinitions()
    {
        RecordingLayoutHandler original = new();
        RecordingLayoutHandler replacement = new();
        (float Percent, ILayoutHandler Handler)[] rowDefinitions = [(1.0f, original)];
        (float Percent, ILayoutHandler Handler)[] columnDefinitions = [(1.0f, original)];
        RowsLayout rows = new(rowDefinitions);
        ColumnsLayout columns = new(columnDefinitions);
        rowDefinitions[0] = (0.0f, replacement);
        columnDefinitions[0] = (0.0f, replacement);
        Rectangle bounds = new(10, 20, 100, 60);

        rows.Layout(bounds, 1.0f);
        columns.Layout(bounds, 1.0f);

        original.CallCount.Should().Be(2);
        replacement.CallCount.Should().Be(0);
    }

    [TestMethod]
    public void RowsAndColumns_CreateExpectedLayouts()
    {
        RecordingLayoutHandler handler = new();

        Layout.Rows((1.0f, handler)).Should().BeOfType<RowsLayout>();
        Layout.Columns((1.0f, handler)).Should().BeOfType<ColumnsLayout>();
    }

    [TestMethod]
    public void PaddedLayout_AsymmetricPadding_UsesNonzeroOriginAndForwardsScale()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((3, 5, 7, 11), handler);

        layout.Layout(new Rectangle(10, 20, 100, 80), 2.0f);

        handler.LastBounds.Should().Be(new Rectangle(16, 30, 80, 48));
        handler.LastScale.Should().Be(2.0f);
    }

    [TestMethod]
    public void PaddedLayout_NegativeMargins_ExpandBounds()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((-10, -20, -30, -40), handler);

        layout.Layout(new Rectangle(10, 20, 100, 80), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(0, 0, 140, 140));
        handler.LastScale.Should().Be(1.0f);
    }

    [TestMethod]
    public void PaddedLayout_MaximumPadding_DoesNotOverflow()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((int.MaxValue, 0, int.MaxValue, 0), handler);

        layout.Layout(new Rectangle(10, 20, 10, 100), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(14, 20, 2, 100));
    }

    [TestMethod]
    public void PaddedLayout_TightAsymmetricPadding_ScalesTrailingEdges()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((1, 1, 10, 10), handler);

        layout.Layout(new Rectangle(10, 20, 5, 5), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(10, 20, 0, 0));
    }

    [TestMethod]
    public void PaddedLayout_MinimumMarginsStillDoNotFit_PreservesAxis()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((10, 0, 10, 0), handler);
        Rectangle bounds = new(10, 20, 1, 5);

        layout.Layout(bounds, 1.0f);

        handler.LastBounds.Should().Be(bounds);
    }

    [TestMethod]
    public void Padding_ImplicitConversions_SetAllFields()
    {
        Padding uniform = 7;
        Padding asymmetric = (1, 2, 3, 4);

        (uniform.Left, uniform.Top, uniform.Right, uniform.Bottom).Should().Be((7, 7, 7, 7));
        (asymmetric.Left, asymmetric.Top, asymmetric.Right, asymmetric.Bottom).Should().Be((1, 2, 3, 4));
    }

    [TestMethod]
    public void PaddingF_ImplicitConversions_SetAllFields()
    {
        PaddingF uniform = 1.5f;
        PaddingF asymmetric = (1.0f, 2.0f, 3.0f, 4.0f);

        (uniform.Left, uniform.Top, uniform.Right, uniform.Bottom).Should().Be((1.5f, 1.5f, 1.5f, 1.5f));
        (asymmetric.Left, asymmetric.Top, asymmetric.Right, asymmetric.Bottom).Should().Be((1.0f, 2.0f, 3.0f, 4.0f));
    }

    [TestMethod]
    public void FillAndMargin_ForwardBoundsAndScale()
    {
        Rectangle bounds = new(10, 20, 100, 80);
        RecordingLayoutHandler fillHandler = new();
        RecordingLayoutHandler marginHandler = new();

        Layout.Fill(fillHandler).Layout(bounds, 1.5f);
        Layout.Margin((1, 2, 3, 4), marginHandler).Layout(bounds, 1.5f);

        fillHandler.LastBounds.Should().Be(bounds);
        fillHandler.LastScale.Should().Be(1.5f);
        marginHandler.LastBounds.Should().Be(new Rectangle(12, 23, 94, 71));
        marginHandler.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void Empty_ReturnsSingletonThatAcceptsLayout()
    {
        Layout.Empty.Should().BeSameAs(EmptyLayout.Instance);

        Action layout = () => Layout.Empty.Layout(new Rectangle(10, 20, 100, 80), 1.5f);

        layout.Should().NotThrow();
    }

    [TestMethod]
    public void ReplaceableLayout_SetBeforeFirstLayout_WaitsForLayout()
    {
        RecordingLayoutHandler initial = new();
        RecordingLayoutHandler replacement = new();
        ReplaceableLayout layout = new(initial);
        Rectangle bounds = new(10, 20, 100, 80);

        layout.Handler = replacement;

        replacement.CallCount.Should().Be(0);
        initial.CallCount.Should().Be(0);

        layout.Layout(bounds, 1.5f);

        replacement.LastBounds.Should().Be(bounds);
        replacement.LastScale.Should().Be(1.5f);
        replacement.CallCount.Should().Be(1);
        initial.CallCount.Should().Be(0);
    }

    [TestMethod]
    public void ReplaceableLayout_ForwardsLatestBoundsAndScaleToReplacement()
    {
        RecordingLayoutHandler initial = new();
        RecordingLayoutHandler replacement = new();
        ReplaceableLayout layout = new(initial);
        Rectangle bounds = new(10, 20, 100, 80);

        layout.Layout(bounds, 1.75f);
        layout.Handler = replacement;

        initial.LastBounds.Should().Be(bounds);
        initial.LastScale.Should().Be(1.75f);
        replacement.LastBounds.Should().Be(bounds);
        replacement.LastScale.Should().Be(1.75f);
    }

    [STATestMethod]
    public void LayoutWindow_UnchangedChildBounds_DoesNotMoveWindow()
    {
        using Window parent = new(
            new Rectangle(100, 100, 400, 300),
            style: WindowStyles.Overlapped | WindowStyles.Caption);
        using Window child = new(
            new Rectangle(30, 40, 120, 80),
            style: WindowStyles.Child | WindowStyles.Border,
            parentWindow: parent);
        Rectangle currentBounds = child.GetWindowRectangle();
        parent.ScreenToClient(ref currentBounds).Should().BeTrue();
        currentBounds.Should().NotBe(child.GetClientRectangle());
        int notificationCount = 0;
        child.MessageHandler += CountPositionChanges;

        ((ILayoutHandler)child).Layout(currentBounds, child.GetScale());

        notificationCount.Should().Be(0);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutWindow_ChangedChildBounds_MovesWindow()
    {
        using Window parent = new(
            new Rectangle(100, 100, 400, 300),
            style: WindowStyles.Overlapped | WindowStyles.Caption);
        using Window child = new(
            new Rectangle(30, 40, 120, 80),
            style: WindowStyles.Child | WindowStyles.Border,
            parentWindow: parent);
        Rectangle requestedBounds = new(50, 60, 140, 90);
        int notificationCount = 0;
        child.MessageHandler += CountPositionChanges;

        ((ILayoutHandler)child).Layout(requestedBounds, child.GetScale());

        Rectangle currentBounds = child.GetWindowRectangle();
        parent.ScreenToClient(ref currentBounds).Should().BeTrue();
        currentBounds.Should().Be(requestedBounds);
        notificationCount.Should().BeGreaterThan(0);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutWindow_UnchangedTopLevelBounds_DoesNotMoveWindow()
    {
        using Window window = new(new Rectangle(100, 100, 200, 100));
        Rectangle currentBounds = window.GetWindowRectangle();
        int notificationCount = 0;
        window.MessageHandler += CountPositionChanges;

        ((ILayoutHandler)window).Layout(currentBounds, window.GetScale());

        notificationCount.Should().Be(0);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutWindow_UnchangedOwnedTopLevelBounds_DoesNotMapToOwner()
    {
        using Window owner = new(new Rectangle(100, 100, 400, 300));
        using Window owned = new(
            new Rectangle(150, 160, 200, 100),
            style: WindowStyles.PopUp,
            parentWindow: owner);
        owned.GetParent().Should().Be(owner.Handle);
        owned.IsChildWindow().Should().BeFalse();
        Rectangle currentBounds = owned.GetWindowRectangle();
        int notificationCount = 0;
        owned.MessageHandler += CountPositionChanges;

        ((ILayoutHandler)owned).Layout(currentBounds, owned.GetScale());

        notificationCount.Should().Be(0);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutBinder_WindowPositionChanged_LaysOutClientBounds()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);

        handler.CallCount.Should().Be(1);
        handler.LastBounds.Should().Be(window.GetClientRectangle());
        handler.LastScale.Should().Be(window.GetScale());

        window.MoveWindow(new Rectangle(20, 30, 300, 150), repaint: false);

        handler.CallCount.Should().BeGreaterThan(1);
        handler.LastBounds.Should().Be(window.GetClientRectangle());
        handler.LastScale.Should().Be(window.GetScale());
    }

    [STATestMethod]
    public void LayoutBinder_UnchangedNotification_DoesNotRepeatLayout()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);
        int notificationCount = 0;
        window.MessageHandler += CountPositionChanges;

        SendWindowPositionChanged(window);

        notificationCount.Should().BeGreaterThan(0);
        handler.CallCount.Should().Be(1);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutBinder_DpiChangedAfterParent_LaysOutUpdatedScale()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);
        uint currentDpi = window.GetDpi();
        uint previousDpi = currentDpi == 96 ? 120u : currentDpi - 24;
        dynamic binderAccessor = binder.TestAccessor.Dynamic;
        binderAccessor._lastScale = previousDpi / 96.0f;

        SendDpiChangedAfterParent(window, previousDpi, currentDpi);

        handler.CallCount.Should().Be(2);
        handler.LastBounds.Should().Be(window.GetClientRectangle());
        handler.LastScale.Should().Be(window.GetScale());
    }

    [STATestMethod]
    public void LayoutBinder_DpiChangedAfterParent_UnchangedScaleDoesNotRepeatLayout()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);
        uint currentDpi = window.GetDpi();
        uint previousDpi = currentDpi == 96 ? 120u : currentDpi - 24;

        SendDpiChangedAfterParent(window, previousDpi, currentDpi);

        handler.CallCount.Should().Be(1);
    }

    [STATestMethod]
    public unsafe void LayoutBinder_DpiChanged_LaysOutAfterSuggestedBoundsAreApplied()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);
        dynamic binderAccessor = binder.TestAccessor.Dynamic;
        binderAccessor._lastScale = window.GetScale() + 0.25f;
        ushort newDpi = checked((ushort)(window.GetDpi() + 24));
        nuint packedDpi = newDpi | ((nuint)newDpi << 16);
        Rectangle suggestedBounds = new(30, 40, 300, 200);
        RECT suggestedRectangle = suggestedBounds;

        _ = window.SendMessage(
            MessageType.DpiChanged,
            (WPARAM)packedDpi,
            (LPARAM)(nint)(&suggestedRectangle));

        handler.CallCount.Should().Be(2);
        handler.LastBounds.Should().Be(window.GetClientRectangle());
        handler.LastScale.Should().Be(window.GetScale());
    }

    [STATestMethod]
    public void LayoutBinder_Dispose_DetachesHandlerAndIsIdempotent()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        LayoutBinder binder = new(window, handler);
        Action layout = binder.TestAccessor.CreateDelegate<Action>("LayoutIfChanged");

        binder.Dispose();
        binder.Dispose();
        layout();
        window.MoveWindow(new Rectangle(20, 30, 300, 150), repaint: false);
        uint currentDpi = window.GetDpi();
        uint previousDpi = currentDpi == 96 ? 120u : currentDpi - 24;
        SendDpiChangedAfterParent(window, previousDpi, currentDpi);

        handler.CallCount.Should().Be(1);
    }

    [STATestMethod]
    public void LayoutBinder_MultipleBinders_DisposeIndependently()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler firstHandler = new();
        RecordingLayoutHandler secondHandler = new();
        LayoutBinder firstBinder = new(window, firstHandler);
        using LayoutBinder secondBinder = new(window, secondHandler);

        firstBinder.Dispose();
        window.MoveWindow(new Rectangle(20, 30, 300, 150), repaint: false);

        firstHandler.CallCount.Should().Be(1);
        secondHandler.CallCount.Should().BeGreaterThan(1);
    }

    [STATestMethod]
    public void LayoutBinder_NullArguments_Throw()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();

        FluentActions.Invoking(() => _ = new LayoutBinder(null!, handler))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => _ = new LayoutBinder(window, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [STATestMethod]
    public void LayoutBinder_InitialLayoutThrows_DetachesHandler()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        ThrowingLayoutHandler handler = new();
        int notificationCount = 0;
        window.MessageHandler += CountPositionChanges;

        FluentActions.Invoking(() => _ = new LayoutBinder(window, handler))
            .Should().Throw<InvalidOperationException>();

        SendWindowPositionChanged(window);
        uint currentDpi = window.GetDpi();
        uint previousDpi = currentDpi == 96 ? 120u : currentDpi - 24;
        SendDpiChangedAfterParent(window, previousDpi, currentDpi);

        notificationCount.Should().BeGreaterThan(0);
        handler.CallCount.Should().Be(1);

        LRESULT? CountPositionChanges(object sender, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.WindowPositionChanged)
            {
                notificationCount++;
            }

            return null;
        }
    }

    [STATestMethod]
    public void LayoutBinder_ReentrantLayoutSucceedsThenOuterThrows_PreservesNestedState()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        CallbackLayoutHandler handler = new();
        using LayoutBinder binder = new(window, handler);
        dynamic accessor = binder.TestAccessor.Dynamic;
        accessor._lastBounds = Rectangle.Empty;
        bool reentered = false;
        handler.Callback = () =>
        {
            if (reentered)
            {
                return;
            }

            reentered = true;
            window.MoveWindow(new Rectangle(20, 30, 300, 150), repaint: false);
            throw new InvalidOperationException("Expected");
        };

        Action layout = binder.TestAccessor.CreateDelegate<Action>("LayoutIfChanged");
        FluentActions.Invoking(layout).Should().Throw<InvalidOperationException>();
        int successfulNestedCallCount = handler.CallCount;

        SendWindowPositionChanged(window);

        handler.CallCount.Should().Be(successfulNestedCallCount);
    }

    private sealed class RecordingLayoutHandler : ILayoutHandler
    {
        public int CallCount { get; private set; }
        public Rectangle LastBounds { get; private set; }
        public float LastScale { get; private set; }

        public void Layout(Rectangle bounds, float scale)
        {
            CallCount++;
            LastBounds = bounds;
            LastScale = scale;
        }
    }

    private static unsafe void SendWindowPositionChanged(Window window)
    {
        Rectangle bounds = window.GetWindowRectangle();
        WINDOWPOS position = new()
        {
            hwnd = window.Handle,
            x = bounds.X,
            y = bounds.Y,
            cx = bounds.Width,
            cy = bounds.Height
        };

        _ = window.SendMessage(MessageType.WindowPositionChanged, lParam: (LPARAM)(nint)(&position));
    }

    private static void SendDpiChangedAfterParent(Window window, uint previousDpi, uint currentDpi)
    {
        dynamic accessor = window.TestAccessor.Dynamic;
        accessor._lastDpi = previousDpi;
        _ = window.SendMessage(MessageType.DpiChangedBeforeParent);
        accessor._lastDpi = currentDpi;
        _ = window.SendMessage(MessageType.DpiChangedAfterParent);
    }

    private sealed class ThrowingLayoutHandler : ILayoutHandler
    {
        public int CallCount { get; private set; }

        public void Layout(Rectangle bounds, float scale)
        {
            CallCount++;
            throw new InvalidOperationException("Expected");
        }
    }

    private sealed class CallbackLayoutHandler : ILayoutHandler
    {
        public int CallCount { get; private set; }
        public Action? Callback { get; set; }

        public void Layout(Rectangle bounds, float scale)
        {
            CallCount++;
            Callback?.Invoke();
        }
    }
}
