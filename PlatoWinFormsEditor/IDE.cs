using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ara3D.Utils;
using Ara3D.Parsing;
using Ara3D.Parsing.Grammars;
using Plato.AST;
using Plato.Compiler;
using Plato.CSharpWriter;
using Plato.Parser;
using Logger = Plato.Compiler.Logger;

namespace PlatoWinFormsEditor;

public class IDE
{
    public TabControl TabControl { get; }
    public RichTextBox OutputTextBox { get; }
    public List<Editor> Editors { get; } = new();
    public Compilation Compilation { get; }   
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
            Logger);

        var editor = new Editor(filePath, edit, OutputTextBox, parser);
        Editors.Add(editor);
    }

    public void ApplyStylesAndOutputErrors()
    {
        foreach (var editor in Editors)
            editor.ApplyStylesAndOutputErrors();
    }

    public IDE(TabControl tabControl, RichTextBox outputTextBox, [CallerFilePath]string filePath = null)
    {
        TabControl = tabControl;
        OutputTextBox = outputTextBox;

        OutputTextBox.WordWrap = false;
        OutputTextBox.MouseDoubleClick += (_, args) 
            => Debug.WriteLine($"Double clicked output. Mouse args = {args}. Selected text = {outputTextBox.SelectedText}");

        var currentFolder = Path.GetDirectoryName(filePath);
        var parentFolder = Path.Combine(currentFolder, ".."); 
        var inputFolder = Path.Combine(parentFolder, "PlatoStandardLibrary");

        Logger.Log("Opening files");

        OpenFile(Path.Combine(inputFolder, "intrinsics.plato"));
        OpenFile(Path.Combine(inputFolder, "concepts.plato"));
        OpenFile(Path.Combine(inputFolder, "types.plato"));
        OpenFile(Path.Combine(inputFolder, "libraries.plato"));

        Logger.Log("Applying syntax coloring");
        ApplyStylesAndOutputErrors();
        Logger.Log("Completed syntax coloring");

        Logger.Log("Gathering parsers");
        var parsers = Editors.Select(p => p.Parser).ToList();
        var parsingSuccess = parsers.All(p => p?.Success == true);
        if (!parsingSuccess)
        {
            Logger.Log("Parsing was not successful");
            return;
        }

        Logger.Log("Creating AST trees");
        var trees = new List<AstNode>();
        foreach (var p in parsers)
        {
            try
            {
                trees.Add(p.CstTree.ToAst());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred '{ex.Message}' when generating AST from CST for {p.Input.File}");
                return;
            }
        }
        Compilation = new Compilation(Logger, trees);

        OutputTextBox.Lines = Logger.Messages.ToArray();
        var logFile = Path.Combine(inputFolder, "log.txt");
        File.WriteAllLines(logFile, Logger.Messages);

        if (!Compilation.CompletedCompilation)
        {
            Logger.Log("Compilation was not completed");
        }
    }
    
}