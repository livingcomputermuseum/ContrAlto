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

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayWordTask provides functionality for the Memory Refresh task.
        /// </summary>
        private sealed class MemoryRefreshTask : Task
        {
            public MemoryRefreshTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.MemoryRefresh;
                
                _wakeup = false;
            }            
            
            protected override void ExecuteSpecialFunction1Early(MicroInstruction instruction)
            {
                //
                // Based on readings of the below MRT microcode comment, the MRT keeps its wakeup
                // until it executes a BLOCK on Alto IIs.  (i.e. no special wakeup handling at all.)
                // On Alto Is, this was accomplished by doing an MAR <- R37.
                //
                // "; This version assumes MRTACT is cleared by BLOCK, not MAR<- R37"
                //
                if (_systemType == SystemType.AltoI &&
                    instruction.F1 == SpecialFunction1.LoadMAR && 
                    _rSelect == 31)
                {
                    BlockTask();
                }

                base.ExecuteSpecialFunction1Early(instruction);
            }

        }
    }
}
