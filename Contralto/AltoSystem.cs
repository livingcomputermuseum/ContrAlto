using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.CPU;
using Contralto.Memory;

namespace Contralto
{
    /// <summary>
    /// Encapsulates all Alto hardware; represents a complete Alto system.
    /// Provides interfaces for controlling and debugging the system externally.
    /// </summary>
    public class AltoSystem
    {
        public AltoSystem()
        {
            _cpu = new AltoCPU(this);
            _mem = new MemoryBus();
            Reset();
        }

        public void Reset()
        {
            _cpu.Reset();
            _mem.Reset();
        }

        public void SingleStep()
        {            
            _mem.Clock();            
            _cpu.ExecuteNext();            
        }

        public AltoCPU CPU
        {
            get { return _cpu; }
        }

        public MemoryBus MemoryBus
        {
            get { return _mem; }
        }

        private AltoCPU _cpu;
        private MemoryBus _mem;
    }
}
