# Using the Layout Engine

Thirtytwo lays out child windows by composing small `ILayoutHandler` objects
into a tree. Attach the root handler to a window with `AddLayoutHandler`; the
window then supplies its client rectangle and current DPI scale whenever layout
is needed.

## Bind a layout

The examples use the layout types and drawing primitives from these namespaces:

```csharp
using System.Drawing;
using Windows;
```

Create child controls before binding the layout. For example, after creating
`_navigation` and `_content` as children of the current window, arrange them as
two columns:

```csharp
this.AddLayoutHandler(Layout.Vertical(
    (.30f, Layout.Margin((8, 8, 4, 8), _navigation)),
    (.70f, Layout.Margin((4, 8, 8, 8), _content))));
```

`AddLayoutHandler` performs an initial layout immediately. It then runs the
layout again when the window reports changed client bounds or DPI scale.

The returned `LayoutBinder` is retained by the window for the window's
lifetime, so ordinary window-owned layouts do not need a field. Keep the binder
only when the layout must be detached early, then call `Dispose` on it.

## Compose handlers

A layout tree has three kinds of nodes:

- A `Window`, such as a control, is a leaf. It moves to the rectangle it
  receives.
- Split handlers divide a rectangle among several child handlers.
- Transform handlers resize, align, or inset a rectangle before passing it to
  one child.

The `Layout` factory provides the common nodes:

| Factory | Behavior |
| --- | --- |
| `Layout.Vertical(...)` | Places children in side-by-side columns by splitting width. |
| `Layout.Horizontal(...)` | Stacks children in rows by splitting height. |
| `Layout.Margin(margin, handler)` | Insets the bounds by DPI-scaled logical margins. |
| `Layout.FixedSize(size, handler, ...)` | Uses a DPI-scaled logical size and aligns it. |
| `Layout.FixedPercent(percent, handler, ...)` | Uses and aligns a fraction of the available bounds. |
| `Layout.Fill(handler)` | Passes all available bounds to its child. |
| `Layout.Empty` | Does nothing, which can leave a split segment empty. |

The `Horizontal` and `Vertical` names describe the bands they create. This
means `Vertical` creates vertical columns and `Horizontal` creates horizontal
rows.

## Split rows and columns

Each child of `Horizontal` or `Vertical` is a tuple containing a fractional
share and a handler. Every share must be between `0.0f` and `1.0f`, and the
shares must total `1.0f`.

This layout creates a main row and a shorter status row. The main row contains
a navigation column and a content column:

```csharp
ILayoutHandler layout = Layout.Horizontal(
    (.90f, Layout.Vertical(
        (.25f, Layout.Margin((8, 8, 4, 4), _navigation)),
        (.75f, Layout.Margin((4, 8, 8, 4), _content)))),
    (.10f, Layout.Margin((8, 4, 8, 8), _status)));

this.AddLayoutHandler(layout);
```

Integer division can leave a few pixels after proportional sizing. The final
child receives the remainder, so the split always covers the complete input
rectangle.

Use `Layout.Empty` when part of a split should remain unused:

```csharp
Layout.Vertical(
    (.20f, Layout.Empty),
    (.80f, _content));
```

## Apply margins, sizes, and alignment

Margins and fixed sizes are logical units. The engine multiplies them by the
window's scale before calculating physical-pixel bounds. Proportional splits
and fixed percentages operate on the available physical bounds directly.

`FixedSize` and `FixedPercent` center their child by default. Pass vertical and
horizontal alignment values to place it elsewhere:

```csharp
ILayoutHandler bottomRightButton = Layout.FixedSize(
    new Size(120, 32),
    _button,
    VerticalAlignment.Bottom,
    HorizontalAlignment.Right);
```

The two-argument percentage overload applies the same factor to width and
height:

```csharp
ILayoutHandler centeredPreview = Layout.FixedPercent(.75f, _preview);
```

Use the width and height overload when the factors differ:

```csharp
ILayoutHandler banner = Layout.FixedPercent(
    widthPercent: 1.0f,
    heightPercent: .25f,
    handler: _banner,
    verticalAlignment: VerticalAlignment.Top);
```

Fixed percentages must be finite and nonnegative. Values greater than `1.0f`
are allowed and deliberately produce bounds larger than the available space.
Margins may also be negative to expand bounds. When positive margins cannot fit
in the available space, the built-in margin handler reduces them until they
fit.

## Replace content at runtime

Use `ReplaceableLayout` when one region keeps the same layout but displays a
different child:

```csharp
_contentLayout = new ReplaceableLayout(_summary);

this.AddLayoutHandler(Layout.Margin((8, 8, 8, 8), _contentLayout));
```

Later, replace the child by assigning `Handler`:

```csharp
_contentLayout.Handler = _details;
```

The assignment synchronously applies the most recently received bounds and
scale to the new handler. Change the controls' visibility separately when both
old and new child windows already exist. Before the replaceable handler has
participated in a layout pass, its stored values are `Rectangle.Empty` and
`1.0f`; bind it before replacing its child when empty initial bounds are not
desired.

## Write a custom handler

Implement `ILayoutHandler` when the factory nodes cannot express a layout. A
handler receives physical-pixel bounds in the current layout coordinate space
and a finite, positive scale measured in physical pixels per logical unit.
Layout is synchronous.

Custom handlers can transform the rectangle and delegate to another handler:

```csharp
private sealed class TopHalfLayout(ILayoutHandler child) : ILayoutHandler
{
    public void Layout(Rectangle bounds, float scale)
    {
        Rectangle topHalf = new(
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height / 2);

        child.Layout(topHalf, scale);
    }
}
```

Use checked arithmetic when custom calculations can exceed integer bounds.
Avoid retaining transient rectangles unless the handler is intentionally
stateful, as `ReplaceableLayout` is.

## Current scope

The engine performs one immediate layout pass from an assigned rectangle. It
does not measure preferred control sizes, calculate intrinsic sizes from text,
or provide grid and wrapping algorithms. Compose explicit sizes and proportions
when controls need those behaviors.

See the complete
[layout sample](../src/samples/Layout/Program.LayoutWindow.cs) for a working
composition with native controls and replaceable content. Maintainers can also
consult the [layout engine evaluation](layout-engine-evaluation.md) for the
architecture assessment and improvement plan.