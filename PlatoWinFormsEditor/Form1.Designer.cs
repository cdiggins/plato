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
            splitContainer1 = new SplitContainer();
            listBoxFiles = new ListBox();
            tabControlEditors = new TabControl();
            TabControl = new TabControl();
            console = new TabPage();
            richTextBoxConsole = new RichTextBox();
            log = new TabPage();
            richTextBoxLog = new RichTextBox();
            errors = new TabPage();
            richTextBoxErrors = new RichTextBox();
            cst = new TabPage();
            richTextBoxCst = new RichTextBox();
            ast = new TabPage();
            richTextBoxAst = new RichTextBox();
            tokens = new TabPage();
            richTextBoxTokens = new RichTextBox();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            fileToolStripMenuItem1 = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            TabControl.SuspendLayout();
            console.SuspendLayout();
            log.SuspendLayout();
            errors.SuspendLayout();
            cst.SuspendLayout();
            ast.SuspendLayout();
            tokens.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new Point(0, 33);
            mainSplitContainer.Margin = new Padding(4, 4, 4, 4);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(splitContainer1);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(TabControl);
            mainSplitContainer.Size = new Size(1000, 529);
            mainSplitContainer.SplitterDistance = 591;
            mainSplitContainer.SplitterWidth = 5;
            mainSplitContainer.TabIndex = 1;
            mainSplitContainer.SplitterMoved += mainSplitContainer_SplitterMoved;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 4, 4, 4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listBoxFiles);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControlEditors);
            splitContainer1.Size = new Size(591, 529);
            splitContainer1.SplitterDistance = 196;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // listBoxFiles
            // 
            listBoxFiles.Dock = DockStyle.Fill;
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.ItemHeight = 25;
            listBoxFiles.Location = new Point(0, 0);
            listBoxFiles.Margin = new Padding(4, 4, 4, 4);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(196, 529);
            listBoxFiles.TabIndex = 0;
            listBoxFiles.SelectedIndexChanged += listBoxFiles_SelectedIndexChanged;
            // 
            // tabControlEditors
            // 
            tabControlEditors.Dock = DockStyle.Fill;
            tabControlEditors.Location = new Point(0, 0);
            tabControlEditors.Margin = new Padding(4, 4, 4, 4);
            tabControlEditors.Name = "tabControlEditors";
            tabControlEditors.SelectedIndex = 0;
            tabControlEditors.Size = new Size(390, 529);
            tabControlEditors.TabIndex = 0;
            // 
            // TabControl
            // 
            TabControl.Controls.Add(console);
            TabControl.Controls.Add(log);
            TabControl.Controls.Add(errors);
            TabControl.Controls.Add(cst);
            TabControl.Controls.Add(ast);
            TabControl.Controls.Add(tokens);
            TabControl.Dock = DockStyle.Fill;
            TabControl.Location = new Point(0, 0);
            TabControl.Margin = new Padding(4, 4, 4, 4);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(404, 529);
            TabControl.TabIndex = 1;
            // 
            // console
            // 
            console.Controls.Add(richTextBoxConsole);
            console.Location = new Point(4, 34);
            console.Margin = new Padding(4, 4, 4, 4);
            console.Name = "console";
            console.Padding = new Padding(4, 4, 4, 4);
            console.Size = new Size(396, 491);
            console.TabIndex = 0;
            console.Text = "Console";
            console.UseVisualStyleBackColor = true;
            // 
            // richTextBoxConsole
            // 
            richTextBoxConsole.Dock = DockStyle.Fill;
            richTextBoxConsole.Location = new Point(4, 4);
            richTextBoxConsole.Margin = new Padding(4, 4, 4, 4);
            richTextBoxConsole.Name = "richTextBoxConsole";
            richTextBoxConsole.Size = new Size(388, 483);
            richTextBoxConsole.TabIndex = 1;
            richTextBoxConsole.Text = "";
            richTextBoxConsole.WordWrap = false;
            // 
            // log
            // 
            log.Controls.Add(richTextBoxLog);
            log.Location = new Point(4, 34);
            log.Margin = new Padding(4, 4, 4, 4);
            log.Name = "log";
            log.Padding = new Padding(4, 4, 4, 4);
            log.Size = new Size(396, 524);
            log.TabIndex = 5;
            log.Text = "Log";
            log.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(4, 4);
            richTextBoxLog.Margin = new Padding(4, 4, 4, 4);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(388, 516);
            richTextBoxLog.TabIndex = 2;
            richTextBoxLog.Text = "";
            richTextBoxLog.WordWrap = false;
            // 
            // errors
            // 
            errors.Controls.Add(richTextBoxErrors);
            errors.Location = new Point(4, 34);
            errors.Margin = new Padding(4, 4, 4, 4);
            errors.Name = "errors";
            errors.Padding = new Padding(4, 4, 4, 4);
            errors.Size = new Size(396, 524);
            errors.TabIndex = 1;
            errors.Text = "Errors";
            errors.UseVisualStyleBackColor = true;
            // 
            // richTextBoxErrors
            // 
            richTextBoxErrors.Dock = DockStyle.Fill;
            richTextBoxErrors.Location = new Point(4, 4);
            richTextBoxErrors.Margin = new Padding(4, 4, 4, 4);
            richTextBoxErrors.Name = "richTextBoxErrors";
            richTextBoxErrors.Size = new Size(388, 516);
            richTextBoxErrors.TabIndex = 2;
            richTextBoxErrors.Text = "";
            richTextBoxErrors.WordWrap = false;
            // 
            // cst
            // 
            cst.Controls.Add(richTextBoxCst);
            cst.Location = new Point(4, 34);
            cst.Margin = new Padding(4, 4, 4, 4);
            cst.Name = "cst";
            cst.Size = new Size(396, 524);
            cst.TabIndex = 2;
            cst.Text = "CST";
            cst.UseVisualStyleBackColor = true;
            // 
            // richTextBoxCst
            // 
            richTextBoxCst.Dock = DockStyle.Fill;
            richTextBoxCst.Location = new Point(0, 0);
            richTextBoxCst.Margin = new Padding(4, 4, 4, 4);
            richTextBoxCst.Name = "richTextBoxCst";
            richTextBoxCst.Size = new Size(396, 524);
            richTextBoxCst.TabIndex = 2;
            richTextBoxCst.Text = "";
            richTextBoxCst.WordWrap = false;
            // 
            // ast
            // 
            ast.Controls.Add(richTextBoxAst);
            ast.Location = new Point(4, 34);
            ast.Margin = new Padding(4, 4, 4, 4);
            ast.Name = "ast";
            ast.Size = new Size(396, 524);
            ast.TabIndex = 3;
            ast.Text = "AST";
            ast.UseVisualStyleBackColor = true;
            // 
            // richTextBoxAst
            // 
            richTextBoxAst.Dock = DockStyle.Fill;
            richTextBoxAst.Location = new Point(0, 0);
            richTextBoxAst.Margin = new Padding(4, 4, 4, 4);
            richTextBoxAst.Name = "richTextBoxAst";
            richTextBoxAst.Size = new Size(396, 524);
            richTextBoxAst.TabIndex = 2;
            richTextBoxAst.Text = "";
            richTextBoxAst.WordWrap = false;
            // 
            // tokens
            // 
            tokens.Controls.Add(richTextBoxTokens);
            tokens.Location = new Point(4, 34);
            tokens.Margin = new Padding(4, 4, 4, 4);
            tokens.Name = "tokens";
            tokens.Size = new Size(396, 524);
            tokens.TabIndex = 4;
            tokens.Text = "Tokens";
            tokens.UseVisualStyleBackColor = true;
            // 
            // richTextBoxTokens
            // 
            richTextBoxTokens.Dock = DockStyle.Fill;
            richTextBoxTokens.Location = new Point(0, 0);
            richTextBoxTokens.Margin = new Padding(4, 4, 4, 4);
            richTextBoxTokens.Name = "richTextBoxTokens";
            richTextBoxTokens.Size = new Size(396, 524);
            richTextBoxTokens.TabIndex = 2;
            richTextBoxTokens.Text = "";
            richTextBoxTokens.WordWrap = false;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem1, fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1000, 33);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(95, 29);
            fileToolStripMenuItem.Text = "&About ...";
            // 
            // fileToolStripMenuItem1
            // 
            fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            fileToolStripMenuItem1.Size = new Size(54, 29);
            fileToolStripMenuItem1.Text = "&File";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 562);
            Controls.Add(mainSplitContainer);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 4, 4, 4);
            Name = "Form1";
            Text = "Plato - Code Viewer";
            Resize += Form1_Resize;
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            TabControl.ResumeLayout(false);
            console.ResumeLayout(false);
            log.ResumeLayout(false);
            errors.ResumeLayout(false);
            cst.ResumeLayout(false);
            ast.ResumeLayout(false);
            tokens.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabPage tabPage2;
        private SplitContainer mainSplitContainer;
        private TabControl TabControl;
        private TabPage console;
        private RichTextBox richTextBoxConsole;
        private TabPage errors;
        private TabPage cst;
        private TabPage ast;
        private TabPage tokens;
        private SplitContainer splitContainer1;
        private ListBox listBoxFiles;
        private RichTextBox richTextBoxEdit;
        private RichTextBox richTextBoxErrors;
        private RichTextBox richTextBoxCst;
        private RichTextBox richTextBoxAst;
        private RichTextBox richTextBoxTokens;
        private TabControl tabControlEditors;
        private TabPage log;
        private RichTextBox richTextBoxLog;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem1;
        private ToolStripMenuItem fileToolStripMenuItem;
    }
}