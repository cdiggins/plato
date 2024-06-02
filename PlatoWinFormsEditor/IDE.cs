using Ara3D.Logging;
using Ara3D.Utils;
using Plato.Compiler;
using Plato.CSharpWriter;

namespace PlatoWinFormsEditor;

public class IDE
{
    public List<Editor> Editors { get; } = new List<Editor>();
    public Compilation Compilation { get; }   
    public ILogger Logger { get; } 
    public TabControl TabControl { get; }
    public RichTextBox LoggingOutputEditor { get; }
    public Action<Editor> EditorChanged { get; }

    public string SymbolErrorsString { get; }
    public string SymbolsString { get; }
    public string TypesString { get; }
    public string SemanticDiagnosticsString { get; }

    public static void InitTextBox(RichTextBox edit)
    {
        edit.WordWrap = false;
        edit.Font = new Font("Consolas", 8F, FontStyle.Regular, GraphicsUnit.Point);
        edit.Dock = DockStyle.Fill;
        edit.CreateControl();
    }

    public Editor OpenFile(string filePath)
    {
        var tabPage = new TabPage(Path.GetFileName(filePath));
        TabControl.TabPages.Add(tabPage);
        
        var editInput = new RichTextBox();
        InitTextBox(editInput);
        tabPage.Controls.Add(editInput);
        Logger.Log($"Parsing {filePath}");
        var editor = new Editor(filePath, editInput, Logger);
        var nErrors = editor.Parser.ParserErrors.Count;
        var status = editor.Parser.Succeeded ? "Succeed" : nErrors > 0 ? $"{nErrors} Errors" : "Unknown Error";
        Logger.Log($"Parsing status: {status}");
        tabPage.Tag = editor;
        Editors.Add(editor);
        return editor;
    }

    public IDE(TabControl tabControl, ILogger logger)
    {
        TabControl = tabControl;
        Logger = logger;

        var inputFolder = SourceCodeLocation.GetFolder()
            .RelativeFolder("..", "PlatoStandardLibrary");

        Logger.Log("Opening files");

        var files = inputFolder.GetFiles("*.plato");
        foreach (var file in files)
            OpenFile(file);

        var parsingSuccessful = Editors.All(e => e.Parser.Succeeded);

        if (!parsingSuccessful)
        {
            Logger.Log("Parsing failed for one of the input files, halting");
            return;
        }
        
        Logger.Log("Parsing succeeded for all files");
        Logger.Log("Compiling");
        var trees = Editors.Select(e => e.Ast);
        Compilation = new Compilation(Logger, trees);
        
        if (!Compilation.CompletedCompilation)
        {
            Logger.Log("Compilation was not completed");
        }

        TypesString = Compilation.TypeDefinitions.Select(td => $"{td.Kind} {td.Name}").JoinStringsWithNewLine();
        SymbolsString = Compilation.Symbols.Select(sym => $"{sym.Name} {sym.Id}").JoinStringsWithNewLine();
        SymbolErrorsString = Compilation.SymbolFactory.Errors.Select(e => $"{e.Message} at {e.Node}").JoinStringsWithNewLine();
        SemanticDiagnosticsString = Compilation.Diagnostics.JoinStringsWithNewLine();

        //Compilation.OutputFunctionAnalysis();
        Compilation.OutputFunctionCallAnalysis();

        if (Compilation.CompletedCompilation)
        {
            var currentFolder = PathUtil.GetCallerSourceFolder();
            var outputFolder = currentFolder.RelativeFolder("..", "PlatoOutput");
            logger.Log("Writing C#");
            var output = Compilation.ToCSharp(outputFolder);
            foreach (var kv in output.Files)
            {
                var fp = new FilePath(kv.Key);
                fp.WriteAllText(kv.Value.ToString());
            }
        }
    }
}