/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

using Contralto.CPU;
using Contralto.IO;
using Contralto.Memory;
using Contralto.Display;
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
            _orbitController = new OrbitController(this);
            _audioDAC = new AudioDAC(this);
            _organKeyboard = new OrganKeyboard(this);

            _cpu = new AltoCPU(this);

            // Attach memory-mapped devices to the bus
            _memBus.AddDevice(_mem);
            _memBus.AddDevice(_keyboard);
            _memBus.AddDevice(_mouse);
            _memBus.AddDevice(_audioDAC);
            _memBus.AddDevice(_organKeyboard);

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
            _orbitController.Reset();            

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

            //
            // Allow the DAC to flush its output
            //
            _audioDAC.Shutdown();
        }

        public void SingleStep()
        {
            // Run every device that needs attention for a single clock cycle.
            _memBus.Clock();
            _cpu.Clock();

            // Clock the scheduler
            _scheduler.Clock();
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

        public OrbitController OrbitController
        {
            get { return _orbitController; }
        }

        public Scheduler Scheduler
        {
            get { return _scheduler; }
        }

        private AltoCPU _cpu;
        private MemoryBus _memBus;
        private Memory.Memory _mem;
        private Keyboard _keyboard;
        private Mouse _mouse;
        private DiskController _diskController;
        private DisplayController _displayController;
        private EthernetController _ethernetController;
        private OrbitController _orbitController;
        private AudioDAC _audioDAC;
        private OrganKeyboard _organKeyboard;

        private Scheduler _scheduler;
    }
}
