using Contralto.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _rb = 0;

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
            ExecuteNext();
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
                _tasks[(int)task].BlockTask();
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

        private void ExecuteNext()
        {
            if (_currentTask.ExecuteNext())
            {
                // Invoke the task switch, this will take effect after
                // the NEXT instruction, not this one.
                TaskSwitch();
            }
            else
            {
                // If we have a new task, switch to it now.
                if (_nextTask != null)
                {
                    _currentTask = _nextTask;
                    _nextTask = null;
                }
            }

            _clocks++;
        }

        private void TaskSwitch()
        {
            // Select the highest-priority eligible task
            for (int i = _tasks.Length - 1; i >= 0; i--)
            {
                if (_tasks[i] != null && _tasks[i].Wakeup)
                {
                    _nextTask = _tasks[i];
                    break;
                }
            }
        }

        // AltoCPU registers
        ushort _t;
        ushort _l;
        ushort _m;        
        ushort _ir;
        
        // R and S register files and bank select
        ushort[] _r;
        ushort[][] _s;
        ushort _rb;     // S register bank select

        // Stores the last carry from the ALU on a Load L
        private ushort _aluC0;

        // Task data
        private Task _nextTask;         // The task to switch two after the next microinstruction
        private Task _currentTask;      // The currently executing task
        private Task[] _tasks = new Task[16];        

        private long _clocks;

        // The system this CPU belongs to
        private AltoSystem _system;

    }
}
