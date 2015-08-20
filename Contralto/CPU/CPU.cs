using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{
    

    public class AltoCPU
    {
        public AltoCPU()
        {
            _tasks[0] = new EmulatorTask(this);

            Reset();
        }

        public void Reset()
        {
            // Reset registers
            _r = new ushort[32];
            _s = new ushort[32];
            _t = 0;
            _l = 0;
            _ir = 0;            
            _aluC0 = 0;

            // Reset tasks.
            for(int i=0;i<_tasks.Length;i++)
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

        public void ExecuteNext()
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
                }
            }
        }

        // Task:
        // Base task class: provides implementation for non-task-specific microcode execution and
        // state.  Task subclasses implement and execute Task-specific behavior and are called into
        // by the base class as necessary.
        private class Task
        {        
            public Task(AltoCPU cpu)
            {
                _wakeup = false;
                _mpc = 0xffff;  // invalid, for sanity checking
                _priority = -1; // invalid
                _cpu = cpu;
            }

            public int Priority
            {
                get { return _priority; }
            }

            public bool Wakeup
            {
                get { return _wakeup; }
            }

            public virtual void Reset()
            {               
                // From The Alto Hardware Manual (section 2, "Initialization"):
                // "...each task start[s] at the location which is its task number"
                // 
                _mpc = (ushort)_priority;
            }

            public virtual void Block()
            {
                // Used only by hardware interfaces, where applicable
                _wakeup = false;
            }

            public bool ExecuteNext()
            {
                // TODO: cache microinstructions (or pre-decode them) to save consing all these up every time.
                MicroInstruction instruction = new MicroInstruction(UCodeMemory.UCodeROM[_mpc]);
                return ExecuteInstruction(instruction);
            }

            public virtual string DisassembleInstruction(ushort addr)
            {
                return String.Empty;
            }

            /// <summary>
            /// ExecuteInstruction causes the Task to execute the next instruction (the one
            /// _mpc is pointing to).  The base implementation covers non-task specific logic; subclasses may
            /// provide their own overrides.
            /// </summary>
            /// <returns>True if a task switch has been requested by a TASK instruction, false otherwise.</returns>
            protected virtual bool ExecuteInstruction(MicroInstruction instruction)
            {
                ushort busData = 0;        // from BUS   
                ushort aluData = 0;        // from ALU
                bool nextTask = false;
                ushort nextModifier = 0;   // for branches (OR'd into NEXT field)                

                // Select BUS data.
                if (instruction.F1 != SpecialFunction1.Constant &&
                    instruction.F2 != SpecialFunction2.Constant)
                {
                    // Normal BUS data (not constant ROM access).
                    switch (instruction.BS)
                    {
                        case BusSource.ReadR:
                            busData = _cpu._r[instruction.RSELECT];
                            break;

                        case BusSource.LoadR:
                            busData = 0;       // "Loading R forces the BUS to 0 so that an ALU function of 0 and T may be executed simultaneously"
                            break;

                        case BusSource.None:
                            busData = 0xffff;  // "Enables no source to the BUS, leaving it all ones"
                            break;

                        case BusSource.TaskSpecific1:
                        case BusSource.TaskSpecific2:
                            busData = GetBusSource((int)instruction.BS);        // task specific -- call into specific implementation
                            break;

                        case BusSource.ReadMD:
                            busData = Memory.MD;
                            break;

                        case BusSource.ReadMouse:
                            throw new NotImplementedException("ReadMouse bus source not implemented.");
                            busData = 0;   // TODO: implement
                            break;

                        case BusSource.ReadDisp:
                            throw new NotImplementedException("ReadDisp bus source not implemented.");
                            busData = 0;   // TODO: implement;
                            break;

                        default:
                            throw new InvalidOperationException(String.Format("Unhandled bus source {0}", instruction.BS));
                    }
                }
                else
                {
                    busData = ConstantMemory.ConstantROM[instruction.RSELECT | ((uint)instruction.BS << 5)];                    
                }

                // Constant ROM access:
                // The constant memory is gated to the bus by F1=7, F2=7, or BS>4.  The constant memory is addressed by the
                // (8 bit) concatenation of RSELECT and BS.  The intent in enabling constants with BS>4 is to provide a masking
                // facility, particularly for the <-MOUSE and <-DISP bus sources.  This works because the processor bus ANDs if
                // more than one source is gated to it.  Up to 32 such mask contans can be provided for each of the four bus sources
                // > 4.
                if ((int)instruction.BS > 4 || 
                    instruction.F1 == SpecialFunction1.Constant ||
                    instruction.F2 == SpecialFunction2.Constant)
                {
                    busData &= ConstantMemory.ConstantROM[instruction.RSELECT | ((uint)instruction.BS << 5)];
                }
                
                // Do ALU operation
                aluData = ALU.Execute(instruction.ALUF, busData, _cpu._t);

                // Reset shifter op
                Shifter.SetOperation(ShifterOp.None, 0);

                //
                // Do Special Functions
                //
                switch(instruction.F1)
                {
                    case SpecialFunction1.None:
                        // Do nothing.  Well, that was easy.
                        break;

                    case SpecialFunction1.LoadMAR:
                        Memory.LoadMAR(aluData);    // Start main memory reference
                        break;

                    case SpecialFunction1.Task:
                        nextTask = true;            // Yield to other more important tasks
                        break;

                    case SpecialFunction1.Block:
                        // "...this function is reserved by convention only; it is *not* done by the microprocessor"
                        throw new InvalidOperationException("BLOCK should never be invoked by microcode.");
                        break;

                    case SpecialFunction1.LLSH1:
                        Shifter.SetOperation(ShifterOp.ShiftLeft, 1);
                        break;

                    case SpecialFunction1.LRSH1:
                        Shifter.SetOperation(ShifterOp.ShiftRight, 1);
                        break;

                    case SpecialFunction1.LLCY8:
                        Shifter.SetOperation(ShifterOp.RotateLeft, 8);
                        break;

                    case SpecialFunction1.Constant:
                        // Ignored here; handled by Constant ROM access logic above.
                        break;

                    default:
                        // Let the specific task implementation take a crack at this.
                        ExecuteSpecialFunction1((int)instruction.F1);
                        break;
                }

                switch(instruction.F2)
                {
                    case SpecialFunction2.None:
                        // Nothing!
                        break;

                    case SpecialFunction2.BusEq0:
                        if (busData == 0)
                        {
                            nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.ShLt0:
                        //
                        // Note:
                        // "the value of SHIFTER OUTPUT is determined by the value of L as the microinstruction
                        // *begins* execution and the shifter function specified during the  *current* microinstruction.
                        //
                        // Since we haven't modifed L yet, and we've selected the shifter function above, we're good to go here.
                        //
                        if ((short)Shifter.DoOperation(_cpu._l) < 0)
                        {
                            nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.ShEq0:
                        // See note above.
                        if (Shifter.DoOperation(_cpu._l) == 0)
                        {
                            nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.Bus:
                        // Select bits 6-15 (bits 0-9 in modern parlance) of the bus
                        nextModifier = (ushort)(busData & 0x3ff);
                        break;

                    case SpecialFunction2.ALUCY:
                        // ALUC0 is the carry produced by the ALU during the most recent microinstruction
                        // that loaded L.  It is *not* the carry produced during the execution of the microinstruction
                        // that contains the ALUCY function.
                        nextModifier = _cpu._aluC0;
                        break;

                    case SpecialFunction2.StoreMD:
                        Memory.StoreMD(busData);
                        break;

                    case SpecialFunction2.Constant:
                        // Ignored here; handled by Constant ROM access logic above.
                        break;

                    default:
                        // Let the specific task implementation take a crack at this.
                        ExecuteSpecialFunction2((int)instruction.F1);
                }

                //
                // Write back to registers:
                //

                // Load T
                if (instruction.LoadT)
                {
                    // Does this operation change the source for T?
                    bool loadTFromALU = false;
                    switch(instruction.ALUF)
                    {
                        case AluFunction.Bus:
                        case AluFunction.BusOrT:
                        case AluFunction.BusPlus1:
                        case AluFunction.BusMinus1:
                        case AluFunction.BusPlusTPlus1:
                        case AluFunction.BusPlusSkip:
                        case AluFunction.AluBusAndT:
                            loadTFromALU = true;
                            break;
                    }

                    _cpu._t = loadTFromALU ? aluData : busData;
                }

                // Load L
                if (instruction.LoadL)
                {
                    _cpu._l = aluData;

                    // Save ALUC0 for use in the next ALUCY special function.
                    _cpu._aluC0 = ALU.Carry;
                }

                // Do shifter

                // Do writeback to selected R register from shifter output
                _cpu._r[instruction.RSELECT] = Shifter.DoOperation(_cpu._l);

                //
                // Select next address
                //
                _mpc = (ushort)(instruction.NEXT | nextModifier);
                return nextTask;
            }


            protected abstract ushort GetBusSource(int bs);
            protected abstract void ExecuteSpecialFunction1(int f1);
            protected abstract void ExecuteSpecialFunction2(int f2);

            protected AltoCPU _cpu;
            protected ushort _mpc;
            protected int _priority;
            protected bool _wakeup;            
        }

        private class EmulatorTask : Task
        {
            public EmulatorTask(AltoCPU cpu) : base(cpu)
            {
                _priority = 0;

                // The Wakeup signal is always true for the Emulator task.
                _wakeup = true;
            }

            public override string DisassembleInstruction(ushort addr)
            {
                return base.DisassembleInstruction(addr);
            }

            protected override ushort GetBusSource(int bs)
            {
                throw new NotImplementedException();
            }

            protected override void ExecuteSpecialFunction1(int f1)
            {
                throw new NotImplementedException();
            }

            protected override void ExecuteSpecialFunction2(int f2)
            {
                throw new NotImplementedException();
            }

            enum EmulatorF1
            {
                SWMODE = 8,
                WRTRAM = 9,
                RDRAM = 10,
                LoadRMR = 11,
                Unused = 12,
                LoadESRB = 13,
                RSNF = 14,
                STARTF = 15,
            }

            enum EmulatorF2
            {
                BUSODD = 8,
                MAGIC = 9,
                LoadDNS = 10,
                ACDEST = 11,
                LoadIR = 12,
                IDISP = 13,
                ACSOURCE = 14,
                Unused = 15,
            }

            enum EmulatorBusSource
            {

            }
        }


        // AltoCPU registers
        ushort _t;
        ushort _l;
        ushort _m;
        ushort _mar;
        ushort _ir;

        ushort[] _r;
        ushort[] _s;

        // Stores the last carry from the ALU on a Load L
        private ushort _aluC0;

        // Task data
        private Task _nextTask;         // The task to switch two after the next microinstruction
        private Task _currentTask;      // The currently executing task
        private Task[] _tasks = new Task[16];

        private long _clocks;

    }
}
