using System.Windows;
using Emu.Controls;
using Peacock;

namespace Emu.Behaviors;

public record DragState(bool IsDragging, Point ControlStart, Point MouseDragStart)
{
    public DragState() : this(false, new(), new()) { }
}

public record DraggingBehavior(object? ControlId) 
    : Behavior<DragState>(ControlId)
{
    public IUpdates CancelDrag(IUpdates updates)
        => UpdateState(updates, x => x with { IsDragging = false });
    
    public IUpdates StartDrag(IUpdates updates, Point controlStart, Point dragStart)
        => UpdateState(updates, x => x with { IsDragging = true, ControlStart = controlStart, MouseDragStart = dragStart });

    public override IUpdates Process(IControl control, InputEvent input, IUpdates updates)
    {
        if (control is not NodeControl nodeControl)
            return base.Process(control, input, updates);

        if (State.IsDragging)
        {
            switch (input)
            {
                case MouseUpEvent:
                    return CancelDrag(updates);

                case MouseMoveEvent mme:
                {
                    if (!input.MouseStatus.LButtonDown)
                        return CancelDrag(updates);

                    var offset = mme.MouseStatus.Location.Subtract(State.MouseDragStart);
                    var newLocation = State.ControlStart.Add(offset);

                    return updates.UpdateModel(nodeControl.View.Node,
                        model => model with { Rect = model.Rect.MoveTo(newLocation) });
                }
            }
        }
        else
        {
            if (input is MouseDownEvent)
            {
                var location = input.MouseStatus.Location;

                if (nodeControl.Absolute.Contains(location))
                {
                    var socket = nodeControl.HitSocket(location);
                    if (socket == null)
                        return StartDrag(updates, nodeControl.View.Node.Rect.TopLeft, input.MouseStatus.Location);
                }
            }
        }

        return updates;
    }
}