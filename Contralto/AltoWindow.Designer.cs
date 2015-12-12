namespace Contralto
{
    partial class AltoWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DisplayBox = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemStartMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drive0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.drive1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDebuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusLine = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.StatusLine.SuspendLayout();
            this.SuspendLayout();
            // 
            // DisplayBox
            // 
            this.DisplayBox.BackColor = System.Drawing.SystemColors.Window;
            this.DisplayBox.Location = new System.Drawing.Point(0, 27);
            this.DisplayBox.Name = "DisplayBox";
            this.DisplayBox.Size = new System.Drawing.Size(606, 808);
            this.DisplayBox.TabIndex = 1;
            this.DisplayBox.TabStop = false;
            this.DisplayBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseDown);
            this.DisplayBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseMove);
            this.DisplayBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseUp);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.debuggerToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(608, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SystemStartMenuItem,
            this.SystemResetMenuItem,
            this.drive0ToolStripMenuItem,
            this.drive1ToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.settingsToolStripMenuItem.Text = "System";
            // 
            // SystemStartMenuItem
            // 
            this.SystemStartMenuItem.Name = "SystemStartMenuItem";
            this.SystemStartMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SystemStartMenuItem.Text = "Start";
            this.SystemStartMenuItem.Click += new System.EventHandler(this.OnSystemStartMenuClick);
            // 
            // SystemResetMenuItem
            // 
            this.SystemResetMenuItem.Enabled = false;
            this.SystemResetMenuItem.Name = "SystemResetMenuItem";
            this.SystemResetMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SystemResetMenuItem.Text = "Reset";
            // 
            // drive0ToolStripMenuItem
            // 
            this.drive0ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem1,
            this.unloadToolStripMenuItem1});
            this.drive0ToolStripMenuItem.Name = "drive0ToolStripMenuItem";
            this.drive0ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.drive0ToolStripMenuItem.Text = "Drive 0";
            // 
            // loadToolStripMenuItem1
            // 
            this.loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            this.loadToolStripMenuItem1.Size = new System.Drawing.Size(119, 22);
            this.loadToolStripMenuItem1.Text = "Load...";
            // 
            // unloadToolStripMenuItem1
            // 
            this.unloadToolStripMenuItem1.Name = "unloadToolStripMenuItem1";
            this.unloadToolStripMenuItem1.Size = new System.Drawing.Size(119, 22);
            this.unloadToolStripMenuItem1.Text = "Unload...";
            // 
            // drive1ToolStripMenuItem
            // 
            this.drive1ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.unloadToolStripMenuItem});
            this.drive1ToolStripMenuItem.Name = "drive1ToolStripMenuItem";
            this.drive1ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.drive1ToolStripMenuItem.Text = "Drive 1";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.loadToolStripMenuItem.Text = "Load...";
            // 
            // unloadToolStripMenuItem
            // 
            this.unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
            this.unloadToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.unloadToolStripMenuItem.Text = "Unload...";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDebuggerToolStripMenuItem});
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            this.debuggerToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.debuggerToolStripMenuItem.Text = "Debugger";
            // 
            // showDebuggerToolStripMenuItem
            // 
            this.showDebuggerToolStripMenuItem.Name = "showDebuggerToolStripMenuItem";
            this.showDebuggerToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.showDebuggerToolStripMenuItem.Text = "Show Debugger";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // StatusLine
            // 
            this.StatusLine.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.StatusLine.Location = new System.Drawing.Point(0, 837);
            this.StatusLine.Name = "StatusLine";
            this.StatusLine.Size = new System.Drawing.Size(608, 22);
            this.StatusLine.TabIndex = 3;
            this.StatusLine.Text = "statusStrip1";
            this.StatusLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(63, 17);
            this.StatusLabel.Text = "StatusLabel";
            // 
            // AltoWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 859);
            this.Controls.Add(this.StatusLine);
            this.Controls.Add(this.DisplayBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimizeBox = false;
            this.Name = "AltoWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ContrAlto";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.StatusLine.ResumeLayout(false);
            this.StatusLine.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox DisplayBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SystemStartMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SystemResetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drive0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem unloadToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem drive1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDebuggerToolStripMenuItem;
        private System.Windows.Forms.StatusStrip StatusLine;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
    }
}