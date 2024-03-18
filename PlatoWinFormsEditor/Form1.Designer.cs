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
            aboutPlatoToolStripMenuItem = new ToolStripMenuItem();
            nodes = new TabPage();
            tree = new TabPage();
            richTextBoxNodes = new RichTextBox();
            richTextBoxParseTree = new RichTextBox();
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
            nodes.SuspendLayout();
            tree.SuspendLayout();
            SuspendLayout();
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new Point(0, 24);
            mainSplitContainer.Margin = new Padding(3, 2, 3, 2);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(splitContainer1);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(TabControl);
            mainSplitContainer.Size = new Size(914, 481);
            mainSplitContainer.SplitterDistance = 539;
            mainSplitContainer.TabIndex = 1;
            mainSplitContainer.SplitterMoved += mainSplitContainer_SplitterMoved;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(3, 2, 3, 2);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listBoxFiles);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControlEditors);
            splitContainer1.Size = new Size(539, 481);
            splitContainer1.SplitterDistance = 177;
            splitContainer1.TabIndex = 0;
            // 
            // listBoxFiles
            // 
            listBoxFiles.Dock = DockStyle.Fill;
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.ItemHeight = 15;
            listBoxFiles.Location = new Point(0, 0);
            listBoxFiles.Margin = new Padding(3, 2, 3, 2);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(177, 481);
            listBoxFiles.TabIndex = 0;
            listBoxFiles.SelectedIndexChanged += listBoxFiles_SelectedIndexChanged;
            // 
            // tabControlEditors
            // 
            tabControlEditors.Dock = DockStyle.Fill;
            tabControlEditors.Location = new Point(0, 0);
            tabControlEditors.Margin = new Padding(3, 2, 3, 2);
            tabControlEditors.Name = "tabControlEditors";
            tabControlEditors.SelectedIndex = 0;
            tabControlEditors.Size = new Size(358, 481);
            tabControlEditors.TabIndex = 0;
            // 
            // TabControl
            // 
            TabControl.Controls.Add(console);
            TabControl.Controls.Add(log);
            TabControl.Controls.Add(errors);
            TabControl.Controls.Add(tokens);
            TabControl.Controls.Add(nodes);
            TabControl.Controls.Add(tree);
            TabControl.Controls.Add(cst);
            TabControl.Controls.Add(ast);
            TabControl.Dock = DockStyle.Fill;
            TabControl.Location = new Point(0, 0);
            TabControl.Margin = new Padding(3, 2, 3, 2);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(371, 481);
            TabControl.TabIndex = 1;
            // 
            // console
            // 
            console.Controls.Add(richTextBoxConsole);
            console.Location = new Point(4, 24);
            console.Margin = new Padding(3, 2, 3, 2);
            console.Name = "console";
            console.Padding = new Padding(3, 2, 3, 2);
            console.Size = new Size(363, 453);
            console.TabIndex = 0;
            console.Text = "Console";
            console.UseVisualStyleBackColor = true;
            // 
            // richTextBoxConsole
            // 
            richTextBoxConsole.Dock = DockStyle.Fill;
            richTextBoxConsole.Location = new Point(3, 2);
            richTextBoxConsole.Margin = new Padding(3, 2, 3, 2);
            richTextBoxConsole.Name = "richTextBoxConsole";
            richTextBoxConsole.Size = new Size(357, 449);
            richTextBoxConsole.TabIndex = 1;
            richTextBoxConsole.Text = "";
            richTextBoxConsole.WordWrap = false;
            // 
            // log
            // 
            log.Controls.Add(richTextBoxLog);
            log.Location = new Point(4, 24);
            log.Margin = new Padding(3, 2, 3, 2);
            log.Name = "log";
            log.Padding = new Padding(3, 2, 3, 2);
            log.Size = new Size(363, 453);
            log.TabIndex = 5;
            log.Text = "File Log";
            log.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(3, 2);
            richTextBoxLog.Margin = new Padding(3, 2, 3, 2);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(357, 449);
            richTextBoxLog.TabIndex = 2;
            richTextBoxLog.Text = "";
            richTextBoxLog.WordWrap = false;
            // 
            // errors
            // 
            errors.Controls.Add(richTextBoxErrors);
            errors.Location = new Point(4, 24);
            errors.Margin = new Padding(3, 2, 3, 2);
            errors.Name = "errors";
            errors.Padding = new Padding(3, 2, 3, 2);
            errors.Size = new Size(363, 453);
            errors.TabIndex = 1;
            errors.Text = "Parse Errors";
            errors.UseVisualStyleBackColor = true;
            // 
            // richTextBoxErrors
            // 
            richTextBoxErrors.Dock = DockStyle.Fill;
            richTextBoxErrors.Location = new Point(3, 2);
            richTextBoxErrors.Margin = new Padding(3, 2, 3, 2);
            richTextBoxErrors.Name = "richTextBoxErrors";
            richTextBoxErrors.Size = new Size(357, 449);
            richTextBoxErrors.TabIndex = 2;
            richTextBoxErrors.Text = "";
            richTextBoxErrors.WordWrap = false;
            // 
            // cst
            // 
            cst.Controls.Add(richTextBoxCst);
            cst.Location = new Point(4, 24);
            cst.Margin = new Padding(3, 2, 3, 2);
            cst.Name = "cst";
            cst.Size = new Size(363, 453);
            cst.TabIndex = 2;
            cst.Text = "CST";
            cst.UseVisualStyleBackColor = true;
            // 
            // richTextBoxCst
            // 
            richTextBoxCst.Dock = DockStyle.Fill;
            richTextBoxCst.Location = new Point(0, 0);
            richTextBoxCst.Margin = new Padding(3, 2, 3, 2);
            richTextBoxCst.Name = "richTextBoxCst";
            richTextBoxCst.Size = new Size(363, 453);
            richTextBoxCst.TabIndex = 2;
            richTextBoxCst.Text = "";
            richTextBoxCst.WordWrap = false;
            // 
            // ast
            // 
            ast.Controls.Add(richTextBoxAst);
            ast.Location = new Point(4, 24);
            ast.Margin = new Padding(3, 2, 3, 2);
            ast.Name = "ast";
            ast.Size = new Size(363, 453);
            ast.TabIndex = 3;
            ast.Text = "AST";
            ast.UseVisualStyleBackColor = true;
            // 
            // richTextBoxAst
            // 
            richTextBoxAst.Dock = DockStyle.Fill;
            richTextBoxAst.Location = new Point(0, 0);
            richTextBoxAst.Margin = new Padding(3, 2, 3, 2);
            richTextBoxAst.Name = "richTextBoxAst";
            richTextBoxAst.Size = new Size(363, 453);
            richTextBoxAst.TabIndex = 2;
            richTextBoxAst.Text = "";
            richTextBoxAst.WordWrap = false;
            // 
            // tokens
            // 
            tokens.Controls.Add(richTextBoxTokens);
            tokens.Location = new Point(4, 24);
            tokens.Margin = new Padding(3, 2, 3, 2);
            tokens.Name = "tokens";
            tokens.Size = new Size(363, 453);
            tokens.TabIndex = 4;
            tokens.Text = "Tokens";
            tokens.UseVisualStyleBackColor = true;
            // 
            // richTextBoxTokens
            // 
            richTextBoxTokens.Dock = DockStyle.Fill;
            richTextBoxTokens.Location = new Point(0, 0);
            richTextBoxTokens.Margin = new Padding(3, 2, 3, 2);
            richTextBoxTokens.Name = "richTextBoxTokens";
            richTextBoxTokens.Size = new Size(363, 453);
            richTextBoxTokens.TabIndex = 2;
            richTextBoxTokens.Text = "";
            richTextBoxTokens.WordWrap = false;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { aboutPlatoToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(4, 1, 0, 1);
            menuStrip1.Size = new Size(914, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // aboutPlatoToolStripMenuItem
            // 
            aboutPlatoToolStripMenuItem.Name = "aboutPlatoToolStripMenuItem";
            aboutPlatoToolStripMenuItem.Size = new Size(94, 22);
            aboutPlatoToolStripMenuItem.Text = "About Plato ...";
            aboutPlatoToolStripMenuItem.Click += aboutPlatoToolStripMenuItem_Click;
            // 
            // nodes
            // 
            nodes.Controls.Add(richTextBoxNodes);
            nodes.Location = new Point(4, 24);
            nodes.Name = "nodes";
            nodes.Size = new Size(363, 453);
            nodes.TabIndex = 6;
            nodes.Text = "Nodes";
            nodes.UseVisualStyleBackColor = true;
            // 
            // tree
            // 
            tree.Controls.Add(richTextBoxParseTree);
            tree.Location = new Point(4, 24);
            tree.Name = "tree";
            tree.Size = new Size(363, 453);
            tree.TabIndex = 7;
            tree.Text = "Parse Tree";
            tree.UseVisualStyleBackColor = true;
            // 
            // richTextBoxNodes
            // 
            richTextBoxNodes.Dock = DockStyle.Fill;
            richTextBoxNodes.Location = new Point(0, 0);
            richTextBoxNodes.Margin = new Padding(3, 2, 3, 2);
            richTextBoxNodes.Name = "richTextBoxNodes";
            richTextBoxNodes.Size = new Size(363, 453);
            richTextBoxNodes.TabIndex = 3;
            richTextBoxNodes.Text = "";
            richTextBoxNodes.WordWrap = false;
            // 
            // richTextBoxParseTree
            // 
            richTextBoxParseTree.Dock = DockStyle.Fill;
            richTextBoxParseTree.Location = new Point(0, 0);
            richTextBoxParseTree.Margin = new Padding(3, 2, 3, 2);
            richTextBoxParseTree.Name = "richTextBoxParseTree";
            richTextBoxParseTree.Size = new Size(363, 453);
            richTextBoxParseTree.TabIndex = 3;
            richTextBoxParseTree.Text = "";
            richTextBoxParseTree.WordWrap = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 505);
            Controls.Add(mainSplitContainer);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
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
            nodes.ResumeLayout(false);
            tree.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabPage tree;
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
        private ToolStripMenuItem aboutPlatoToolStripMenuItem;
        private TabPage nodes;
        private RichTextBox richTextBoxNodes;
        private RichTextBox richTextBoxParseTree;
    }
}