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
