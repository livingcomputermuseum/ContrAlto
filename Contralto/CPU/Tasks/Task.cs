using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        // Task:
        // Base task class: provides implementation for non-task-specific microcode execution and
        // state.  Task subclasses implement and execute Task-specific behavior and are called into
        // by the base class as necessary.
        public abstract class Task
        {
            public Task(AltoCPU cpu)
            {
                _wakeup = false;
                _mpc = 0xffff;  // invalid, for sanity checking
                _taskType = TaskType.Invalid;
                _cpu = cpu;
            }

            public int Priority
            {
                get { return (int)_taskType; }
            }

            public bool Wakeup
            {
                get { return _wakeup; }
            }

            public ushort MPC
            {
                get { return _mpc; }
            }

            public virtual void Reset()
            {
                // From The Alto Hardware Manual (section 2, "Initialization"):
                // "...each task start[s] at the location which is its task number"
                // 
                _mpc = (ushort)_taskType;
            }

            public virtual void BlockTask()
            {
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
                ushort nextModifier = 0;
                _loadS = false;
                _rSelect = 0;
                _busData = 0;


                Shifter.SetMagic(false);

                //
                // Wait for memory state machine if a memory operation is requested by this instruction and
                // the memory isn't ready yet.
                // TODO: this needs to be seriously cleaned up.
                //
                if (instruction.BS == BusSource.ReadMD ||
                    instruction.F1 == SpecialFunction1.LoadMAR ||
                    instruction.F2 == SpecialFunction2.StoreMD)
                {

                    MemoryOperation op;

                    if (instruction.BS == BusSource.ReadMD)
                    {
                        op = MemoryOperation.Read;
                    }
                    else if (instruction.F1 == SpecialFunction1.LoadMAR)
                    {
                        op = MemoryOperation.LoadAddress;
                    }
                    else
                    {
                        op = MemoryOperation.Store;
                    }

                    if (!_cpu._system.MemoryBus.Ready(op))
                    {
                        // Suspend operation for this cycle.
                        return false;
                    }
                }

                // If we have a modified next field from the last instruction, make sure it gets applied to this one.
                nextModifier = _nextModifier;
                _nextModifier = 0;

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
                            _busData = _cpu._system.MemoryBus.ReadMD();
                            break;

                        case BusSource.ReadMouse:
                            //throw new NotImplementedException("ReadMouse bus source not implemented.");
                            _busData = 0;   // TODO: implement;
                            break;

                        case BusSource.ReadDisp:
                            // "The high-order bits of IR cannot be read directly, but the displacement field of IR (8 low order bits),
                            // may be read with the <-DISP bus source.  If the X field of the instruction is zero (i.e. it specifies page 0
                            // addressing) then the DISP field of the instruction is put on BUS[8-15] and BUS[0-7] is zeroed.  If the X
                            // field of the instruction is nonzero (i.e. it specifies PC-relative or base-register addressing) then the DISP
                            // field is sign-extended and put on the bus."
                            // NB: the "X" field of the NOVA instruction is IR[6-7]                            
                            _busData = (ushort)(_cpu._ir & 0xff);

                            if ((_cpu._ir & 0x300) != 0)
                            {
                                // sign extend if necessary
                                if ((_cpu._ir & 0x80) == 0x80)
                                {
                                    _busData |= (0xff00);
                                }
                            }
                            break;

                        default:
                            throw new InvalidOperationException(String.Format("Unhandled bus source {0}.", instruction.BS));
                            break;
                    }
                }
                else
                {
                    // See also comments below.
                    _busData = ControlROM.ConstantROM[(instruction.RSELECT << 3) | ((uint)instruction.BS)];
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
                    _busData &= ControlROM.ConstantROM[(instruction.RSELECT << 3) | ((uint)instruction.BS)];
                }

                // Do ALU operation
                aluData = ALU.Execute(instruction.ALUF, _busData, _cpu._t, _skip);

                // Reset shifter op
                Shifter.SetOperation(ShifterOp.None, 0);

                //
                // Do Special Functions
                //
                switch (instruction.F1)
                {
                    case SpecialFunction1.None:
                        // Do nothing.  Well, that was easy.
                        break;

                    case SpecialFunction1.LoadMAR:
                        _cpu._system.MemoryBus.LoadMAR(aluData);    // Start main memory reference
                        break;

                    case SpecialFunction1.Task:
                        nextTask = true;            // Yield to other more important tasks
                        break;

                    case SpecialFunction1.Block:
                        // Technically this is to be invoked by the hardware device associated with a task.
                        // That logic would be circituous and unless there's a good reason not to that is discovered
                        // later, I'm just going to directly block the current task here.
                        _cpu.BlockTask(this._taskType);
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

                switch (instruction.F2)
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
                        _cpu._system.MemoryBus.LoadMD(_busData);
                        break;

                    case SpecialFunction2.Constant:
                        // Ignored here; handled by Constant ROM access logic above.
                        break;

                    default:
                        // Let the specific task implementation take a crack at this.
                        ExecuteSpecialFunction2((int)instruction.F2);
                        break;
                }

                //
                // Write back to registers:
                //
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

                // Load T
                if (instruction.LoadT)
                {
                    // Does this operation change the source for T?
                    bool loadTFromALU = false;
                    switch (instruction.ALUF)
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

                // Load L (and M) from ALU outputs.
                if (instruction.LoadL)
                {
                    _cpu._l = aluData;
                    _cpu._m = aluData;

                    // Save ALUC0 for use in the next ALUCY special function.
                    _cpu._aluC0 = (ushort)ALU.Carry;
                }

                //
                // Select next address, using the address modifier from the last instruction.
                //
                _mpc = (ushort)(instruction.NEXT | nextModifier);
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
            protected uint _rSelect;        // RSELECT field from current instruction, potentially modified by task
            protected bool _loadS;          // Whether to load S from M at and of cycle


            //
            // Global Task Data
            //
            protected AltoCPU _cpu;
            protected ushort _mpc;
            protected TaskType _taskType;
            protected bool _wakeup;

            // Emulator Task-specific data.  This is placed here because it is used by the ALU and it's easier to reference in the
            // base class even if it does break encapsulation.  See notes in the EmulatorTask class for meaning.        
            protected int _skip;
        }
    }
}
