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
