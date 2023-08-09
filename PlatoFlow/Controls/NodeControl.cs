using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Emu.Behaviors;
using Peacock;

namespace Emu.Controls;

public record NodeStyle(ShapeStyle ShapeStyle, TextStyle TextStyle, Radius Radius);

public record NodeView(Node Node, NodeStyle Style) : View(Node, Node.Id);

public record NodeControl(Measures Measures, NodeView View, IReadOnlyList<SlotControl> Slots, Func<IUpdates, IControl, IControl, IUpdates> Callback) 
    : Control<NodeView>(Measures, View, Slots, Callback)
{
    public StyledRect StyledShape() 
        => new(View.Style.ShapeStyle with { PenStyle = new(Colors.White, 0.5) }, Shape());

    public Rect HeaderRect()
        => Client.SetHeight(ControlFactory.HeaderHeight(View.Node)).ShrinkFromCenter(new(4, 0));

    public StyledText StyledText()
        => new(View.Style.TextStyle, HeaderRect(), View.Node.Label);

    public RoundedRect Shape() 
        => new(Client, View.Style.Radius);

    public override ICanvas Draw(ICanvas canvas)
        //=> canvas.Draw(StyledShape()).Draw(StyledText());
        => canvas.Draw(StyledShape()).Draw(Header()).Draw(StyledText());

    public RoundedRect HeaderShape()
        => new(HeaderRect(), View.Style.Radius);

    public StyledRect Header() 
        => new(new ShapeStyle(Colors.Transparent, View.Style.ShapeStyle.PenStyle), HeaderShape());

    public override IEnumerable<IBehavior> GetDefaultBehaviors()
        => new IBehavior[] { new DraggingBehavior(this), new ResizingBehavior(this) };
}