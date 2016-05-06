using Contralto.Display;
using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayHorizontalTask provides implementations of the DHT task functions.
        /// </summary>
        private sealed class DisplayHorizontalTask : Task
        {
            public DisplayHorizontalTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayHorizontal;                
                _wakeup = false;

                _displayController = _cpu._system.DisplayController;
            }

            public override void OnTaskSwitch()
            {
                // We put ourselves back to sleep immediately once we've started running.
                _wakeup = false;
            }

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DisplayHorizontalF2 dh2 = (DisplayHorizontalF2)instruction.F2;
                switch (dh2)
                {
                    case DisplayHorizontalF2.EVENFIELD:
                        _nextModifier |= (ushort)(_displayController.EVENFIELD ? 1 : 0);
                        break;

                    case DisplayHorizontalF2.SETMODE:
                        // "If bit 0 = 1, the bit clock rate is set to 100ns period (at the start of the next scan line),
                        // and a 1 is merged into NEXT[9]."
                        _displayController.SETMODE(_busData);

                        if ((_busData & 0x8000) != 0)
                        {
                            _nextModifier |= 1;
                        }
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display word F2 {0}.", dh2));                        
                }
            }

            protected override void ExecuteBlock()
            {
                _displayController.DHTBLOCK = true;                
            }

            private DisplayController _displayController;
        }
    }
}
