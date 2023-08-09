using System;
using Peacock;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Emu;

[Mutable]
public class TestData
{
    public static string Text =
            
        @"Point 2D
* Value *
* X : Number *
* Y : Number *
  Values[] *
--
Mouse
  Point : Point 2D *
  Left : Boolean *
  Middle : Boolean *
  Right : Boolean * 
--
Keyboard
  Keys : Array * 
--
Time
  Seconds : Number *  
--
Pair 
* Value *
* A : Any *
* B : Any *  
--
Line
* Value *
* A : Point 2D * 
* B : Point 2D *
  Middle : Point 2D *
  Length : Number *   
--
Vector 2D
* Value *
* X : Number *
* Y : Number * 
  Normal : Vector 2D *
  Magnitude : Number *
  Values : Array *
--
Size 2D
* Value * 
* Width : Number * 
* Height : Number *
  Magnitude : Number *
--
Rect
* Value *
* Position : Point 2D *
* Size : Size 2D *
  Point : Point 2D *
--
Arithmetic
* A : Any *
* B : Any *
  Add : Any *
  Subtract : Any *
  Multiply : Any *
  Divide : Any *
  Average : Any *
  Interval : Any * 
  Magnitude : Any * 
--
Trig Ops
* Theta : Angle * 
  Sine : Number *
  Cosine : Number * 
  Tangent : Number *
  Secant : Number *
  Cosecant : Number *
  Cotangent : Number *
--
Inv Trig Ops
* Input : Number * 
  ArcSine : Angle *
  ArcCosine : Angle * 
  ArcTangent : Angle *
  ArcSecant : Angle *
  ArcCosecant : Angle *
  ArcCotangent : Angle *
--
Number Ops
* Input : Number *
  Negate : Number *
  Inverse : Number *
  Sign : Number *
  Magnitude : Number *
  Sqrt : Number *  
  Deg To Rad : Number *
  Rad to Deg : Number *
--
Repeat
* Element : Any *
* Count : Integer *
  Output : Array *
--
Sequence
* Start : Integer *
* Count : Integer * 
  Output : Array *
--
Pair
* A : Any *
* B : Any *
  Output : Array *
--
Subsequence
* A : Array *
* Interval : Interval *
  Output : Array *
--
Element At
* A : Array *
* Index : Integer *
  Output : Array *
--
Where
* A : Array *
* Filter : Array *
  Output : Array *
--
Conditional
* A : Any *
* B : Any *
* Condition : Boolean *
  Output : Any * 
--
Lerp
* Interval : Interval *
* Amount : Number *
  Output : Any *
--
Inverse Lerp
* Interval : Interval *
* Lerp : Any *
  Output : Number *
--
Interval
* Value *
* A : Any *
* B : Any *
* Low : Any *
* High : Any *
  Magnitude : Number *
  Middle : Any * 
--
Clamp 
* Input : Any *
* Interval : Interval *
  Output : Any *
--
Circle
* Value *
* Point : Point 2D *
* Radius : Number * 
--
Chord
* Value *
* Circle : Circle *
* Interval : Interval *
--
Comparison
* A : Any * 
* B : Any *
  > : Boolean *
  >= : Boolean *
  < : Boolean *
  <= : Boolean *
  == : Boolean *
  != : Boolean *
  Max : Any *
  Min : Any *
--
Boolean
* A : Boolean *
* B : Boolean *
  And : Boolean *
  Or : Boolean *
  Nand : Boolean *
  Nor : Boolean *
  Xor : Boolean *
  Not A : Boolean *
  Not B : Boolean *
--
Set 
* Set : Array * 
* Element : Any *
  Contains : Boolean*
  Add : Array *
  Remove : Array * 
--
Set Ops
* A : Array *
* B : Array *
  Union : Array *
  Difference : Array *
  Intersection : Array *
--
Sample
* Interval : Interval *
* Count : Integer * 
  Output : Array *
  Random : Array *
  Poissonian : Array *
--
Array Ops
* Input : Array *
  Reverse : Array *
  Sort : Array * 
  Shuffle : Array *
  Count : Integer *
  First : Any *
  Last : Any *
  Range : Any * 
-- 
Select Index
* Input : Array * 
* Indices : Array *
  Output : Array *
--
Bezier Curve
* Start : Any *
* Control A : Any *
* Control B : Any *
* End : Any *
* Amount : Number * 
  Output : Any * 
--
Transform 2D
* Value *
* Translation : Vector 2D *
* Scale : Size 2D * 
* Rotation : Angle *
";

    public static double DefaultNodeWidth = 110;
    public static double DefaultNodeHeight(int slots) => DefaultHeaderHeight + slots * DefaultSlotHeight;
    public static double DefaultSlotHeight = 20;
    public static double DefaultHeaderHeight = DefaultSlotHeight * 1.5;

    public static IReadOnlyList<Node> CreateNodes(string s)
    {

        var nodes = new List<Node>();
        const int rows = 3;
        foreach (var subString in s.Split("--").Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var i = nodes.Count;
            var pos = new Point(20 + i / rows * DefaultNodeWidth * 1.3, 20);

            if (i % rows != 0)
            {
                var prevNode = nodes[i - 1];
                pos = new(pos.X, prevNode.Rect.Bottom + 20);
            }

            var node = CreateNode(pos, subString);
            nodes.Add(node);
        }

        return nodes;
    }

    public static Guid NewGuid() => Guid.NewGuid();

    public static Slot CreateSlot(string s, string nodeName)
    {
        s = s.Trim();

        var hasLeftSocket = s.StartsWith("*");
        var hasRightSocket = s.EndsWith("*");

        s = s.TrimStart('*', ' ').TrimEnd('*', ' ').Trim();

        var n = s.IndexOf(':');
        var name = s.Trim();
        var type = nodeName.Trim();

        if (n >= 0)
        {
            name = s.Substring(0, n - 1).Trim();
            type = s.Substring(n + 1).Trim();
        }

        var leftSocket = hasLeftSocket ? new Socket(NewGuid(), type, true) : null;
        var rightSocket = hasRightSocket ? new Socket(NewGuid(), type, false) : null;
        return new Slot(NewGuid(), name, type, leftSocket, rightSocket);
    }

    public static Node CreateNode(Point pos, string s)
        => CreateNode(pos, s.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList());

    public static Node CreateNode(Point pos, List<string> contents)
    {
        var label = contents[0].Trim();
        contents = contents.Skip(1).ToList();

        var slots = new List<Slot>();
        for (var i=0; i < contents.Count; ++i)
        {
            var c = contents[i];
            var slot = CreateSlot(c, label);
            slots.Add(slot);
        }

        var kind = slots[0].Label == "Value"
            ? NodeKind.PropertySet
            : NodeKind.OperatorSet;

        if (!slots.Any(slot => slot.Left != null))
            kind = NodeKind.Output;

        var rect = new Rect(pos, new Size(DefaultNodeWidth, DefaultNodeHeight(slots.Count)));
        return new Node(NewGuid(), rect, label, kind, slots);
    }

    public static Graph CreateGraph()
        => new(NewGuid(), CreateNodes(Text), Array.Empty<Connection>());
}