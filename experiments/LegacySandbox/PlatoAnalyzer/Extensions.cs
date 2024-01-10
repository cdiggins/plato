using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAnalyzer
{
    public static partial class Extensions
    {
        public static string ToCommaDelimitedStrings<T>(this IEnumerable<T> self) 
            => string.Join(", ", self);

        public static IReadOnlyList<T> ToListOrEmpty<T>(this IEnumerable<T> self)
            => self?.ToList() ?? new List<T>();

        public static string PrimitiveToTypeName(string s)
        {
            switch (s)
            {
                case "Boolean": return "bool";
                case "Byte": return "byte";
                case "SByte": return "sbyte";
                case "Int16": return "short";
                case "UInt16": return "short";
                case "Int32": return "short";
                case "UInt32": return "short";
                case "Int64": return "long";
                case "UInt64": return "ulong";
                case "Decimal": return "decimal";
                case "Single": return "float";
                case "Double": return "double";
                default: return s;
            }
        }
        public static string TypeToName(Type t)
            => t.IsPrimitive ? PrimitiveToTypeName(t.Name) : t.FullName;

        public static PlatoTypeExpr ToPlatoType(this Type t)
            => new PlatoTypeExpr(0, t.FullName);

        public static PlatoLiteral ToPlatoLiteral(this object value)
            => new PlatoLiteral(0, value.GetType().ToPlatoType(), value);
    }
}