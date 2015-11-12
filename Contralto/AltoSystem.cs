using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.CPU;
using Contralto.IO;
using Contralto.Memory;
using Contralto.Display;

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
            _displayController = new DisplayController(this);
            _fakeDisplayController = new FakeDisplayController(this);

            // Attach memory-mapped devices to the bus
            _memBus.AddDevice(_mem);
            _memBus.AddDevice(_keyboard);

            // Register devices that need clocks
            _clockableDevices = new List<IClockable>();
            _clockableDevices.Add(_memBus);
            _clockableDevices.Add(_diskController);
            _clockableDevices.Add(_displayController);
            _clockableDevices.Add(_fakeDisplayController);
            _clockableDevices.Add(_cpu);            

            Reset();
        }

        public void Reset()
        {
            _cpu.Reset();
            _memBus.Reset();
            _mem.Reset();
            ALU.Reset();
            Shifter.Reset();
            _diskController.Reset();
            _displayController.Reset();
        }

        /// <summary>
        /// Attaches an emulated display device to the system.
        /// TODO: This is currently tightly-coupled with the Debugger, make
        /// more general.
        /// </summary>
        /// <param name="d"></param>
        public void AttachDisplay(Debugger d)
        {
            _displayController.AttachDisplay(d);
            _fakeDisplayController.AttachDisplay(d);
        }

        public void SingleStep()
        {
            // Run every device that needs attention for a single clock cycle.
            for (int i = 0; i < _clockableDevices.Count; i++)
            {
                _clockableDevices[i].Clock();
            }
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

        public DisplayController DisplayController
        {
            get { return _displayController; }
        }

        public Keyboard Keyboard
        {
            get { return _keyboard; }
        }

        /// <summary>
        /// Time (in msec) for one system clock
        /// </summary>
        ///
        public static double ClockInterval
        {
            get { return 0.00017; } // appx 170nsec, TODO: more accurate value?
        }

        private AltoCPU _cpu;
        private MemoryBus _memBus;
        private Contralto.Memory.Memory _mem;
        private Keyboard _keyboard;
        private DiskController _diskController;
        private DisplayController _displayController;
        private FakeDisplayController _fakeDisplayController;

        private List<IClockable> _clockableDevices;
    }
}
