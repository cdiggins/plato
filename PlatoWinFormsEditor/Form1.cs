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

            var types = IDE.Compilation.AstTree.GetAllTypes().ToList();

            foreach (var type in types)
            {
                //richTextBoxOutput.AppendText($"{type.Kind} {type.Name.Text} {Environment.NewLine}");
            }

            var lookup = new TypeNames(types);
            foreach (var kv in lookup.Dictionary)
            {
                richTextBoxOutput.AppendText($"{kv.Key}");
                richTextBoxOutput.AppendText(Environment.NewLine);

                var memberNames = kv.Value;
                
                foreach (var kv2 in memberNames.Members)
                {
                    richTextBoxOutput.AppendText($"{kv2.Key} = {kv2.Value.GetType().Name}");
                    richTextBoxOutput.AppendText(Environment.NewLine);
                }
            }
        }
        
    }
}