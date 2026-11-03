// How a closed sum type (a tagged union) is laid out and used in generated C++.
using System;
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Everything the writer knows about the representation of a sum type: its data fields, and how a case
    /// is constructed, compared, hashed and tested. JSON reads and writes the fields <see cref="Fields"/>
    /// lists, so a new layout changes JSON by changing the fields. Only the Cpu dialect emits sums.
    /// </summary>
    public interface ISumLayout
    {
        /// <summary>The data members of the struct, in declaration order.</summary>
        IReadOnlyList<CppField> Fields(IReadOnlyList<SumCaseDef> cases);

        /// <summary>The member declarations that name each case's tag, one per line, each ending in a newline.</summary>
        string TagDeclarations(IReadOnlyList<SumCaseDef> cases);

        /// <summary>A function body, starting with a space, that builds case <paramref name="c"/> from <paramref name="values"/>.</summary>
        string Construct(string typeName, SumCaseDef c, IReadOnlyList<string> values);

        /// <summary>The condition under which <c>a</c> and <c>b</c> are equal.</summary>
        string EqualsCondition(string typeName, IReadOnlyList<SumCaseDef> cases);

        /// <summary>A function body, starting with a space, that returns the hash of <c>a</c>.</summary>
        string HashBody(string typeName, IReadOnlyList<SumCaseDef> cases, Func<CppField, string> hashOf);

        /// <summary>A function body, starting with a space, for the predicate <c>Is&lt;Case&gt;(const T&amp; s)</c>.</summary>
        string PredicateBody(string typeName, SumCaseDef c);
    }
}
