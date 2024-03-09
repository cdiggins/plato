using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Parakeet;
using Ara3D.Parakeet.Grammars;
using Ara3D.Services;
using Plato.AST;
using Plato.Compiler;
using Plato.Parser;
using Logger = Plato.Compiler.Logger;

namespace PlatoWinFormsEditor;

public class IDE
{
    public List<Editor> Editors { get; } = new List<Editor>();
    public Compilation Compilation { get; }   
    public ILogger Logger { get; } 
    public TabControl TabControl { get; }
    public RichTextBox LoggingOutputEditor { get; }

    public ILogWriter CreateWriter()
        => LogWriter.Create(OnLogEntry);

    public void OnLogEntry(string msg)
        => LoggingOutputEditor.AppendText(msg + Environment.NewLine);

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

        var editInput = new RichTextBox();
        splitPanel.Panel1.Controls.Add(editInput);
        InitTextBox(editInput);

        var editOutput = new RichTextBox();
        splitPanel.Panel2.Controls.Add(editOutput);
        InitTextBox(editOutput);

        var editor = new Editor(filePath, editInput, editOutput, Logger);
        Editors.Add(editor);

        editor.ApplyStylesAndOutputErrors();

        return editor;
    }

    public IDE(TabControl tabControl, RichTextBox loggingOutputEditor)
    {
        TabControl = tabControl;
        LoggingOutputEditor = loggingOutputEditor;
        Logger = new Logger();

        var inputFolder = SourceCodeLocation.GetFolder()
            .RelativeFolder("..", "PlatoStandardLibrary");

        Logger.Log("Opening files");

        var files = inputFolder.GetFiles("*.plato");
        foreach (var file in files)
            OpenFile(file);

        // TODO: Splitter distance 

        var parsingSuccessful = true;
        foreach (var e in Editors)
        {
            if (e.Parser.Success)
            {
                Logger.Log($"Parsing passed: {e.BaseFileName}");
            }
            else
            {
                Logger.Log($"Parsing failed: {e.BaseFileName}");
                parsingSuccessful = false;
            }
        }

        if (!parsingSuccessful)
        {
            Logger.Log("Parsing failed overall");
            return;
        }

        Logger.Log("Compiling");
        var trees = Editors.Select(e => e.Ast);
        Compilation = new Compilation(Logger, trees);
        
        if (!Compilation.CompletedCompilation)
        {
            Logger.Log("Compilation was not completed");
        }
    }
}