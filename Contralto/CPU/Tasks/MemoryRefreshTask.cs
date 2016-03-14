namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DisplayWordTask provides functionality for the Memory Refresh task
        /// </summary>
        private sealed class MemoryRefreshTask : Task
        {
            public MemoryRefreshTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.MemoryRefresh;
                
                _wakeup = false;
            }

            /*
            protected override InstructionCompletion ExecuteInstruction(MicroInstruction instruction)
            {
                //
                // Based on readings of the MRT microcode, the MRT keeps its wakeup
                // until it executes a BLOCK.
                // "; This version assumes MRTACT is cleared by BLOCK, not MAR<- R37"
                //
                return base.ExecuteInstruction(instruction);
            }*/
        }
    }
}
