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
        Emulator = 0,
        DiskSector = 4,
        Ethernet = 5,
        DiskWord = 9,
        Cursor = 10,
        DisplayHorizontal = 11,
        DisplayVertical = 12,
        Refresh = 14,
    }

    public class AltoCPU
    {
        public AltoCPU()
        {
            _tasks[(int)TaskType.Emulator] = new EmulatorTask(this);

            Reset();
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

        /// <summary>
        /// Used by hardware devices to cause a specific task to have its
        /// "wakeup" signal triggered
        /// </summary>
        /// <param name="task"></param>
        public void WakeupTask(int task)
        {            
            if (_tasks[task] != null)
            {
                _tasks[task].WakeupTask();
            }
        }

        /// <summary>
        /// Used by hardware devices to cause a specific task to have its
        /// "wakeup" signal cleared
        /// </summary>
        /// <param name="task"></param>
        public void BlockTask(int task)
        {
            if (_tasks[task] != null)
            {
                _tasks[task].BlockTask();
            }
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
        private abstract class Task
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

            public virtual void BlockTask()
            {
                // Used only by hardware interfaces, where applicable
                _wakeup = false;
            }

            public virtual void WakeupTask()
            {
                _wakeup = true;
            }

            public bool ExecuteNext()
            {
                // TODO: cache microinstructions (or pre-decode them) to save consing all these up every time.
                MicroInstruction instruction = new MicroInstruction(UCodeMemory.UCodeROM[_mpc]);
                return ExecuteInstruction(instruction);
            }

            /// <summary>
            /// ExecuteInstruction causes the Task to execute the next instruction (the one
            /// _mpc is pointing to).  The base implementation covers non-task specific logic; subclasses may
            /// provide their own overrides.
            /// </summary>
            /// <returns>True if a task switch has been requested by a TASK instruction, false otherwise.</returns>
            protected virtual bool ExecuteInstruction(MicroInstruction instruction)
            {               
                bool nextTask = false;
                bool loadR = false;
                ushort aluData = 0;
                _loadS = false;
                _rSelect = 0;
                _busData = 0;
                

                Shifter.SetMagic(false);
                  
                //
                // Wait for memory state machine if a memory operation is requested by this instruction and
                // the memory isn't ready yet.
                //
                if ((instruction.BS == BusSource.ReadMD ||
                    instruction.F1 == SpecialFunction1.LoadMAR ||
                    instruction.F2 == SpecialFunction2.StoreMD) 
                    && !MemoryBus.Ready(MemoryOperation.Load))  //TODO: fix
                {
                    // Suspend operation for this cycle.
                    return false;
                }

                _rSelect = instruction.RSELECT;

                // Give tasks the chance to modify parameters early on (like RSELECT)
                ExecuteSpecialFunction2Early((int)instruction.F2);

                // Select BUS data.
                if (instruction.F1 != SpecialFunction1.Constant &&
                    instruction.F2 != SpecialFunction2.Constant)
                {
                    // Normal BUS data (not constant ROM access).
                    switch (instruction.BS)
                    {
                        case BusSource.ReadR:
                            _busData = _cpu._r[_rSelect];
                            break;

                        case BusSource.LoadR:
                            _busData = 0;       // "Loading R forces the BUS to 0 so that an ALU function of 0 and T may be executed simultaneously"
                            loadR = true;
                            break;

                        case BusSource.None:
                            _busData = 0xffff;  // "Enables no source to the BUS, leaving it all ones"
                            break;

                        case BusSource.TaskSpecific1:
                        case BusSource.TaskSpecific2:
                            _busData = GetBusSource((int)instruction.BS);        // task specific -- call into specific implementation
                            break;

                        case BusSource.ReadMD:
                            _busData = MemoryBus.ReadMD();
                            break;

                        case BusSource.ReadMouse:
                            throw new NotImplementedException("ReadMouse bus source not implemented.");
                            _busData = 0;   // TODO: implement
                            break;

                        case BusSource.ReadDisp:
                            throw new NotImplementedException("ReadDisp bus source not implemented.");
                            _busData = 0;   // TODO: implement;
                            break;

                        default:
                            throw new InvalidOperationException(String.Format("Unhandled bus source {0}.", instruction.BS));
                            break;
                    }
                }
                else
                {
                    // See also comments below.
                    _busData = ConstantMemory.ConstantROM[(instruction.RSELECT << 3) | ((uint)instruction.BS)];                    
                }

                // Constant ROM access:
                // The constant memory is gated to the bus by F1=7, F2=7, or BS>4.  The constant memory is addressed by the
                // (8 bit) concatenation of RSELECT and BS.  The intent in enabling constants with BS>4 is to provide a masking
                // facility, particularly for the <-MOUSE and <-DISP bus sources.  This works because the processor bus ANDs if
                // more than one source is gated to it.  Up to 32 such mask contans can be provided for each of the four bus sources
                // > 4.
                // NOTE also:
                // "Note that the [emulator task F2] functions which replace the low bits of RSELECT with IR aaffect only the 
                // selection of R; they do not affect the address supplied to the constant ROM."
                // Hence we use the unmodified RSELECT value here and above.
                if ((int)instruction.BS > 4 || 
                    instruction.F1 == SpecialFunction1.Constant ||
                    instruction.F2 == SpecialFunction2.Constant)
                {
                    _busData &= ConstantMemory.ConstantROM[(instruction.RSELECT << 3) | ((uint)instruction.BS)];
                }
                
                // Do ALU operation
                aluData = ALU.Execute(instruction.ALUF, _busData, _cpu._t);

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
                        MemoryBus.LoadMAR(aluData);    // Start main memory reference
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
                        if (_busData == 0)
                        {
                            _nextModifier = 1;
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
                        if ((short)Shifter.DoOperation(_cpu._l, _cpu._t) < 0)
                        {
                            _nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.ShEq0:
                        // See note above.
                        if (Shifter.DoOperation(_cpu._l, _cpu._t) == 0)
                        {
                            _nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.Bus:
                        // Select bits 6-15 (bits 0-9 in modern parlance) of the bus
                        _nextModifier = (ushort)(_busData & 0x3ff);
                        break;

                    case SpecialFunction2.ALUCY:
                        // ALUC0 is the carry produced by the ALU during the most recent microinstruction
                        // that loaded L.  It is *not* the carry produced during the execution of the microinstruction
                        // that contains the ALUCY function.
                        _nextModifier = _cpu._aluC0;
                        break;

                    case SpecialFunction2.StoreMD:
                        MemoryBus.LoadMD(_busData);
                        break;

                    case SpecialFunction2.Constant:
                        // Ignored here; handled by Constant ROM access logic above.
                        break;

                    default:
                        // Let the specific task implementation take a crack at this.
                        ExecuteSpecialFunction2((int)instruction.F1);
                        break;
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

                    _cpu._t = loadTFromALU ? aluData : _busData;
                }

                // Load L (and M) from ALU
                if (instruction.LoadL)
                {
                    _cpu._l = aluData;
                    _cpu._m = aluData;

                    // Save ALUC0 for use in the next ALUCY special function.
                    _cpu._aluC0 = (ushort)ALU.Carry;
                }
               
                // Do writeback to selected R register from shifter output
                if (loadR)
                {
                    _cpu._r[_rSelect] = Shifter.DoOperation(_cpu._l, _cpu._t);
                }

                // Do writeback to selected R register from M
                if (_loadS)
                {
                    _cpu._s[_cpu._rb][_rSelect] = _cpu._m;
                }

                //
                // Select next address  -- TODO: this is incorrect!  _nextModifer should be applied to the *NEXT* instruction, not the current one!
                //
                _mpc = (ushort)(instruction.NEXT | _nextModifier);
                return nextTask;
            }            

            protected abstract ushort GetBusSource(int bs);
            protected abstract void ExecuteSpecialFunction1(int f1);

            /// <summary>
            /// Used to allow Task-specific F2s that need to modify RSELECT to do so.
            /// </summary>
            /// <param name="f2"></param>
            protected virtual void ExecuteSpecialFunction2Early(int f2)
            {
                // Nothing by default.
            }

            protected abstract void ExecuteSpecialFunction2(int f2);

            //
            // Per uInstruction Task Data:
            // Modified by both the base Task implementation and any subclasses
            //
            // TODO: maybe instead of these being shared (which feels kinda bad)
            // these could be encapsulated in an object and passed to subclass implementations?            
            protected ushort _busData;        // Data placed onto the bus (ANDed from multiple sources)
            protected ushort _nextModifier;   // Bits ORed onto the NEXT field of the current instruction
            protected uint   _rSelect;        // RSELECT field from current instruction, potentially modified by task
            protected bool   _loadS;          // Whether to load S from M at and of cycle


            //
            // Global Task Data
            //
            protected AltoCPU _cpu;
            protected ushort _mpc;
            protected int _priority;
            protected bool _wakeup;            
        }

        /// <summary>
        /// EmulatorTask provides emulator (NOVA instruction set) specific operations.
        /// </summary>
        private class EmulatorTask : Task
        {
            public EmulatorTask(AltoCPU cpu) : base(cpu)
            {
                _priority = 0;

                // The Wakeup signal is always true for the Emulator task.
                _wakeup = true;
            }

            public override void BlockTask()
            {
                throw new InvalidOperationException("The emulator task cannot be blocked.");
            }

            public override void WakeupTask()
            {
                throw new InvalidOperationException("The emulator task is always in wakeup state.");
            }            

            protected override ushort GetBusSource(int bs)
            {
                EmulatorBusSource ebs = (EmulatorBusSource)bs;

                switch(ebs)
                {
                    case EmulatorBusSource.ReadSLocation:
                        return _cpu._s[_cpu._rb][_rSelect];

                    case EmulatorBusSource.LoadSLocation:
                        _loadS = true;
                        return 0;       // TODO: technically this is an "undefined value" not zero.

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled bus source {0}", bs));
                }
            }

            protected override void ExecuteSpecialFunction1(int f1)
            {
                EmulatorF1 ef1 = (EmulatorF1)f1;
                switch (ef1)
                {
                    case EmulatorF1.RSNF:
                        // TODO: make configurable
                        // "...decoded by the Ethernet interface, which gates the host address wired on the
                        // backplane onto BUS[8-15].  BUS[0-7] is not driven and will therefore be -1.  If
                        // no Ethernet interface is present, BUS will be -1.
                        //
                        _busData &= (0xff00 | 0x42);
                        break;

                    case EmulatorF1.STARTF:
                        // Dispatch function to I/O based on contents of AC0... (TBD: what are these?)
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled emulator F1 {0}.", ef1));
                }
            }

            protected override void ExecuteSpecialFunction2Early(int f2)
            {
                EmulatorF2 ef2 = (EmulatorF2)f2;
                switch (ef2)
                {
                    case EmulatorF2.ACSOURCE:
                        // Early: modify R select field:
                        // "...it replaces the two-low order bits of the R select field with
                        // the complement of the SrcAC field of IR, (IR[1-2] XOR 3), allowing the emulator
                        // to address its accumulators (which are assigned to R0-R3)."
                        _rSelect = (_rSelect & 0xfffc) | ((((uint)_cpu._ir & 0x6000) >> 13) ^ 3);
                        break;

                    case EmulatorF2.ACDEST:
                        // "...causes (IR[3-4] XOR 3) to be used as the low-order two bits of the RSELECT field.
                        // This address the accumulators from the destination field of the instruction.  The selected
                        // register may be loaded or read."
                        _rSelect = (_rSelect & 0xfffc) | ((((uint)_cpu._ir & 0x1800) >> 11) ^ 3);
                        break;
                    
                }
            }

            protected override void ExecuteSpecialFunction2(int f2)
            {
                EmulatorF2 ef2 = (EmulatorF2)f2;
                switch (ef2)
                {
                    case EmulatorF2.LoadIR:
                        // based on block diagram, this always comes from the bus
                        _cpu._ir = _busData;

                        // "IR<- also merges bus bits 0, 5, 6 and 7 into NEXT[6-9] which does a first level
                        // instruction dispatch."
                        // TODO: is this an AND or an OR operation?  (how is the "merge" done?)
                        // Assuming for now this is an OR operation like everything else that modifies NEXT.
                        _nextModifier = (ushort)(((_busData & 0x8000) >> 6) | ((_busData & 0x0700) >> 2));
                        break;

                    case EmulatorF2.IDISP:
                        // "The IDISP function (F2=15B) does a 16 way dispatch under control of a PROM and a
                        // multiplexer.  The values are tabulated below:
                        //   Conditions             ORed onto NEXT          Comment
                        //
                        //   if IR[0] = 1           3-IR[8-9]               complement of SH field of IR
                        //   elseif IR[1-2] = 0     IR[3-4]                 JMP, JSR, ISZ, DSZ
                        //   elseif IR[1-2] = 1     4                       LDA
                        //   elseif IR[1-2] = 2     5                       STA
                        //   elseif IR[4-7] = 0     1                       
                        //   elseif IR[4-7] = 1     0
                        //   elseif IR[4-7] = 6     16B                     CONVERT
                        //   elseif IR[4-7] = 16B   6
                        //   else                   IR[4-7]
                        // NB: as always, Xerox labels bits in the opposite order from modern convention;
                        // (bit 0 is bit 15...)
                        if ((_cpu._ir & 0x8000) != 0)
                        {
                            _nextModifier = (ushort)(3 - ((_cpu._ir & 0xc0) >> 6));
                        }
                        else if((_cpu._ir & 0x6000) == 0)
                        {
                            _nextModifier = (ushort)((_cpu._ir & 0x1800) >> 11);
                        }
                        else if((_cpu._ir & 0x6000) == 0x4000)
                        {
                            _nextModifier = 4;
                        }
                        else if ((_cpu._ir & 0x6000) == 0x6000)
                        {
                            _nextModifier = 5;
                        }
                        else if ((_cpu._ir & 0x0f00) == 0)
                        {
                            _nextModifier = 1;
                        }
                        else if ((_cpu._ir & 0x0f00) == 0x0100)
                        {
                            _nextModifier = 0;
                        }
                        else if ((_cpu._ir & 0x0f00) == 0x0600)
                        {
                            _nextModifier = 0xe;
                        }
                        else if ((_cpu._ir & 0x0f00) == 0x0e00)
                        {
                            _nextModifier = 0x6;
                        }
                        else
                        {
                            _nextModifier = (ushort)((_cpu._ir & 0x0f00) >> 8);
                        }
                        break;

                    case EmulatorF2.ACSOURCE:
                        // Late:
                        // "...a dispatch is performed:
                        //   Conditions             ORed onto NEXT          Comment
                        //
                        //   if IR[0] = 1           3-IR[8-9]               complement of SH field of IR
                        //   if IR[1-2] = 3         IR[5]                   the Indirect bit of R
                        //   if IR[3-7] = 0         2                       CYCLE
                        //   if IR[3-7] = 1         5                       RAMTRAP
                        //   if IR[3-7] = 2         3                       NOPAR -- parameterless opcode group
                        //   if IR[3-7] = 3         6                       RAMTRAP
                        //   if IR[3-7] = 4         7                       RAMTRAP
                        //   if IR[3-7] = 11B       4                       JSRII
                        //   if IR[3-7] = 12B       4                       JSRIS
                        //   if IR[3-7] = 16B       1                       CONVERT
                        //   if IR[3-7] = 37B       17B                     ROMTRAP -- used by Swat, the debugger
                        //   else                   16B                     ROMTRAP
                        if ((_cpu._ir & 0x8000) != 0)
                        {
                            _nextModifier = (ushort)(3 - ((_cpu._ir & 0xc0) >> 6));
                        }
                        else if ((_cpu._ir & 0xc000) == 0xc000)
                        {
                            _nextModifier = (ushort)((_cpu._ir & 0x400) >> 10);
                        }
                        else if ((_cpu._ir & 0x1f00) == 0)
                        {
                            _nextModifier = 2;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0100)
                        {
                            _nextModifier = 5;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0200)
                        {
                            _nextModifier = 3;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0300)
                        {
                            _nextModifier = 6;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0400)
                        {
                            _nextModifier = 7;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0900)
                        {
                            _nextModifier = 4;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0a00)
                        {
                            _nextModifier = 4;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x0e00)
                        {
                            _nextModifier = 1;
                        }
                        else if ((_cpu._ir & 0x1f00) == 0x1f00)
                        {
                            _nextModifier = 0xf;
                        }
                        else
                        {
                            _nextModifier = 0xe;
                        }
                        break;

                    case EmulatorF2.ACDEST:
                        // Handled in early handler
                        break;

                    case EmulatorF2.BUSODD:
                        // "...merges BUS[15] into NEXT[9]."
                        // TODO: is this an AND or an OR?
                        _nextModifier = (ushort)((_nextModifier & 0xffbf) | ((_busData & 0x1) << 6));
                        break;

                    case EmulatorF2.MAGIC:
                        Shifter.SetMagic(true);
                        break;
                    

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled emulator F2 {0}.", ef2));
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

    }
}
