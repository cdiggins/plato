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
        public IDE IDE = new IDE();
        
        public Form1()
        {
            InitializeComponent();
            richTextBoxInput.Text = IDE.Input;
            richTextBoxOutput.Text = IDE.Output;
            richTextBoxParseTree.Text = IDE.ParseTree;
            richTextBoxCst.Text = IDE.CstXml;
            richTextBoxAst.Text = IDE.AstXml;
            richTextBoxJavaScript.Text = IDE.JavaScriptAst;
            richTextBoxAbstractValue.Text = IDE.AbstractValuesXml;
        }
    }
}