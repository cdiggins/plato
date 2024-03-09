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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            richTextBoxLog = new RichTextBox();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            tabPage6 = new TabPage();
            splitContainer1 = new SplitContainer();
            listBoxFiles = new ListBox();
            richTextBoxErrors = new RichTextBox();
            richTextBoxCST = new RichTextBox();
            richTextBoxAst = new RichTextBox();
            richTextBoxAnalysis = new RichTextBox();
            tabControlEditors = new TabControl();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
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
            mainSplitContainer.Panel1.Controls.Add(splitContainer1);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(tabControl1);
            mainSplitContainer.Size = new Size(800, 450);
            mainSplitContainer.SplitterDistance = 473;
            mainSplitContainer.TabIndex = 1;
            mainSplitContainer.SplitterMoved += mainSplitContainer_SplitterMoved;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(323, 450);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(richTextBoxLog);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(315, 417);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Log";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(3, 3);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(309, 411);
            richTextBoxLog.TabIndex = 1;
            richTextBoxLog.Text = "";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(richTextBoxErrors);
            tabPage3.Location = new Point(4, 29);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(315, 417);
            tabPage3.TabIndex = 1;
            tabPage3.Text = "Errors";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(richTextBoxCST);
            tabPage4.Location = new Point(4, 29);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(315, 417);
            tabPage4.TabIndex = 2;
            tabPage4.Text = "CST";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(richTextBoxAst);
            tabPage5.Location = new Point(4, 29);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(315, 417);
            tabPage5.TabIndex = 3;
            tabPage5.Text = "AST";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            tabPage6.Controls.Add(richTextBoxAnalysis);
            tabPage6.Location = new Point(4, 29);
            tabPage6.Name = "tabPage6";
            tabPage6.Size = new Size(315, 417);
            tabPage6.TabIndex = 4;
            tabPage6.Text = "Analysis";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listBoxFiles);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControlEditors);
            splitContainer1.Size = new Size(473, 450);
            splitContainer1.SplitterDistance = 157;
            splitContainer1.TabIndex = 0;
            // 
            // listBoxFiles
            // 
            listBoxFiles.Dock = DockStyle.Fill;
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.ItemHeight = 20;
            listBoxFiles.Location = new Point(0, 0);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(157, 450);
            listBoxFiles.TabIndex = 0;
            // 
            // richTextBoxErrors
            // 
            richTextBoxErrors.Dock = DockStyle.Fill;
            richTextBoxErrors.Location = new Point(3, 3);
            richTextBoxErrors.Name = "richTextBoxErrors";
            richTextBoxErrors.Size = new Size(309, 411);
            richTextBoxErrors.TabIndex = 2;
            richTextBoxErrors.Text = "";
            // 
            // richTextBoxCST
            // 
            richTextBoxCST.Dock = DockStyle.Fill;
            richTextBoxCST.Location = new Point(0, 0);
            richTextBoxCST.Name = "richTextBoxCST";
            richTextBoxCST.Size = new Size(315, 417);
            richTextBoxCST.TabIndex = 2;
            richTextBoxCST.Text = "";
            // 
            // richTextBoxAst
            // 
            richTextBoxAst.Dock = DockStyle.Fill;
            richTextBoxAst.Location = new Point(0, 0);
            richTextBoxAst.Name = "richTextBoxAst";
            richTextBoxAst.Size = new Size(315, 417);
            richTextBoxAst.TabIndex = 2;
            richTextBoxAst.Text = "";
            // 
            // richTextBoxAnalysis
            // 
            richTextBoxAnalysis.Dock = DockStyle.Fill;
            richTextBoxAnalysis.Location = new Point(0, 0);
            richTextBoxAnalysis.Name = "richTextBoxAnalysis";
            richTextBoxAnalysis.Size = new Size(315, 417);
            richTextBoxAnalysis.TabIndex = 2;
            richTextBoxAnalysis.Text = "";
            // 
            // tabControlEditors
            // 
            tabControlEditors.Dock = DockStyle.Fill;
            tabControlEditors.Location = new Point(0, 0);
            tabControlEditors.Name = "tabControlEditors";
            tabControlEditors.SelectedIndex = 0;
            tabControlEditors.Size = new Size(312, 450);
            tabControlEditors.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(mainSplitContainer);
            Name = "Form1";
            Text = "Plato - Code Viewer";
            Resize += Form1_Resize;
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            tabPage6.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabPage tabPage2;
        private SplitContainer mainSplitContainer;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private RichTextBox richTextBoxLog;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private SplitContainer splitContainer1;
        private ListBox listBoxFiles;
        private RichTextBox richTextBoxEdit;
        private RichTextBox richTextBoxErrors;
        private RichTextBox richTextBoxCST;
        private RichTextBox richTextBoxAst;
        private RichTextBox richTextBoxAnalysis;
        private TabControl tabControlEditors;
    }
}