using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using Plato.Compiler;
using Ptarmigan.Utils;

namespace PlatoWinFormsEditor;

public class IDE
{
    public string Input { get; set; }
    public string Output { get; set; }
    public Parser Parser { get; set; }

    public TabControl TabControl { get; }
    public RichTextBox OutputTextBox { get; }
    public List<Editor> Editors { get; } = new();
    public Compiler Compiler { get; }
    public Logger Logger { get; } = new Logger();

    public void OpenFile(string filePath)
    {
        var tabPage = new TabPage(Path.GetFileName(filePath));
        TabControl.TabPages.Add(tabPage);
        var edit = new RichTextBox();
        edit.Font = OutputTextBox.Font;
        edit.WordWrap = false;
        edit.Dock = DockStyle.Fill;
        tabPage.Controls.Add(edit);

        // This strange sequence of calls is required to force the editor to do what it does
        // Replace the newlines (usually /r/n) with a single character (/n).
        // Without creating control, and making a small change, the character position offsets will be wrong. 
        // https://stackoverflow.com/a/7070915/184528
        edit.CreateControl();
        edit.Text = File.ReadAllText(filePath);
        edit.AppendText(Environment.NewLine);

        var input = new ParserInput(edit.Text, filePath);
        var parser = new Parser(
            filePath,
            input,
            PlatoGrammar.Instance.File,
            PlatoTokenGrammar.Instance.Tokenizer,
            Compiler);

        var editor = new Editor(filePath, edit, OutputTextBox, parser);
        Editors.Add(editor);
    }

    public void Parse()
    {
        foreach (var editor in Editors)
            editor.Parse();
    }

    public IDE(TabControl tabControl, RichTextBox outputTextBox)
    {
        Compiler = new Compiler(new CstNodeFactory(), new AstNodeFactory(), Logger);

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
        var parsers = Editors.Select(p => p.Parser).ToList();
        Compiler.Compile(parsers);

        OutputTextBox.Lines = Logger.Messages.ToArray();

        var outputFolder = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        File.WriteAllText(Path.Combine(outputFolder, "output.cs"), Compiler.ToCSharp());
        File.WriteAllText(Path.Combine(outputFolder, "output.plato.html"), Compiler.ToPlatoHtml());

        var inputFolder = outputFolder;
        var prologue = File.ReadAllText(Path.Combine(inputFolder, "prologue.js"));
        var epilogue = File.ReadAllText(Path.Combine(inputFolder, "epilogue.js"));
        var output = prologue 
                     + Environment.NewLine
                     + Compiler.ToJavaScript() 
                     + Environment.NewLine 
                     + epilogue;
        File.WriteAllText(Path.Combine(outputFolder, "output.js"), output);

        var vsgFolder = Path.Combine(outputFolder, "vsg");
        // TODO: clear the directory
        
        FileUtil.CreateAndClearDirectory(vsgFolder);
        var i = 0;
        foreach (var f in Compiler.Graphs)
        {
            var text = JsonSerializer.Serialize(f, new JsonSerializerOptions() { WriteIndented = true });
            var filePath = Path.Combine(vsgFolder, $"{f.Name}_{i++}.json");
            File.WriteAllText(filePath, text);
        }

        //Output += GetConstraintsOutput();
        //Output += GetOperationsOutput();
        //Output += GetTypeGuesserOutput();
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