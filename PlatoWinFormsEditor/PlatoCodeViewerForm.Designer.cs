namespace PlatoWinFormsEditor
{
    partial class PlatoCodeViewerForm
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
            consoleTabPage = new TabPage();
            richTextBoxConsole = new RichTextBox();
            logTabPage = new TabPage();
            richTextBoxLog = new RichTextBox();
            errorsTabPage = new TabPage();
            richTextBoxErrors = new RichTextBox();
            tokensTabPage = new TabPage();
            richTextBoxTokens = new RichTextBox();
            nodesTabPage = new TabPage();
            richTextBoxNodes = new RichTextBox();
            treeTabPage = new TabPage();
            richTextBoxParseTree = new RichTextBox();
            cstTabPage = new TabPage();
            richTextBoxCst = new RichTextBox();
            astTabPage = new TabPage();
            richTextBoxAst = new RichTextBox();
            typesTabPage = new TabPage();
            richTextBoxTypes = new RichTextBox();
            symbolErrorsTabPage = new TabPage();
            richTextBoxSymbolErrors = new RichTextBox();
            symbolsTabPage = new TabPage();
            richTextBoxSymbols = new RichTextBox();
            semanticsTabPage = new TabPage();
            richTextBoxSemantics = new RichTextBox();
            menuStrip1 = new MenuStrip();
            aboutPlatoToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            TabControl.SuspendLayout();
            consoleTabPage.SuspendLayout();
            logTabPage.SuspendLayout();
            errorsTabPage.SuspendLayout();
            tokensTabPage.SuspendLayout();
            nodesTabPage.SuspendLayout();
            treeTabPage.SuspendLayout();
            cstTabPage.SuspendLayout();
            astTabPage.SuspendLayout();
            typesTabPage.SuspendLayout();
            symbolErrorsTabPage.SuspendLayout();
            symbolsTabPage.SuspendLayout();
            semanticsTabPage.SuspendLayout();
            menuStrip1.SuspendLayout();
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
            TabControl.Controls.Add(consoleTabPage);
            TabControl.Controls.Add(logTabPage);
            TabControl.Controls.Add(errorsTabPage);
            TabControl.Controls.Add(tokensTabPage);
            TabControl.Controls.Add(nodesTabPage);
            TabControl.Controls.Add(treeTabPage);
            TabControl.Controls.Add(cstTabPage);
            TabControl.Controls.Add(astTabPage);
            TabControl.Controls.Add(typesTabPage);
            TabControl.Controls.Add(symbolErrorsTabPage);
            TabControl.Controls.Add(symbolsTabPage);
            TabControl.Controls.Add(semanticsTabPage);
            TabControl.Dock = DockStyle.Fill;
            TabControl.Location = new Point(0, 0);
            TabControl.Margin = new Padding(3, 2, 3, 2);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(371, 481);
            TabControl.TabIndex = 1;
            // 
            // consoleTabPage
            // 
            consoleTabPage.Controls.Add(richTextBoxConsole);
            consoleTabPage.Location = new Point(4, 24);
            consoleTabPage.Margin = new Padding(3, 2, 3, 2);
            consoleTabPage.Name = "consoleTabPage";
            consoleTabPage.Padding = new Padding(3, 2, 3, 2);
            consoleTabPage.Size = new Size(363, 453);
            consoleTabPage.TabIndex = 0;
            consoleTabPage.Text = "Console";
            consoleTabPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxConsole
            // 
            richTextBoxConsole.Dock = DockStyle.Fill;
            richTextBoxConsole.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            richTextBoxConsole.Location = new Point(3, 2);
            richTextBoxConsole.Margin = new Padding(3, 2, 3, 2);
            richTextBoxConsole.Name = "richTextBoxConsole";
            richTextBoxConsole.Size = new Size(357, 449);
            richTextBoxConsole.TabIndex = 1;
            richTextBoxConsole.Text = "";
            richTextBoxConsole.WordWrap = false;
            // 
            // logTabPage
            // 
            logTabPage.Controls.Add(richTextBoxLog);
            logTabPage.Location = new Point(4, 24);
            logTabPage.Margin = new Padding(3, 2, 3, 2);
            logTabPage.Name = "logTabPage";
            logTabPage.Padding = new Padding(3, 2, 3, 2);
            logTabPage.Size = new Size(363, 453);
            logTabPage.TabIndex = 5;
            logTabPage.Text = "File Log";
            logTabPage.UseVisualStyleBackColor = true;
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
            // errorsTabPage
            // 
            errorsTabPage.Controls.Add(richTextBoxErrors);
            errorsTabPage.Location = new Point(4, 24);
            errorsTabPage.Margin = new Padding(3, 2, 3, 2);
            errorsTabPage.Name = "errorsTabPage";
            errorsTabPage.Padding = new Padding(3, 2, 3, 2);
            errorsTabPage.Size = new Size(363, 453);
            errorsTabPage.TabIndex = 1;
            errorsTabPage.Text = "Parse Errors";
            errorsTabPage.UseVisualStyleBackColor = true;
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
            // tokensTabPage
            // 
            tokensTabPage.Controls.Add(richTextBoxTokens);
            tokensTabPage.Location = new Point(4, 24);
            tokensTabPage.Margin = new Padding(3, 2, 3, 2);
            tokensTabPage.Name = "tokensTabPage";
            tokensTabPage.Size = new Size(363, 453);
            tokensTabPage.TabIndex = 4;
            tokensTabPage.Text = "Tokens";
            tokensTabPage.UseVisualStyleBackColor = true;
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
            // nodesTabPage
            // 
            nodesTabPage.Controls.Add(richTextBoxNodes);
            nodesTabPage.Location = new Point(4, 24);
            nodesTabPage.Name = "nodesTabPage";
            nodesTabPage.Size = new Size(363, 453);
            nodesTabPage.TabIndex = 6;
            nodesTabPage.Text = "Nodes";
            nodesTabPage.UseVisualStyleBackColor = true;
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
            // treeTabPage
            // 
            treeTabPage.Controls.Add(richTextBoxParseTree);
            treeTabPage.Location = new Point(4, 24);
            treeTabPage.Name = "treeTabPage";
            treeTabPage.Size = new Size(363, 453);
            treeTabPage.TabIndex = 7;
            treeTabPage.Text = "Parse Tree";
            treeTabPage.UseVisualStyleBackColor = true;
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
            // cstTabPage
            // 
            cstTabPage.Controls.Add(richTextBoxCst);
            cstTabPage.Location = new Point(4, 24);
            cstTabPage.Margin = new Padding(3, 2, 3, 2);
            cstTabPage.Name = "cstTabPage";
            cstTabPage.Size = new Size(363, 453);
            cstTabPage.TabIndex = 2;
            cstTabPage.Text = "CST";
            cstTabPage.UseVisualStyleBackColor = true;
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
            // astTabPage
            // 
            astTabPage.Controls.Add(richTextBoxAst);
            astTabPage.Location = new Point(4, 24);
            astTabPage.Margin = new Padding(3, 2, 3, 2);
            astTabPage.Name = "astTabPage";
            astTabPage.Size = new Size(363, 453);
            astTabPage.TabIndex = 3;
            astTabPage.Text = "AST";
            astTabPage.UseVisualStyleBackColor = true;
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
            // typesTabPage
            // 
            typesTabPage.Controls.Add(richTextBoxTypes);
            typesTabPage.Location = new Point(4, 24);
            typesTabPage.Name = "typesTabPage";
            typesTabPage.Padding = new Padding(3);
            typesTabPage.Size = new Size(363, 453);
            typesTabPage.TabIndex = 8;
            typesTabPage.Text = "Types";
            typesTabPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxTypes
            // 
            richTextBoxTypes.Dock = DockStyle.Fill;
            richTextBoxTypes.Location = new Point(3, 3);
            richTextBoxTypes.Margin = new Padding(3, 2, 3, 2);
            richTextBoxTypes.Name = "richTextBoxTypes";
            richTextBoxTypes.Size = new Size(357, 447);
            richTextBoxTypes.TabIndex = 3;
            richTextBoxTypes.Text = "";
            richTextBoxTypes.WordWrap = false;
            // 
            // symbolErrorsTabPage
            // 
            symbolErrorsTabPage.Controls.Add(richTextBoxSymbolErrors);
            symbolErrorsTabPage.Location = new Point(4, 24);
            symbolErrorsTabPage.Name = "symbolErrorsTabPage";
            symbolErrorsTabPage.Size = new Size(363, 453);
            symbolErrorsTabPage.TabIndex = 9;
            symbolErrorsTabPage.Text = "Symbol Errors";
            symbolErrorsTabPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxSymbolErrors
            // 
            richTextBoxSymbolErrors.Dock = DockStyle.Fill;
            richTextBoxSymbolErrors.Location = new Point(0, 0);
            richTextBoxSymbolErrors.Margin = new Padding(3, 2, 3, 2);
            richTextBoxSymbolErrors.Name = "richTextBoxSymbolErrors";
            richTextBoxSymbolErrors.Size = new Size(363, 453);
            richTextBoxSymbolErrors.TabIndex = 3;
            richTextBoxSymbolErrors.Text = "";
            richTextBoxSymbolErrors.WordWrap = false;
            // 
            // symbolsTabPage
            // 
            symbolsTabPage.Controls.Add(richTextBoxSymbols);
            symbolsTabPage.Location = new Point(4, 24);
            symbolsTabPage.Name = "symbolsTabPage";
            symbolsTabPage.Size = new Size(363, 453);
            symbolsTabPage.TabIndex = 10;
            symbolsTabPage.Text = "Symbols";
            symbolsTabPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxSymbols
            // 
            richTextBoxSymbols.Dock = DockStyle.Fill;
            richTextBoxSymbols.Location = new Point(0, 0);
            richTextBoxSymbols.Margin = new Padding(3, 2, 3, 2);
            richTextBoxSymbols.Name = "richTextBoxSymbols";
            richTextBoxSymbols.Size = new Size(363, 453);
            richTextBoxSymbols.TabIndex = 3;
            richTextBoxSymbols.Text = "";
            richTextBoxSymbols.WordWrap = false;
            // 
            // semanticsTabPage
            // 
            semanticsTabPage.Controls.Add(richTextBoxSemantics);
            semanticsTabPage.Location = new Point(4, 24);
            semanticsTabPage.Name = "semanticsTabPage";
            semanticsTabPage.Size = new Size(363, 453);
            semanticsTabPage.TabIndex = 11;
            semanticsTabPage.Text = "Semantics";
            semanticsTabPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxSemantics
            // 
            richTextBoxSemantics.Dock = DockStyle.Fill;
            richTextBoxSemantics.Location = new Point(0, 0);
            richTextBoxSemantics.Margin = new Padding(3, 2, 3, 2);
            richTextBoxSemantics.Name = "richTextBoxSemantics";
            richTextBoxSemantics.Size = new Size(363, 453);
            richTextBoxSemantics.TabIndex = 4;
            richTextBoxSemantics.Text = "";
            richTextBoxSemantics.WordWrap = false;
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
            // PlatoCodeViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 505);
            Controls.Add(mainSplitContainer);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
            Name = "PlatoCodeViewerForm";
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
            consoleTabPage.ResumeLayout(false);
            logTabPage.ResumeLayout(false);
            errorsTabPage.ResumeLayout(false);
            tokensTabPage.ResumeLayout(false);
            nodesTabPage.ResumeLayout(false);
            treeTabPage.ResumeLayout(false);
            cstTabPage.ResumeLayout(false);
            astTabPage.ResumeLayout(false);
            typesTabPage.ResumeLayout(false);
            symbolErrorsTabPage.ResumeLayout(false);
            symbolsTabPage.ResumeLayout(false);
            semanticsTabPage.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabPage treeTabPage;
        private SplitContainer mainSplitContainer;
        private TabControl TabControl;
        private TabPage consoleTabPage;
        private RichTextBox richTextBoxConsole;
        private TabPage errorsTabPage;
        private TabPage cstTabPage;
        private TabPage astTabPage;
        private TabPage tokensTabPage;
        private SplitContainer splitContainer1;
        private ListBox listBoxFiles;
        private RichTextBox richTextBoxEdit;
        private RichTextBox richTextBoxErrors;
        private RichTextBox richTextBoxCst;
        private RichTextBox richTextBoxAst;
        private RichTextBox richTextBoxTokens;
        private TabControl tabControlEditors;
        private TabPage logTabPage;
        private RichTextBox richTextBoxLog;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem aboutPlatoToolStripMenuItem;
        private TabPage nodesTabPage;
        private RichTextBox richTextBoxNodes;
        private RichTextBox richTextBoxParseTree;
        private TabPage typesTabPage;
        private TabPage symbolErrorsTabPage;
        private TabPage symbolsTabPage;
        private RichTextBox richTextBoxTypes;
        private RichTextBox richTextBoxSymbolErrors;
        private RichTextBox richTextBoxSymbols;
        private TabPage semanticsTabPage;
        private RichTextBox richTextBoxSemantics;
    }
}