using Ara3D.Logging;

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
            mainSplitContainer.SplitterDistance = (int)(mainSplitContainer.Width * SplitterRatio);
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
    }
}