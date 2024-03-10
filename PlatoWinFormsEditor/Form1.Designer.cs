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
            mainSplitContainer.Panel2.Controls.Add(TabControl);
            mainSplitContainer.Size = new Size(800, 450);
            mainSplitContainer.SplitterDistance = 473;
            mainSplitContainer.TabIndex = 1;
            mainSplitContainer.SplitterMoved += mainSplitContainer_SplitterMoved;
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
            listBoxFiles.SelectedIndexChanged += listBoxFiles_SelectedIndexChanged;
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
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(323, 450);
            TabControl.TabIndex = 1;
            // 
            // console
            // 
            console.Controls.Add(richTextBoxConsole);
            console.Location = new Point(4, 29);
            console.Name = "console";
            console.Padding = new Padding(3);
            console.Size = new Size(315, 417);
            console.TabIndex = 0;
            console.Text = "Console";
            console.UseVisualStyleBackColor = true;
            // 
            // richTextBoxConsole
            // 
            richTextBoxConsole.Dock = DockStyle.Fill;
            richTextBoxConsole.Location = new Point(3, 3);
            richTextBoxConsole.Name = "richTextBoxConsole";
            richTextBoxConsole.Size = new Size(309, 411);
            richTextBoxConsole.TabIndex = 1;
            richTextBoxConsole.Text = "";
            richTextBoxConsole.WordWrap = false;
            // 
            // log
            // 
            log.Controls.Add(richTextBoxLog);
            log.Location = new Point(4, 29);
            log.Name = "log";
            log.Padding = new Padding(3);
            log.Size = new Size(315, 417);
            log.TabIndex = 5;
            log.Text = "Log";
            log.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(3, 3);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(309, 411);
            richTextBoxLog.TabIndex = 2;
            richTextBoxLog.Text = "";
            richTextBoxLog.WordWrap = false;
            // 
            // errors
            // 
            errors.Controls.Add(richTextBoxErrors);
            errors.Location = new Point(4, 29);
            errors.Name = "errors";
            errors.Padding = new Padding(3);
            errors.Size = new Size(315, 417);
            errors.TabIndex = 1;
            errors.Text = "Errors";
            errors.UseVisualStyleBackColor = true;
            // 
            // richTextBoxErrors
            // 
            richTextBoxErrors.Dock = DockStyle.Fill;
            richTextBoxErrors.Location = new Point(3, 3);
            richTextBoxErrors.Name = "richTextBoxErrors";
            richTextBoxErrors.Size = new Size(309, 411);
            richTextBoxErrors.TabIndex = 2;
            richTextBoxErrors.Text = "";
            richTextBoxErrors.WordWrap = false;
            // 
            // cst
            // 
            cst.Controls.Add(richTextBoxCst);
            cst.Location = new Point(4, 29);
            cst.Name = "cst";
            cst.Size = new Size(315, 417);
            cst.TabIndex = 2;
            cst.Text = "CST";
            cst.UseVisualStyleBackColor = true;
            // 
            // richTextBoxCst
            // 
            richTextBoxCst.Dock = DockStyle.Fill;
            richTextBoxCst.Location = new Point(0, 0);
            richTextBoxCst.Name = "richTextBoxCst";
            richTextBoxCst.Size = new Size(315, 417);
            richTextBoxCst.TabIndex = 2;
            richTextBoxCst.Text = "";
            richTextBoxCst.WordWrap = false;
            // 
            // ast
            // 
            ast.Controls.Add(richTextBoxAst);
            ast.Location = new Point(4, 29);
            ast.Name = "ast";
            ast.Size = new Size(315, 417);
            ast.TabIndex = 3;
            ast.Text = "AST";
            ast.UseVisualStyleBackColor = true;
            // 
            // richTextBoxAst
            // 
            richTextBoxAst.Dock = DockStyle.Fill;
            richTextBoxAst.Location = new Point(0, 0);
            richTextBoxAst.Name = "richTextBoxAst";
            richTextBoxAst.Size = new Size(315, 417);
            richTextBoxAst.TabIndex = 2;
            richTextBoxAst.Text = "";
            richTextBoxAst.WordWrap = false;
            // 
            // tokens
            // 
            tokens.Controls.Add(richTextBoxTokens);
            tokens.Location = new Point(4, 29);
            tokens.Name = "tokens";
            tokens.Size = new Size(315, 417);
            tokens.TabIndex = 4;
            tokens.Text = "Tokens";
            tokens.UseVisualStyleBackColor = true;
            // 
            // richTextBoxTokens
            // 
            richTextBoxTokens.Dock = DockStyle.Fill;
            richTextBoxTokens.Location = new Point(0, 0);
            richTextBoxTokens.Name = "richTextBoxTokens";
            richTextBoxTokens.Size = new Size(315, 417);
            richTextBoxTokens.TabIndex = 2;
            richTextBoxTokens.Text = "";
            richTextBoxTokens.WordWrap = false;
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
            ResumeLayout(false);
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
    }
}