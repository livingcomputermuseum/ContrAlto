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
            this.OptionsTabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.AltoI1KROMRadioButton = new System.Windows.Forms.RadioButton();
            this.AltoII3KRAMRadioButton = new System.Windows.Forms.RadioButton();
            this.AltoII2KROMRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.AltoII1KROMRadioButton = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.NoEncapsulationRadioButton = new System.Windows.Forms.RadioButton();
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
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.DACOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.DACOutputCapturePathTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EnableDACCaptureCheckBox = new System.Windows.Forms.CheckBox();
            this.EnableDACCheckBox = new System.Windows.Forms.CheckBox();
            this.PrintingTab = new System.Windows.Forms.TabPage();
            this.PrintingOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.ReversePageOrderCheckBox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.PrintOutputPathTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.EnablePrintingCheckBox = new System.Windows.Forms.CheckBox();
            this.DialogOKButton = new System.Windows.Forms.Button();
            this.DialogCancelButton = new System.Windows.Forms.Button();
            this.OptionsTabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.HostInterfaceGroupBox.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.DACOptionsGroupBox.SuspendLayout();
            this.PrintingTab.SuspendLayout();
            this.PrintingOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // OptionsTabs
            // 
            this.OptionsTabs.Controls.Add(this.tabPage1);
            this.OptionsTabs.Controls.Add(this.tabPage2);
            this.OptionsTabs.Controls.Add(this.tabPage3);
            this.OptionsTabs.Controls.Add(this.tabPage4);
            this.OptionsTabs.Controls.Add(this.PrintingTab);
            this.OptionsTabs.Location = new System.Drawing.Point(3, 5);
            this.OptionsTabs.Name = "OptionsTabs";
            this.OptionsTabs.SelectedIndex = 0;
            this.OptionsTabs.Size = new System.Drawing.Size(368, 227);
            this.OptionsTabs.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.AltoI1KROMRadioButton);
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
            // AltoI1KROMRadioButton
            // 
            this.AltoI1KROMRadioButton.AutoSize = true;
            this.AltoI1KROMRadioButton.Location = new System.Drawing.Point(14, 30);
            this.AltoI1KROMRadioButton.Name = "AltoI1KROMRadioButton";
            this.AltoI1KROMRadioButton.Size = new System.Drawing.Size(214, 17);
            this.AltoI1KROMRadioButton.TabIndex = 4;
            this.AltoI1KROMRadioButton.Text = "Alto I, 1K Control ROM, 1K Control RAM";
            this.AltoI1KROMRadioButton.UseVisualStyleBackColor = true;
            // 
            // AltoII3KRAMRadioButton
            // 
            this.AltoII3KRAMRadioButton.AutoSize = true;
            this.AltoII3KRAMRadioButton.Location = new System.Drawing.Point(14, 99);
            this.AltoII3KRAMRadioButton.Name = "AltoII3KRAMRadioButton";
            this.AltoII3KRAMRadioButton.Size = new System.Drawing.Size(236, 17);
            this.AltoII3KRAMRadioButton.TabIndex = 3;
            this.AltoII3KRAMRadioButton.Text = "Alto II XM, 1K Control ROM, 3K Control RAM";
            this.AltoII3KRAMRadioButton.UseVisualStyleBackColor = true;
            this.AltoII3KRAMRadioButton.CheckedChanged += new System.EventHandler(this.OnSystemTypeCheckChanged);
            // 
            // AltoII2KROMRadioButton
            // 
            this.AltoII2KROMRadioButton.AutoSize = true;
            this.AltoII2KROMRadioButton.Location = new System.Drawing.Point(14, 76);
            this.AltoII2KROMRadioButton.Name = "AltoII2KROMRadioButton";
            this.AltoII2KROMRadioButton.Size = new System.Drawing.Size(236, 17);
            this.AltoII2KROMRadioButton.TabIndex = 2;
            this.AltoII2KROMRadioButton.Text = "Alto II XM, 2K Control ROM, 1K Control RAM";
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
            this.AltoII1KROMRadioButton.Checked = true;
            this.AltoII1KROMRadioButton.Location = new System.Drawing.Point(14, 53);
            this.AltoII1KROMRadioButton.Name = "AltoII1KROMRadioButton";
            this.AltoII1KROMRadioButton.Size = new System.Drawing.Size(236, 17);
            this.AltoII1KROMRadioButton.TabIndex = 0;
            this.AltoII1KROMRadioButton.TabStop = true;
            this.AltoII1KROMRadioButton.Text = "Alto II XM, 1K Control ROM, 1K Control RAM";
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
            this.groupBox1.Controls.Add(this.NoEncapsulationRadioButton);
            this.groupBox1.Controls.Add(this.RawEthernetRadioButton);
            this.groupBox1.Controls.Add(this.UDPRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(10, 30);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 39);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ethernet Encapsulation";
            // 
            // NoEncapsulationRadioButton
            // 
            this.NoEncapsulationRadioButton.AutoSize = true;
            this.NoEncapsulationRadioButton.Location = new System.Drawing.Point(255, 16);
            this.NoEncapsulationRadioButton.Name = "NoEncapsulationRadioButton";
            this.NoEncapsulationRadioButton.Size = new System.Drawing.Size(51, 17);
            this.NoEncapsulationRadioButton.TabIndex = 2;
            this.NoEncapsulationRadioButton.TabStop = true;
            this.NoEncapsulationRadioButton.Text = "None";
            this.NoEncapsulationRadioButton.UseVisualStyleBackColor = true;
            // 
            // RawEthernetRadioButton
            // 
            this.RawEthernetRadioButton.AutoSize = true;
            this.RawEthernetRadioButton.Location = new System.Drawing.Point(63, 16);
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
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.DACOptionsGroupBox);
            this.tabPage4.Controls.Add(this.EnableDACCheckBox);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(360, 201);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "DAC";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // DACOptionsGroupBox
            // 
            this.DACOptionsGroupBox.Controls.Add(this.BrowseButton);
            this.DACOptionsGroupBox.Controls.Add(this.DACOutputCapturePathTextBox);
            this.DACOptionsGroupBox.Controls.Add(this.label2);
            this.DACOptionsGroupBox.Controls.Add(this.EnableDACCaptureCheckBox);
            this.DACOptionsGroupBox.Location = new System.Drawing.Point(15, 52);
            this.DACOptionsGroupBox.Name = "DACOptionsGroupBox";
            this.DACOptionsGroupBox.Size = new System.Drawing.Size(335, 139);
            this.DACOptionsGroupBox.TabIndex = 1;
            this.DACOptionsGroupBox.TabStop = false;
            this.DACOptionsGroupBox.Text = "DAC options";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(251, 53);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 3;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // DACOutputCapturePathTextBox
            // 
            this.DACOutputCapturePathTextBox.Location = new System.Drawing.Point(127, 55);
            this.DACOutputCapturePathTextBox.Name = "DACOutputCapturePathTextBox";
            this.DACOutputCapturePathTextBox.Size = new System.Drawing.Size(110, 20);
            this.DACOutputCapturePathTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output capture path:";
            // 
            // EnableDACCaptureCheckBox
            // 
            this.EnableDACCaptureCheckBox.AutoSize = true;
            this.EnableDACCaptureCheckBox.Location = new System.Drawing.Point(18, 28);
            this.EnableDACCaptureCheckBox.Name = "EnableDACCaptureCheckBox";
            this.EnableDACCaptureCheckBox.Size = new System.Drawing.Size(156, 17);
            this.EnableDACCaptureCheckBox.TabIndex = 0;
            this.EnableDACCaptureCheckBox.Text = "Enable DAC output capture";
            this.EnableDACCaptureCheckBox.UseVisualStyleBackColor = true;
            this.EnableDACCaptureCheckBox.CheckedChanged += new System.EventHandler(this.EnableDACCaptureCheckBox_CheckedChanged);
            // 
            // EnableDACCheckBox
            // 
            this.EnableDACCheckBox.AutoSize = true;
            this.EnableDACCheckBox.Location = new System.Drawing.Point(19, 22);
            this.EnableDACCheckBox.Name = "EnableDACCheckBox";
            this.EnableDACCheckBox.Size = new System.Drawing.Size(275, 17);
            this.EnableDACCheckBox.TabIndex = 0;
            this.EnableDACCheckBox.Text = "Enable Audio DAC (Used by Smalltalk Music System)";
            this.EnableDACCheckBox.UseVisualStyleBackColor = true;
            this.EnableDACCheckBox.CheckedChanged += new System.EventHandler(this.OnEnableDACCheckboxChanged);
            // 
            // PrintingTab
            // 
            this.PrintingTab.Controls.Add(this.PrintingOptionsGroupBox);
            this.PrintingTab.Controls.Add(this.EnablePrintingCheckBox);
            this.PrintingTab.Location = new System.Drawing.Point(4, 22);
            this.PrintingTab.Name = "PrintingTab";
            this.PrintingTab.Padding = new System.Windows.Forms.Padding(3);
            this.PrintingTab.Size = new System.Drawing.Size(360, 201);
            this.PrintingTab.TabIndex = 4;
            this.PrintingTab.Text = "Printing";
            this.PrintingTab.UseVisualStyleBackColor = true;
            // 
            // PrintingOptionsGroupBox
            // 
            this.PrintingOptionsGroupBox.Controls.Add(this.ReversePageOrderCheckBox);
            this.PrintingOptionsGroupBox.Controls.Add(this.button1);
            this.PrintingOptionsGroupBox.Controls.Add(this.PrintOutputPathTextBox);
            this.PrintingOptionsGroupBox.Controls.Add(this.label5);
            this.PrintingOptionsGroupBox.Location = new System.Drawing.Point(14, 52);
            this.PrintingOptionsGroupBox.Name = "PrintingOptionsGroupBox";
            this.PrintingOptionsGroupBox.Size = new System.Drawing.Size(335, 139);
            this.PrintingOptionsGroupBox.TabIndex = 3;
            this.PrintingOptionsGroupBox.TabStop = false;
            this.PrintingOptionsGroupBox.Text = "Printing options";
            // 
            // ReversePageOrderCheckBox
            // 
            this.ReversePageOrderCheckBox.AutoSize = true;
            this.ReversePageOrderCheckBox.Location = new System.Drawing.Point(22, 51);
            this.ReversePageOrderCheckBox.Name = "ReversePageOrderCheckBox";
            this.ReversePageOrderCheckBox.Size = new System.Drawing.Size(158, 17);
            this.ReversePageOrderCheckBox.TabIndex = 4;
            this.ReversePageOrderCheckBox.Text = "Reverse Output Page Order";
            this.ReversePageOrderCheckBox.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(254, 24);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnPrintOutputBrowseButtonClicked);
            // 
            // PrintOutputPathTextBox
            // 
            this.PrintOutputPathTextBox.Location = new System.Drawing.Point(113, 25);
            this.PrintOutputPathTextBox.Name = "PrintOutputPathTextBox";
            this.PrintOutputPathTextBox.Size = new System.Drawing.Size(125, 20);
            this.PrintOutputPathTextBox.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "PDF output path:";
            // 
            // EnablePrintingCheckBox
            // 
            this.EnablePrintingCheckBox.AutoSize = true;
            this.EnablePrintingCheckBox.Location = new System.Drawing.Point(19, 22);
            this.EnablePrintingCheckBox.Name = "EnablePrintingCheckBox";
            this.EnablePrintingCheckBox.Size = new System.Drawing.Size(211, 17);
            this.EnablePrintingCheckBox.TabIndex = 2;
            this.EnablePrintingCheckBox.Text = "Enable Printing (via Orbit / Dover ROS)";
            this.EnablePrintingCheckBox.UseVisualStyleBackColor = true;
            this.EnablePrintingCheckBox.CheckedChanged += new System.EventHandler(this.EnablePrintingCheckBox_CheckedChanged);
            // 
            // DialogOKButton
            // 
            this.DialogOKButton.Location = new System.Drawing.Point(211, 239);
            this.DialogOKButton.Name = "DialogOKButton";
            this.DialogOKButton.Size = new System.Drawing.Size(75, 23);
            this.DialogOKButton.TabIndex = 1;
            this.DialogOKButton.Text = "OK";
            this.DialogOKButton.UseVisualStyleBackColor = true;
            this.DialogOKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // DialogCancelButton
            // 
            this.DialogCancelButton.Location = new System.Drawing.Point(292, 239);
            this.DialogCancelButton.Name = "DialogCancelButton";
            this.DialogCancelButton.Size = new System.Drawing.Size(75, 23);
            this.DialogCancelButton.TabIndex = 2;
            this.DialogCancelButton.Text = "Cancel";
            this.DialogCancelButton.UseVisualStyleBackColor = true;
            this.DialogCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SystemOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 271);
            this.Controls.Add(this.DialogCancelButton);
            this.Controls.Add(this.DialogOKButton);
            this.Controls.Add(this.OptionsTabs);
            this.Name = "SystemOptions";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "System Options";
            this.OptionsTabs.ResumeLayout(false);
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
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.DACOptionsGroupBox.ResumeLayout(false);
            this.DACOptionsGroupBox.PerformLayout();
            this.PrintingTab.ResumeLayout(false);
            this.PrintingTab.PerformLayout();
            this.PrintingOptionsGroupBox.ResumeLayout(false);
            this.PrintingOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl OptionsTabs;
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
        private System.Windows.Forms.Button DialogOKButton;
        private System.Windows.Forms.Button DialogCancelButton;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.CheckBox ThrottleSpeedCheckBox;
        private System.Windows.Forms.CheckBox InterlaceDisplayCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RawEthernetRadioButton;
        private System.Windows.Forms.RadioButton UDPRadioButton;
        private System.Windows.Forms.RadioButton NoEncapsulationRadioButton;
        private System.Windows.Forms.RadioButton AltoI1KROMRadioButton;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox DACOptionsGroupBox;
        private System.Windows.Forms.CheckBox EnableDACCheckBox;
        private System.Windows.Forms.CheckBox EnableDACCaptureCheckBox;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox DACOutputCapturePathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage PrintingTab;
        private System.Windows.Forms.GroupBox PrintingOptionsGroupBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox PrintOutputPathTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox EnablePrintingCheckBox;
        private System.Windows.Forms.CheckBox ReversePageOrderCheckBox;
    }
}