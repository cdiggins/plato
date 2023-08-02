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
        }
    }
}