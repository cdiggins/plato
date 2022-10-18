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

        public static PlatoTypeExpr ToPlatoType(this Type t)
            => new PlatoTypeExpr(0, t.FullName);

        public static PlatoLiteral ToPlatoLiteral(this object value)
            => new PlatoLiteral(0, value.GetType().ToPlatoType(), value);
    }
}