using Ara3D.Logging;

namespace PlatoWinFormsEditor
{
    public partial class Form1 : Form
    {
        public IDE IDE { get; }
        public double SplitterRatio { get; private set; }

        public ILogger Logger { get; }

        public Form1()
        {
            InitializeComponent();
            ComputeSplitterRatio();
            
            // TODO: maximize the window (the below does not work I thnik)
            //this.WindowState = FormWindowState.Maximized;
            
            // TODO: create the logger. 
            Logger = Logger.Create("Log", OnLogMsg);

            IDE = new IDE(tabControlEditors, Logger);
        }

        public void OnLogMsg(string msg)
        {
            richTextBoxLog.AppendText(msg + Environment.NewLine);
        }

        private void ComputeSplitterRatio()
        {
            SplitterRatio = mainSplitContainer.SplitterDistance / (double)mainSplitContainer.Width ;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            mainSplitContainer.SplitterDistance = (int)(mainSplitContainer.Width * SplitterRatio);
        }

        private void mainSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ComputeSplitterRatio();
        }
    }
}