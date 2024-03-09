namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public IDE IDE { get; }
        
        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            IDE = new IDE(mainTabControl, mainOutputBox);
            
        }
    }
}