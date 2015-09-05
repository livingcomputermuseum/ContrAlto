using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.CPU;
using Contralto.IO;
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
            _memBus = new MemoryBus();
            _mem = new Memory.Memory();
            _keyboard = new Keyboard();
            _diskController = new DiskController(this);

            // Attach memory-mapped devices to the bus
            _memBus.AddDevice(_mem);
            _memBus.AddDevice(_keyboard);
            
            Reset();
        }

        public void Reset()
        {
            _cpu.Reset();
            _memBus.Reset();
        }

        public void SingleStep()
        {
            _memBus.Clock();
            _diskController.Clock();          
            _cpu.ExecuteNext();            
        }

        public AltoCPU CPU
        {
            get { return _cpu; }
        }

        public MemoryBus MemoryBus
        {
            get { return _memBus; }
        }

        public DiskController DiskController
        {
            get { return _diskController; }
        }

        /// <summary>
        /// Time (in msec) for one system clock
        /// </summary>
        public static double ClockInterval
        {
            get { return 0.00017; } // appx 170nsec, TODO: more accurate value?
        }

        private AltoCPU _cpu;
        private MemoryBus _memBus;
        private Contralto.Memory.Memory _mem;
        private Keyboard _keyboard;
        private DiskController _diskController;
    }
}
