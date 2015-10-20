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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.ExecutionStateLabel = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this._diskData = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResetButton = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this._debugTasks = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RunToNextTaskButton = new System.Windows.Forms.Button();
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
            ((System.ComponentModel.ISupportInitialize)(this._debugTasks)).BeginInit();
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
            this.JumpToAddress.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnJumpAddressKeyDown);
            // 
            // _sourceViewer
            // 
            this._sourceViewer.AllowUserToAddRows = false;
            this._sourceViewer.AllowUserToDeleteRows = false;
            this._sourceViewer.AllowUserToResizeColumns = false;
            this._sourceViewer.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this._sourceViewer.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
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
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._sourceViewer.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this._sourceViewer.RowHeadersVisible = false;
            this._sourceViewer.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._sourceViewer.RowTemplate.Height = 18;
            this._sourceViewer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._sourceViewer.ShowCellErrors = false;
            this._sourceViewer.ShowEditingIcon = false;
            this._sourceViewer.ShowRowErrors = false;
            this._sourceViewer.Size = new System.Drawing.Size(584, 571);
            this._sourceViewer.TabIndex = 1;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.T.DefaultCellStyle = dataGridViewCellStyle2;
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
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Addr.DefaultCellStyle = dataGridViewCellStyle3;
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
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Source.DefaultCellStyle = dataGridViewCellStyle4;
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
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._registerData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            this._registerData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._registerData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RegNum,
            this.R,
            this.S});
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._registerData.DefaultCellStyle = dataGridViewCellStyle7;
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
            // StepButton
            // 
            this.StepButton.Location = new System.Drawing.Point(0, 954);
            this.StepButton.Name = "StepButton";
            this.StepButton.Size = new System.Drawing.Size(44, 23);
            this.StepButton.TabIndex = 3;
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
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(207, 954);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(43, 23);
            this.StopButton.TabIndex = 6;
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
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._taskData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle8;
            this._taskData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._taskData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TaskName,
            this.TaskState,
            this.TaskPC});
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._taskData.DefaultCellStyle = dataGridViewCellStyle9;
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
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._otherRegs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle10;
            this._otherRegs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._otherRegs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Reg,
            this.RegValue});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._otherRegs.DefaultCellStyle = dataGridViewCellStyle11;
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
            this.groupBox4.Location = new System.Drawing.Point(319, 634);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(144, 344);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Memory";
            // 
            // _memoryData
            // 
            this._memoryData.AllowUserToAddRows = false;
            this._memoryData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._memoryData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle12;
            this._memoryData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._memoryData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Data});
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._memoryData.DefaultCellStyle = dataGridViewCellStyle13;
            this._memoryData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._memoryData.Location = new System.Drawing.Point(6, 19);
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
            this._memoryData.Size = new System.Drawing.Size(132, 319);
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
            this.groupBox5.Location = new System.Drawing.Point(150, 634);
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
            dataGridViewCellStyle14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._diskData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle14;
            this._diskData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._diskData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle15.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._diskData.DefaultCellStyle = dataGridViewCellStyle15;
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
            this.ResetButton.Location = new System.Drawing.Point(256, 955);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(57, 23);
            this.ResetButton.TabIndex = 12;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this._debugTasks);
            this.groupBox6.Location = new System.Drawing.Point(3, 634);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(141, 298);
            this.groupBox6.TabIndex = 12;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Debug Tasks";
            // 
            // _debugTasks
            // 
            this._debugTasks.AllowUserToAddRows = false;
            this._debugTasks.AllowUserToDeleteRows = false;
            dataGridViewCellStyle16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._debugTasks.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle16;
            this._debugTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._debugTasks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._debugTasks.DefaultCellStyle = dataGridViewCellStyle17;
            this._debugTasks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._debugTasks.Location = new System.Drawing.Point(6, 19);
            this._debugTasks.MultiSelect = false;
            this._debugTasks.Name = "_debugTasks";
            this._debugTasks.ReadOnly = true;
            this._debugTasks.RowHeadersVisible = false;
            this._debugTasks.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._debugTasks.RowTemplate.Height = 18;
            this._debugTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._debugTasks.ShowCellErrors = false;
            this._debugTasks.ShowCellToolTips = false;
            this._debugTasks.ShowEditingIcon = false;
            this._debugTasks.ShowRowErrors = false;
            this._debugTasks.Size = new System.Drawing.Size(129, 273);
            this._debugTasks.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridViewTextBoxColumn3.HeaderText = "Debug";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 16;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewTextBoxColumn3.Width = 16;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.HeaderText = "Task";
            this.dataGridViewTextBoxColumn4.MinimumWidth = 16;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // RunToNextTaskButton
            // 
            this.RunToNextTaskButton.Location = new System.Drawing.Point(150, 954);
            this.RunToNextTaskButton.Name = "RunToNextTaskButton";
            this.RunToNextTaskButton.Size = new System.Drawing.Size(51, 23);
            this.RunToNextTaskButton.TabIndex = 13;
            this.RunToNextTaskButton.Text = "Run T";
            this.RunToNextTaskButton.UseVisualStyleBackColor = true;
            this.RunToNextTaskButton.Click += new System.EventHandler(this.RunToNextTaskButton_Click);
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 997);
            this.Controls.Add(this.RunToNextTaskButton);
            this.Controls.Add(this.groupBox6);
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
            this.Name = "Debugger";
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.Debugger_Load);
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
            ((System.ComponentModel.ISupportInitialize)(this._debugTasks)).EndInit();
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
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
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
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.DataGridView _debugTasks;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Button RunToNextTaskButton;
    }
}