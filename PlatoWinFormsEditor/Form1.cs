using System.Text.RegularExpressions;
using Ara3D.Logging;
using Ara3D.Utils;
using System.Windows.Forms;
using System;

namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public IDE IDE { get; }
        public double SplitterRatio { get; private set; }

        public ILogger Logger { get; }

        public Form1()
        {
            InitializeComponent();
            ComputeSplitterRatio();

            // TODO: maximize the window (the below does not work I thnik)
            //this.WindowState = FormWindowState.Maximized;

            // TODO: create the logger. 
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
            AddHandlers(richTextBoxAst);
            AddHandlers(richTextBoxCst);
            AddHandlers(richTextBoxErrors);
        }

        public static Regex LineNumRegex = new Regex(@"Ln\:\s*(\d+)");
        public static Regex RangeRegex = new Regex(@"\((\d+),(\d+),(\d+),(\d+)\)");

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
                    match = RangeRegex.Match(lineText);
                    if (match.Success)
                    {
                        Verifier.Assert(match.Groups.Count >= 5);
                        if (!int.TryParse(match.Groups[1].Value, out var beginLine)) return;
                        if (!int.TryParse(match.Groups[2].Value, out var beginCol)) return;
                        if (!int.TryParse(match.Groups[3].Value, out var endLine)) return;
                        if (!int.TryParse(match.Groups[4].Value, out var endCol)) return;
                        GotoLine(beginLine, beginCol, endLine, endCol);
                    }
                }
            };
        }

        public Editor CurrentEditor
            => tabControlEditors.SelectedTab?.Tag as Editor;

        public RichTextBox CurrentEditBox
            => CurrentEditor?.InputEditor;

        public void GotoLine(int beginLine, int beginCol, int endLine, int endCol)
        {
            // TODO: there is some stuff here that could go into a WinForms utility
            var editBox = CurrentEditBox;
            if (editBox == null) return;
            if (beginLine < 0 || beginLine >= editBox.Lines.Length) return;
            if (endLine < 0 || endLine >= editBox.Lines.Length) return;
            var beginIndex = editBox.GetFirstCharIndexFromLine(beginLine) + beginCol;
            var endIndex = editBox.GetFirstCharIndexFromLine(endLine) + endCol;
            editBox.SelectionStart = beginIndex;
            editBox.SelectionLength = endIndex - beginIndex;
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
        }

        public void OnLogMsg(string msg)
        {
            richTextBoxConsole.AppendText(msg + Environment.NewLine);
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
            ProcessUtil.OpenUrl("https://github.com");
        }
    }
}