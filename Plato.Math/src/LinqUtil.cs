using System;
using System.Collections.Generic;
using System.Text;

namespace Plato.Math
{
    public static class LinqUtil
    {
        public static Box ToBox(this IEnumerable<Vector3> self)
            => Box.Create(self);
    }
}
