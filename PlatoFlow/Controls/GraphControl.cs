using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Emu.Behaviors;
using Peacock;

namespace Emu.Controls;

public record GraphStyle(ShapeStyle ShapeStyle, TextStyle TextStyle, double GridDistance, bool ShowGrid);

public record GraphView(Graph Graph, GraphStyle Style) : View(Graph, Graph.Id);

public record GraphControl(Measures Measures,
    GraphView View,
    IReadOnlyList<NodeControl> Nodes,
    Func<IUpdates, IControl, IControl, IUpdates> Callback)
    : Control<GraphView>(Measures, View, Nodes, Callback)
{
    public override IEnumerable<IBehavior> GetDefaultBehaviors()
        => new[] { new ConnectingBehavior(this) };

    public static Geometry ConnectorGeometry(Point a, Point b)
    {
        var xDelta = Math.Abs(a.X - b.X);
        var xDist = Math.Clamp(xDelta * 0.4, 100, double.MaxValue);
        var controlPointA = a.Add(new Point(xDist, 0));
        var controlPointB = b.Subtract(new Point(xDist, 0));
        var segment1 = new BezierSegment
        {
            Point1 = controlPointA,
            Point2 = controlPointB,
            Point3 = b
        };
        var pathFigure = new PathFigure(a, new[] { segment1 }, false);
        var pathGeometry = new PathGeometry(new[] { pathFigure });
        return pathGeometry;
    }

    // TODO: get the style from the style class. 
    public ICanvas DrawConnector(ICanvas canvas, Point a, Point b)
        => canvas
        .Draw(new(Colors.Transparent), new(Colors.Black, 5), ConnectorGeometry(a, b))
        .Draw(new(Colors.Transparent), new(Colors.Blue, 4), ConnectorGeometry(a, b))
        ;

    public override ICanvas Draw(ICanvas canvas)
    {
        var gridDistance = View.Style.GridDistance;

        // Fill the background
        canvas = canvas.Draw(new StyledRect(View.Style.ShapeStyle, Client));

        if (View.Style.ShowGrid)
        {
            // Draw vertical lines 
            for (var i = gridDistance; i < Client.Width; i += gridDistance)
            {
                canvas = canvas.Draw(new StyledLine(View.Style.ShapeStyle.PenStyle, 
                    new Line(new Point(i, 0), new Point(i, Client.Height))));
            }

            // Draw horizontal lines 
            for (var i = gridDistance; i < Client.Height; i += gridDistance)
            {
                canvas = canvas.Draw(new StyledLine(View.Style.ShapeStyle.PenStyle, 
                    new Line(new Point(0, i), new Point(Client.Width, i))));
            }
        }
        var socketPoints = this.GetSockets().ToDictionary(s => s.View.Model.Id, s => s.AbsoluteCenter());
        foreach (var c in View.Graph.Connections)
        {
            var p1 = socketPoints[c.SourceId];
            var p2 = socketPoints[c.DestinationId];
            canvas = DrawConnector(canvas, p1, p2);
        }
        return base.Draw(canvas);
    }
}