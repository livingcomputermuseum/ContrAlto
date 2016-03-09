using System.Timers;

using Contralto.Logging;

namespace Contralto.CPU
{
    public enum TaskType
    {
        Invalid = -1,
        Emulator = 0,
        DiskSector = 4,
        Ethernet = 7,
        MemoryRefresh = 8,
        DisplayWord = 9,
        Cursor = 10,
        DisplayHorizontal = 11,
        DisplayVertical = 12,
        Parity = 13,
        DiskWord = 14,
    }

    public partial class AltoCPU : IClockable
    {
        public AltoCPU(AltoSystem system)
        {
            _system = system;

            _tasks[(int)TaskType.Emulator] = new EmulatorTask(this);
            _tasks[(int)TaskType.DiskSector] = new DiskTask(this, true);
            _tasks[(int)TaskType.DiskWord] = new DiskTask(this, false);
            _tasks[(int)TaskType.DisplayWord] = new DisplayWordTask(this);
            _tasks[(int)TaskType.DisplayHorizontal] = new DisplayHorizontalTask(this);
            _tasks[(int)TaskType.DisplayVertical] = new DisplayVerticalTask(this);
            _tasks[(int)TaskType.Cursor] = new CursorTask(this);
            _tasks[(int)TaskType.MemoryRefresh] = new MemoryRefreshTask(this);
            _tasks[(int)TaskType.Ethernet] = new EthernetTask(this);
            _tasks[(int)TaskType.Parity] = new ParityTask(this);

            Reset();
        }

        public Task[] Tasks
        {
            get { return _tasks; }
        }

        public Task CurrentTask
        {
            get { return _currentTask; }
        }

        public ushort[] R
        {
            get { return _r; }
        }

        public ushort[][] S
        {
            get { return _s; }
        }

        public ushort T
        {
            get { return _t; }
        }

        public ushort L
        {
            get { return _l; }
        }

        public ushort M
        {
            get { return _m; }
        }

        public ushort IR
        {
            get { return _ir; }
        }

        public ushort ALUC0
        {
            get { return _aluC0; }
        }


        public void Reset()
        {
            // Reset registers
            _r = new ushort[32];
            _s = new ushort[8][];

            for(int i=0;i<_s.Length;i++)
            {
                _s[i] = new ushort[32];
            }

            _t = 0;
            _l = 0;
            _m = 0;            
            _ir = 0;            
            _aluC0 = 0;            
            _rmr = 0xffff;      // Start all tasks in ROM0

            // Reset tasks.
            for (int i=0;i<_tasks.Length;i++)
            {
                if (_tasks[i] != null)
                {
                    _tasks[i].Reset();
                }
            }

            // Execute the initial task switch.
            TaskSwitch();

            _currentTask = _nextTask;
            _nextTask = null;
        }        

        public void Clock()
        {
            if (_currentTask.ExecuteNext())
            {
                // Invoke the task switch, this will take effect after
                // the NEXT instruction, not this one.
                TaskSwitch();
            }            
            else if (_nextTask != null)
            {
                // If we have a new task, switch to it now.                                
                _currentTask = _nextTask;
                _nextTask = null;
            }
        }

        /// <summary>
        /// "Silent Boot":  (See Section 9.2.2)
        /// Used when a BOOT STARTF is invoked; resets task MPCs and sets
        /// the starting bank (RAM0 or ROM0) appropriately based on the contents
        /// of RMR.
        /// All other register contents are left as-is.
        /// </summary>
        public void SoftReset()
        {
            // Soft-Reset tasks.
            for (int i = 0; i < _tasks.Length; i++)
            {
                if (_tasks[i] != null)
                {
                    _tasks[i].SoftReset();
                }
            }

            UCodeMemory.LoadBanksFromRMR(_rmr);
            _rmr = 0xffff;      // Reset RMR (all tasks start in ROM0)
          
            // Start in Emulator
            _currentTask = _tasks[0];

            //
            // TODO:
            // This is a hack of sorts, it ensures that the sector task initializes
            // itself as soon as the Emulator task yields after the reset.  (CopyDisk is broken otherwise due to the
            // sector task stomping over the KBLK CopyDisk sets up after the reset.  This is a race of sorts.)
            // Unsure if there is a deeper issue here or if there are other reset semantics
            // in play here.
            //
            WakeupTask(CPU.TaskType.DiskSector);            

            Log.Write(LogComponent.CPU, "Silent Boot; microcode banks initialized to {0}", Conversion.ToOctal(_rmr));
        }

        /// <summary>
        /// Used by hardware devices to cause a specific task to have its
        /// "wakeup" signal triggered
        /// </summary>
        /// <param name="task"></param>
        public void WakeupTask(TaskType task)
        {            
            if (_tasks[(int)task] != null)
            {             
                Log.Write(LogComponent.TaskSwitch, "Wakeup enabled for Task {0}", task);            
                _tasks[(int)task].WakeupTask();                
            }
        }

        /// <summary>
        /// Used by hardware devices to cause a specific task to have its
        /// "wakeup" signal cleared
        /// </summary>
        /// <param name="task"></param>
        public void BlockTask(TaskType task)
        {
            if (_tasks[(int)task] != null)
            {                
                Log.Write(LogComponent.TaskSwitch, "Removed wakeup for Task {0}", task);                
                _tasks[(int)task].BlockTask();
            }
        }

        public bool IsBlocked(TaskType task)
        {
            if (_tasks[(int)task] != null)
            {
                return _tasks[(int)task].Wakeup;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Used by the debugger to determine if a task switch is taking
        /// place.
        /// </summary>
        public Task NextTask
        {
            get { return _nextTask; }
        }        

        private void TaskSwitch()
        {
            // Select the highest-priority eligible task
            for (int i = _tasks.Length - 1; i >= 0; i--)
            {
                if (_tasks[i] != null && _tasks[i].Wakeup)
                {                    
                    _nextTask = _tasks[i];
                    _nextTask.FirstInstructionAfterSwitch = true;

                    /*
                    if (_nextTask != _currentTask && _currentTask != null)
                    {                        
                        Log.Write(LogComponent.TaskSwitch, "TASK: Next task will be {0} (pri {1}); current task {2} (pri {3})",
                        (TaskType)_nextTask.Priority, _nextTask.Priority,
                        (TaskType)_currentTask.Priority, _currentTask.Priority);                        
                    } */
                    break;
                }
            }
        }

        // AltoCPU registers
        private ushort _t;
        private ushort _l;
        private ushort _m;        
        private ushort _ir;
        
        // R and S register files and bank select
        private ushort[] _r;
        private ushort[][] _s;        

        // Stores the last carry from the ALU on a Load L
        private ushort _aluC0;

        // RMR (Reset Mode Register)
        ushort _rmr;

        // Task data
        private Task _nextTask;         // The task to switch two after the next microinstruction
        private Task _currentTask;      // The currently executing task
        private Task[] _tasks = new Task[16];                

        // The system this CPU belongs to
        private AltoSystem _system;        
    }
}
