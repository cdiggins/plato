// Replaces supported array combinators with loop markers and temporary references.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only marker (--loops): a reference to a lowered loop's result local.</summary>
public class TirTempRef : TirNode
{
    public string Name { get; }
    public TirTempRef(string name, TypeExpression type, Symbol origin) : base(type, origin) => Name = name;
    public override string ToString() => Name;
}

