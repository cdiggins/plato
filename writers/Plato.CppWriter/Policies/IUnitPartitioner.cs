// How the generated definitions are divided into translation units (the splitting decision).
using System.Collections.Generic;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>The kind of definitions being divided.</summary>
    public enum UnitKind { Functions, Json }

    /// <summary>Divides an ordered sequence of definitions into translation units.</summary>
    public interface IUnitPartitioner
    {
        /// <summary>
        /// Returns at least one unit. Each is <paramref name="head"/>, a run of whole definitions in their
        /// given order, then <paramref name="tail"/>.
        /// </summary>
        IEnumerable<string> Partition(IEnumerable<string> definitions, string head, string tail, UnitKind kind);
    }
}
