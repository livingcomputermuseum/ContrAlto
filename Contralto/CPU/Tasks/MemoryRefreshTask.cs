using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

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
        }
    }
}
