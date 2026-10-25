using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Integer = Ara3D.Geometry.Integer;
using Boolean = Ara3D.Geometry.Boolean;

// Private experiment only: TIR proves eligibility; Roslyn keeps the emitted setup unchanged.
static class Qualification
{
    public delegate IReadOnlyList<Integer> Ring(Integer count, Boolean forward);

    public static void Run(TirFunction function, CSharpWriter writer, string emitted, string output, bool timing)
    {
        var loop = function.AllNodes.OfType<TirLoweredLoop>().Single();
        var conditional = ((TirLambda)loop.Fn).Body as TirConditional
            ?? throw new Exception("Expected direct conditional");
        var coercion = conditional.Condition as TirCoerce ?? throw new Exception("Expected scalar annotation");
        var boolean = writer.Compilation.TypeDefinitionsByName["Boolean"];
        bool Canonical(TypeExpression t) => t != null && t.TypeArgs.Count == 0
            && ReferenceEquals(t.Def, boolean) && boolean.IsPrimitiveDeclaration;
        bool Identity(TirCoerce c) => c.ConversionFn == null && Equals(c.FromType, c.ToType)
            && Canonical(c.FromType) && Canonical(c.ToType) && Canonical(c.Inner.Type);
        if (!Identity(coercion) || coercion.Inner is not TirParameter parameter
            || !function.Parameters.Any(p => ReferenceEquals(p, parameter.Def))
            || function.AllNodes.OfType<TirAssign>().Any()
            || loop.Kind != "MapRange" || loop.SourceStages.Count != 0
            || !TirIntrinsicIdentity.IsIntrinsic(loop.SourceFunction, writer.Compilation, "MapRange"))
            throw new Exception("Identity/coercion/loop proof failed");
        Console.WriteLine($"PROOF From={coercion.FromType} To={coercion.ToType} converter=null canonical=true outer={parameter.Def.Name}");
        var integer = writer.Compilation.TypeDefinitionsByName["Integer"].ToTypeExpression();
        if (Identity(new TirCoerce(coercion.Inner, integer, coercion.ToType, null, null)))
            throw new Exception("Nonidentity coercion was admitted");
        // Reuse a real resolved function as adversarial executable converter metadata.
        if (Identity(new TirCoerce(coercion.Inner, coercion.FromType, coercion.ToType, loop.SourceFunction, null)))
            throw new Exception("Converter-backed coercion was admitted");

        var syntax = SyntaxFactory.ParseStatement(emitted);
        var fill = syntax.DescendantNodes().OfType<ForStatementSyntax>().Single();
        var choice = fill.DescendantNodes().OfType<ConditionalExpressionSyntax>().Single();
        var bound = ((BinaryExpressionSyntax)fill.Condition!).Right;
        var yes = fill.ReplaceNode(choice, choice.WhenTrue);
        var no = fill.ReplaceNode(choice, choice.WhenFalse);
        var split = SyntaxFactory.IfStatement(
            SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, bound,
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))),
            SyntaxFactory.Block(SyntaxFactory.IfStatement(choice.Condition, SyntaxFactory.Block(yes),
                SyntaxFactory.ElseClause(SyntaxFactory.Block(no)))));
        var enabled = syntax.ReplaceNode(fill, split);
        if (syntax.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Count() != 1
            || enabled.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Count() != 1
            || enabled.DescendantNodes().OfType<ForStatementSyntax>().Count() != 2)
            throw new Exception("Unexpected generated setup/fill structure");
        // Replacing the split with its original loop must recover every original setup/capture token.
        if (!enabled.ReplaceNode(enabled.DescendantNodes().OfType<IfStatementSyntax>().First(), fill).IsEquivalentTo(syntax))
            throw new Exception("Setup changed outside the fill loop");
        var off = Compile(emitted, "off", output);
        var on = Compile(enabled.NormalizeWhitespace().ToFullString(), "on", output);
        foreach (var forward in new[] { false, true })
        foreach (var n in new[] { -9, 0, 1, 2, 17, 4096 })
        {
            var a = off(n, forward); var b = on(n, forward);
            if (!a.SequenceEqual(b) || a.Count != Math.Max(0, n)) throw new Exception("Differential mismatch");
            for (var i = 0; i < a.Count; i++)
                if ((long)a[i] != (forward ? i : n - 1 - i)) throw new Exception("Ring oracle mismatch");
        }
        foreach (var count in new[] { (long)int.MaxValue + 1, (long)int.MinValue - 1 })
        foreach (var forward in new[] { false, true })
        foreach (var run in new[] { off, on })
        {
            try { run((Integer)count, forward); throw new Exception("Missing narrowing fault"); }
            catch (OverflowException) { }
        }
        Console.WriteLine("PASS identity metadata/refusals, one allocation/setup preservation, ring values and narrowing faults; no timings");
        if (timing) Timing.Run(off, on, output);
    }

    static Ring Compile(string body, string mode, string output)
    {
        // ABI adapters use the frozen runtime's existing wrapper operators, preserving arithmetic.
        var source = "using System; using System.Collections.Generic; namespace Ara3D.Geometry { public static class RingProbe { "
            + "public static Integer Add(this Integer a,Integer b)=>a+b; public static Integer Subtract(this Integer a,Integer b)=>a-b;"
            + "public static IReadOnlyList<Integer> Run(Integer count, Boolean forward) " + body + " } }";
        File.WriteAllText(Path.Combine(output, "qualified-" + mode + ".cs.txt"), source);
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(Integer).Assembly.Location).Where(File.Exists)
            .Where(p => Path.GetFileName(p) != "Ara3D.Geometry.dll").Distinct();
        var compilation = CSharpCompilation.Create("Ring_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Default)) },
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        File.WriteAllText(Path.Combine(output, "qualified-" + mode + "-compile.txt"), string.Join("\n", result.Diagnostics));
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics));
        return Assembly.Load(stream.ToArray()).GetType("Ara3D.Geometry.RingProbe")!.GetMethod("Run")!.CreateDelegate<Ring>();
    }
}
