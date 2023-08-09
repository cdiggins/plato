using System;
using System.Windows;
using Peacock;

namespace Emu.Controls;

public record SocketStyle(ShapeStyle ShapeStyle, TextStyle TextStyle, Radius Radius, double ClickRadius);

public record SocketView(Socket Socket, SocketStyle Style) : View(Socket, Socket.Id);

public record SocketControl(Measures Measures, SocketView View, Func<IUpdates, IControl, IControl, IUpdates> Callback) 
    : Control<SocketView>(Measures, View, Callback)
{
    public override ICanvas Draw(ICanvas canvas)
        => canvas.Draw(StyledShape());

    public StyledEllipse StyledShape() 
        => new(View.Style.ShapeStyle, Shape());

    public Ellipse Shape() 
        => new(Client.Center(), View.Style.Radius);

    public Point AbsoluteCenter()
        => Absolute.Center();
}