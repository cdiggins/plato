namespace PlatoWinFormsEditor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainSplitContainer = new SplitContainer();
            mainTabControl = new TabControl();
            mainOutputBox = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new Point(0, 0);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(mainTabControl);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(mainOutputBox);
            mainSplitContainer.Size = new Size(800, 450);
            mainSplitContainer.SplitterDistance = 421;
            mainSplitContainer.TabIndex = 1;
            // 
            // mainTabControl
            // 
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Location = new Point(0, 0);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(421, 450);
            mainTabControl.TabIndex = 1;
            // 
            // mainOutputBox
            // 
            mainOutputBox.Dock = DockStyle.Fill;
            mainOutputBox.Location = new Point(0, 0);
            mainOutputBox.Name = "mainOutputBox";
            mainOutputBox.Size = new Size(375, 450);
            mainOutputBox.TabIndex = 0;
            mainOutputBox.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(mainSplitContainer);
            Name = "Form1";
            Text = "Form1";
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabPage tabPage2;
        private SplitContainer mainSplitContainer;
        private TabControl mainTabControl;
        private RichTextBox mainOutputBox;
    }
}