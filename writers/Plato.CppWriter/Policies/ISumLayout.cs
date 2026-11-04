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

        /// <summary>The test, appended to a sum value, that it holds case <paramref name="c"/>.</summary>
        string CaseTest(string typeName, SumCaseDef c);

        /// <summary>The member read, appended to a sum value, that yields its tag when a chain of case predicates on one
        /// value is written as a <c>switch</c>; null (the default) leaves the chain as conditionals.</summary>
        string SwitchOn => null;

        /// <summary>This layout, told which cases may share storage (a case whose fields are all trivially copyable and
        /// destructible); a layout that never shares returns itself. The writer binds once its types are known.</summary>
        ISumLayout BindStorage(Func<SumCaseDef, bool> mayShareStorage) => this;

        /// <summary>The member declarations of the struct (the tag constants are <see cref="TagDeclarations"/>), each line
        /// ending in a newline. <paramref name="cppType"/> spells a field's C++ type.</summary>
        string MemberDeclarations(string typeName, IReadOnlyList<SumCaseDef> cases, Func<CppField, string> cppType);

        /// <summary>The member path, relative to a value of the sum, that reads field <paramref name="f"/> of case <paramref name="c"/>.</summary>
        string FieldAccess(SumCaseDef c, SumCaseField f);

        /// <summary>The statements, each ending in a newline, that fill <c>v</c> from the object <c>j</c>.</summary>
        string JsonRead(string typeName, IReadOnlyList<SumCaseDef> cases);

        /// <summary>The statements, each ending in a newline, that set every field of <c>v</c> on the object <c>j</c>.</summary>
        string JsonWrite(string typeName, IReadOnlyList<SumCaseDef> cases);
    }
}
