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
            this.EthernetBootEnabled = new System.Windows.Forms.CheckBox();
            this.BootFileGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BootFileComboBox = new System.Windows.Forms.ComboBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.BootFileGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // EthernetBootEnabled
            // 
            this.EthernetBootEnabled.AutoSize = true;
            this.EthernetBootEnabled.Location = new System.Drawing.Point(7, 12);
            this.EthernetBootEnabled.Name = "EthernetBootEnabled";
            this.EthernetBootEnabled.Size = new System.Drawing.Size(133, 17);
            this.EthernetBootEnabled.TabIndex = 0;
            this.EthernetBootEnabled.Text = "Ethernet Boot Enabled";
            this.EthernetBootEnabled.UseVisualStyleBackColor = true;
            this.EthernetBootEnabled.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // BootFileGroup
            // 
            this.BootFileGroup.Controls.Add(this.label1);
            this.BootFileGroup.Controls.Add(this.BootFileComboBox);
            this.BootFileGroup.Location = new System.Drawing.Point(4, 35);
            this.BootFileGroup.Name = "BootFileGroup";
            this.BootFileGroup.Size = new System.Drawing.Size(397, 62);
            this.BootFileGroup.TabIndex = 1;
            this.BootFileGroup.TabStop = false;
            this.BootFileGroup.Text = "Boot File";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(359, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a standard boot file number below, or enter a custom value in octal. ";
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
            this.OKButton.Location = new System.Drawing.Point(245, 103);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(326, 103);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // AlternateBootOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 128);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.BootFileGroup);
            this.Controls.Add(this.EthernetBootEnabled);
            this.Name = "AlternateBootOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ethernet Boot Options";
            this.BootFileGroup.ResumeLayout(false);
            this.BootFileGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox EthernetBootEnabled;
        private System.Windows.Forms.GroupBox BootFileGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox BootFileComboBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
    }
}