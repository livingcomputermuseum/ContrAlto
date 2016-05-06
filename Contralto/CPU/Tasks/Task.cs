using System;

using Contralto.Memory;
using Contralto.Logging;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        public enum InstructionCompletion
        {
            Normal,
            TaskSwitch,
            MemoryWait,
        }

        /// <summary>
        /// Base task class: provides implementation for non-task-specific microcode execution and
        /// state.  Task subclasses implement and execute Task-specific behavior and are called into
        /// by the base class as necessary.
        /// </summary>
        public abstract class Task
        {
            public Task(AltoCPU cpu)
            {
                _wakeup = false;
                _mpc = 0xffff;  // invalid, for sanity checking
                _taskType = TaskType.Invalid;
                _cpu = cpu;

                _systemType = Configuration.SystemType;
            }

            public int Priority
            {
                get { return (int)_taskType; }
            }

            public TaskType TaskType
            {
                get { return _taskType; }
            }

            public bool Wakeup
            {
                get { return _wakeup; }
            }

            public ushort MPC
            {
                get { return _mpc; }
            }

            /// <summary>
            /// Indicates whether a task switch just happened.  TASK instructions behave differently on the
            /// first instruction after a switch.  This is not documented, but observed on the real hardware.
            /// (See the implementation of the Task SF for more details.)
            /// </summary>
            public bool FirstInstructionAfterSwitch
            {
                get { return _firstInstructionAfterSwitch; }
                set { _firstInstructionAfterSwitch = value; }
            }

            public virtual void Reset()
            {
                // From The Alto Hardware Manual (section 2, "Initialization"):
                // "...each task start[s] at the location which is its task number"
                // 
                _mpc = (ushort)_taskType;
                _rdRam = false;
                _rb = 0;
                _firstInstructionAfterSwitch = false;

                _swMode = false;
                _wrtRam = false;
                _wakeup = false;
                _skip = 0;
            }

            public virtual void SoftReset()
            {
                //
                // As above but we leave all other state alone.
                // 
                _mpc = (ushort)_taskType;
            }

            /// <summary>
            /// Removes the Wakeup signal for this task.
            /// </summary>
            public virtual void BlockTask()
            {
                _wakeup = false;
            }

            /// <summary>
            /// Sets the Wakeup signal for this task.
            /// </summary>
            public virtual void WakeupTask()
            {
                _wakeup = true;
            }

            /// <summary>
            /// Executes a single microinstruction.
            /// </summary>
            /// <returns>An InstructionCompletion indicating whether this instruction calls for a task switch or not.</returns>
            public InstructionCompletion ExecuteNext()
            {                
                MicroInstruction instruction = UCodeMemory.GetInstruction(_mpc, _taskType);                             
                return ExecuteInstruction(instruction);
            }

            /// <summary>
            /// ExecuteInstruction causes the Task to execute the next instruction (the one
            /// _mpc is pointing to).  The base implementation covers non-task specific logic,
            /// subclasses (specific task implementations) may provide their own implementations.
            /// </summary>
            /// <returns>An InstructionCompletion indicating whether this instruction calls for a task switch or not.</returns>
            protected virtual InstructionCompletion ExecuteInstruction(MicroInstruction instruction)
            {
                InstructionCompletion completion = InstructionCompletion.Normal;
                bool swMode = false;
                bool block = false;       
                ushort aluData;
                ushort nextModifier;
                _loadR = false;
                _loadS = false;
                _rSelect = 0;
                _srSelect = 0;
                _busData = 0;
                _softReset = false;

                Shifter.Reset();                

                //
                // Wait for memory state machine if a memory operation is requested by this instruction and
                // the memory isn't ready yet.                
                //                
                if (instruction.MemoryAccess) 
                {                    
                    if (!_cpu._system.MemoryBus.Ready(instruction.MemoryOperation))
                    {
                        // Suspend operation for this cycle.
                        return InstructionCompletion.MemoryWait;
                    }
                }

                // If we have a modified next field from the last instruction, make sure it gets applied to this one.
                nextModifier = _nextModifier;
                _nextModifier = 0;

                _srSelect = _rSelect = instruction.RSELECT;                

                // Give tasks the chance to modify parameters early on (like RSELECT)
                ExecuteSpecialFunction2Early(instruction);

                // Select BUS data.
                if (!instruction.ConstantAccess)
                {
                    // Normal BUS data (not constant ROM access).
                    switch (instruction.BS)
                    {
                        case BusSource.ReadR:
                            _busData = _cpu._r[_rSelect];
                            break;

                        case BusSource.LoadR:
                            _busData = 0;       // "Loading R forces the BUS to 0 so that an ALU function of 0 and T may be executed simultaneously"
                            _loadR = true;
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
                            _busData = _cpu._system.Mouse.PollMouseBits();
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
                    }
                }
                else
                {
                    // See also comments below.
                    _busData = instruction.ConstantValue;
                }

                // Constant ROM access:
                // "The constant memory is gated to the bus by F1=7, F2=7, or BS>4.  The constant memory is addressed by the
                // (8 bit) concatenation of RSELECT and BS.  The intent in enabling constants with BS>4 is to provide a masking
                // facility, particularly for the <-MOUSE and <-DISP bus sources.  This works because the processor bus ANDs if
                // more than one source is gated to it.  Up to 32 such mask contans can be provided for each of the four bus sources
                // > 4."
                // This is precached by the MicroInstruction object.
                if (instruction.ConstantAccessOrBS4)
                {
                    _busData &= instruction.ConstantValue;
                }

                //
                // If there was a RDRAM operation last cycle, we AND in the uCode RAM data here.
                //
                if (_rdRam)
                {
                    _busData &= UCodeMemory.ReadRAM();
                    _rdRam = false;
                }

                //
                // Let F1s that need to modify bus data before the ALU runs do their thing
                // (this is used by the emulator RSNF and Ethernet EILFCT)
                //
                ExecuteSpecialFunction1Early(instruction);

                // Do ALU operation.
                // Small optimization: if we're just taking bus data across the ALU, we
                // won't go through the ALU.Execute call; this is a decent performance gain for a bit
                // more ugly code...
                if (instruction.ALUF != AluFunction.Bus)
                {
                    aluData = ALU.Execute(instruction.ALUF, _busData, _cpu._t, _skip);
                }
                else
                {
                    aluData = _busData;
                    ALU.Carry = 0;
                }

                //
                // If there was a WRTRAM operation last cycle, we write the uCode RAM here
                // using the results of this instruction's ALU operation and the M register
                // from the last instruction.
                //
                if (_wrtRam)
                {
                    UCodeMemory.WriteRAM(aluData, _cpu._m);
                    _wrtRam = false;
                }

                //
                // If there was an SWMODE operation last cycle, we set the flag to ensure it
                // takes effect at the end of this cycle.
                //
                if (_swMode)
                {
                    _swMode = false;
                    swMode = true;
                }
                
                //
                // Do Special Functions
                //
                switch (instruction.F1)
                {
                    case SpecialFunction1.None:
                        // Do nothing.  Well, that was easy.
                        break;

                    case SpecialFunction1.LoadMAR:  
                        // Do MAR or XMAR reference based on whether F2 is MD<- (for Alto IIs), indicating an extended memory reference.
                        _cpu._system.MemoryBus.LoadMAR(
                            aluData, 
                            _taskType, 
                            _systemType == SystemType.AltoI ? false : instruction.F2 == SpecialFunction2.StoreMD);                      
                        break;

                    case SpecialFunction1.Task:
                        //
                        // If the first uOp executed after a task switch contains a TASK F1, it does not take effect.
                        // This is observed on the real hardware, and does not appear to be documented.
                        // It also doensn't appear to affect the execution of the standard Alto uCode in any significant
                        // way, but is included here for correctness.
                        //
                        if (!_firstInstructionAfterSwitch)
                        {
                            // Yield to other more important tasks
                            completion = InstructionCompletion.TaskSwitch;            
                        }
                        break;

                    case SpecialFunction1.Block:
                        // Technically this is to be invoked by the hardware device associated with a task.
                        // That logic would be circuituous and unless there's a good reason not to that is discovered
                        // later, I'm just going to directly block the current task here.
                        _cpu.BlockTask(this._taskType);

                        // Let task-specific behavior take place at the end of this cycle.
                        block = true;                        
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
                        ExecuteSpecialFunction1(instruction);
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
                        // Handled below, after the Shifter runs
                        break;

                    case SpecialFunction2.ShEq0:
                        // Same as above.
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
                        // Special case for XMAR on non-Alto I machines: if F1 is a LoadMAR we do nothing here;
                        // the handler for LoadMAR will load the correct bank.
                        if (_systemType == SystemType.AltoI)
                        {
                            _cpu._system.MemoryBus.LoadMD(_busData);
                        }
                        else if(instruction.F1 != SpecialFunction1.LoadMAR)
                        {
                            _cpu._system.MemoryBus.LoadMD(_busData);
                        }
                        break;

                    case SpecialFunction2.Constant:
                        // Ignored here; handled by Constant ROM access logic above.
                        break;

                    default:
                        // Let the specific task implementation take a crack at this.
                        ExecuteSpecialFunction2(instruction);
                        break;
                }

                //
                // Do the shifter operation if we're doing an operation that requires the shifter output (loading R, doing a LoadDNS,
                // modifying NEXT based on the shifter output.)
                //
                if (_loadR || instruction.NeedShifterOutput)
                {
                    // A crude optimization:  if there's no shifter operation,
                    // we bypass the call to DoOperation and stuff L in Shifter.Output ourselves.
                    if (Shifter.Op == ShifterOp.None)
                    {
                        Shifter.Output = _cpu._l;
                    }
                    else
                    {
                        Shifter.DoOperation(_cpu._l, _cpu._t);
                    }
                }

                //
                // Handle NEXT modifiers that rely on the Shifter output.
                //
                switch(instruction.F2)
                {
                    case SpecialFunction2.ShLt0:
                        //
                        // Note:
                        // "the value of SHIFTER OUTPUT is determined by the value of L as the microinstruction
                        // *begins* execution and the shifter function specified during the  *current* microinstruction.
                        //
                        // Since we haven't modifed L yet, and we've calculated the shifter output above, we're good to go here.
                        //
                        if ((short)Shifter.Output < 0)
                        {
                            _nextModifier = 1;
                        }
                        break;

                    case SpecialFunction2.ShEq0:
                        // See note above.
                        if (Shifter.Output == 0)
                        {
                            _nextModifier = 1;
                        }
                        break;
                }

                //
                // Write back to registers:
                //
                // Do writeback to selected R register from shifter output.
                //
                if (_loadR)
                {
                    _cpu._r[_rSelect] = Shifter.Output;
                }

                // Do writeback to selected S register from M
                if (_loadS)
                {
                    _cpu._s[_rb][_srSelect] = _cpu._m;
                }

                // Load T
                if (instruction.LoadT)
                {
                    // Does this operation change the source for T?                    
                    _cpu._t = instruction.LoadTFromALU ? aluData : _busData;

                    //
                    // Control RAM: "...the control RAM address is specified by the control RAM
                    // address register... which is loaded from the ALU output whenver T is loaded
                    // from its source."
                    //
                    UCodeMemory.LoadControlRAMAddress(aluData);                    
                }

                // Load L (and M) from ALU outputs.
                if (instruction.LoadL)
                {
                    _cpu._l = aluData;

                    // Only RAM-related tasks can modify M.  (Currently only the Emulator.)
                    if (_taskType == TaskType.Emulator)
                    {
                        _cpu._m = aluData;
                    }

                    // Save ALUC0 for use in the next ALUCY special function.
                    _cpu._aluC0 = (ushort)ALU.Carry;
                }

                //
                // Execute special functions that happen late in the cycle
                //                
                ExecuteSpecialFunction2Late(instruction);

                //
                // Switch banks if the last instruction had an SWMODE F1;
                // this depends on the value of the NEXT field in this instruction.
                // (And apparently the modifier applied to NEXT in this instruction -- MADTEST expects this.)
                //
                if (swMode)
                {
                    // Log.Write(Logging.LogComponent.Microcode, "SWMODE: uPC {0}, next uPC {1} (NEXT is {2})", Conversion.ToOctal(_mpc), Conversion.ToOctal(instruction.NEXT | nextModifier), Conversion.ToOctal(instruction.NEXT));
                    UCodeMemory.SwitchMode((ushort)(instruction.NEXT | nextModifier), _taskType);                    
                }

                //
                // Do task-specific BLOCK behavior if the last instruction had a BLOCK F1.
                //
                if (block)
                {
                    ExecuteBlock();
                }

                //
                // Select next address, using the address modifier from the last instruction.
                // (Unless a soft reset occurred during this instruction)
                //
                if (!_softReset)
                {
                    _mpc = (ushort)(instruction.NEXT | nextModifier);
                }

                _firstInstructionAfterSwitch = false;
                return completion;
            }

            /// <summary>
            /// Provides task-specific implementations the opportunity to handle task-specific bus sources.
            /// </summary>
            /// <param name="bs"></param>
            /// <returns></returns>
            protected virtual ushort GetBusSource(int bs)
            {
                // Nothing by default.
                return 0;
            }

            /// <summary>
            /// Executes before the ALU runs but after bus sources have been selected.
            /// </summary>
            /// <param name="instruction"></param>
            protected virtual void ExecuteSpecialFunction1Early(MicroInstruction instruction)
            {
                // Nothing by default
            }

            /// <summary>
            /// Executes after the ALU has run but before the shifter runs, provides task-specific implementations 
            /// the opportunity to handle task-specific F1s.
            /// </summary>
            /// <param name="instruction"></param>
            protected virtual void ExecuteSpecialFunction1(MicroInstruction instruction)
            {
                // Nothing by default
            }

            /// <summary>
            /// Executes before bus sources are selected.  Used to allow Task-specific F2s that need to 
            /// modify RSELECT to do so.
            /// </summary>
            /// <param name="f2"></param>
            protected virtual void ExecuteSpecialFunction2Early(MicroInstruction instruction)
            {
                // Nothing by default.
            }

            /// <summary>
            /// Executes after the ALU has run but before the shifter runs, provides task-specific implementations 
            /// the opportunity to handle task-specific F2s.
            /// </summary>
            protected virtual void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                // Nothing by default.
            }

            /// <summary>
            /// Executes after the shifter has run, provides task-specific implementations the opportunity
            /// to handle task-specific F2s late in the cycle.
            /// </summary>
            /// <param name="instruction"></param>
            protected virtual void ExecuteSpecialFunction2Late(MicroInstruction instruction)
            {
                // Nothing by default.
            }

            /// <summary>
            /// Allows task-specific handling for BLOCK microinstructions.
            /// (Disk and Display BLOCKs have side effects apart from removing Wakeup from the task, for example).
            /// </summary>
            protected virtual void ExecuteBlock()
            {
                // Nothing by default
            }

            /// <summary>
            /// Allows task-specific behavior when a new task begins execution.
            /// (Generally this is used to remove wakeup immediately.)
            /// </summary>
            public virtual void OnTaskSwitch()
            {
                // Nothing by default
            }

            /// <summary>
            /// Cache the system type.
            /// </summary>
            protected SystemType _systemType;

            //
            // Per uInstruction Task Data:
            // Modified by both the base Task implementation and any subclasses
            //
            // TODO: maybe instead of these being shared (which feels kinda bad)
            // these could be encapsulated in an object and passed to subclass implementations?            
            protected ushort _busData;          // Data placed onto the bus (ANDed from multiple sources)
            protected ushort _nextModifier;     // Bits ORed onto the NEXT field of the current instruction
            protected uint _rSelect;            // RSELECT field from current instruction, potentially modified by task
            protected uint _srSelect;           // RSELECT field as used by S register access (not modified in the same way as normal _rSelect).
            protected bool _loadS;              // Whether to load S from M at and of cycle
            protected bool _loadR;              // Whether to load R from shifter at end of cycle.
            protected bool _rdRam;              // Whether to load uCode RAM onto the bus during the next cycle.
            protected bool _wrtRam;             // Whether to write uCode RAM from M and ALU outputs during the next cycle.
            protected bool _swMode;             // Whether to switch uCode banks during the next cycle.
            protected bool _softReset;          // Whether this instruction caused a soft reset (so MPC should not come from instruction's NEXT field)


            //
            // Global Task Data
            //
            protected AltoCPU _cpu;
            protected ushort _mpc;
            protected ushort _rb;     // S register bank select
            protected TaskType _taskType;
            protected bool _wakeup;
            protected bool _firstInstructionAfterSwitch;

            // Emulator Task-specific data.  This is placed here because it is used by the ALU and it's easier to reference in the
            // base class even if it does break encapsulation.  See notes in the EmulatorTask class for meaning.        
            protected int _skip;
        }
    }
}
