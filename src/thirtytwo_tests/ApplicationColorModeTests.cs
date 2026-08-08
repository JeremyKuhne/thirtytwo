// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Controls.RichEdit;
using Windows.Win32.UI.ViewManagement;

namespace Windows;

[TestClass]
[DoNotParallelize]
public class ApplicationColorModeTests
{
    [TestCleanup]
    public void Cleanup()
    {
        Application.ColorMode = ApplicationColorMode.System;
        Application.UseUndocumentedDarkModeApis = true;
    }

    [TestMethod]
    public void ColorMode_Default_IsSystem()
    {
        Application.ColorMode.Should().Be(ApplicationColorMode.System);
    }

    [TestMethod]
    public void UseUndocumentedDarkModeApis_Default_IsEnabled()
    {
        Application.UseUndocumentedDarkModeApis.Should().BeTrue();
    }

    [STATestMethod]
    public void UseUndocumentedDarkModeApis_Change_UpdatesExistingWindow()
    {
        Application.UseUndocumentedDarkModeApis = true;
        using ColorModeWindow window = new();

        Application.UseUndocumentedDarkModeApis = false;

        window.ColorModeChangeCount.Should().Be(1);
    }

    [TestMethod]
    public void IsLight_DarkAndLightColors_ClassifiesPerDocumentedFormula()
    {
        SystemColorModeProvider.IsLight(new UISettingsColor { A = 255, R = 0, G = 0, B = 0 }).Should().BeFalse();
        SystemColorModeProvider.IsLight(new UISettingsColor { A = 255, R = 255, G = 255, B = 255 }).Should().BeTrue();
    }

    [TestMethod]
    public void TryGetIsDark_CurrentInteractiveSession_Succeeds()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            return;
        }

        SystemColorModeProvider.TryGetIsDark(out _).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void IUISettings3_Iid_IsExpected()
    {
        (*IID.Get<IUISettings3>()).Should().Be(new Guid("03021be4-5254-4781-8194-5168f7d06d7b"));
    }

    [TestMethod]
    public void ColorPalette_Dark_UsesWinUI23ThemeTokens()
    {
        ApplicationColorPalette palette = ApplicationColorPalette.Create(dark: true, highContrast: false);

        palette.WindowBackground.ToArgb().Should().Be(Color.FromArgb(32, 32, 32).ToArgb());
        palette.WindowForeground.ToArgb().Should().Be(Color.White.ToArgb());
        palette.ControlBackground.ToArgb().Should().Be(Color.FromArgb(45, 45, 45).ToArgb());
        palette.ControlForeground.ToArgb().Should().Be(Color.White.ToArgb());
        palette.DisabledForeground.ToArgb().Should().Be(Color.FromArgb(122, 122, 122).ToArgb());
        palette.Border.ToArgb().Should().Be(Color.FromArgb(48, 48, 48).ToArgb());
        palette.SelectionForeground.ToArgb().Should().Be(Color.White.ToArgb());
    }

    [TestMethod]
    public void ColorPalette_Light_UsesWinUI23ThemeTokens()
    {
        ApplicationColorPalette palette = ApplicationColorPalette.Create(dark: false, highContrast: false);

        palette.WindowBackground.ToArgb().Should().Be(Color.FromArgb(243, 243, 243).ToArgb());
        palette.WindowForeground.ToArgb().Should().Be(Color.FromArgb(26, 26, 26).ToArgb());
        palette.ControlBackground.ToArgb().Should().Be(Color.FromArgb(251, 251, 251).ToArgb());
        palette.ControlForeground.ToArgb().Should().Be(Color.FromArgb(27, 27, 27).ToArgb());
        palette.DisabledForeground.ToArgb().Should().Be(Color.FromArgb(160, 160, 160).ToArgb());
        palette.Border.ToArgb().Should().Be(Color.FromArgb(229, 229, 229).ToArgb());
        palette.SelectionForeground.ToArgb().Should().Be(Color.White.ToArgb());
    }

    [TestMethod]
    public void ColorPalette_EquivalentInstances_AreValueEqual()
    {
        ApplicationColorPalette first = ApplicationColorPalette.Create(dark: true, highContrast: false);
        ApplicationColorPalette second = ApplicationColorPalette.Create(dark: true, highContrast: false);

        first.Should().NotBeSameAs(second);
        first.Should().Be(second);
    }

    [TestMethod]
    public void ColorPalette_HighContrast_UsesDocumentedSystemColors()
    {
        ApplicationColorPalette palette = ApplicationColorPalette.Create(dark: true, highContrast: true);

        palette.WindowBackground.ToArgb().Should().Be(GetSystemColor(SystemColor.Window).ToArgb());
        palette.WindowForeground.ToArgb().Should().Be(GetSystemColor(SystemColor.WindowText).ToArgb());
        palette.ControlBackground.ToArgb().Should().Be(GetSystemColor(SystemColor.ButtonFace).ToArgb());
        palette.ControlForeground.ToArgb().Should().Be(GetSystemColor(SystemColor.ButtonText).ToArgb());
        palette.DisabledForeground.ToArgb().Should().Be(GetSystemColor(SystemColor.GrayText).ToArgb());
        palette.Border.ToArgb().Should().Be(GetSystemColor(SystemColor.WindowText).ToArgb());
        palette.SelectionBackground.ToArgb().Should().Be(GetSystemColor(SystemColor.Highlight).ToArgb());
        palette.SelectionForeground.ToArgb().Should().Be(GetSystemColor(SystemColor.HightlightText).ToArgb());

        static Color GetSystemColor(SystemColor color)
            => new COLORREF(PInvoke.GetSysColor((SYS_COLOR_INDEX)color));
    }

    [TestMethod]
    public void ColorPalette_SelectionBackground_UsesUISettingsAccent()
    {
        if (!SystemColorModeProvider.TryGetColor(UISettingsColorType.Accent, out UISettingsColor accent))
        {
            return;
        }

        ApplicationColorPalette palette = ApplicationColorPalette.Create(dark: true, highContrast: false);

        palette.SelectionBackground.Should().Be(Color.FromArgb(accent.A, accent.R, accent.G, accent.B));
    }

    [TestMethod]
    public void ColorMode_InvalidValue_ThrowsWithoutChangingMode()
    {
        Application.ColorMode = ApplicationColorMode.Dark;

        Action action = () => Application.ColorMode = (ApplicationColorMode)int.MaxValue;

        action.Should().Throw<ArgumentOutOfRangeException>();
        Application.ColorMode.Should().Be(ApplicationColorMode.Dark);
    }

    [STATestMethod]
    public unsafe void ColorMode_Change_UpdatesExistingWindowAndInheritedControlColors()
    {
        Application.ColorMode = ApplicationColorMode.Light;
        using ColorModeWindow window = new();
        using StaticControl label = new(text: "Label", parentWindow: window);
        using DeviceContext context = label.GetDeviceContext();

        Application.ColorMode = ApplicationColorMode.Dark;
        _ = window.SendMessage(
            MessageType.ControlColorStatic,
            (WPARAM)(nuint)context.Handle.Value,
            (LPARAM)label.Handle);

        window.ColorModeChangeCount.Should().Be(1);
        context.GetBackgroundColor().Should().Be(Color.FromArgb(32, 32, 32));
        context.GetTextColor().ToArgb().Should().Be(Color.White.ToArgb());
    }

    [STATestMethod]
    public void ColorMode_SameValue_DoesNotNotifyExistingWindow()
    {
        Application.ColorMode = ApplicationColorMode.Dark;
        using ColorModeWindow window = new();

        Application.ColorMode = ApplicationColorMode.Dark;

        window.ColorModeChangeCount.Should().Be(0);
    }

    [TestMethod]
    public void CurrentColorState_RepeatedRead_ReturnsSameInstance()
    {
        Application.ColorMode = ApplicationColorMode.Dark;

        ApplicationColorState first = Application.CurrentColorState;
        ApplicationColorState second = Application.CurrentColorState;

        second.Should().BeSameAs(first);
        second.Palette.Should().BeSameAs(first.Palette);
    }

    [STATestMethod]
    public unsafe void ColorMode_WindowCreatedAfterChange_UsesSelectedPalette()
    {
        Application.ColorMode = ApplicationColorMode.Dark;
        using MainWindow window = new(Window.DefaultBounds);
        using StaticControl label = new(text: "Label", parentWindow: window);
        using DeviceContext context = label.GetDeviceContext();

        _ = window.SendMessage(
            MessageType.ControlColorStatic,
            (WPARAM)(nuint)context.Handle.Value,
            (LPARAM)label.Handle);

        context.GetBackgroundColor().Should().Be(Color.FromArgb(32, 32, 32));
        context.GetTextColor().ToArgb().Should().Be(Color.White.ToArgb());
    }

    [STATestMethod]
    public unsafe void ColorMode_RadioButton_UsesWindowSurfaceColors()
    {
        Application.ColorMode = ApplicationColorMode.Light;
        using MainWindow window = new(Window.DefaultBounds);
        using ButtonControl radioButton = new(
            text: "Radio button",
            buttonStyle: ButtonControl.Styles.AutoRadioButton,
            parentWindow: window);
        using DeviceContext context = radioButton.GetDeviceContext();

        _ = window.SendMessage(
            MessageType.ControlColorButton,
            (WPARAM)(nuint)context.Handle.Value,
            (LPARAM)radioButton.Handle);

        context.GetBackgroundColor().Should().Be(Application.CurrentColorState.Palette.WindowBackground);
        context.GetTextColor().ToArgb().Should().Be(
            Application.CurrentColorState.Palette.WindowForeground.ToArgb());
    }

    [STATestMethod]
    public unsafe void ColorMode_Edit_UsesControlSurfaceColors()
    {
        Application.ColorMode = ApplicationColorMode.Light;
        using MainWindow window = new(Window.DefaultBounds);
        using EditControl edit = new(text: "Edit", parentWindow: window);
        using DeviceContext context = edit.GetDeviceContext();

        _ = window.SendMessage(
            MessageType.ControlColorEdit,
            (WPARAM)(nuint)context.Handle.Value,
            (LPARAM)edit.Handle);

        context.GetBackgroundColor().Should().Be(Application.CurrentColorState.Palette.ControlBackground);
        context.GetTextColor().ToArgb().Should().Be(
            Application.CurrentColorState.Palette.ControlForeground.ToArgb());
    }

    [STATestMethod]
    public void ColorMode_Change_NotifiesRadioButton()
    {
        Application.ColorMode = ApplicationColorMode.Light;
        using MainWindow window = new(Window.DefaultBounds);
        using TrackingRadioButton radioButton = new(window);

        Application.ColorMode = ApplicationColorMode.Dark;

        radioButton.ColorModeChangeCount.Should().Be(1);
    }

    [STATestMethod]
    public void ColorMode_ExplicitBackground_RemainsUnchanged()
    {
        Color explicitColor = Color.Crimson;
        using MainWindow window = new(Window.DefaultBounds, backgroundColor: explicitColor);

        Application.ColorMode = ApplicationColorMode.Dark;
        Color actual = window.TestAccessor.Dynamic._backgroundColor;

        actual.Should().Be(explicitColor);
    }

    [STATestMethod]
    public void CurrentColorState_DerivedControl_ExposesResolvedRenderingContract()
    {
        Application.ColorMode = ApplicationColorMode.Dark;
        using MainWindow window = new(Window.DefaultBounds);
        using PublicColorControl control = new(window);

        ApplicationColorState state = Application.CurrentColorState;
        control.ApplyNativeTheme();

        state.RequestedMode.Should().Be(ApplicationColorMode.Dark);
        state.IsDark.Should().BeTrue();
        state.UseUndocumentedDarkModeApis.Should().Be(Application.UseUndocumentedDarkModeApis);
        state.UndocumentedDarkModeApisSupported.Should().Be(UndocumentedDarkMode.IsSupported);
        state.Generation.Should().BeGreaterThan(0);
        state.Palette.Should().Be(Application.CurrentColorState.Palette);
        control.EffectiveWindowBackground.ToArgb().Should().Be(state.Palette.WindowBackground.ToArgb());
        control.EffectiveControlBackground.ToArgb().Should().Be(state.Palette.ControlBackground.ToArgb());
        control.EffectiveWindowForeground.ToArgb().Should().Be(state.Palette.WindowForeground.ToArgb());
        control.EffectiveControlForeground.ToArgb().Should().Be(state.Palette.ControlForeground.ToArgb());

        Application.ColorMode = ApplicationColorMode.Light;

        control.ColorModeChangeCount.Should().Be(1);
        control.LastColorState.RequestedMode.Should().Be(ApplicationColorMode.Light);
        control.LastColorState.Generation.Should().NotBe(state.Generation);
    }

    [STATestMethod]
    public void GetEffectiveBackgroundColor_ExplicitParent_InheritsColor()
    {
        Color explicitColor = Color.Crimson;
        using MainWindow window = new(Window.DefaultBounds, backgroundColor: explicitColor);
        using PublicColorControl control = new(window);

        control.EffectiveWindowBackground.Should().Be(explicitColor);
        control.EffectiveControlBackground.Should().Be(explicitColor);
    }

    [STATestMethod]
    public unsafe void ColorMode_RichEdit_UpdatesBackgroundAndTextColors()
    {
        Application.ColorMode = ApplicationColorMode.Dark;
        using MainWindow window = new(Window.DefaultBounds);
        using RichEditControl richEdit = new(new(0, 0, 200, 100), text: "Rich text", parentWindow: window);

        ApplicationColorPalette palette = Application.CurrentColorState.Palette;
        COLORREF expectedBackground = (COLORREF)palette.ControlBackground;
        LRESULT previousBackground = richEdit.SendMessage(
            (MessageType)PInvoke.EM_SETBKGNDCOLOR,
            default,
            (LPARAM)(nint)expectedBackground.Value);

        ((uint)previousBackground.Value).Should().Be(expectedBackground.Value);
        GetCharacterFormat(richEdit, PInvoke.SCF_DEFAULT).Base.crTextColor
            .Should().Be((COLORREF)palette.ControlForeground);

        richEdit.SetSelection(0, -1);
        GetCharacterFormat(richEdit, PInvoke.SCF_SELECTION).Base.crTextColor
            .Should().Be((COLORREF)palette.ControlForeground);

        static CHARFORMAT2W GetCharacterFormat(RichEditControl richEdit, uint scope)
        {
            CHARFORMAT2W characterFormat = new();
            characterFormat.Base.cbSize = (uint)sizeof(CHARFORMAT2W);
            richEdit.SendMessage(
                (MessageType)PInvoke.EM_GETCHARFORMAT,
                (WPARAM)scope,
                (LPARAM)(nint)(&characterFormat));
            characterFormat.Base.dwMask.HasFlag(CFM_MASK.CFM_COLOR).Should().BeTrue();
            return characterFormat;
        }
    }

    private sealed class ColorModeWindow : MainWindow
    {
        internal ColorModeWindow()
            : base(Window.DefaultBounds)
        {
        }

        internal int ColorModeChangeCount { get; private set; }

        protected override void OnColorModeChanged()
        {
            ColorModeChangeCount++;
            base.OnColorModeChanged();
        }
    }

    private sealed class TrackingRadioButton : ButtonControl
    {
        internal TrackingRadioButton(Window parentWindow)
            : base(buttonStyle: Styles.AutoRadioButton, parentWindow: parentWindow)
        {
        }

        internal int ColorModeChangeCount { get; private set; }

        protected override void OnColorModeChanged()
        {
            ColorModeChangeCount++;
            base.OnColorModeChanged();
        }
    }

    private sealed class PublicColorControl : CustomControl
    {
        internal PublicColorControl(Window parentWindow)
            : base(parentWindow: parentWindow)
        {
            LastColorState = Application.CurrentColorState;
        }

        internal int ColorModeChangeCount { get; private set; }

        internal ApplicationColorState LastColorState { get; private set; }

        internal Color EffectiveWindowBackground => GetEffectiveBackgroundColor();

        internal Color EffectiveControlBackground => GetEffectiveBackgroundColor(controlSurface: true);

        internal Color EffectiveWindowForeground => GetEffectiveForegroundColor(controlSurface: false);

        internal Color EffectiveControlForeground => GetEffectiveForegroundColor();

        internal void ApplyNativeTheme()
        {
            ApplyApplicationDarkModeTheme("DarkMode_Explorer");
            ApplyApplicationDarkModeTheme(Handle, "DarkMode_Explorer");
        }

        protected override void OnColorModeChanged()
        {
            ColorModeChangeCount++;
            LastColorState = Application.CurrentColorState;
            ApplyApplicationDarkModeTheme("DarkMode_Explorer");
            base.OnColorModeChanged();
        }
    }
}
