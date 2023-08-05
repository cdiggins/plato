using System.Diagnostics;
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
    public Parser Parser { get; set; }

    public TabControl TabControl { get; }
    public RichTextBox OutputTextBox { get; }
    public List<Editor> Editors { get; } = new();
    public Compiler Compiler { get; set;  }
    public Logger Logger { get; } = new Logger();

    public void OpenFile(string fileName)
    {
        var tabPage = new TabPage(Path.GetFileName(fileName));
        TabControl.TabPages.Add(tabPage);
        var edit = new RichTextBox();
        edit.Font = OutputTextBox.Font;
        edit.WordWrap = false;
        edit.Dock = DockStyle.Fill;
        tabPage.Controls.Add(edit);
        var editor = new Editor(fileName, edit, OutputTextBox);
        Editors.Add(editor);
    }

    public void Parse()
    {
        foreach (var editor in Editors)
            editor.Parse(Logger);
    }

    public IDE(TabControl tabControl, RichTextBox outputTextBox)
    {
        TabControl = tabControl;
        OutputTextBox = outputTextBox;

        OutputTextBox.WordWrap = false;
        OutputTextBox.MouseDoubleClick += (sender, args) => Debug.WriteLine($"Double clicked output. Mouse args = {args}. Selected text = {outputTextBox.SelectedText}");

        var inputPath = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        OpenFile(Path.Combine(inputPath, "intrinsics.plato"));
        OpenFile(Path.Combine(inputPath, "concepts.plato"));
        OpenFile(Path.Combine(inputPath, "types.plato"));
        OpenFile(Path.Combine(inputPath, "libraries.plato"));
        
        Parse();

        Compiler = new Compiler(Editors.Select(e => e.Parser), Logger);

        OutputTextBox.Lines = Logger.Messages.ToArray();
        /*
        Parser = Compile(Input);
        
        var outputFolder = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        File.WriteAllText(Path.Combine(outputFolder, "output.cs"), Parser.ToCSharp());
        File.WriteAllText(Path.Combine(outputFolder, "output.plato.html"), Parser.ToPlatoHtml());

        var inputFolder = outputFolder;
        var prologue = File.ReadAllText(Path.Combine(inputFolder, "prologue.js"));
        var epilogue = File.ReadAllText(Path.Combine(inputFolder, "epilogue.js"));
        var output = prologue 
                     + Environment.NewLine
                     + Parser.ToJavaScript() 
                     + Environment.NewLine 
                     + epilogue;
        File.WriteAllText(Path.Combine(outputFolder, "output.js"), output);

        //Output += GetConstraintsOutput();
        //Output += GetOperationsOutput();
        //Output += GetTypeGuesserOutput();
        */
    }

    public string GetOperationsOutput()
    {
        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("= Operations =").AppendLine();
        var ops = Compiler.Operations;
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

    public string GetConstraintsOutput()
    {
        // Get the type

        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("= Constraints =").AppendLine();
        foreach (var t in Compiler.TypeDefs)
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
    }

    public static StringBuilder OutputTypeResolverDetails(TypeResolver tr, StringBuilder sb = null)
    {
        sb ??= new StringBuilder();
        var dontKnowType = tr.Functions.Count(f => tr.GetType(f.Body) == null);
        sb.AppendLine($"Don't know the type of {dontKnowType} funcs of {tr.Functions.Count} total");
        return sb;
    }

    /*
    public static StringBuilder AnalyzeTypesAndFunctions(Parser c, StringBuilder sb = null)     
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
        OutputTypeResolverDetails(tr, sb);
        
        c.TypeResolver.ComputeExpressionTypes();
        OutputTypeResolverDetails(tr, sb);

        c.TypeResolver.ComputeExpressionTypes();
        OutputTypeResolverDetails(tr, sb);

        c.TypeResolver.ComputeExpressionTypes();
        OutputTypeResolverDetails(tr, sb);

        OutputCandidates(tr, sb);

        return sb;
    }
    */

    public static StringBuilder OutputCandidates(TypeResolver tr, StringBuilder sb)
    {
        foreach (var p in tr.Parameters)
        {
            if (tr.GetType(p) != null)
                continue;
            
            var candidates = tr.GetCandidateTypes(p).ToList();
            sb.AppendLine($"Candidates for {p}");
            foreach (var c in candidates)
                sb.AppendLine($" {c}");
            var constraints = tr.GetParameterConstraints(p).ToList();
            sb.AppendLine($"Constraints for {p}");
            foreach (var c in constraints)
                sb.AppendLine($" {c}");
        }

        return sb;
    }
}