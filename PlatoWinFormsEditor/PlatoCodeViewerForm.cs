using System.Text.RegularExpressions;
using Ara3D.Logging;
using Ara3D.Utils;
using System.Windows.Forms;
using System;

namespace PlatoWinFormsEditor
{
    public partial class PlatoCodeViewerForm : Form
    {
        public IDE IDE { get; }
#pragma warning disable WFO1000
        public double SplitterRatio { get; private set; }
#pragma warning restore WFO1000

        public ILogger Logger { get; }

        public PlatoCodeViewerForm()
        {
            InitializeComponent();
            ComputeSplitterRatio();

            // TODO: maximize the window (the below does not work I thnik)
            //this.WindowState = FormWindowState.Maximized;

            Logger = Logger.Create("Log", OnLogMsg);

            IDE = new IDE(tabControlEditors, Logger);

            foreach (TabPage tab in tabControlEditors.TabPages)
            {
                var editor = tab.Tag as Editor;
                if (editor == null)
                    throw new NotImplementedException();
                var name = $"{editor.BaseFileName}";
                var nErrors = editor.Parser.ParserErrors.Count;
                if (editor.Parser.Succeeded)
                    name += " (Success)";
                else if (nErrors > 0)
                    name += $" ({nErrors} Errors)";
                else
                    name += $" (Failed)";

                listBoxFiles.Items.Add(name);
            }

            tabControlEditors.Selected += TabControlEditors_Selected;
            EditorChanged(CurrentEditor);
            
            AddHandlers(richTextBoxLog);
            AddHandlers(richTextBoxConsole);
            AddHandlers(richTextBoxTokens);
            AddHandlers(richTextBoxParseTree);
            AddHandlers(richTextBoxNodes);
            AddHandlers(richTextBoxAst);
            AddHandlers(richTextBoxCst);
            AddHandlers(richTextBoxErrors);
            AddHandlers(richTextBoxSymbols);
            AddHandlers(richTextBoxSymbolErrors);
            AddHandlers(richTextBoxTypes);

            OnCompilationCompleted();
        }

        public static Regex LineNumRegex = new Regex(@"Line\:\s*(\d+)");
        
        public void AddHandlers(RichTextBox textBox)
        {
            textBox.DetectUrls = false;
            textBox.WordWrap = false;
            textBox.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // TODO: there is some stuff here that go into a WinForms utility
                    var p = textBox.GetCharIndexFromPosition(e.Location);
                    var lineIndex = textBox.GetLineFromCharIndex(p);
                    if (lineIndex < 0) return;
                    var lineText = textBox.Lines[lineIndex];
                    var match = LineNumRegex.Match(lineText);
                    if (match.Success)
                    {
                        Verifier.Assert(match.Groups.Count >= 2);
                        if (int.TryParse(match.Groups[1].Value, out var parseLineIndex))
                        {
                            GotoLine(parseLineIndex);
                        }
                            
                        return;
                    }

                    var f = FileAndRange.Parse(lineText);
                    if (f != null)
                    {
                        GotoEditor(f.FilePath);
                        GotoPosition(f.StartIndex, f.EndIndex);
                    }
                }
            };
        }

        public void GotoEditor(FilePath fp)
        {
            if (fp == null) return;
            if (!fp.Exists()) return;
            foreach (TabPage tab in tabControlEditors.TabPages)
            {
                var editor = tab.Tag as Editor;
                if (editor == null)
                    return;
                if (editor.FilePath.IsSameFile(fp))
                {
                    tabControlEditors.SelectedTab = tab;
                }
            }
        }

        public Editor CurrentEditor
            => tabControlEditors.SelectedTab?.Tag as Editor;

        public RichTextBox CurrentEditBox
            => CurrentEditor?.InputEditor;

        public void GotoPosition(int startIndex, int endIndex)
        {
            // TODO: there is some stuff here that could go into a WinForms utility
            var editBox = CurrentEditBox;
            if (editBox == null) return;
            if (endIndex < startIndex) endIndex = startIndex;
            editBox.SelectionStart = startIndex;
            editBox.SelectionLength = endIndex - startIndex;
            editBox.Focus();
            editBox.ScrollToCaret();

        }

        public void GotoLine(int lineIndex)
        {
            // TODO: there is some stuff here that could go into a WinForms utility
            var editBox = CurrentEditBox;
            if (editBox == null) return;
            if (lineIndex < 0 || lineIndex >= editBox.Lines.Length) return;
            var index = editBox.GetFirstCharIndexFromLine(lineIndex);
            var line = editBox.Lines[lineIndex];
            editBox.SelectionStart = index;
            editBox.SelectionLength = line.Length;
            editBox.Focus();
            editBox.ScrollToCaret();
        }

        private void TabControlEditors_Selected(object? sender, TabControlEventArgs e)
        {
            EditorChanged(e.TabPage?.Tag as Editor);
            if (listBoxFiles.SelectedIndex != e.TabPageIndex)
                listBoxFiles.SelectedItem = e.TabPage;
        }

        private void EditorChanged(Editor? editor)
        {
            richTextBoxLog.Text = editor?.LogString ?? "";
            richTextBoxAst.Text = editor?.AstString ?? "";
            richTextBoxCst.Text = editor?.CstString ?? "";
            richTextBoxErrors.Text = editor?.ErrorsString ?? "";
            richTextBoxTokens.Text = editor?.TokensString ?? "";
            richTextBoxParseTree.Text = editor?.ParseTreeString ?? "";
            richTextBoxNodes.Text = editor?.ParseNodesString ?? "";
        }

        private void OnCompilationCompleted()
        {
            richTextBoxSymbols.Text = IDE?.SymbolsString ?? "";
            richTextBoxSymbolErrors.Text = IDE?.SymbolErrorsString ?? "";
            richTextBoxTypes.Text = IDE?.TypesString ?? "";
            richTextBoxSemantics.Text = IDE?.SemanticDiagnosticsString ?? "";
        }

        public void OnLogMsg(string msg)
        {
            var start = richTextBoxConsole.TextLength;
            richTextBoxConsole.AppendText(msg);
            var end = richTextBoxConsole.TextLength;
            if (msg.ToLowerInvariant().Contains("error"))
            {
                richTextBoxConsole.Select(start, end - start);
                richTextBoxConsole.SelectionColor = Color.Brown;
            }
            richTextBoxConsole.AppendText(Environment.NewLine);
            
        }

        private void ComputeSplitterRatio()
        {
            SplitterRatio = mainSplitContainer.SplitterDistance / (double)mainSplitContainer.Width;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //mainSplitContainer.SplitterDistance = (int)(mainSplitContainer.Width * SplitterRatio);
        }

        private void mainSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ComputeSplitterRatio();
        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var n = listBoxFiles.SelectedIndex;
            if (n < 0) return;
            tabControlEditors.SelectedTab = tabControlEditors.TabPages[n];
        }

        private void aboutPlatoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessUtil.OpenUrl("https://github.com/cdiggins/plato");
        }
    }
}