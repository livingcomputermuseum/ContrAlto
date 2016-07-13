/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using Contralto.Display;
using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayVerticalTask provides functionality for the DVT task
        /// </summary>
        private sealed class DisplayVerticalTask : Task
        {
            public DisplayVerticalTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayVertical;                
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
                DisplayVerticalF2 dv2 = (DisplayVerticalF2)instruction.F2;
                switch (dv2)
                {
                    case DisplayVerticalF2.EVENFIELD:
                        _nextModifier |= (ushort)(_displayController.EVENFIELD ? 1 : 0);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display vertical F2 {0}.", dv2));                        
                }
            }

            private DisplayController _displayController;
        }
    }
}
