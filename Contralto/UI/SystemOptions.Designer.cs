namespace Contralto.UI
{
    partial class SystemOptions
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.AltoII3KRAMRadioButton = new System.Windows.Forms.RadioButton();
            this.AltoII2KROMRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.AltoII1KROMRadioButton = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RawEthernetRadioButton = new System.Windows.Forms.RadioButton();
            this.UDPRadioButton = new System.Windows.Forms.RadioButton();
            this.HostInterfaceGroupBox = new System.Windows.Forms.GroupBox();
            this.EthernetInterfaceListBox = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.AltoEthernetAddressTextBox = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ThrottleSpeedCheckBox = new System.Windows.Forms.CheckBox();
            this.InterlaceDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.HostInterfaceGroupBox.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(3, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(368, 227);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.AltoII3KRAMRadioButton);
            this.tabPage1.Controls.Add(this.AltoII2KROMRadioButton);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.AltoII1KROMRadioButton);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(360, 201);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "CPU";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // AltoII3KRAMRadioButton
            // 
            this.AltoII3KRAMRadioButton.AutoSize = true;
            this.AltoII3KRAMRadioButton.Location = new System.Drawing.Point(14, 75);
            this.AltoII3KRAMRadioButton.Name = "AltoII3KRAMRadioButton";
            this.AltoII3KRAMRadioButton.Size = new System.Drawing.Size(217, 17);
            this.AltoII3KRAMRadioButton.TabIndex = 3;
            this.AltoII3KRAMRadioButton.TabStop = true;
            this.AltoII3KRAMRadioButton.Text = "Alto II, 1K Control ROM, 3K Control RAM";
            this.AltoII3KRAMRadioButton.UseVisualStyleBackColor = true;
            this.AltoII3KRAMRadioButton.CheckedChanged += new System.EventHandler(this.OnSystemTypeCheckChanged);
            // 
            // AltoII2KROMRadioButton
            // 
            this.AltoII2KROMRadioButton.AutoSize = true;
            this.AltoII2KROMRadioButton.Location = new System.Drawing.Point(14, 52);
            this.AltoII2KROMRadioButton.Name = "AltoII2KROMRadioButton";
            this.AltoII2KROMRadioButton.Size = new System.Drawing.Size(217, 17);
            this.AltoII2KROMRadioButton.TabIndex = 2;
            this.AltoII2KROMRadioButton.TabStop = true;
            this.AltoII2KROMRadioButton.Text = "Alto II, 2K Control ROM, 1K Control RAM";
            this.AltoII2KROMRadioButton.UseVisualStyleBackColor = true;
            this.AltoII2KROMRadioButton.CheckedChanged += new System.EventHandler(this.OnSystemTypeCheckChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "System configuration:";
            // 
            // AltoII1KROMRadioButton
            // 
            this.AltoII1KROMRadioButton.AutoSize = true;
            this.AltoII1KROMRadioButton.Location = new System.Drawing.Point(14, 29);
            this.AltoII1KROMRadioButton.Name = "AltoII1KROMRadioButton";
            this.AltoII1KROMRadioButton.Size = new System.Drawing.Size(217, 17);
            this.AltoII1KROMRadioButton.TabIndex = 0;
            this.AltoII1KROMRadioButton.TabStop = true;
            this.AltoII1KROMRadioButton.Text = "Alto II, 1K Control ROM, 1K Control RAM";
            this.AltoII1KROMRadioButton.UseVisualStyleBackColor = true;
            this.AltoII1KROMRadioButton.CheckedChanged += new System.EventHandler(this.OnSystemTypeCheckChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.HostInterfaceGroupBox);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.AltoEthernetAddressTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(360, 201);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Ethernet";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RawEthernetRadioButton);
            this.groupBox1.Controls.Add(this.UDPRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(10, 30);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 39);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ethernet Encapsulation";
            // 
            // RawEthernetRadioButton
            // 
            this.RawEthernetRadioButton.AutoSize = true;
            this.RawEthernetRadioButton.Location = new System.Drawing.Point(77, 16);
            this.RawEthernetRadioButton.Name = "RawEthernetRadioButton";
            this.RawEthernetRadioButton.Size = new System.Drawing.Size(186, 17);
            this.RawEthernetRadioButton.TabIndex = 1;
            this.RawEthernetRadioButton.TabStop = true;
            this.RawEthernetRadioButton.Text = "Raw Ethernet (requires WinPCAP)";
            this.RawEthernetRadioButton.UseVisualStyleBackColor = true;
            this.RawEthernetRadioButton.CheckedChanged += new System.EventHandler(this.OnEthernetTypeCheckedChanged);
            // 
            // UDPRadioButton
            // 
            this.UDPRadioButton.AutoSize = true;
            this.UDPRadioButton.Location = new System.Drawing.Point(9, 16);
            this.UDPRadioButton.Name = "UDPRadioButton";
            this.UDPRadioButton.Size = new System.Drawing.Size(48, 17);
            this.UDPRadioButton.TabIndex = 0;
            this.UDPRadioButton.TabStop = true;
            this.UDPRadioButton.Text = "UDP";
            this.UDPRadioButton.UseVisualStyleBackColor = true;
            this.UDPRadioButton.CheckedChanged += new System.EventHandler(this.OnEthernetTypeCheckedChanged);
            // 
            // HostInterfaceGroupBox
            // 
            this.HostInterfaceGroupBox.Controls.Add(this.EthernetInterfaceListBox);
            this.HostInterfaceGroupBox.Controls.Add(this.label4);
            this.HostInterfaceGroupBox.Location = new System.Drawing.Point(10, 75);
            this.HostInterfaceGroupBox.Name = "HostInterfaceGroupBox";
            this.HostInterfaceGroupBox.Size = new System.Drawing.Size(335, 120);
            this.HostInterfaceGroupBox.TabIndex = 2;
            this.HostInterfaceGroupBox.TabStop = false;
            this.HostInterfaceGroupBox.Text = "Host Interface";
            // 
            // EthernetInterfaceListBox
            // 
            this.EthernetInterfaceListBox.FormattingEnabled = true;
            this.EthernetInterfaceListBox.Location = new System.Drawing.Point(9, 41);
            this.EthernetInterfaceListBox.Name = "EthernetInterfaceListBox";
            this.EthernetInterfaceListBox.Size = new System.Drawing.Size(326, 69);
            this.EthernetInterfaceListBox.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(245, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Select the Network interface to use with ContrAlto:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Alto Address (octal):";
            // 
            // AltoEthernetAddressTextBox
            // 
            this.AltoEthernetAddressTextBox.Location = new System.Drawing.Point(114, 10);
            this.AltoEthernetAddressTextBox.Name = "AltoEthernetAddressTextBox";
            this.AltoEthernetAddressTextBox.Size = new System.Drawing.Size(49, 20);
            this.AltoEthernetAddressTextBox.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.ThrottleSpeedCheckBox);
            this.tabPage3.Controls.Add(this.InterlaceDisplayCheckBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(360, 201);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Display";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // ThrottleSpeedCheckBox
            // 
            this.ThrottleSpeedCheckBox.AutoSize = true;
            this.ThrottleSpeedCheckBox.Location = new System.Drawing.Point(19, 22);
            this.ThrottleSpeedCheckBox.Name = "ThrottleSpeedCheckBox";
            this.ThrottleSpeedCheckBox.Size = new System.Drawing.Size(188, 17);
            this.ThrottleSpeedCheckBox.TabIndex = 1;
            this.ThrottleSpeedCheckBox.Text = "Throttle Framerate at 60 fields/sec";
            this.ThrottleSpeedCheckBox.UseVisualStyleBackColor = true;
            // 
            // InterlaceDisplayCheckBox
            // 
            this.InterlaceDisplayCheckBox.AutoSize = true;
            this.InterlaceDisplayCheckBox.Location = new System.Drawing.Point(19, 45);
            this.InterlaceDisplayCheckBox.Name = "InterlaceDisplayCheckBox";
            this.InterlaceDisplayCheckBox.Size = new System.Drawing.Size(196, 17);
            this.InterlaceDisplayCheckBox.TabIndex = 0;
            this.InterlaceDisplayCheckBox.Text = "Interlaced Display (headache mode)";
            this.InterlaceDisplayCheckBox.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(211, 239);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(292, 239);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SystemOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 271);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.tabControl1);
            this.Name = "SystemOptions";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "System Options";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.HostInterfaceGroupBox.ResumeLayout(false);
            this.HostInterfaceGroupBox.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RadioButton AltoII3KRAMRadioButton;
        private System.Windows.Forms.RadioButton AltoII2KROMRadioButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton AltoII1KROMRadioButton;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox HostInterfaceGroupBox;
        private System.Windows.Forms.ListBox EthernetInterfaceListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox AltoEthernetAddressTextBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.CheckBox ThrottleSpeedCheckBox;
        private System.Windows.Forms.CheckBox InterlaceDisplayCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RawEthernetRadioButton;
        private System.Windows.Forms.RadioButton UDPRadioButton;
    }
}