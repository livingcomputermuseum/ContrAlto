namespace Contralto
{
    partial class Debugger
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            this.Microcode = new System.Windows.Forms.GroupBox();
            this._sourceViewer = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.StepButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this._registerData = new System.Windows.Forms.DataGridView();
            this.RegNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.R = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.S = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._taskData = new System.Windows.Forms.DataGridView();
            this.TaskName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TaskState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TaskPC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.T = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Addr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._otherRegs = new System.Windows.Forms.DataGridView();
            this.Reg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RegValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._memoryData = new System.Windows.Forms.DataGridView();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Microcode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._sourceViewer)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._registerData)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._taskData)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._otherRegs)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._memoryData)).BeginInit();
            this.SuspendLayout();
            // 
            // Microcode
            // 
            this.Microcode.Controls.Add(this._sourceViewer);
            this.Microcode.Location = new System.Drawing.Point(3, 3);
            this.Microcode.Name = "Microcode";
            this.Microcode.Size = new System.Drawing.Size(603, 625);
            this.Microcode.TabIndex = 0;
            this.Microcode.TabStop = false;
            this.Microcode.Text = "Microcode Source";
            // 
            // _sourceViewer
            // 
            this._sourceViewer.AllowUserToAddRows = false;
            this._sourceViewer.AllowUserToDeleteRows = false;
            this._sourceViewer.AllowUserToResizeColumns = false;
            this._sourceViewer.AllowUserToResizeRows = false;
            dataGridViewCellStyle14.BackColor = System.Drawing.Color.Silver;
            this._sourceViewer.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle14;
            this._sourceViewer.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this._sourceViewer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._sourceViewer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.T,
            this.Addr,
            this.Source});
            this._sourceViewer.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._sourceViewer.Location = new System.Drawing.Point(10, 19);
            this._sourceViewer.Name = "_sourceViewer";
            this._sourceViewer.ReadOnly = true;
            this._sourceViewer.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._sourceViewer.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this._sourceViewer.RowHeadersVisible = false;
            this._sourceViewer.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._sourceViewer.RowTemplate.Height = 18;
            this._sourceViewer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._sourceViewer.ShowCellErrors = false;
            this._sourceViewer.ShowEditingIcon = false;
            this._sourceViewer.ShowRowErrors = false;
            this._sourceViewer.Size = new System.Drawing.Size(584, 600);
            this._sourceViewer.TabIndex = 1;
            this._sourceViewer.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._registerData);
            this.groupBox1.Location = new System.Drawing.Point(614, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(137, 625);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General Registers";
            // 
            // StepButton
            // 
            this.StepButton.Location = new System.Drawing.Point(3, 634);
            this.StepButton.Name = "StepButton";
            this.StepButton.Size = new System.Drawing.Size(44, 23);
            this.StepButton.TabIndex = 3;
            this.StepButton.Text = "Step";
            this.StepButton.UseVisualStyleBackColor = true;
            this.StepButton.Click += new System.EventHandler(this.OnStepButtonClicked);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(54, 634);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(47, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Auto";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(108, 634);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(53, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Run";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(168, 634);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(49, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Stop";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // _registerData
            // 
            this._registerData.AllowUserToAddRows = false;
            this._registerData.AllowUserToDeleteRows = false;
            this._registerData.AllowUserToResizeColumns = false;
            this._registerData.AllowUserToResizeRows = false;
            dataGridViewCellStyle19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._registerData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle19;
            this._registerData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._registerData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RegNum,
            this.R,
            this.S});
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle20.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._registerData.DefaultCellStyle = dataGridViewCellStyle20;
            this._registerData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._registerData.Location = new System.Drawing.Point(7, 19);
            this._registerData.MultiSelect = false;
            this._registerData.Name = "_registerData";
            this._registerData.ReadOnly = true;
            this._registerData.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this._registerData.RowHeadersVisible = false;
            this._registerData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._registerData.RowTemplate.Height = 18;
            this._registerData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this._registerData.ShowCellErrors = false;
            this._registerData.ShowCellToolTips = false;
            this._registerData.ShowEditingIcon = false;
            this._registerData.ShowRowErrors = false;
            this._registerData.Size = new System.Drawing.Size(123, 600);
            this._registerData.TabIndex = 0;
            // 
            // RegNum
            // 
            this.RegNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.RegNum.HeaderText = "#";
            this.RegNum.Name = "RegNum";
            this.RegNum.ReadOnly = true;
            this.RegNum.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.RegNum.Width = 20;
            // 
            // R
            // 
            this.R.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.R.HeaderText = "R";
            this.R.Name = "R";
            this.R.ReadOnly = true;
            this.R.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.R.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.R.Width = 50;
            // 
            // S
            // 
            this.S.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.S.HeaderText = "S";
            this.S.Name = "S";
            this.S.ReadOnly = true;
            this.S.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.S.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.S.Width = 50;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._taskData);
            this.groupBox2.Location = new System.Drawing.Point(469, 634);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(137, 344);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tasks";
            // 
            // _taskData
            // 
            this._taskData.AllowUserToAddRows = false;
            this._taskData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._taskData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle21;
            this._taskData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._taskData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TaskName,
            this.TaskState,
            this.TaskPC});
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle22.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle22.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle22.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle22.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle22.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle22.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._taskData.DefaultCellStyle = dataGridViewCellStyle22;
            this._taskData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._taskData.Location = new System.Drawing.Point(7, 19);
            this._taskData.MultiSelect = false;
            this._taskData.Name = "_taskData";
            this._taskData.ReadOnly = true;
            this._taskData.RowHeadersVisible = false;
            this._taskData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._taskData.RowTemplate.Height = 18;
            this._taskData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._taskData.ShowCellErrors = false;
            this._taskData.ShowCellToolTips = false;
            this._taskData.ShowEditingIcon = false;
            this._taskData.ShowRowErrors = false;
            this._taskData.Size = new System.Drawing.Size(123, 319);
            this._taskData.TabIndex = 0;
            // 
            // TaskName
            // 
            this.TaskName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.TaskName.HeaderText = "T";
            this.TaskName.MinimumWidth = 16;
            this.TaskName.Name = "TaskName";
            this.TaskName.ReadOnly = true;
            this.TaskName.Width = 16;
            // 
            // TaskState
            // 
            this.TaskState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            this.TaskState.HeaderText = "S";
            this.TaskState.MinimumWidth = 16;
            this.TaskState.Name = "TaskState";
            this.TaskState.ReadOnly = true;
            this.TaskState.Width = 16;
            // 
            // TaskPC
            // 
            this.TaskPC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TaskPC.HeaderText = "uPC";
            this.TaskPC.Name = "TaskPC";
            this.TaskPC.ReadOnly = true;
            // 
            // T
            // 
            this.T.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.T.DefaultCellStyle = dataGridViewCellStyle15;
            this.T.HeaderText = "T";
            this.T.Name = "T";
            this.T.ReadOnly = true;
            this.T.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.T.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.T.Width = 5;
            // 
            // Addr
            // 
            this.Addr.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridViewCellStyle16.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Addr.DefaultCellStyle = dataGridViewCellStyle16;
            this.Addr.HeaderText = "Addr";
            this.Addr.Name = "Addr";
            this.Addr.ReadOnly = true;
            this.Addr.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Addr.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Addr.Width = 5;
            // 
            // Source
            // 
            this.Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Source.DefaultCellStyle = dataGridViewCellStyle17;
            this.Source.HeaderText = "Source Code";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Source.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._otherRegs);
            this.groupBox3.Location = new System.Drawing.Point(612, 634);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(137, 344);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Other Registers";
            // 
            // _otherRegs
            // 
            this._otherRegs.AllowUserToAddRows = false;
            this._otherRegs.AllowUserToDeleteRows = false;
            dataGridViewCellStyle23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._otherRegs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle23;
            this._otherRegs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._otherRegs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Reg,
            this.RegValue});
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle24.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle24.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._otherRegs.DefaultCellStyle = dataGridViewCellStyle24;
            this._otherRegs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._otherRegs.Location = new System.Drawing.Point(7, 19);
            this._otherRegs.MultiSelect = false;
            this._otherRegs.Name = "_otherRegs";
            this._otherRegs.ReadOnly = true;
            this._otherRegs.RowHeadersVisible = false;
            this._otherRegs.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._otherRegs.RowTemplate.Height = 18;
            this._otherRegs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._otherRegs.ShowCellErrors = false;
            this._otherRegs.ShowCellToolTips = false;
            this._otherRegs.ShowEditingIcon = false;
            this._otherRegs.ShowRowErrors = false;
            this._otherRegs.Size = new System.Drawing.Size(123, 319);
            this._otherRegs.TabIndex = 0;
            // 
            // Reg
            // 
            this.Reg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Reg.HeaderText = "Reg";
            this.Reg.MinimumWidth = 16;
            this.Reg.Name = "Reg";
            this.Reg.ReadOnly = true;
            this.Reg.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Reg.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Reg.Width = 16;
            // 
            // RegValue
            // 
            this.RegValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RegValue.HeaderText = "Value";
            this.RegValue.MinimumWidth = 16;
            this.RegValue.Name = "RegValue";
            this.RegValue.ReadOnly = true;
            this.RegValue.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.RegValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this._memoryData);
            this.groupBox4.Location = new System.Drawing.Point(223, 634);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(240, 344);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Memory";
            // 
            // _memoryData
            // 
            this._memoryData.AllowUserToAddRows = false;
            this._memoryData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle25.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._memoryData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle25;
            this._memoryData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._memoryData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Data});
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle26.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._memoryData.DefaultCellStyle = dataGridViewCellStyle26;
            this._memoryData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._memoryData.Location = new System.Drawing.Point(7, 19);
            this._memoryData.MultiSelect = false;
            this._memoryData.Name = "_memoryData";
            this._memoryData.ReadOnly = true;
            this._memoryData.RowHeadersVisible = false;
            this._memoryData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._memoryData.RowTemplate.Height = 18;
            this._memoryData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._memoryData.ShowCellErrors = false;
            this._memoryData.ShowCellToolTips = false;
            this._memoryData.ShowEditingIcon = false;
            this._memoryData.ShowRowErrors = false;
            this._memoryData.Size = new System.Drawing.Size(227, 319);
            this._memoryData.TabIndex = 0;
            // 
            // Address
            // 
            this.Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Address.HeaderText = "Addr";
            this.Address.MinimumWidth = 16;
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            this.Address.Width = 54;
            // 
            // Data
            // 
            this.Data.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Data.HeaderText = "Data";
            this.Data.MinimumWidth = 16;
            this.Data.Name = "Data";
            this.Data.ReadOnly = true;
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 997);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.StepButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Microcode);
            this.Name = "Debugger";
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.Debugger_Load);
            this.Microcode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._sourceViewer)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._registerData)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._taskData)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._otherRegs)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._memoryData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox Microcode;
        private System.Windows.Forms.DataGridView _sourceViewer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button StepButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridView _registerData;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn R;
        private System.Windows.Forms.DataGridViewTextBoxColumn S;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView _taskData;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskState;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskPC;
        private System.Windows.Forms.DataGridViewTextBoxColumn T;
        private System.Windows.Forms.DataGridViewTextBoxColumn Addr;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView _otherRegs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reg;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegValue;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView _memoryData;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
    }
}