using Plato.Compiler;

namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public IDE IDE = new IDE();
        
        public Form1()
        {
            InitializeComponent();
            richTextBoxInput.Text = IDE.Input;
            richTextBoxOutput.Text = IDE.Output;
            richTextBoxParseTree.Text = IDE.ParseTree;
            richTextBoxCst.Text = IDE.CstXml;
            richTextBoxAst.Text = IDE.AstXml;
            richTextBoxJavaScript.Text = IDE.Compilation.ToJavaScript();
            richTextBoxSymbols.Text = IDE.AbstractValuesXml;
        }
    }
}