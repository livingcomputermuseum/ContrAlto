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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltoWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemStartMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drive0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Drive0ImageName = new System.Windows.Forms.ToolStripMenuItem();
            this.drive1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Drive1ImageName = new System.Windows.Forms.ToolStripMenuItem();
            this.AlternateBootToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemEthernetBootMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SystemShowDebuggerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusLine = new System.Windows.Forms.StatusStrip();
            this.DiskStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.FPSLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.CaptureStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SystemStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.DisplayBox = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.StatusLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
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
            this.saveScreenshotToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveScreenshotToolStripMenuItem
            // 
            this.saveScreenshotToolStripMenuItem.Name = "saveScreenshotToolStripMenuItem";
            this.saveScreenshotToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.P)));
            this.saveScreenshotToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.saveScreenshotToolStripMenuItem.Text = "Save Screenshot...";
            this.saveScreenshotToolStripMenuItem.Click += new System.EventHandler(this.OnFileSaveScreenshotClick);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnFileExitClick);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SystemStartMenuItem,
            this.SystemResetMenuItem,
            this.drive0ToolStripMenuItem,
            this.drive1ToolStripMenuItem,
            this.AlternateBootToolStripMenuItem,
            this.SystemEthernetBootMenu,
            this.optionsToolStripMenuItem,
            this.SystemShowDebuggerMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.settingsToolStripMenuItem.Text = "System";
            // 
            // SystemStartMenuItem
            // 
            this.SystemStartMenuItem.Name = "SystemStartMenuItem";
            this.SystemStartMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
            this.SystemStartMenuItem.Size = new System.Drawing.Size(223, 22);
            this.SystemStartMenuItem.Text = "Start";
            this.SystemStartMenuItem.Click += new System.EventHandler(this.OnSystemStartMenuClick);
            // 
            // SystemResetMenuItem
            // 
            this.SystemResetMenuItem.Enabled = false;
            this.SystemResetMenuItem.Name = "SystemResetMenuItem";
            this.SystemResetMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.R)));
            this.SystemResetMenuItem.Size = new System.Drawing.Size(223, 22);
            this.SystemResetMenuItem.Text = "Reset";
            this.SystemResetMenuItem.Click += new System.EventHandler(this.OnSystemResetMenuClick);
            // 
            // drive0ToolStripMenuItem
            // 
            this.drive0ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem1,
            this.unloadToolStripMenuItem1,
            this.Drive0ImageName});
            this.drive0ToolStripMenuItem.Name = "drive0ToolStripMenuItem";
            this.drive0ToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.drive0ToolStripMenuItem.Text = "Drive 0";
            // 
            // loadToolStripMenuItem1
            // 
            this.loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            this.loadToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.L)));
            this.loadToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.loadToolStripMenuItem1.Text = "Load...";
            this.loadToolStripMenuItem1.Click += new System.EventHandler(this.OnSystemDrive0LoadClick);
            // 
            // unloadToolStripMenuItem1
            // 
            this.unloadToolStripMenuItem1.Name = "unloadToolStripMenuItem1";
            this.unloadToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.unloadToolStripMenuItem1.Text = "Unload...";
            this.unloadToolStripMenuItem1.Click += new System.EventHandler(this.OnSystemDrive0UnloadClick);
            // 
            // Drive0ImageName
            // 
            this.Drive0ImageName.Enabled = false;
            this.Drive0ImageName.Name = "Drive0ImageName";
            this.Drive0ImageName.Size = new System.Drawing.Size(172, 22);
            this.Drive0ImageName.Text = "Image Name";
            // 
            // drive1ToolStripMenuItem
            // 
            this.drive1ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.unloadToolStripMenuItem,
            this.Drive1ImageName});
            this.drive1ToolStripMenuItem.Name = "drive1ToolStripMenuItem";
            this.drive1ToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.drive1ToolStripMenuItem.Text = "Drive 1";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.loadToolStripMenuItem.Text = "Load...";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.OnSystemDrive1LoadClick);
            // 
            // unloadToolStripMenuItem
            // 
            this.unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
            this.unloadToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.unloadToolStripMenuItem.Text = "Unload...";
            this.unloadToolStripMenuItem.Click += new System.EventHandler(this.OnSystemDrive1UnloadClick);
            // 
            // Drive1ImageName
            // 
            this.Drive1ImageName.Enabled = false;
            this.Drive1ImageName.Name = "Drive1ImageName";
            this.Drive1ImageName.Size = new System.Drawing.Size(142, 22);
            this.Drive1ImageName.Text = "Image Name";
            // 
            // AlternateBootToolStripMenuItem
            // 
            this.AlternateBootToolStripMenuItem.Name = "AlternateBootToolStripMenuItem";
            this.AlternateBootToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.AlternateBootToolStripMenuItem.Text = "Start with Alternate Boot";
            this.AlternateBootToolStripMenuItem.Click += new System.EventHandler(this.OnStartWithAlternateBootClicked);
            // 
            // SystemEthernetBootMenu
            // 
            this.SystemEthernetBootMenu.Name = "SystemEthernetBootMenu";
            this.SystemEthernetBootMenu.Size = new System.Drawing.Size(223, 22);
            this.SystemEthernetBootMenu.Text = "Alternate Boot Options...";
            this.SystemEthernetBootMenu.Click += new System.EventHandler(this.OnAlternateBootOptionsClicked);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.optionsToolStripMenuItem.Text = "System Configuration...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OnSystemOptionsClick);
            // 
            // SystemShowDebuggerMenuItem
            // 
            this.SystemShowDebuggerMenuItem.Name = "SystemShowDebuggerMenuItem";
            this.SystemShowDebuggerMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D)));
            this.SystemShowDebuggerMenuItem.Size = new System.Drawing.Size(223, 22);
            this.SystemShowDebuggerMenuItem.Text = "Show Debugger";
            this.SystemShowDebuggerMenuItem.Click += new System.EventHandler(this.OnDebuggerShowClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnHelpAboutClick);
            // 
            // StatusLine
            // 
            this.StatusLine.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DiskStatusLabel,
            this.FPSLabel,
            this.CaptureStatusLabel,
            this.SystemStatusLabel});
            this.StatusLine.Location = new System.Drawing.Point(0, 834);
            this.StatusLine.Name = "StatusLine";
            this.StatusLine.Size = new System.Drawing.Size(608, 25);
            this.StatusLine.TabIndex = 3;
            this.StatusLine.Text = "statusStrip1";
            this.StatusLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            // 
            // DiskStatusLabel
            // 
            this.DiskStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.DiskStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DiskStatusLabel.Image = global::Contralto.Properties.Resources.DiskNoAccess;
            this.DiskStatusLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.DiskStatusLabel.Name = "DiskStatusLabel";
            this.DiskStatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.DiskStatusLabel.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.DiskStatusLabel.Size = new System.Drawing.Size(26, 20);
            this.DiskStatusLabel.Text = "DiskStatusLabel";
            // 
            // FPSLabel
            // 
            this.FPSLabel.AutoSize = false;
            this.FPSLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.FPSLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.FPSLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.FPSLabel.Name = "FPSLabel";
            this.FPSLabel.Size = new System.Drawing.Size(80, 20);
            // 
            // CaptureStatusLabel
            // 
            this.CaptureStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.CaptureStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.CaptureStatusLabel.Name = "CaptureStatusLabel";
            this.CaptureStatusLabel.Size = new System.Drawing.Size(113, 20);
            this.CaptureStatusLabel.Text = "CaptureStatusLabel";
            // 
            // SystemStatusLabel
            // 
            this.SystemStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.SystemStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.SystemStatusLabel.Name = "SystemStatusLabel";
            this.SystemStatusLabel.Size = new System.Drawing.Size(109, 20);
            this.SystemStatusLabel.Text = "SystemStatusLabel";
            // 
            // DisplayBox
            // 
            this.DisplayBox.BackColor = System.Drawing.SystemColors.Window;
            this.DisplayBox.Location = new System.Drawing.Point(0, 27);
            this.DisplayBox.Name = "DisplayBox";
            this.DisplayBox.Size = new System.Drawing.Size(606, 808);
            this.DisplayBox.TabIndex = 1;
            this.DisplayBox.TabStop = false;
            this.DisplayBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.DisplayBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseDown);
            this.DisplayBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseMove);
            this.DisplayBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnDisplayMouseUp);
            // 
            // AltoWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 859);
            this.Controls.Add(this.StatusLine);
            this.Controls.Add(this.DisplayBox);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "AltoWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ContrAlto";
            this.Deactivate += new System.EventHandler(this.OnWindowDeactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnAltoWindowClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.Leave += new System.EventHandler(this.OnWindowLeave);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.StatusLine.ResumeLayout(false);
            this.StatusLine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).EndInit();
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
        private System.Windows.Forms.StatusStrip StatusLine;
        private System.Windows.Forms.ToolStripStatusLabel CaptureStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem SystemShowDebuggerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem Drive0ImageName;
        private System.Windows.Forms.ToolStripMenuItem Drive1ImageName;
        private System.Windows.Forms.ToolStripStatusLabel SystemStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel DiskStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem SystemEthernetBootMenu;
        private System.Windows.Forms.ToolStripMenuItem AlternateBootToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveScreenshotToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel FPSLabel;
    }
}