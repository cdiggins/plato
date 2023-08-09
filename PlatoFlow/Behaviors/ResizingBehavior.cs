using System.Windows;
using Emu.Controls;
using Peacock;

namespace Emu.Behaviors;

public enum Corner
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public record ResizingState(bool IsResizing, Rect OriginalRect, Point MouseDragStart, Corner Corner)
{
    public ResizingState() : this(false, new(), new(), Corner.BottomRight) { }
}

public record ResizingBehavior(object? ControlId)
    : Behavior<ResizingState>(ControlId)
{
    public IUpdates CancelResize(IUpdates updates)
        => UpdateState(updates, x => x with { IsResizing = false });

    public IUpdates StartResize(IUpdates updates, Rect controlRect, Point dragStart, Corner corner)
        => UpdateState(updates, x => x with { IsResizing = true, OriginalRect = controlRect, MouseDragStart = dragStart, Corner = corner });

    public static Rect FromPoints(double left, double top, double right, double bottom)
        => new(new Point(left, top), new Point(right, bottom));

    public static Rect AddOffsetToCorner(Rect rect, Point offset, Corner corner)
        => corner switch
        {
            Corner.TopLeft  => FromPoints(rect.Left + offset.X, rect.Top + offset.Y, rect.Right, rect.Bottom),
            Corner.TopRight => FromPoints(rect.Left, rect.Top + offset.Y, rect.Right + offset.X, rect.Bottom),
            Corner.BottomRight => FromPoints(rect.Left, rect.Top, rect.Right + offset.X, rect.Bottom + offset.Y),
            Corner.BottomLeft => FromPoints(rect.Left + offset.X, rect.Top, rect.Right, rect.Bottom + offset.Y),
            _ => rect
        };

    public Point ComputeOffset(InputEvent input)
        => input.MouseStatus.Location.Subtract(State.MouseDragStart);

    public Node UpdateNodeRect(Node node, Point offset)
        => node with { Rect = AddOffsetToCorner(State.OriginalRect, offset, State.Corner) };

    public IUpdates UpdateResize(IUpdates updates, NodeControl nodeControl, InputEvent input)
        => updates.UpdateModel(nodeControl.View.Node, node => UpdateNodeRect(node, ComputeOffset(input)));

    public bool HitTest(Point target, Point point)
        => target.Distance(point) <= 5;

    public Corner WhichCornerHit(Rect rect, Point location)
        => HitTest(rect.TopLeft, location) ? Corner.TopLeft
            : HitTest(rect.TopRight, location) ? Corner.TopRight
            : HitTest(rect.BottomRight, location) ? Corner.BottomRight
            : HitTest(rect.BottomLeft, location) ? Corner.BottomLeft
            : Corner.None;

    public override IUpdates Process(IControl control, InputEvent input, IUpdates updates)
    {
        if (control is not NodeControl nodeControl)
            return base.Process(control, input, updates);

        if (State.IsResizing)
        {
            switch (input)
            {
                case MouseUpEvent:
                    return CancelResize(updates);

                case MouseMoveEvent mme:
                    return !input.MouseStatus.LButtonDown 
                        ? CancelResize(updates) 
                        : UpdateResize(updates, nodeControl, input);
            }
        }
        else
        {
            if (input is MouseDownEvent)
            {
                var location = input.MouseStatus.Location;

                var corner = WhichCornerHit(nodeControl.Absolute, location);
                if (corner == Corner.None)
                    return UpdateState(updates, state => state with { IsResizing = false });

                return StartResize(updates, nodeControl.View.Node.Rect, location, corner);
            }
        }

        return updates;
    }
}
