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

        public void Compile(ParserInput input)
        {
            outputEdit.Clear();
            var rule = Grammar.File;
            
            ParserState ps = null;
            try
            {
                ps = input.Parse(rule);
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Parsing exception {pe.Message} occured at {pe.LastValidState} ");
            }

            if (ps != null)
            {
                OutputParseErrors(ps);
            }

            if (ps == null)
            {
                Console.WriteLine($"FAILED");
            }
            else if (ps.AtEnd())
            {
                Console.WriteLine($"PASSED");
            }
            else
            {
                Console.WriteLine($"PARTIAL PASSED: {ps.Position}/{ps.Input.Length}");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}