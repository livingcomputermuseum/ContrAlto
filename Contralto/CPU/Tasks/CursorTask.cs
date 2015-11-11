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
        /// <summary>
        /// DisplayWordTask provides functionality for the DWT task
        /// </summary>
        private class CursorTask : Task
        {
            public CursorTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayWord;

                // The Wakeup signal is always true for the Emulator task.
                _wakeup = false;
            }            
            
            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DisplayWordF2 ef2 = (DisplayWordF2)instruction.F2;
                switch (ef2)
                {
                    case DisplayWordF2.LoadDDR:

                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display word F2 {0}.", ef2));
                        break;
                }
            }
        }
    }
}
