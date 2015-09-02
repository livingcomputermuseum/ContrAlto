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

namespace Contralto
{
    public partial class Debugger : Form
    {
        public Debugger(AltoSystem system)
        {
            _system = system;

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

            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                SourceLine src = new SourceLine(line);

                int i = _sourceViewer.Rows.Add(
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

        private void RefreshUI()
        {
            // Registers
            for(int i=0;i<32;i++)
            {
                _registerData.Rows[i].Cells[0].Value = OctalHelpers.ToOctal(i,2);
                _registerData.Rows[i].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.R[i], 6);
                _registerData.Rows[i].Cells[2].Value = OctalHelpers.ToOctal(_system.CPU.S[0][i], 6);
            }

            // Tasks
            for (int i=0;i<16;i++)
            {
                _taskData.Rows[i].Cells[0].Value = GetTextForTask((TaskType)i);
                _taskData.Rows[i].Cells[1].Value = GetTextForTaskState(_system.CPU.Tasks[i]);
                _taskData.Rows[i].Cells[2].Value =
                    _system.CPU.Tasks[i] != null ? OctalHelpers.ToOctal(_system.CPU.Tasks[i].MPC, 4) : String.Empty;
            }

            // Other registers            
            _otherRegs.Rows[0].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.L, 6);
            _otherRegs.Rows[1].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.T, 6);
            _otherRegs.Rows[2].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.M, 6);
            _otherRegs.Rows[3].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.IR, 6);
            _otherRegs.Rows[4].Cells[1].Value = OctalHelpers.ToOctal(_system.CPU.ALUC0, 1);
            _otherRegs.Rows[5].Cells[1].Value = OctalHelpers.ToOctal(_system.MemoryBus.MAR, 6);
            _otherRegs.Rows[6].Cells[1].Value = OctalHelpers.ToOctal(_system.MemoryBus.MD, 6);
            _otherRegs.Rows[7].Cells[1].Value = OctalHelpers.ToOctal(_system.MemoryBus.Cycle, 2);

            // Find the right source line
            foreach(DataGridViewRow row in _sourceViewer.Rows)
            {
                if (row.Tag != null &&
                    (ushort)(row.Tag) == _system.CPU.CurrentTask.MPC)
                {
                    _sourceViewer.ClearSelection();
                    row.Selected = true;
                    _sourceViewer.CurrentCell = row.Cells[0];                    
                    break;
                }
            }
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

            /*
            for (int i=0;i<65536;i++)
            {
                _memoryData.Rows.Add();
            } */

            _otherRegs.Rows.Add("L", "0");
            _otherRegs.Rows.Add("T", "0");
            _otherRegs.Rows.Add("M", "0");
            _otherRegs.Rows.Add("IR", "0");
            _otherRegs.Rows.Add("ALUC0", "0");
            _otherRegs.Rows.Add("MAR", "0");
            _otherRegs.Rows.Add("MD", "0");
            _otherRegs.Rows.Add("MCycle", "0");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private string GetTextForTaskState(AltoCPU.Task task)
        {
            if (task == null)
            {
                return String.Empty;
            }
            else
            {
                // TODO: block status
                return task.Wakeup ? "W" : String.Empty;
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
                Color.LightGoldenrodYellow, // 11 - display horizontal
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

        private void OnStepButtonClicked(object sender, EventArgs e)
        {
            _system.SingleStep();
            Refresh();
        }

        private AltoSystem _system;

        // Unicode character for the Arrow used by Alto microcode
        private const char _arrowChar = (char)0x2190;

        
    }
}
