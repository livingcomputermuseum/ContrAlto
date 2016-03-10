using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// CursorTask provides functionality for the Cursor-specific task functions
        /// </summary>
        private sealed class CursorTask : Task
        {
            public CursorTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.Cursor;                
                _wakeup = false;
            }

            protected override InstructionCompletion ExecuteInstruction(MicroInstruction instruction)
            {
                // We put ourselves back to sleep immediately once we've started running
                _wakeup = false;

                return base.ExecuteInstruction(instruction);
            }

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                CursorF2 cf2 = (CursorF2)instruction.F2;
                switch (cf2)
                {
                    case CursorF2.LoadXPREG:
                        // Load cursor X-position register from bus
                        _cpu._system.DisplayController.LoadXPREG(_busData);
                        break;

                    case CursorF2.LoadCSR:
                        // Load cursor shift register from bus
                        _cpu._system.DisplayController.LoadCSR(_busData);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled cursor F2 {0}.", cf2));                        
                }
            }
        }
    }
}
