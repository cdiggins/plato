using System;
using System.Windows;
using Peacock;

namespace Emu.Controls;

public record SlotStyle(ShapeStyle ShapeStyle, TextStyle TextStyle, TextStyle SmallTextStyle, Radius Radius, double TextOffset, double TabRadius);

public record SlotView(Slot Slot, SlotStyle Style) : View(Slot, Slot.Id);

public record SlotControl(Measures Measures, SlotView View, SocketControl? Left, SocketControl? Right, Func<IUpdates, IControl, IControl, IUpdates> Callback) 
    : Control<SlotView>(Measures, View, ToChildren(Left, Right), Callback)
{
    public override ICanvas Draw(ICanvas canvas) 
        => canvas.Draw(StyledText());

    public ICanvas DrawTabs(ICanvas canvas)
    {       
        var tabSize = View.Style.TabRadius;
        var leftRect = Client.LeftCenter().Subtract(new Size(0, tabSize/2)).ToRect(new(tabSize/2, tabSize));
        var rightRect = Client.RightCenter().Subtract(new Size(0, tabSize/2)).ToRect(new(tabSize/2, tabSize));
        var leftTab = new StyledRect(View.Style.ShapeStyle, new (leftRect, View.Style.Radius));
        var rightTab = new StyledRect(View.Style.ShapeStyle, new (rightRect, View.Style.Radius));
        if (View.Slot.Left != null)
            canvas = canvas.Draw(leftTab);
        if (View.Slot.Right != null)
            canvas = canvas.Draw(rightTab);
        return canvas;
    }

    public StyledText StyledText() 
        => new(View.Style.TextStyle, Client.Offset(TextOffset()).Shrink(TextOffset()), View.Slot.Label);
        
    public Size TextOffset() 
        => new(View.Style.TextOffset, 0);


// Uncomment for certain types of behaviors
//    public override IEnumerable<IBehavior> GetDefaultBehaviors()
//        => new IBehavior[] { new ShowTabBehavior(this) };
}