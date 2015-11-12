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
        /// DisplayVerticalTask provides functionality for the DVT task
        /// </summary>
        private class DisplayVerticalTask : Task
        {
            public DisplayVerticalTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayVertical;                
                _wakeup = false;
            }

            protected override bool ExecuteInstruction(MicroInstruction instruction)
            {
                // We put ourselves back to sleep immediately once we've started running
                _wakeup = false;

                return base.ExecuteInstruction(instruction);
            }

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DisplayVerticalF2 dv2 = (DisplayVerticalF2)instruction.F2;
                switch (dv2)
                {
                    case DisplayVerticalF2.EVENFIELD:
                        _nextModifier |= (ushort)(_cpu._system.DisplayController.EVENFIELD ? 1 : 0);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display vertical F2 {0}.", dv2));
                        break;
                }
            }
        }
    }
}
