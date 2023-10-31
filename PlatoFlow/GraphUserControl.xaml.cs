using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Peacock;
using Peacock.Wpf;
using Plato.Compiler;
using Plato.Compiler.Vsg;

namespace Emu;

/// <summary>
/// Interaction logic for GraphUserControl.xaml
/// TODO: a lot of this could go into the Peacock.Wpf layer.
/// </summary>
public partial class GraphUserControl : UserControl
{
    public ControlManager Manager;
    public IControlFactory Factory;
    public Graph Graph;

    public WpfCanvas Canvas = new();
    public int WheelZoom;
    public double ZoomFactor => Math.Pow(1.15, WheelZoom / 120.0);

    public DispatcherTimer Timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(20)
    };

    public DateTimeOffset Started;

    public class MouseStatus : IMouseStatus
    {
        public MouseStatus(GraphUserControl control) => Control = control;
        public GraphUserControl Control { get; }
        public Point Location => Mouse.GetPosition(Control).Multiply(1.0 / Control.ZoomFactor);
        public bool LButtonDown => Mouse.LeftButton == MouseButtonState.Pressed;
        public bool RButtonDown => Mouse.RightButton == MouseButtonState.Pressed;
        public bool MButtonDown => Mouse.MiddleButton == MouseButtonState.Pressed;
    }

    public static double DefaultNodeWidth = 110;
    public static double DefaultNodeHeight(int slots) => DefaultHeaderHeight + slots * DefaultSlotHeight;
    public static double DefaultSlotHeight = 20;
    public static double DefaultHeaderHeight = DefaultSlotHeight * 1.5;
    
    public static Rect GetNodeRect(Point pos, int numSlots) 
        => new(pos, new Size(DefaultNodeWidth, DefaultNodeHeight(numSlots)));

    public record RelativeLocation(int Layer, int Position);

    public static Dictionary<Guid, RelativeLocation> ComputeNodeLocations(VisualSyntaxGraph vsg)
    {
        var gq = new VisualSyntaxGraphQueries(vsg);
        var r = new Dictionary<Guid, RelativeLocation>();
        
        // Start from the output. 
        var pos = 0;
        var layer = 0;
        var currentLayer = new List<Guid>();
        foreach (var n in gq.GetSinkNodes())
        {
            if (!r.ContainsKey(n.Id))
            {
                r.Add(n.Id, new RelativeLocation(layer, pos++));
                currentLayer.Add(n.Id);
            }
        }

        var firstLayers = currentLayer;

        while (currentLayer.Count > 0)
        {
            layer++;
            pos = 0;
            var nextLayer = new List<Guid>();
            foreach (var id in currentLayer.ToList())
            {
                r[id] = new RelativeLocation(layer, pos++);
                foreach (var tmp in gq.GetInputNodeIds(id))
                {
                    nextLayer.Add(tmp);
                }
            }

            currentLayer = nextLayer;
        }
        return r;
    }

    public static Dictionary<Guid, Rect> ComputeNodeRects(VisualSyntaxGraph vsg)
    {
        var d = ComputeNodeLocations(vsg);
        var maxLayer = d.Values.Max(v => v.Layer);
        const int nodeSpacing = 20;
        var r = new Dictionary<Guid, Rect>();
        var ys = new double[maxLayer + 1];
        foreach (var kv in d)
        {
            var layer = kv.Value.Layer;
            var pos = kv.Value.Position;
            var width = DefaultNodeWidth;
            var x = nodeSpacing + (maxLayer - layer) * (width + nodeSpacing);
            var y = nodeSpacing + ys[layer];
            var node = vsg.Nodes[kv.Key];
            var height = DefaultNodeHeight(node.Inputs.Count + node.Outputs.Count);
            ys[layer] += height + nodeSpacing;
            var rect = new Rect(new Point(x, y), new Size(width, height));
            r.Add(kv.Key, rect);
        }

        return r;
    }

    public static string GetSourceFile([CallerFilePath] string thisFile = "")
        => Path.GetDirectoryName(thisFile);

    public static Graph Convert(VisualSyntaxGraph vsg)
    {
        var rects = ComputeNodeRects(vsg);        

        var nodes = new List<Node>();
        var conns = new List<Connection>();

        var i = 0;
        foreach (var node in vsg.Nodes.Values)
        {
            var slots = new List<Slot>();
            foreach (var input in node.Inputs)
            {
                var inputSocket = new Socket(input.Id, "Any", true);
                var slot = new Slot(Guid.NewGuid(), input.Label, "Any", inputSocket, null);
                slots.Add(slot);
            }

            foreach (var output in node.Outputs)
            {
                var outputSocket = new Socket(output.Id, "Any", false);
                var outputSlot = new Slot(Guid.NewGuid(), output.Label ?? "Output", "Any", null, outputSocket);
                slots.Add(outputSlot);
            }

            var rect = rects[node.Id];
            var newNode = new Node(node.Id, rect, node.Label, NodeKind.OperatorSet, slots);
            nodes.Add(newNode);
        }
        foreach (var conn in vsg.Connections.Values)
        {
            var tmp = new Connection(conn.Id, conn.Source, conn.Destination);
            conns.Add(tmp);
        }

        return new Graph(Guid.NewGuid(), nodes, conns);
    }

    public GraphUserControl()
    {
        InitializeComponent();
        Focusable = true;
        Focus();

        // TODO:
        var thisFolder = GetSourceFile();
        var fileName = Path.Combine(thisFolder, "..", "PlatoStandardLibrary", "vsg", "Lerp_149.json");
        var text = File.ReadAllText(fileName);
        var vsg = JsonSerializer.Deserialize<VisualSyntaxGraph>(text);
        Graph = Convert(vsg);
        //Graph = TestData.CreateGraph();
        
        Factory = new ControlFactory();
        Manager = new ControlManager(Factory);
        Manager.UpdateControlTree(Graph, new Rect(RenderSize));

        //(this.Parent as Window).PreviewKeyDown += (sender, args) => Console.WriteLine("Parent key press");
        PreviewKeyDown += (sender, args) => ProcessInput(new KeyDownEvent(args));
        PreviewKeyUp += (sender, args) => ProcessInput(new KeyUpEvent(args));
        PreviewMouseDoubleClick += (sender, args) => ProcessInput(new MouseDoubleClickEvent(args));
        PreviewMouseDown += (sender, args) => ProcessInput(new MouseDownEvent(args));
        PreviewMouseUp += (sender, args) => ProcessInput(new MouseUpEvent(args));
        PreviewMouseMove += (sender, args) => ProcessInput(new MouseMoveEvent(args));
        PreviewMouseWheel += (sender, args) => ProcessInput(new MouseWheelEvent(args));
        SizeChanged += (sender, args) => ProcessInput(new ResizeEvent(args));
            
        // Animation timer
        Timer.Tick += (sender, args) => ProcessInput(new ClockEvent((DateTimeOffset.Now - Started).TotalSeconds));
        Timer.Start();
        Started = DateTimeOffset.Now;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        Render(drawingContext);
        base.OnRender(drawingContext);
    }

    public void Render(DrawingContext drawingContext)
    {
        var rect = new Rect(new(), RenderSize);
        drawingContext.PushClip(new RectangleGeometry(rect));
        drawingContext.DrawRectangle(new SolidColorBrush(Colors.SlateGray), new Pen(), rect);
        var scaleTransform = new ScaleTransform(ZoomFactor, ZoomFactor);
        drawingContext.PushTransform(scaleTransform);
        Canvas.Context = drawingContext;
        Render(Canvas);
        drawingContext.Pop();
        drawingContext.Pop();
    }

    public void Render(ICanvas canvas)
    {
        Manager.Draw(canvas);
    }

    public void ProcessInput<T>(T inputEvent)
        where T : InputEvent
    {
        if (inputEvent is MouseWheelEvent mwe)
        {
            WheelZoom += mwe.Args.Delta;
        }

        inputEvent.MouseStatus = new MouseStatus(this);

        var updates = Manager.ProcessInput(inputEvent);
        Manager.ApplyChanges(updates);
        Graph = updates.UpdateModel(Graph);
        Manager.UpdateControlTree(Graph, new Rect(new(), new Size(10000, 10000)));
        InvalidateVisual();
    }
}