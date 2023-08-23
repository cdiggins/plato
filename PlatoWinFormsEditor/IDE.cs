using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using Plato.Compiler;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using Ptarmigan.Utils;

namespace PlatoWinFormsEditor;

public class IDE
{
    public TabControl TabControl { get; }
    public RichTextBox OutputTextBox { get; }
    public List<Editor> Editors { get; } = new();
    public Compiler Compiler { get; }
    public Logger Logger { get; } = new();

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

    public void ApplyStylesAndOutputErrors()
    {
        foreach (var editor in Editors)
            editor.ApplyStylesAndOutputErrors();
    }

    public IDE(TabControl tabControl, RichTextBox outputTextBox)
    {
        Compiler = new Compiler(new CstNodeFactory(), new AstNodeFactory(), Logger);

        TabControl = tabControl;
        OutputTextBox = outputTextBox;

        OutputTextBox.WordWrap = false;
        OutputTextBox.MouseDoubleClick += (sender, args) => Debug.WriteLine($"Double clicked output. Mouse args = {args}. Selected text = {outputTextBox.SelectedText}");

        var inputPath = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        Logger.Log("Opening files");

        OpenFile(Path.Combine(inputPath, "intrinsics.plato"));
        OpenFile(Path.Combine(inputPath, "concepts.plato"));
        OpenFile(Path.Combine(inputPath, "types.plato"));
        OpenFile(Path.Combine(inputPath, "libraries.plato"));

        Logger.Log("Applying syntax coloring");
        ApplyStylesAndOutputErrors();
        Logger.Log("Completed syntax coloring");
        var parsers = Editors.Select(p => p.Parser).ToList();
        Compiler.Compile(parsers);

        OutputTextBox.Lines = Logger.Messages.ToArray();


        if (!Compiler.CompletedCompilation)
        {
            // No files will be output
            return;
        }

        // NOTE: even if CompletedCompilation is true, there might still be errors. 
        // The goal is to still be able to partially generate output. 

        var outputFolder = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\";

        Logger.Log("Writing C#");
        File.WriteAllText(Path.Combine(outputFolder, "output.cs"), Compiler.ToCSharp());

        //Logger.Log("Writing HTML");
        //File.WriteAllText(Path.Combine(outputFolder, "output.plato.html"), Compiler.ToPlatoHtml());

        /*
        Logger.Log("Writing JavaScript");
        var inputFolder = outputFolder;
        var prologue = File.ReadAllText(Path.Combine(inputFolder, "prologue.js"));
        var epilogue = File.ReadAllText(Path.Combine(inputFolder, "epilogue.js"));
        var output = prologue 
                     + Environment.NewLine
                     + Compiler.ToJavaScript() 
                     + Environment.NewLine 
                     + epilogue;
        File.WriteAllText(Path.Combine(outputFolder, "output.js"), output);

        */

        /*
        var vsgFolder = Path.Combine(outputFolder, "vsg");
        FileUtil.CreateAndClearDirectory(vsgFolder);
        var i = 0;
        foreach (var f in Compiler.Graphs)
        {
            var text = JsonSerializer.Serialize(f, new JsonSerializerOptions() { WriteIndented = true });
            var filePath = Path.Combine(vsgFolder, $"{f.Name}_{i++}.json");
            File.WriteAllText(filePath, text);
        }
        */

        //Output += GetConstraintsOutput();
        //Output += GetOperationsOutput();
        //Output += GetTypeGuesserOutput();
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
}