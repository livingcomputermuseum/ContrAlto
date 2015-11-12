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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle31 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle35 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle32 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle33 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle34 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle36 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle37 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle38 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle39 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle40 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle41 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle42 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle43 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle44 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle45 = new System.Windows.Forms.DataGridViewCellStyle();
            this.Microcode = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.JumpToAddress = new System.Windows.Forms.TextBox();
            this._sourceViewer = new System.Windows.Forms.DataGridView();
            this.Breakpoint = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.T = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Addr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._registerData = new System.Windows.Forms.DataGridView();
            this.RegNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.R = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.S = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StepButton = new System.Windows.Forms.Button();
            this.AutoStep = new System.Windows.Forms.Button();
            this.RunButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._taskData = new System.Windows.Forms.DataGridView();
            this.TaskName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TaskState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TaskPC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._otherRegs = new System.Windows.Forms.DataGridView();
            this.Reg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RegValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._memoryData = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.ExecutionStateLabel = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this._diskData = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResetButton = new System.Windows.Forms.Button();
            this.RunToNextTaskButton = new System.Windows.Forms.Button();
            this.NovaStep = new System.Windows.Forms.Button();
            this.Bkpt = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Disassembly = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.DisplayBox = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
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
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._diskData)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Microcode
            // 
            this.Microcode.Controls.Add(this.label2);
            this.Microcode.Controls.Add(this.JumpToAddress);
            this.Microcode.Controls.Add(this._sourceViewer);
            this.Microcode.Location = new System.Drawing.Point(3, 3);
            this.Microcode.Name = "Microcode";
            this.Microcode.Size = new System.Drawing.Size(603, 625);
            this.Microcode.TabIndex = 0;
            this.Microcode.TabStop = false;
            this.Microcode.Text = "Microcode Source";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 599);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Jump to:";
            // 
            // JumpToAddress
            // 
            this.JumpToAddress.Location = new System.Drawing.Point(59, 596);
            this.JumpToAddress.Name = "JumpToAddress";
            this.JumpToAddress.Size = new System.Drawing.Size(48, 20);
            this.JumpToAddress.TabIndex = 12;
            this.JumpToAddress.TabStop = false;
            this.JumpToAddress.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnJumpAddressKeyDown);
            // 
            // _sourceViewer
            // 
            this._sourceViewer.AllowUserToAddRows = false;
            this._sourceViewer.AllowUserToDeleteRows = false;
            this._sourceViewer.AllowUserToResizeColumns = false;
            this._sourceViewer.AllowUserToResizeRows = false;
            dataGridViewCellStyle31.BackColor = System.Drawing.Color.Silver;
            this._sourceViewer.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle31;
            this._sourceViewer.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this._sourceViewer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._sourceViewer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Breakpoint,
            this.T,
            this.Addr,
            this.Source});
            this._sourceViewer.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._sourceViewer.Location = new System.Drawing.Point(10, 19);
            this._sourceViewer.Name = "_sourceViewer";
            this._sourceViewer.ReadOnly = true;
            this._sourceViewer.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle35.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle35.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle35.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle35.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle35.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle35.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle35.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._sourceViewer.RowHeadersDefaultCellStyle = dataGridViewCellStyle35;
            this._sourceViewer.RowHeadersVisible = false;
            this._sourceViewer.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._sourceViewer.RowTemplate.Height = 18;
            this._sourceViewer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._sourceViewer.ShowCellErrors = false;
            this._sourceViewer.ShowEditingIcon = false;
            this._sourceViewer.ShowRowErrors = false;
            this._sourceViewer.Size = new System.Drawing.Size(584, 571);
            this._sourceViewer.TabIndex = 1;
            this._sourceViewer.TabStop = false;
            this._sourceViewer.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SourceViewCellClick);
            // 
            // Breakpoint
            // 
            this.Breakpoint.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Breakpoint.FalseValue = "false";
            this.Breakpoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Breakpoint.HeaderText = "B";
            this.Breakpoint.IndeterminateValue = "null";
            this.Breakpoint.Name = "Breakpoint";
            this.Breakpoint.ReadOnly = true;
            this.Breakpoint.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Breakpoint.TrueValue = "true";
            this.Breakpoint.Width = 20;
            // 
            // T
            // 
            this.T.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridViewCellStyle32.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle32.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.T.DefaultCellStyle = dataGridViewCellStyle32;
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
            dataGridViewCellStyle33.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Addr.DefaultCellStyle = dataGridViewCellStyle33;
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
            dataGridViewCellStyle34.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Source.DefaultCellStyle = dataGridViewCellStyle34;
            this.Source.HeaderText = "Source Code";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Source.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            // _registerData
            // 
            this._registerData.AllowUserToAddRows = false;
            this._registerData.AllowUserToDeleteRows = false;
            this._registerData.AllowUserToResizeColumns = false;
            this._registerData.AllowUserToResizeRows = false;
            dataGridViewCellStyle36.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._registerData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle36;
            this._registerData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._registerData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RegNum,
            this.R,
            this.S});
            dataGridViewCellStyle37.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle37.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle37.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle37.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle37.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle37.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle37.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._registerData.DefaultCellStyle = dataGridViewCellStyle37;
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
            this._registerData.TabStop = false;
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
            // StepButton
            // 
            this.StepButton.Location = new System.Drawing.Point(0, 954);
            this.StepButton.Name = "StepButton";
            this.StepButton.Size = new System.Drawing.Size(44, 23);
            this.StepButton.TabIndex = 3;
            this.StepButton.TabStop = false;
            this.StepButton.Text = "Step";
            this.StepButton.UseVisualStyleBackColor = true;
            this.StepButton.Click += new System.EventHandler(this.OnStepButtonClicked);
            // 
            // AutoStep
            // 
            this.AutoStep.Location = new System.Drawing.Point(50, 954);
            this.AutoStep.Name = "AutoStep";
            this.AutoStep.Size = new System.Drawing.Size(47, 23);
            this.AutoStep.TabIndex = 4;
            this.AutoStep.TabStop = false;
            this.AutoStep.Text = "Auto";
            this.AutoStep.UseVisualStyleBackColor = true;
            this.AutoStep.Click += new System.EventHandler(this.OnAutoStepButtonClicked);
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(103, 954);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(41, 23);
            this.RunButton.TabIndex = 5;
            this.RunButton.TabStop = false;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(351, 955);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(43, 23);
            this.StopButton.TabIndex = 6;
            this.StopButton.TabStop = false;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.OnStopButtonClicked);
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
            dataGridViewCellStyle38.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._taskData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle38;
            this._taskData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._taskData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TaskName,
            this.TaskState,
            this.TaskPC});
            dataGridViewCellStyle39.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle39.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle39.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle39.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle39.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle39.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle39.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._taskData.DefaultCellStyle = dataGridViewCellStyle39;
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
            this._taskData.TabStop = false;
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
            dataGridViewCellStyle40.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._otherRegs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle40;
            this._otherRegs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._otherRegs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Reg,
            this.RegValue});
            dataGridViewCellStyle41.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle41.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle41.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle41.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle41.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle41.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle41.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._otherRegs.DefaultCellStyle = dataGridViewCellStyle41;
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
            this.groupBox4.Location = new System.Drawing.Point(172, 634);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(291, 298);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Memory";
            // 
            // _memoryData
            // 
            this._memoryData.AllowUserToAddRows = false;
            this._memoryData.AllowUserToDeleteRows = false;
            this._memoryData.AllowUserToResizeRows = false;
            dataGridViewCellStyle42.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._memoryData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle42;
            this._memoryData.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._memoryData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._memoryData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Bkpt,
            this.Address,
            this.Data,
            this.Disassembly});
            dataGridViewCellStyle43.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle43.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle43.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle43.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle43.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle43.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle43.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._memoryData.DefaultCellStyle = dataGridViewCellStyle43;
            this._memoryData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._memoryData.Location = new System.Drawing.Point(6, 19);
            this._memoryData.MultiSelect = false;
            this._memoryData.Name = "_memoryData";
            this._memoryData.ReadOnly = true;
            this._memoryData.RowHeadersVisible = false;
            this._memoryData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._memoryData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._memoryData.RowTemplate.Height = 18;
            this._memoryData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._memoryData.ShowCellErrors = false;
            this._memoryData.ShowCellToolTips = false;
            this._memoryData.ShowEditingIcon = false;
            this._memoryData.ShowRowErrors = false;
            this._memoryData.Size = new System.Drawing.Size(279, 273);
            this._memoryData.TabIndex = 0;
            this._memoryData.TabStop = false;
            this._memoryData.VirtualMode = true;
            this._memoryData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.MemoryViewCellClick);
            this._memoryData.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.OnMemoryCellValueNeeded);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 935);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Execution State:";
            // 
            // ExecutionStateLabel
            // 
            this.ExecutionStateLabel.AutoSize = true;
            this.ExecutionStateLabel.Location = new System.Drawing.Point(92, 935);
            this.ExecutionStateLabel.Name = "ExecutionStateLabel";
            this.ExecutionStateLabel.Size = new System.Drawing.Size(33, 13);
            this.ExecutionStateLabel.TabIndex = 10;
            this.ExecutionStateLabel.Text = "unset";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this._diskData);
            this.groupBox5.Location = new System.Drawing.Point(3, 634);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(163, 298);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Disk";
            // 
            // _diskData
            // 
            this._diskData.AllowUserToAddRows = false;
            this._diskData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle44.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._diskData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle44;
            this._diskData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._diskData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            dataGridViewCellStyle45.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle45.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle45.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle45.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle45.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle45.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle45.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._diskData.DefaultCellStyle = dataGridViewCellStyle45;
            this._diskData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._diskData.Location = new System.Drawing.Point(6, 19);
            this._diskData.MultiSelect = false;
            this._diskData.Name = "_diskData";
            this._diskData.ReadOnly = true;
            this._diskData.RowHeadersVisible = false;
            this._diskData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._diskData.RowTemplate.Height = 18;
            this._diskData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._diskData.ShowCellErrors = false;
            this._diskData.ShowCellToolTips = false;
            this._diskData.ShowEditingIcon = false;
            this._diskData.ShowRowErrors = false;
            this._diskData.Size = new System.Drawing.Size(147, 273);
            this._diskData.TabIndex = 1;
            this._diskData.TabStop = false;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "Data";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 16;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 55;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "Value";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 16;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(400, 955);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(57, 23);
            this.ResetButton.TabIndex = 12;
            this.ResetButton.TabStop = false;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // RunToNextTaskButton
            // 
            this.RunToNextTaskButton.Location = new System.Drawing.Point(150, 954);
            this.RunToNextTaskButton.Name = "RunToNextTaskButton";
            this.RunToNextTaskButton.Size = new System.Drawing.Size(51, 23);
            this.RunToNextTaskButton.TabIndex = 13;
            this.RunToNextTaskButton.TabStop = false;
            this.RunToNextTaskButton.Text = "Run T";
            this.RunToNextTaskButton.UseVisualStyleBackColor = true;
            this.RunToNextTaskButton.Click += new System.EventHandler(this.RunToNextTaskButton_Click);
            // 
            // NovaStep
            // 
            this.NovaStep.Location = new System.Drawing.Point(207, 954);
            this.NovaStep.Name = "NovaStep";
            this.NovaStep.Size = new System.Drawing.Size(66, 23);
            this.NovaStep.TabIndex = 14;
            this.NovaStep.TabStop = false;
            this.NovaStep.Text = "Nova Step";
            this.NovaStep.UseVisualStyleBackColor = true;
            this.NovaStep.Click += new System.EventHandler(this.NovaStep_Click);
            // 
            // Bkpt
            // 
            this.Bkpt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Bkpt.FalseValue = "false";
            this.Bkpt.HeaderText = "B";
            this.Bkpt.Name = "Bkpt";
            this.Bkpt.ReadOnly = true;
            this.Bkpt.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Bkpt.ToolTipText = "Breakpoint";
            this.Bkpt.TrueValue = "true";
            this.Bkpt.Width = 20;
            // 
            // Address
            // 
            this.Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Address.HeaderText = "Addr";
            this.Address.MinimumWidth = 16;
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            this.Address.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Address.ToolTipText = "Address";
            this.Address.Width = 54;
            // 
            // Data
            // 
            this.Data.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Data.HeaderText = "Data";
            this.Data.MinimumWidth = 16;
            this.Data.Name = "Data";
            this.Data.ReadOnly = true;
            this.Data.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Data.ToolTipText = "Data";
            this.Data.Width = 55;
            // 
            // Disassembly
            // 
            this.Disassembly.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Disassembly.HeaderText = "Disassembly";
            this.Disassembly.Name = "Disassembly";
            this.Disassembly.ReadOnly = true;
            this.Disassembly.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Disassembly.ToolTipText = "Disassembly";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.DisplayBox);
            this.groupBox6.Location = new System.Drawing.Point(758, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(617, 834);
            this.groupBox6.TabIndex = 15;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Display";
            // 
            // DisplayBox
            // 
            this.DisplayBox.BackColor = System.Drawing.SystemColors.Window;
            this.DisplayBox.Location = new System.Drawing.Point(6, 19);
            this.DisplayBox.Name = "DisplayBox";
            this.DisplayBox.Size = new System.Drawing.Size(606, 808);
            this.DisplayBox.TabIndex = 0;
            this.DisplayBox.TabStop = false;
            this.DisplayBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DisplayBox_PreviewKeyDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(799, 873);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.TabStop = false;
            this.button1.Text = "HACK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1376, 997);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.NovaStep);
            this.Controls.Add(this.RunToNextTaskButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.ExecutionStateLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.AutoStep);
            this.Controls.Add(this.StepButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Microcode);
            this.KeyPreview = true;
            this.Name = "Debugger";
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.Debugger_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Debugger_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Debugger_KeyUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Debugger_PreviewKeyDown);
            this.Microcode.ResumeLayout(false);
            this.Microcode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._sourceViewer)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._registerData)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._taskData)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._otherRegs)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._memoryData)).EndInit();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._diskData)).EndInit();
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DisplayBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox Microcode;
        private System.Windows.Forms.DataGridView _sourceViewer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button StepButton;
        private System.Windows.Forms.Button AutoStep;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.DataGridView _registerData;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn R;
        private System.Windows.Forms.DataGridViewTextBoxColumn S;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView _taskData;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskState;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskPC;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView _otherRegs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reg;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegValue;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView _memoryData;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Breakpoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn T;
        private System.Windows.Forms.DataGridViewTextBoxColumn Addr;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ExecutionStateLabel;
        private System.Windows.Forms.TextBox JumpToAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.DataGridView _diskData;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Button RunToNextTaskButton;
        private System.Windows.Forms.Button NovaStep;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Bkpt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
        private System.Windows.Forms.DataGridViewTextBoxColumn Disassembly;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.PictureBox DisplayBox;
        private System.Windows.Forms.Button button1;
    }
}