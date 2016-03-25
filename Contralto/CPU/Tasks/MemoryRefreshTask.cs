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
            
            //
            // MRT has no special functions or special behavior, but here's a note regarding the MRT
            // wakeup behavior, for future reference:
            //
            // Based on readings of the MRT microcode, the MRT keeps its wakeup
            // until it executes a BLOCK.  (i.e. no special wakeup handling at all.)
            // "; This version assumes MRTACT is cleared by BLOCK, not MAR<- R37"
            //            
        }
    }
}
