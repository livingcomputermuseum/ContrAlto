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

using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// ParityTask is provided for completeness only, and implements the logic for the Parity task.
        /// The Parity Task will never actually be woken because I'm not planning on emulating faulty memory.
        /// </summary>
        private sealed class ParityTask : Task
        {
            public ParityTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.Parity;                
                _wakeup = false;
            }            
        }
    }
}
