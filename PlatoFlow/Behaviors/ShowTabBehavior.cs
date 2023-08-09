using Emu.Controls;
using Peacock;

namespace Emu.Behaviors;

public record ShowTabState(bool ShowTab)
{
    public ShowTabState() : this(false) { }
}

public record ShowTabBehavior(object? ControlId)
: Behavior<ShowTabState>(ControlId)
{
    public override IUpdates Process(IControl control, InputEvent input, IUpdates updates)
    {
        if (control is SlotControl slotControl)
        {
            if (input is MouseMoveEvent mouseMoveEvent)
            {
                var hitTest = slotControl.Absolute.Contains(mouseMoveEvent.MouseStatus.Location);
                return UpdateState(updates, state => state with { ShowTab = hitTest });
            }
        }
        return base.Process(control, input, updates);
    }

    public override ICanvas PreDraw(ICanvas canvas, IControl control)
    {
        if (control is SlotControl slotControl && State.ShowTab == true)
        {
            return slotControl.DrawTabs(canvas);
        }
        return canvas;
    }
}