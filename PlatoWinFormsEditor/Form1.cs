using Parakeet;
using Parakeet.Demos;

namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var inputFile = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\math.funcs.plato.cs";
            inputEdit.Text = File.ReadAllText(inputFile);
            Compile(inputEdit.Text);
        }

        public CSharpGrammar Grammar = new();

        public void OutputParseErrors(ParserState state)
        {
            if (state == null)
            {
                outputEdit.Text = "Failed to parse input";
                return;
            }

            var curError = 0;
            for (var e = state.LastError; e != null; e = e.Previous)
            {
                outputEdit.AppendText($"Error {curError} at {e.LastState} failed expected rule {e.Expected}, parent state is {e.ParentState}, message is {e.Message}");
                outputEdit.AppendText(Environment.NewLine);
                outputEdit.AppendText(e.LastState.CurrentLine);
                outputEdit.AppendText(Environment.NewLine);
                outputEdit.AppendText(e.LastState.Indicator);
                outputEdit.AppendText(Environment.NewLine);
            }
        }

        public Compilation Compile(string input)
        {
            var c = new Compilation(input, Grammar.File);

            outputEdit.Clear();
            
            if (c.State != null)
            {
                OutputParseErrors(c.State);
            }
        }

        public void OutputCST(ParserTree tree)
        {
            richTextBoxCst.Clear();
            OutputCST(cst);
        }

        public void OutputCST(CstNode node, string indent = "")
        {
            var xs = node.GetType().Name;
            if (node.IsLeaf)
            {
                richTextBoxCst.AppendText($"{indent}<{xs}>{node.GetText()}</{xs}>");
            }
            else
            {
                richTextBoxCst.AppendText($"{indent}<{xs}>");
                foreach (var child in node.Children)
                    OutputCST(child, indent + "  ");
                richTextBoxCst.AppendText($"{indent}</{xs}>");
            }
        }

        public void OutputParseTree(ParserState state)
        {
            var tree = state.Node.ToParseTree();
            richTextBoxParseTree.Text = tree.ToString();
            OutputCST(tree);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}