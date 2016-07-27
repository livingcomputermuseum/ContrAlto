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

using Contralto.Display;
using System;


namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayWordTask provides functionality for the DWT task
        /// </summary>
        private sealed class DisplayWordTask : Task
        {
            public DisplayWordTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.DisplayWord;
                _wakeup = false;

                _displayController = _cpu._system.DisplayController;
            }           

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DisplayWordF2 dw2 = (DisplayWordF2)instruction.F2;
                switch (dw2)
                {
                    case DisplayWordF2.LoadDDR:
                        _displayController.LoadDDR(_busData);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled display word F2 {0}.", dw2));                        
                }
            }

            protected override void ExecuteBlock()
            {
                _displayController.DWTBLOCK = true;

                //
                // Wake up DHT if it has not blocked itself.
                //
                if (!_displayController.DHTBLOCK)
                {
                    _cpu.WakeupTask(TaskType.DisplayHorizontal);
                }
            }

            private DisplayController _displayController;
        }
    }
}
