using System.Collections.Generic;

using Contralto.CPU;
using Contralto.IO;
using Contralto.Memory;
using Contralto.Display;
using System.Timers;
using System.IO;
using System;

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
            _scheduler = new Scheduler();
            
            _memBus = new MemoryBus();
            _mem = new Memory.Memory();            
            _keyboard = new Keyboard();
            _diskController = new DiskController(this);
            _displayController = new DisplayController(this);
            _mouse = new Mouse();
            _ethernetController = new EthernetController(this);

            _cpu = new AltoCPU(this);


            // Attach memory-mapped devices to the bus
            _memBus.AddDevice(_mem);
            _memBus.AddDevice(_keyboard);
            _memBus.AddDevice(_mouse);

            // Register devices that need clocks
            _clockableDevices = new List<IClockable>();            
            _clockableDevices.Add(_memBus);                    
            _clockableDevices.Add(_cpu);

            Reset();           
        }

        public void Reset()
        {
            _scheduler.Reset();
            
            _memBus.Reset();
            _mem.Reset();
            ALU.Reset();
            Shifter.Reset();
            _diskController.Reset();
            _displayController.Reset();
            _keyboard.Reset();
            _mouse.Reset();
            _cpu.Reset();
            _ethernetController.Reset();

            UCodeMemory.Reset();            
        }

        /// <summary>
        /// Attaches an emulated display device to the system.
        /// </summary>
        /// <param name="d"></param>
        public void AttachDisplay(IAltoDisplay d)
        {
            _displayController.AttachDisplay(d);           
        }

        public void DetachDisplay()
        {
            _displayController.DetachDisplay();
        }

        public void Shutdown()
        {
            // Kill any host interface threads that are running.
            if (_ethernetController.HostInterface != null)
            {
                _ethernetController.HostInterface.Shutdown();
            }
        }

        public void SingleStep()
        {
            // Run every device that needs attention for a single clock cycle.
            int count = _clockableDevices.Count;
            for (int i = 0; i < count; i++)
            {
                _clockableDevices[i].Clock();
            }
            
            _scheduler.Clock();

            _clocks++;
        }

        public void LoadDrive(int drive, string path)
        {
            if (drive < 0 || drive > 1)
            {
                throw new InvalidOperationException("drive must be 0 or 1.");
            }

            DiabloDiskType type;

            //
            // We select the disk type based on the file extension.  Very elegant.
            //
            switch(Path.GetExtension(path).ToLowerInvariant())
            {
                case ".dsk44":
                    type = DiabloDiskType.Diablo44;
                    break;

                default:
                    type = DiabloDiskType.Diablo31;
                    break;
            }                

            DiabloPack newPack = new DiabloPack(type);

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {                
                newPack.Load(fs, path, false);
                fs.Close();                
            }

            _diskController.Drives[drive].LoadPack(newPack);
        }

        public void UnloadDrive(int drive)
        {
            if (drive < 0 || drive > 1)
            {
                throw new InvalidOperationException("drive must be 0 or 1.");
            }

            _diskController.Drives[drive].UnloadPack();
        }

        //
        // Disk handling
        //
        public void CommitDiskPack(int driveId)
        {
            DiabloDrive drive = _diskController.Drives[driveId];
            if (drive.IsLoaded)
            {
                using (FileStream fs = new FileStream(drive.Pack.PackName, FileMode.Create, FileAccess.Write))
                {
                    try
                    {
                        drive.Pack.Save(fs);
                    }
                    catch (Exception e)
                    {
                        // TODO: this does not really belong here.
                        System.Windows.Forms.MessageBox.Show(String.Format("Unable to save disk {0}'s contents.  Error {0}.  Any changes have been lost.", e.Message), "Disk save error");
                    }
                }
            }
        }

        public void PressBootKeys(AlternateBootType bootType)
        {
            switch(bootType)
            {
                case AlternateBootType.Disk:
                    _keyboard.PressBootKeys(Configuration.BootAddress, false);
                    break;

                case AlternateBootType.Ethernet:
                    _keyboard.PressBootKeys(Configuration.BootFile, true);
                    break;
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

        public Mouse Mouse
        {
            get { return _mouse; }
        }

        public EthernetController EthernetController
        {
            get { return _ethernetController; }
        }

        public Scheduler Scheduler
        {
            get { return _scheduler; }
        }

        public int _novaInst;        

        private AltoCPU _cpu;
        private MemoryBus _memBus;
        private Memory.Memory _mem;
        private Keyboard _keyboard;
        private Mouse _mouse;
        private DiskController _diskController;
        private DisplayController _displayController;
        private EthernetController _ethernetController;

        private Scheduler _scheduler;
        private ulong _clocks;

        private List<IClockable> _clockableDevices;
    }
}
