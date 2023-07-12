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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBoxInput = new System.Windows.Forms.RichTextBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.richTextBoxParseTree = new System.Windows.Forms.RichTextBox();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.richTextBoxCst = new System.Windows.Forms.RichTextBox();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.richTextBoxAst = new System.Windows.Forms.RichTextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.richTextBoxJavaScript = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl2);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(400, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBoxInput);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(392, 417);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Source";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBoxInput
            // 
            this.richTextBoxInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxInput.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxInput.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxInput.Name = "richTextBoxInput";
            this.richTextBoxInput.Size = new System.Drawing.Size(386, 411);
            this.richTextBoxInput.TabIndex = 0;
            this.richTextBoxInput.Text = "";
            this.richTextBoxInput.WordWrap = false;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.richTextBoxParseTree);
            this.tabPage6.Location = new System.Drawing.Point(4, 29);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(392, 417);
            this.tabPage6.TabIndex = 1;
            this.tabPage6.Text = "ParseTree";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // richTextBoxParseTree
            // 
            this.richTextBoxParseTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxParseTree.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxParseTree.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxParseTree.Name = "richTextBoxParseTree";
            this.richTextBoxParseTree.Size = new System.Drawing.Size(386, 411);
            this.richTextBoxParseTree.TabIndex = 1;
            this.richTextBoxParseTree.Text = "";
            this.richTextBoxParseTree.WordWrap = false;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.richTextBoxCst);
            this.tabPage7.Location = new System.Drawing.Point(4, 29);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(392, 417);
            this.tabPage7.TabIndex = 2;
            this.tabPage7.Text = "CST";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // richTextBoxCst
            // 
            this.richTextBoxCst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxCst.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxCst.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxCst.Name = "richTextBoxCst";
            this.richTextBoxCst.Size = new System.Drawing.Size(386, 411);
            this.richTextBoxCst.TabIndex = 1;
            this.richTextBoxCst.Text = "";
            this.richTextBoxCst.WordWrap = false;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage8);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(396, 450);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.richTextBoxAst);
            this.tabPage8.Location = new System.Drawing.Point(4, 29);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(388, 417);
            this.tabPage8.TabIndex = 4;
            this.tabPage8.Text = "AST";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // richTextBoxAst
            // 
            this.richTextBoxAst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxAst.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxAst.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxAst.Name = "richTextBoxAst";
            this.richTextBoxAst.Size = new System.Drawing.Size(382, 411);
            this.richTextBoxAst.TabIndex = 1;
            this.richTextBoxAst.Text = "";
            this.richTextBoxAst.WordWrap = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.richTextBoxJavaScript);
            this.tabPage4.Location = new System.Drawing.Point(4, 29);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(388, 417);
            this.tabPage4.TabIndex = 2;
            this.tabPage4.Text = "JavaScript";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // richTextBoxJavaScript
            // 
            this.richTextBoxJavaScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxJavaScript.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxJavaScript.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxJavaScript.Name = "richTextBoxJavaScript";
            this.richTextBoxJavaScript.Size = new System.Drawing.Size(382, 411);
            this.richTextBoxJavaScript.TabIndex = 1;
            this.richTextBoxJavaScript.Text = "";
            this.richTextBoxJavaScript.WordWrap = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.richTextBoxOutput);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(388, 417);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Errors";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBoxOutput
            // 
            this.richTextBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxOutput.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBoxOutput.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxOutput.Name = "richTextBoxOutput";
            this.richTextBoxOutput.Size = new System.Drawing.Size(382, 411);
            this.richTextBoxOutput.TabIndex = 0;
            this.richTextBoxOutput.Text = "";
            this.richTextBoxOutput.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabControl tabControl2;
        private TabPage tabPage3;
        private RichTextBox richTextBoxInput;
        private RichTextBox richTextBoxOutput;
        private TabPage tabPage4;
        private RichTextBox richTextBoxJavaScript;
        private TabPage tabPage6;
        private RichTextBox richTextBoxParseTree;
        private TabPage tabPage7;
        private RichTextBox richTextBoxCst;
        private TabPage tabPage8;
        private RichTextBox richTextBoxAst;
    }
}