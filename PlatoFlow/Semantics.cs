using System.Collections.Generic;

namespace Emu;

public static class Semantics
{
    public static List<(string, string)> TwoWayConverters = new()
    {
        ("AbsoluteCenter 2D", "Vector 2D"),
        ("AbsoluteCenter 2D", "Size 2D"),
        ("Size 2D", "Vector 2D"),
        ("Line", "Interval"),
        ("Line", "Rect"),
        ("Angle", "Number"),
        ("Integer", "Number"),
        ("Integer", "Angle"),
        ("Boolean", "Number"),
        ("Boolean", "Integer"),
    };

    public static Dictionary<string, HashSet<string>> CanConvert = new();

    static Semantics()
    {
        foreach (var (type1, type2) in TwoWayConverters)
        {
            if (!CanConvert.ContainsKey(type1))
                CanConvert.Add(type1, new());
            CanConvert[type1].Add(type2);

            if (!CanConvert.ContainsKey(type2))
                CanConvert.Add(type2, new());
            CanConvert[type2].Add(type1);
        }
    }

    public static bool CanConnect(string fromType, string toType)
    {
        if (fromType == "Any" || toType == "Any")
            return true;
        if (toType == "Array")
            return true;
        if (fromType == toType)
            return true;
        if (CanConvert.TryGetValue(fromType, out var hashSet) && hashSet.Contains(toType))
            return true;
        return false;
    }

    public static bool CanConnect(Socket source, Socket dest)
        => source.LeftOrRight ^ dest.LeftOrRight && CanConnect(source.Type, dest.Type);
}