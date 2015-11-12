using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayWordTask provides functionality for the DWT task
        /// </summary>
        private class DisplayWordTask : Task
        {
            public DisplayWordTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayWord;                
                _wakeup = false;
            }            

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DisplayWordF2 dw2 = (DisplayWordF2)instruction.F2;
                switch (dw2)
                {
                    case DisplayWordF2.LoadDDR:
                        _cpu._system.DisplayController.LoadDDR(_busData);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display word F2 {0}.", dw2));
                        break;
                }
            }
        }
    }
}
