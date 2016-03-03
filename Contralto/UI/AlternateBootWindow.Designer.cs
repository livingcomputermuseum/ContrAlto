namespace Contralto
{
    partial class AlternateBootOptions
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
            this.EthernetBootFileGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BootFileComboBox = new System.Windows.Forms.ComboBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.EthernetBootRadioButton = new System.Windows.Forms.RadioButton();
            this.DiskBootRadioButton = new System.Windows.Forms.RadioButton();
            this.DiskBootGroupBox = new System.Windows.Forms.GroupBox();
            this.DiskBootAddressTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EthernetBootFileGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.DiskBootGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // EthernetBootFileGroup
            // 
            this.EthernetBootFileGroup.Controls.Add(this.label1);
            this.EthernetBootFileGroup.Controls.Add(this.BootFileComboBox);
            this.EthernetBootFileGroup.Location = new System.Drawing.Point(4, 106);
            this.EthernetBootFileGroup.Name = "EthernetBootFileGroup";
            this.EthernetBootFileGroup.Size = new System.Drawing.Size(397, 62);
            this.EthernetBootFileGroup.TabIndex = 1;
            this.EthernetBootFileGroup.TabStop = false;
            this.EthernetBootFileGroup.Text = "Ethernet Boot File";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(356, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a standard boot file number below, or enter a custom value in octal:";
            // 
            // BootFileComboBox
            // 
            this.BootFileComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.BootFileComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.BootFileComboBox.FormattingEnabled = true;
            this.BootFileComboBox.Location = new System.Drawing.Point(6, 32);
            this.BootFileComboBox.Name = "BootFileComboBox";
            this.BootFileComboBox.Size = new System.Drawing.Size(158, 21);
            this.BootFileComboBox.TabIndex = 0;
            this.BootFileComboBox.SelectedIndexChanged += new System.EventHandler(this.BootFileComboBox_SelectedIndexChanged);
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(245, 174);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(326, 174);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.EthernetBootRadioButton);
            this.groupBox1.Controls.Add(this.DiskBootRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(4, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(397, 43);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alternate Boot Type";
            // 
            // EthernetBootRadioButton
            // 
            this.EthernetBootRadioButton.AutoSize = true;
            this.EthernetBootRadioButton.Location = new System.Drawing.Point(86, 20);
            this.EthernetBootRadioButton.Name = "EthernetBootRadioButton";
            this.EthernetBootRadioButton.Size = new System.Drawing.Size(90, 17);
            this.EthernetBootRadioButton.TabIndex = 1;
            this.EthernetBootRadioButton.TabStop = true;
            this.EthernetBootRadioButton.Text = "Ethernet Boot";
            this.EthernetBootRadioButton.UseVisualStyleBackColor = true;            

            // 
            // DiskBootRadioButton
            // 
            this.DiskBootRadioButton.AutoSize = true;
            this.DiskBootRadioButton.Location = new System.Drawing.Point(9, 20);
            this.DiskBootRadioButton.Name = "DiskBootRadioButton";
            this.DiskBootRadioButton.Size = new System.Drawing.Size(71, 17);
            this.DiskBootRadioButton.TabIndex = 0;
            this.DiskBootRadioButton.TabStop = true;
            this.DiskBootRadioButton.Text = "Disk Boot";
            this.DiskBootRadioButton.UseVisualStyleBackColor = true;
            
            // 
            // DiskBootGroupBox
            // 
            this.DiskBootGroupBox.Controls.Add(this.DiskBootAddressTextBox);
            this.DiskBootGroupBox.Controls.Add(this.label2);
            this.DiskBootGroupBox.Location = new System.Drawing.Point(4, 58);
            this.DiskBootGroupBox.Name = "DiskBootGroupBox";
            this.DiskBootGroupBox.Size = new System.Drawing.Size(397, 42);
            this.DiskBootGroupBox.TabIndex = 2;
            this.DiskBootGroupBox.TabStop = false;
            this.DiskBootGroupBox.Text = "Disk Boot Address";
            // 
            // DiskBootAddressTextBox
            // 
            this.DiskBootAddressTextBox.Location = new System.Drawing.Point(186, 13);
            this.DiskBootAddressTextBox.Name = "DiskBootAddressTextBox";
            this.DiskBootAddressTextBox.Size = new System.Drawing.Size(94, 20);
            this.DiskBootAddressTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(177, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Enter the octal disk address to boot:";
            // 
            // AlternateBootOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 201);
            this.Controls.Add(this.DiskBootGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.EthernetBootFileGroup);
            this.Name = "AlternateBootOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Alternate Boot Options";
            this.EthernetBootFileGroup.ResumeLayout(false);
            this.EthernetBootFileGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.DiskBootGroupBox.ResumeLayout(false);
            this.DiskBootGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox EthernetBootFileGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox BootFileComboBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton EthernetBootRadioButton;
        private System.Windows.Forms.RadioButton DiskBootRadioButton;
        private System.Windows.Forms.GroupBox DiskBootGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DiskBootAddressTextBox;
    }
}