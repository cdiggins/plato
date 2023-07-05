using System.Diagnostics;
using System.Text;
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
            var inputFile1 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\math.types.plato.cs";
            var inputFile2 = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\math.funcs.plato.cs";
            var input1 = File.ReadAllText(inputFile1);
            var input2 = File.ReadAllText(inputFile2);
            inputEdit.Text = input1 + Environment.NewLine + input2;
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
            //richTextBoxCSharp.Text = Try(() => c.AstTree.ToCSharp());
            richTextBoxJavaScript.Text = Try(() => c.AstTree.ToJavaScript());

            richTextBoxOutput.AppendText(Try(() => AstDeclarations(c.AstTree)));
            return c;
        }

        public static string AstDeclarations(AstNode node)
        {
            var ns = node as AstNamespace;
            Debug.Assert(ns != null);
            var dl = new DeclarationLookup(ns);
            var sb = new StringBuilder();

            /*
            sb.AppendLine("Declarations");
            foreach (var pair in dl.GetDeclarations())
            {
                sb.AppendLine($"{pair.Item1} = {pair.Item2}");
            }
            */

            sb.AppendLine("Operations");
            var ops = Operations.Create(ns.GetAllTypes());
            foreach (var kv in ops.OperationsFromTypes)
            {
                foreach (var m in kv.Value)
                {
                    sb.AppendLine($"{kv.Key} has {m.Name.Text}");
                }
            }

            return sb.ToString();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}