using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.CSharp;
using PlatoAst;

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
                richTextBoxOutput.Text = "Failed to parse input";
                return;
            }

            var curError = 0;
            for (var e = state.LastError; e != null; e = e.Previous)
            {
                richTextBoxOutput.AppendText($"Error {curError} at {e.LastState} failed expected rule {e.Expected}, parent state is {e.ParentState}, message is {e.Message}");
                richTextBoxOutput.AppendText(Environment.NewLine);
                richTextBoxOutput.AppendText(e.LastState.CurrentLine);
                richTextBoxOutput.AppendText(Environment.NewLine);
                richTextBoxOutput.AppendText(e.LastState.Indicator);
                richTextBoxOutput.AppendText(Environment.NewLine);
            }
        }

        public string Try(Func<string> f)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                return e.Message;
            };
        }

        public Compilation Compile(string input)
        {
            var c = new Compilation(input, Grammar.File, CstNodeFactory.Create, cst => new AstFromCst().ToAst(cst));

            richTextBoxOutput.Clear();

            richTextBoxOutput.AppendText(c.Success ? "Compilation Success" : "Compilation Failure");
            if (c.State != null)
            {
                OutputParseErrors(c.State);
            }
            richTextBoxOutput.AppendText(c.Message);

            richTextBoxParseTree.Text = Try(() => c.ParseTree?.ToString());
            richTextBoxCst.Text = Try(() => c.CstTree?.ToXml().ToString());
            richTextBoxAst.Text = Try(() => c.AstTree.ToXml());
            richTextBoxCSharp.Text = Try(() => c.AstTree.ToCSharp());
            richTextBoxJavaScript.Text = Try(() => c.AstTree.ToJavaScript());
            richTextBoxPail.Text = Try(() => c.AstTree.ToPail());
            return c;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}