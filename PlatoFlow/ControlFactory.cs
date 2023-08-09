using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Emu.Controls;
using Peacock;

namespace Emu;

// We like: borders. ANd borders around sockets. We don't love borders around slots.
// WE want themes.
// Flip text on and off.

public record ControlFactory : IControlFactory
{
    public static double NodeHeaderMultiplier = 1.2;
    public int NodeSlotHeight => 16;
    public int NodeWidth => 130;
    public int SocketRadius => 5;

    public double SlotTextOffset => 10;

    public double BorderWidth => 1;

    public int TabRadius => SocketRadius + 10;

    public int GridDistance = 40;
    public bool ShowGrid = false;

    // Socket colors? 
    // Types: Compound, Simple (number/other), Array, Any.

    public Color GetSocketColor(Socket socket)
        => socket.Type switch
        {
            "Angle" or "Boolean" or "Number" or "Decimal" 
                => Color.FromRgb(0x6e, 0x0d, 0xd0),
            "Array" => Colors.Cyan,
            _ => Colors.Chartreuse,
        };

    public Color GetNodeColor(NodeKind kind)
        => kind switch
        {
            NodeKind.PropertySet => Colors.Chartreuse,
            NodeKind.OperatorSet => Colors.Yellow,
            NodeKind.Output => Colors.DeepPink,
            NodeKind.Input => Colors.Cyan,
            _ => Colors.White
        };

    public Color BackgroundColor = Colors.Black;
    public Color GridColor = Color.FromRgb(0x33, 0x33, 0x33);

    //public string Font => "Lucida Sans";//  Fonts[7];
    //public string Font => "Segoe UI";//  Fonts[7];
    public string Font => "Century Gothic";//  Fonts[7];
    //public string Font => @"Gill Sans Nova"; //  Fonts[7];
    //public string Font => @"Roboto"; //  Fonts[7];

    public string[] Fonts => new[]
    {
        "Verdana",
        "Open Sans",
        "Noto",
        "Lato",
        "Roboto",
        "Segoe UI",
        "Trebuchet MS",
        "Barlow",
        "Calibri",
        "Gill Sans Nova"
    };
    public TextStyle TextStyle => new(Colors.WhiteSmoke, Font, Peacock.FontWeight.Normal, 10, new(AlignmentX.Center, AlignmentY.Center));

    public TextStyle NodeTextStyle => TextStyle with { FontSize = 14, FontFamily = "Roboto", Weight = Peacock.FontWeight.Bold };
    public TextStyle SlotTextStyle => TextStyle with { FontSize = 10, Alignment = Alignment.LeftCenter,  };
    public TextStyle SlotTypeTextStyle => TextStyle with { FontSize = 8, Alignment = Alignment.RightTop };
    public TextStyle SocketTextStyle => TextStyle with { FontSize = 6, Alignment = Alignment.RightTop };

    public GraphStyle GraphStyle
        => new(new(Color.FromRgb(0x22, 0x22, 0x22), GridColor), NodeTextStyle, GridDistance, ShowGrid);

    public NodeStyle GetNodeStyle(Node node)
        => new(new(BackgroundColor, new(GetNodeColor(node.Kind), BorderWidth)), NodeTextStyle, 4);

    public SlotStyle GetSlotStyle(Node node, Slot slot)
        => new(GetNodeStyle(node).ShapeStyle, SlotTextStyle, SocketTextStyle, 4, SlotTextOffset, TabRadius);

    public SocketStyle GetSocketStyle(Socket socket)
        => new(new(GetSocketColor(socket), new PenStyle(BackgroundColor, 2)), SocketTextStyle, SocketRadius, SocketRadius + 2);

    public IUpdates UpdateModel(IUpdates updates, IControl oldControl, IControl newControl)
        => newControl switch
        {
            GraphControl gc 
                => updates,

            NodeControl nc
                // TODO: So this will probably work, but it seems a bit heavy-handed. 
                // The current model is just obliterated with the one in the newControl. 
                // There is no way to apply incremental updates. Each newControl just replaces  
                // any other model change that another previously did. 
                // What I really want is to compute the delta from the previous to the next,
                // and that this function is responsible for applying the delta.
                => updates.UpdateModel(nc.View.Node, _ => nc.View.Node),

            SlotControl sc
                => updates,

            SocketControl sc 
                => updates,

            _
                => throw new NotImplementedException($"Unrecognized newControl {oldControl}")
        };

    public GraphControl Create(Graph graph, Rect rect)
        => new(new Measures(new Point(0,0), rect), new(
            graph, GraphStyle),
            graph.Nodes.Select(Create).ToList(),
            UpdateModel);

    public Measures NodeMeasures(Node node)
        => new(new Point(), node.Rect);

    public static double HeaderHeight(Node node)
        => SlotHeight(node) * NodeHeaderMultiplier;

    public static double SlotHeight(Node node)
        => node.Rect.Height / (node.Slots.Count + NodeHeaderMultiplier);

    public double SlotXOffset => 14;

    public Rect SlotRect(Node node, int i)
        => new Rect(
            new(0, HeaderHeight(node) + SlotHeight(node) * i), 
            new Size(node.Rect.Width, SlotHeight(node))).ShrinkFromCenter(new(SlotXOffset, 0));

    public Measures SlotMeasures(Node node, int i)
        => NodeMeasures(node).Relative(SlotRect(node, i));

    public Rect SocketRect(Point point)
        => point.ToSquareWithCenter(SocketRadius * 2);
    
    public Measures SocketMeasures(Socket socket, Measures slotMeasures)
        => socket.LeftOrRight
            ? slotMeasures.Relative(SocketRect(slotMeasures.ClientRect.LeftCenter())) 
            : slotMeasures.Relative(SocketRect(slotMeasures.ClientRect.RightCenter()));

    public NodeControl Create(Node node)
        => new(
            NodeMeasures(node),
            new(node, GetNodeStyle(node)),
            node.Slots.Select((slot, i) => Create(node, slot, SlotMeasures(node, i))).ToList(), 
            UpdateModel);

    public SlotControl Create(Node node, Slot slot, Measures slotMeasures)
        => new(slotMeasures,
            new(slot, GetSlotStyle(node, slot)),
            Create(slot.Left, slotMeasures),
            Create(slot.Right, slotMeasures), UpdateModel);

    public SocketControl? Create(Socket? socket, Measures slotMeasures)
        => socket == null 
            ? null 
            : new(SocketMeasures(socket, slotMeasures), new(socket, GetSocketStyle(socket)), UpdateModel);

    public IEnumerable<IControl> Create(IModel model, Rect rect)
        => new[] { Create((Graph)model, rect) };

}