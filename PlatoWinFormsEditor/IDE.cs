using System.Text;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using Plato.Compiler;

namespace PlatoWinFormsEditor;

public class IDE
{
    public string Input { get; set; }
    public string Output { get; set; }
    public Compilation Compilation { get; set; }

    public string ParseTree => Try(() => Compilation?.ParseTree?.ToString());
    public string CstXml => Try(() => Compilation?.CstTree?.ToXml().ToString());
    public string AstXml => Try(() => Compilation?.AstTree?.ToXml());
    public string AbstractValuesXml => Compilation.SymbolResolver.TypeDefs.ToXml();

    public IDE()
    {
        var inputFile1 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\concepts.plato";
        var inputFile2 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\types.plato";
        var inputFile3 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\libraries.plato";
        var input1 = File.ReadAllText(inputFile1);
        var input2 = File.ReadAllText(inputFile2);
        var input3 = File.ReadAllText(inputFile3);
        Input = input1 + Environment.NewLine + input2 + Environment.NewLine + input3;
        Compilation = Compile(Input);

        var outputFolder = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        File.WriteAllText(Path.Combine(outputFolder, "output.cs"), Compilation.ToCSharp());
        File.WriteAllText(Path.Combine(outputFolder, "output.plato.html"), Compilation.ToPlatoHtml());

        var inputFolder = outputFolder;
        var prologue = File.ReadAllText(Path.Combine(inputFolder, "prologue.js"));
        var epilogue = File.ReadAllText(Path.Combine(inputFolder, "epilogue.js"));
        var output = prologue 
                     + Environment.NewLine
                     + Compilation.ToJavaScript() 
                     + Environment.NewLine 
                     + epilogue;
        File.WriteAllText(Path.Combine(outputFolder, "output.js"), output);

        //Output += GetConstraintsOutput();
        //Output += GetOperationsOutput();
        //Output += GetTypeGuesserOutput();
    }

    public string GetOperationsOutput()
    {
        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("= Operations =").AppendLine();
        var ops = Compilation.Operations;
        foreach (var kv in ops.Lookup)
        {
            sb.AppendLine(kv.Key);
            foreach (var v in kv.Value)
            {
                sb.AppendLine(v.ToString());
            }
        }

        return sb.ToString();
    }

    public string GetTypeGuesserOutput()
    {
        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("= Type Guesser =").AppendLine();
        var tg = Compilation.TypeResolver;
        foreach (var kv in tg.CandidateTypes)
        {
            sb.AppendLine($"Candidates for parameter {kv.Key} are");
            foreach (var c in kv.Value)
            {
                sb.AppendLine($"{c.Kind} {c.Name}");
            }

        }

        return sb.ToString();
    }

    public string GetConstraintsOutput()
    {
        // Get the type

        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("= Constraints =").AppendLine();
        foreach (var t in Compilation.TypeDefs)
        {
            sb.AppendLine($"{t.Kind} {t.Name}");
            foreach (var m in t.Methods)
            {
                sb.AppendLine($"Method {m.Name}");
                var lookup = Constraints.GetParameterConstraints(m.Function);
                foreach (var kv in lookup)
                {
                    var refs = string.Join(", ", kv.Value);
                    sb.AppendLine($"{kv.Key} = {refs}");
                }
            }
        }

        return sb.ToString();
    }

    public string Try(Func<string?> f)
    {
        try
        {
            return f() ?? "";
        }
        catch (Exception e)
        {
            return e.Message;
        }

        ;
    }

    public Compilation Compile(string input)
    {
        var c = new Compilation(input, PlatoGrammar.Instance.File,
            CstNodeFactory.Create, AstFromPlatoCst.Convert);

        var outputBuilder = new StringBuilder();

        outputBuilder.AppendLine(c.Success ? "Compilation Success" : "Compilation Failure");

        var state = c.State;
        if (state != null)
        {
            var curError = 0;
            for (var e = state.LastError; e != null; e = e.Previous)
            {
                outputBuilder.AppendLine($"Error {curError} at {e.LastState}");
                outputBuilder.AppendLine(
                    $"failed expected rule {e.Expected}, parent state is {e.ParentState}, message is {e.Message}");
                outputBuilder.AppendLine(e.LastState.CurrentLine);
                outputBuilder.AppendLine(e.LastState.Indicator);
            }

            outputBuilder.AppendLine(c.Message);

            if (!state.AtEnd())
            {
                outputBuilder.AppendLine("Parsing did not reach the end");
            }
        }
        else
        {
            outputBuilder.AppendLine("Parsing failed");
        }

        AnalyzeTypesAndFunctions(c, outputBuilder);

        Output = outputBuilder.ToString();
        return c;
    }

    public static StringBuilder OutputTypeResolverDetails(TypeResolver tr, StringBuilder sb = null)
    {
        sb.AppendLine($"# of expressions total = {tr.ExpressionTypes.Count}");
        sb.AppendLine($"# of expressions typed = {tr.ExpressionTypes.Count(kv => kv.Value != null)}");

        var dontKnowType = 0;
        var consideredFuncs = 0;
        foreach (var f in tr.TypedFunctions.Values)
        {
            if (f.ReturnType == null && f.Function.Body != null)
            {
                var result = tr.ExpressionTypes[f.Function.Body];
                dontKnowType += result == null ? 1: 0;
                consideredFuncs++;
            }
        }

        sb.AppendLine($"Don't know the type of {dontKnowType} funcs of {consideredFuncs} total");

        return sb;
    }

    public static StringBuilder OutputConstraints(Compilation c, StringBuilder sb = null)
    {
        var tr = c.TypeResolver;
        var groups = tr.TypedFunctions.Values.GroupBy(f => f.Id);

        sb.AppendLine($"Total Typed functions = {tr.TypedFunctions.Count}");
        sb.AppendLine($"Total Groups = {groups.Count()}");
        sb.AppendLine($"Groups with multiple options = {groups.Count(g => g.Count() > 1)}");

        /*
        var i = 0;
        foreach (var g in groups)
        {
            if (g.Count() > 1)
            {
                var str = string.Join(",", g);
                sb.AppendLine($"{i++} = {str}");

                foreach (var tf in g)
                {
                    sb.AppendLine($"  Considering Typed Function {tf}");
                    var d = Constraints.GetParameterConstraints(tf.Function);

                    foreach (var p in tf.Function.Parameters)
                    {
                        var constraints = d[p];
                        sb.AppendLine($"    Constraints for {p}");

                        foreach (var constraint in constraints)
                        {
                            sb.AppendLine(constraint.ToString());
                        }
                    }
                }
            }
        }
        */

        return sb;
    }

    public static StringBuilder AnalyzeTypesAndFunctions(Compilation c, StringBuilder sb = null)
    {
        sb ??= new StringBuilder();

        var functions = c.Functions;
        sb.AppendLine($"Number of functions = {functions.Count}");

        var typedFunctions = functions.Where(f => f.IsExplicitlyTyped()).ToList(); // TODO: look for type annotations (e.g. concepts)
        sb.AppendLine($"Number of explicitly type functions = {typedFunctions.Count}");

        var funCalls = functions.SelectMany(f => f.Body.GetDescendantSymbols().OfType<FunctionCallSymbol>()).ToList();
        sb.AppendLine($"Number of function calls = {funCalls.Count}");

        var determinedFunctionTypes = 0;
        //sb.AppendLine($"Number of known implicitly typed functions = {determinedFunctionTypes}");

        var unambiguousFunCalls = 0;
        //sb.AppendLine($"Number of unambiguous function calls = {unambiguousFunCalls}");

        var ambiguousFunctions = new List<RefSymbol>();
        //sb.AppendLine($"Number of ambiguous function calls");

        var expressions = functions.SelectMany(f => f.GetDescendantSymbols()).Where(s => s.IsExpression()).ToList();
        sb.AppendLine($"Number of expressions to process {expressions.Count}");

        var referencedGroups = functions
            .SelectMany(f => f.GetDescendantSymbols())
            .OfType<RefSymbol>()
            .Select(rs => rs.Def)
            .OfType<FunctionGroupSymbol>()
            .Distinct()
            .ToList();

        sb.AppendLine($"Number of referenced function groups {referencedGroups.Count}");

        var referencedMultiGroups = referencedGroups.Where(g => g.Functions.Count > 1).ToList();
        sb.AppendLine($"Number of those groups with multiple functions {referencedMultiGroups.Count}");

        var tr = c.TypeResolver;
        var groups = tr.TypedFunctions.Values.GroupBy(f => f.Id);

        sb.AppendLine($"Total Typed functions = {tr.TypedFunctions.Count}");
        sb.AppendLine($"Total Groups = {groups.Count()}");
        sb.AppendLine($"Groups with multiple options = {groups.Count(g => g.Count() > 1)}");

        OutputConstraints(c, sb);

        OutputTypeResolverDetails(tr, sb);

        return sb;
    }
}