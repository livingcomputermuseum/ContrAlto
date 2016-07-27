/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// CursorTask provides the implementation of Cursor-specific task functions
        /// </summary>
        private sealed class CursorTask : Task
        {
            public CursorTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.Cursor;                
                _wakeup = false;
            }

            public override void OnTaskSwitch()
            {
                // We put ourselves back to sleep immediately once we've started running.
                _wakeup = false;                
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
