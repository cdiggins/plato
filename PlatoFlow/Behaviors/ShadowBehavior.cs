using System.Windows;
using System.Windows.Media;
using Peacock;

namespace Emu.Behaviors;

public record ShadowState(Color Color, Point Offset)
{
    public ShadowState()
        : this(Color.FromArgb(0x99, 0x2A, 0x2A, 0x2A), new(5, 5))
    { }
}

public record ShadowBehavior(object? ControlId)
: Behavior<ShadowState>(ControlId)
{
    public override ICanvas PreDraw(ICanvas canvas, IControl control)
        => canvas; //canvas.Draw(new StyledRect(new(State.Color, Colors.Transparent), control.Measures.ClientRect.MoveBy(State.Offset)));
}