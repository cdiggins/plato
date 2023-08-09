namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public IDE IDE { get; }
        
        public Form1()
        {
            InitializeComponent();
            IDE = new IDE(tabControl1, richTextBoxOutput);
        }
    }
}