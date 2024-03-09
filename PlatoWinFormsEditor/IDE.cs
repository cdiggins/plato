using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Parakeet;
using Ara3D.Parakeet.Grammars;
using Ara3D.Services;
using Plato.AST;
using Plato.Compiler;
using Logger = Plato.Compiler.Logger;

namespace PlatoWinFormsEditor;

public class IDE
{
    public List<Editor> Editors { get; } = new List<Editor>();
    public Compilation Compilation { get; }   
    public ILogger Logger { get; } 
    public TabControl TabControl { get; }
    public RichTextBox LoggingOutputEditor { get; }

    public static void InitTextBox(RichTextBox edit)
    {
        edit.WordWrap = false;
        edit.Font = new Font("Lucida Console", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
        edit.Dock = DockStyle.Fill;
        edit.CreateControl();
    }

    public Editor OpenFile(string filePath)
    {
        var tabPage = new TabPage(Path.GetFileName(filePath));
        TabControl.TabPages.Add(tabPage);

        var splitPanel = new SplitContainer();
        tabPage.Controls.Add(splitPanel);
        splitPanel.Dock = DockStyle.Fill;

        var editInput = new RichTextBox();
        splitPanel.Panel1.Controls.Add(editInput);
        InitTextBox(editInput);

        var editOutput = new RichTextBox();
        splitPanel.Panel2.Controls.Add(editOutput);
        InitTextBox(editOutput);

        var editor = new Editor(filePath, editInput, Logger);
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

        // TODO: Splitter distance 

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
    }
}