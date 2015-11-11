using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Contralto.CPU;
using System.Threading;
using System.Drawing.Imaging;

namespace Contralto
{
    /// <summary>
    /// A basic & hacky debugger.  To be improved.
    /// </summary>
    public partial class Debugger : Form
    {
        public Debugger(AltoSystem system)
        {
            _system = system;
            _microcodeBreakpointEnabled = new bool[1024];
            _novaBreakpointEnabled = new bool[65536];

            InitializeComponent();
            InitControls();
            RefreshUI();         
        }        

        public void LoadSourceCode(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(path, "Microcode path must be specified.");
            }

            StreamReader sr = new StreamReader(path);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                SourceLine src = new SourceLine(line);

                int i = _sourceViewer.Rows.Add(
                    false,  // breakpoint
                    GetTextForTask(src.Task),
                    src.Address,
                    src.Text);

                // Give the row a color based on the task
                _sourceViewer.Rows[i].DefaultCellStyle.BackColor = GetColorForTask(src.Task);              

                // Tag the row based on the PROM address (if any) to make it easy to find.
                if (!String.IsNullOrEmpty(src.Address))
                {
                    _sourceViewer.Rows[i].Tag = Convert.ToUInt16(src.Address, 8);
                }
            }

            // Ensure the UI view gets refreshed to display the current MPC source
            Refresh();    
        }

        public override void Refresh()
        {
            base.Refresh();

            RefreshUI();
        }        

        public void RefreshAltoDisplay()
        {
            BeginInvoke(new StepDelegate(RefreshDisplayBox));            
        }

        private void RefreshDisplayBox()
        {
            // Update the display
            BitmapData data = _displayBuffer.LockBits(_displayRect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            IntPtr ptr = data.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_displayData, 0, ptr, _displayData.Length);

            _displayBuffer.UnlockBits(data);
            DisplayBox.Refresh();
        }

        /// <summary>
        /// Invoked by the DisplayController to put a word on the emulated screen.
        /// </summary>
        /// <param name="scanline"></param>
        /// <param name="wordOffset"></param>
        /// <param name="word"></param>
        public void DrawDisplayWord(int scanline, int wordOffset, ushort word)
        {
            // TODO: move magic numbers to constants
            int address = scanline * 76 + wordOffset * 2;

            if (address > _displayData.Length)
            {
                throw new InvalidOperationException("Display word address is out of bounds.");
            }

            _displayData[address] = (byte)(word >> 8);
            _displayData[address + 1] = (byte)(word);
        }

        private void RefreshUI()
        {
            // Registers
            for(int i=0;i<32;i++)
            {
                _registerData.Rows[i].Cells[0].Value = Conversion.ToOctal(i,2);
                _registerData.Rows[i].Cells[1].Value = Conversion.ToOctal(_system.CPU.R[i], 6);
                _registerData.Rows[i].Cells[2].Value = Conversion.ToOctal(_system.CPU.S[0][i], 6);
            }

            // Tasks
            for (int i=0;i<16;i++)
            {
                _taskData.Rows[i].Cells[0].Value = GetTextForTask((TaskType)i);
                _taskData.Rows[i].Cells[1].Value = GetTextForTaskState(_system.CPU.Tasks[i]);
                _taskData.Rows[i].Cells[2].Value =
                    _system.CPU.Tasks[i] != null ? Conversion.ToOctal(_system.CPU.Tasks[i].MPC, 4) : String.Empty;                
            }

            // Other registers            
            _otherRegs.Rows[0].Cells[1].Value = Conversion.ToOctal(_system.CPU.L, 6);
            _otherRegs.Rows[1].Cells[1].Value = Conversion.ToOctal(_system.CPU.T, 6);
            _otherRegs.Rows[2].Cells[1].Value = Conversion.ToOctal(_system.CPU.M, 6);
            _otherRegs.Rows[3].Cells[1].Value = Conversion.ToOctal(_system.CPU.IR, 6);
            _otherRegs.Rows[4].Cells[1].Value = Conversion.ToOctal(_system.CPU.ALUC0, 1);
            //_otherRegs.Rows[4].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.Carry, 1);
            //_otherRegs.Rows[4].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.Skip, 1);
            _otherRegs.Rows[5].Cells[1].Value = Conversion.ToOctal(_system.MemoryBus.MAR, 6);
            _otherRegs.Rows[6].Cells[1].Value = Conversion.ToOctal(_system.MemoryBus.MD, 6);
            _otherRegs.Rows[7].Cells[1].Value = Conversion.ToOctal(_system.MemoryBus.Cycle & 0x3f, 2);

            // Disk info
            _diskData.Rows[0].Cells[1].Value = _system.DiskController.ClocksUntilNextSector.ToString("0.00");
            _diskData.Rows[1].Cells[1].Value = _system.DiskController.Cylinder.ToString();
            _diskData.Rows[2].Cells[1].Value = _system.DiskController.SeekCylinder.ToString();
            _diskData.Rows[3].Cells[1].Value = _system.DiskController.Head.ToString();
            _diskData.Rows[4].Cells[1].Value = _system.DiskController.Sector.ToString();
            _diskData.Rows[5].Cells[1].Value = Conversion.ToOctal(_system.DiskController.KDATA, 6);
            _diskData.Rows[6].Cells[1].Value = Conversion.ToOctal(_system.DiskController.KADR, 6);
            _diskData.Rows[7].Cells[1].Value = Conversion.ToOctal(_system.DiskController.KCOM, 6);
            _diskData.Rows[8].Cells[1].Value = Conversion.ToOctal(_system.DiskController.KSTAT, 6);
            _diskData.Rows[9].Cells[1].Value = _system.DiskController.RECNO.ToString();           

            // Find the right source line.
            HighlightMicrocodeSourceLine(_system.CPU.CurrentTask.MPC);

            // Highlight the nova memory location corresponding to the emulator PC.
            // TODO: this should be configurable
            ushort pc = _system.CPU.R[6];

            HighlightNovaSourceLine(pc);                

            // Exec state
            switch(_execState)
            {
                case ExecutionState.Stopped:
                    ExecutionStateLabel.Text = "Stopped";
                    break;

                case ExecutionState.SingleStep:
                    ExecutionStateLabel.Text = "Stepping";
                    break;

                case ExecutionState.AutoStep:
                    ExecutionStateLabel.Text = "Stepping (auto)";
                    break;

                case ExecutionState.Running:
                    ExecutionStateLabel.Text = "Running";
                    break;

                case ExecutionState.BreakpointStop:
                    ExecutionStateLabel.Text = "Stopped (bkpt)";
                    break;

                case ExecutionState.InternalError:
                    ExecutionStateLabel.Text = String.Format("Stopped (error {0})", _lastExceptionText);
                    break;
            }

            // Update the display
            BitmapData data = _displayBuffer.LockBits(_displayRect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            IntPtr ptr = data.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_displayData, 0, ptr, _displayData.Length);

            _displayBuffer.UnlockBits(data);

        }

        private void InitControls()
        {
            for (int i = 0; i < 32; i++)
            {
                _registerData.Rows.Add(-1, -1 ,-1);
            }

            for (int i = 0; i < 16; i++)
            {
                _taskData.Rows.Add("0", "0", "0");
            }

            // TODO: handle extended memory
            _memoryData.RowCount = 65536;            

            _otherRegs.Rows.Add("L", "0");
            _otherRegs.Rows.Add("T", "0");
            _otherRegs.Rows.Add("M", "0");
            _otherRegs.Rows.Add("IR", "0");
            _otherRegs.Rows.Add("ALUC0", "0");
            //_otherRegs.Rows.Add("CARRY", "0");
            //_otherRegs.Rows.Add("SKIP", "0");
            _otherRegs.Rows.Add("MAR", "0");
            _otherRegs.Rows.Add("MD", "0");
            _otherRegs.Rows.Add("MCycle", "0");            

            _diskData.Rows.Add("Cycles", "0");
            _diskData.Rows.Add("Cylinder", "0");
            _diskData.Rows.Add("D.Cylinder", "0");
            _diskData.Rows.Add("Head", "0");
            _diskData.Rows.Add("Sector", "0");
            _diskData.Rows.Add("KDATA", "0");
            _diskData.Rows.Add("KADR", "0");
            _diskData.Rows.Add("KCOM", "0");
            _diskData.Rows.Add("KSTAT", "0");
            _diskData.Rows.Add("RECNO", "0");

            _displayBuffer = new Bitmap(608, 808, PixelFormat.Format1bppIndexed);

            DisplayBox.Image = _displayBuffer;

        }

        /// <summary>
        /// Handle breakpoint placement on column 0.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceViewCellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check for breakpoint column click.
            if (e.ColumnIndex == 0)
            {
                // See if this is a source line, if so check/uncheck the box
                // and set/unset a breakpoint for the line
                if (_sourceViewer.Rows[e.RowIndex].Tag != null)
                {
                    bool value = (bool)_sourceViewer.Rows[e.RowIndex].Cells[0].Value;
                    _sourceViewer.Rows[e.RowIndex].Cells[0].Value = !value;

                    ModifyMicrocodeBreakpoint((UInt16)_sourceViewer.Rows[e.RowIndex].Tag, !value);
                }

            }
        }

        private void MemoryViewCellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check for breakpoint column click.
            if (e.ColumnIndex == 0)
            {
                // Check/uncheck the box and set/unset a breakpoint for the line                
                bool value = (bool)_memoryData.Rows[e.RowIndex].Cells[0].Value;
                _memoryData.Rows[e.RowIndex].Cells[0].Value = !value;

                ModifyNovaBreakpoint((UInt16)e.RowIndex, !value);
            }
        }


        private void HighlightMicrocodeSourceLine(UInt16 address)
        {
            foreach (DataGridViewRow row in _sourceViewer.Rows)
            {
                if (row.Tag != null &&
                    (ushort)(row.Tag) == address)
                {
                    _sourceViewer.ClearSelection();
                    row.Selected = true;
                    _sourceViewer.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void HighlightNovaSourceLine(UInt16 address)
        {
            if (address < _memoryData.Rows.Count)
            {
                _memoryData.ClearSelection();
                _memoryData.Rows[address].Selected = true;
                _memoryData.CurrentCell = _memoryData.Rows[address].Cells[0];            
            }
        }


        /// <summary>
        /// Fill in memory view on demand.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMemoryCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            // TODO: handle extended memory
            if (e.RowIndex > 65535)
            {
                // Top of memory, nothing to do
                return;
            }

            switch(_memoryData.Columns[e.ColumnIndex].Name)
            {
                case "Bkpt":
                    e.Value = GetNovaBreakpoint((UInt16)e.RowIndex);
                    break;

                case "Address":
                    e.Value = Conversion.ToOctal(e.RowIndex, 6);
                    break;

                case "Data":
                    e.Value = Conversion.ToOctal(_system.MemoryBus.DebugReadWord((ushort)e.RowIndex), 6);
                    
                    break;

                case "Disassembly":
                    e.Value = CPU.Nova.NovaDisassembler.DisassembleInstruction(
                        (ushort)e.RowIndex, 
                        _system.MemoryBus.DebugReadWord((ushort)e.RowIndex));
                    break;
            }

        }

        private void ModifyMicrocodeBreakpoint(UInt16 address, bool set)
        {
            _microcodeBreakpointEnabled[address] = set;
        }

        private bool GetNovaBreakpoint(UInt16 address)
        {
            return _novaBreakpointEnabled[address];
        }

        private void ModifyNovaBreakpoint(UInt16 address, bool set)
        {
            _novaBreakpointEnabled[address] = set;
        }

        private string GetTextForTaskState(AltoCPU.Task task)
        {
            if (task == null)
            {
                return String.Empty;
            }
            else
            {
                // Wakeup bit
                string status = task.Wakeup ? "W" : String.Empty;

                // Run bit
                if (task == _system.CPU.CurrentTask)
                {
                    status += "R";
                }

                return status;
            }
        }

        private string GetTextForTask(TaskType task)
        {
            string[] taskText =
            {
                "EM",   // 0 - emulator
                String.Empty,
                String.Empty,
                String.Empty,
                "KS",   // 4 - disk sector
                String.Empty,
                String.Empty,
                "EN",   // 7 - ethernet
                "MR",   // 8 - memory refresh
                "DW",   // 9 - display word
                "CU",   // 10 - cursor
                "DH",   // 11 - display horizontal
                "DV",   // 12 - display vertical
                "PA",   // 13 - parity
                "KW",   // 14 - disk word
                String.Empty,
            };

            if (task == TaskType.Invalid)
            {
                return String.Empty;
            }
            else
            {
                return taskText[(int)task];
            }
        }

        private Color GetColorForTask(TaskType task)
        {
            Color[] taskColors =
            {
                Color.LightBlue,    // 0 - emulator
                Color.LightGray,    // 1 - unused
                Color.LightGray,    // 2 - unused
                Color.LightGray,    // 3 - unused
                Color.LightGreen,   // 4 - disk sector
                Color.LightGray,    // 5 - unused
                Color.LightGray,    // 6 - unused
                Color.LightSalmon,  // 7 - ethernet
                Color.LightSeaGreen,// 8 - memory refresh
                Color.LightYellow,  // 9 - display word
                Color.LightPink,    // 10 - cursor
                Color.Chartreuse, // 11 - display horizontal
                Color.LightCoral,   // 12 - display vertical
                Color.LightSteelBlue, // 13 - parity
                Color.Gray,         // 14 - disk word
                Color.LightGray,    // 15 - unused
            };

            if (task == TaskType.Invalid)
            {
                return Color.White;
            }
            else
            {
                return taskColors[(int)task];
            }
        }        

        private struct SourceLine
        {
            public SourceLine(string sourceText)
            {
                //
                // Mangle "<-" found in the source into the unicode arrow character, just to be neat.
                //
                sourceText = sourceText.Replace("<-", _arrowChar.ToString());

                // See if line begins with something of the form "TNxxxxx>".
                // If it does then we have extra metadata to parse out.
                string[] tokens = sourceText.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                bool annotated = false;
                
                // Make the compiler happy
                Text = sourceText;
                Address = String.Empty;
                Task = TaskType.Invalid;

                if (tokens.Length > 0 &&
                    tokens[0].Length == 8 &&
                    tokens[0].EndsWith(">"))
                {
                    // Close enough.  Look for the task tag and parse out the (octal) address
                    switch(tokens[0].Substring(0,2))
                    {
                        case "EM":
                            Task = TaskType.Emulator;
                            break;

                        case "SE":
                            Task = TaskType.DiskSector;
                            break;

                        case "EN":
                            Task = TaskType.Emulator;
                            break;

                        case "MR":
                            Task = TaskType.MemoryRefresh;
                            break;

                        case "DW":
                            Task = TaskType.DisplayWord;
                            break;

                        case "CU":
                            Task = TaskType.Cursor;
                            break;

                        case "DH":
                            Task = TaskType.DisplayHorizontal;
                            break;

                        case "DV":
                            Task = TaskType.DisplayVertical;
                            break;

                        case "PA":
                            Task = TaskType.Parity;
                            break;

                        case "KW":
                            Task = TaskType.DiskWord;
                            break;

                        default:
                            Task = TaskType.Invalid;                            
                            break;
                    }

                    if (Task != TaskType.Invalid)
                    {
                        try
                        {
                            // Belongs to a task, so we can grab the address out as well
                            Address = sourceText.Substring(2, 5);
                        }
                        catch
                        {
                            // That didn't work for whatever reason, just treat this as a normal source line.
                            annotated = false;
                        }

                        Text = sourceText.Substring(tokens[0].Length + 1, sourceText.Length - tokens[0].Length -1);
                        annotated = true;
                    }
                    else
                    {
                        // We will just display this as a non-source line
                        annotated = false;
                    }                                        
                }
                
                if (!annotated)
                {
                    Text = sourceText;
                    Address = String.Empty;
                    Task = TaskType.Invalid;
                }
            }

            public string Text;
            public string Address;
            public TaskType Task;

        }

        private void Debugger_Load(object sender, EventArgs e)
        {

        }        

        private void OnJumpAddressKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter)
            {
                try
                {
                    UInt16 address = Convert.ToUInt16(JumpToAddress.Text, 8);

                    // find the source address that matches this, if any.
                    HighlightMicrocodeSourceLine(address);
                }
                catch
                {
                    // eh, just do nothing for now
                }
            }
        }

        private void OnStepButtonClicked(object sender, EventArgs e)
        {
            StopExecThread();

            SetExecutionState(ExecutionState.SingleStep);            
            ExecuteStep();
            SetExecutionState(ExecutionState.Stopped);            
        }

        private void OnAutoStepButtonClicked(object sender, EventArgs e)
        {
            StopExecThread();
            //
            // Continuously step (and update the UI)
            // until the "Stop" button is pressed or something bad happens.
            //
            _execThread = new Thread(new System.Threading.ParameterizedThreadStart(ExecuteProc));
            _execThread.Start(ExecutionType.Auto);
            SetExecutionState(ExecutionState.AutoStep);
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            StopExecThread();
            //
            // Continuously execute, but do not update UI
            // until the "Stop" button is pressed or something bad happens.
            //
            //if (_execThread == null)
            {
                _execThread = new Thread(new System.Threading.ParameterizedThreadStart(ExecuteProc));
                _execThread.Start(ExecutionType.Normal);
                SetExecutionState(ExecutionState.Running);
            }
        }

        private void RunToNextTaskButton_Click(object sender, EventArgs e)
        {
            StopExecThread();
            //
            // Continuously execute until the next task switch but do not update UI
            // until the "Stop" button is pressed or something bad happens.
            //
            //if (_execThread == null)
            {
                _execThread = new Thread(new System.Threading.ParameterizedThreadStart(ExecuteProc));
                _execThread.Start(ExecutionType.NextTask);
                SetExecutionState(ExecutionState.Running);
            }
        }

        /// <summary>
        /// Runs microcode until next Nova instruction is started
        /// This is done by simply breaking whenever the uPC for the emulator
        /// task returns to 20(octal) -- this is the restart point for the emulator
        /// task.        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovaStep_Click(object sender, EventArgs e)
        {
            StopExecThread();

            {
                _execThread = new Thread(new System.Threading.ParameterizedThreadStart(ExecuteProc));
                _execThread.Start(ExecutionType.NextNovaInstruction);
                SetExecutionState(ExecutionState.Running);
            }
        }

        private void OnStopButtonClicked(object sender, EventArgs e)
        {            
            StopExecThread();
            Refresh();
        }
        

        private void ResetButton_Click(object sender, EventArgs e)
        {
            StopExecThread();
            _system.Reset();
            Refresh();
        }

        private void ExecuteStep()
        {
            StopExecThread();
            _system.SingleStep();
            Refresh();
        }

        private void StopExecThread()
        {
            if (_execThread != null &&
                _execThread.IsAlive)
            {
                // Signal for the exec thread to end
                _execAbort = true;

                // Wait for the thread to exit.
                _execThread.Join();

                _execThread = null;
            }

            SetExecutionState(ExecutionState.Stopped);
        }

        private void ExecuteProc(object param)
        {
            ExecutionType execType = (ExecutionType)param;

            StepDelegate refUI = new StepDelegate(RefreshUI);
            StepDelegate inv = new StepDelegate(Invalidate);
            while (true)
            {
                bool internalError = false;

                try
                {
                    switch (execType)
                    {
                        case ExecutionType.Auto:
                            {
                                // Execute a single step, then update UI and 
                                // sleep to give messages time to run.
                                _system.SingleStep();

                                this.BeginInvoke(refUI);
                                this.BeginInvoke(inv);
                                System.Threading.Thread.Sleep(10);
                            }
                            break;

                        case ExecutionType.Step:
                        case ExecutionType.Normal:
                        case ExecutionType.NextTask:
                        case ExecutionType.NextNovaInstruction:
                            {
                                // Just execute one step, do not update UI.
                                _system.SingleStep();
                            }
                            break;
                    }
                }
                catch(Exception e)
                {
                    internalError = true;
                    _lastExceptionText = e.Message;
                }

                if (internalError)
                {
                    //
                    // Stop here because of an execution error.
                    //
                    this.BeginInvoke(refUI);
                    this.BeginInvoke(inv);


                    SetExecutionState(ExecutionState.InternalError);                    

                    break;
                }

                if (_execAbort ||                                               // The Stop button was hit
                    _microcodeBreakpointEnabled[_system.CPU.CurrentTask.MPC] || // A microcode breakpoint was hit
                    (execType == ExecutionType.NextTask && 
                        _system.CPU.NextTask != null && 
                        _system.CPU.NextTask != _system.CPU.CurrentTask) ||     // The next task was switched to                    
                    (_system.CPU.CurrentTask.MPC == 0x10 &&                     // MPC is 20(octal) meaning a new Nova instruction and...
                        (_novaBreakpointEnabled[_system.CPU.R[6]] ||            // A breakpoint is set here
                          execType == ExecutionType.NextNovaInstruction)))      // or we're running only a single Nova instruction. 
                {
                    // Stop here as we've hit a breakpoint or have been stopped 
                    // Update UI to indicate where we stopped.
                    this.BeginInvoke(refUI);
                    this.BeginInvoke(inv);

                    if (!_execAbort)
                    {
                        SetExecutionState(ExecutionState.BreakpointStop);
                    }

                    _execAbort = false;                    
                    break;
                }                
            }
        }

        private void SetExecutionState(ExecutionState state)
        {
            _execState = state;
            this.BeginInvoke(new StepDelegate(RefreshUI));
        }


        private enum ExecutionType
        {
            None = 0,
            Step,
            Auto,
            Normal,
            NextTask,
            NextNovaInstruction,
        }

        private enum ExecutionState
        {
            Stopped = 0,
            SingleStep,
            AutoStep,
            Running,
            BreakpointStop,
            InternalError,
        }

        private delegate void StepDelegate();

        private AltoSystem _system;

        // Unicode character for the Arrow used by Alto microcode
        private const char _arrowChar = (char)0x2190;

        // Thread used for execution other than single-step
        private Thread _execThread;
        private bool _execAbort;
        private ExecutionState _execState;
        private string _lastExceptionText;


        // Microcode Debugger breakpoints; one entry per address since we only need
        // to worry about a 10 bit address space, this is fast and uses little memory.
        private bool[] _microcodeBreakpointEnabled;

        // Nova Debugger breakpoints; same as above
        private bool[] _novaBreakpointEnabled;

        // Display related data.
        // At some point this should move elsewhere.
        // Note: display is actually 606 pixels wide, but that's not an even multiple of 8, so we round up.
        private byte[] _displayData = new byte[808 * 76];
        private Bitmap _displayBuffer;
        private Rectangle _displayRect = new Rectangle(0, 0, 608, 808);

        private void button1_Click(object sender, EventArgs e)
        {
            _system.CPU.Hack();
        }
    }
}
