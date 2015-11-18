using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayWordTask provides functionality for the DWT task
        /// </summary>
        private sealed class CursorTask : Task
        {
            public CursorTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.Cursor;                
                _wakeup = false;
            }            
            
            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                CursorF2 cf2 = (CursorF2)instruction.F2;
                switch (cf2)
                {
                    case CursorF2.LoadXPREG:
                        // TODO: load cursor X-position register from bus
                        break;

                    case CursorF2.LoadCSR:
                        // TODO: load cursor shift register from bus
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled cursor F2 {0}.", cf2));                        
                }
            }
        }
    }
}
