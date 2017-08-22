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
            _tridentController = new TridentController(this);

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
            _tridentController.Reset();

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

            //
            // Save disk contents
            //
            _diskController.CommitDisk(0);
            _diskController.CommitDisk(1);

            for (int i = 0; i < 8; i++)
            {
                _tridentController.CommitDisk(i);
            }
        }

        public void SingleStep()
        {
            // Run every device that needs attention for a single clock cycle.
            _memBus.Clock();
            _cpu.Clock();

            // Clock the scheduler
            _scheduler.Clock();
        }

        public void LoadDiabloDrive(int drive, string path, bool newImage)
        {
            if (drive < 0 || drive > 1)
            {
                throw new InvalidOperationException("drive must be 0 or 1.");
            }

            //
            // Commit the current disk first
            //
            _diskController.CommitDisk(drive);

            DiskGeometry geometry;

            //
            // We select the disk type based on the file extension.  Very elegant.
            //
            switch(Path.GetExtension(path).ToLowerInvariant())
            {
                case ".dsk":
                    geometry = DiskGeometry.Diablo31;
                    break;

                case ".dsk44":
                    geometry = DiskGeometry.Diablo44;
                    break;

                default:
                    geometry = DiskGeometry.Diablo31;
                    break;
            }


            IDiskPack newPack;

            if (newImage)
            {
                newPack = InMemoryDiskPack.CreateEmpty(geometry, path);
            }
            else
            {
                newPack = InMemoryDiskPack.Load(geometry, path);
            }

            _diskController.Drives[drive].LoadPack(newPack);
        }

        public void UnloadDiabloDrive(int drive)
        {
            if (drive < 0 || drive > 1)
            {
                throw new InvalidOperationException("drive must be 0 or 1.");
            }

            //
            // Commit the current disk first
            //
            _diskController.CommitDisk(drive);

            _diskController.Drives[drive].UnloadPack();
        }

        public void LoadTridentDrive(int drive, string path, bool newImage)
        {
            if (drive < 0 || drive > 8)
            {
                throw new InvalidOperationException("drive must be between 0 and 7.");
            }

            //
            // Commit the current disk first
            //
            _tridentController.CommitDisk(drive);

            DiskGeometry geometry;

            //
            // We select the disk type based on the file extension.  Very elegant.
            //
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".t80":
                    geometry = DiskGeometry.TridentT80;
                    break;

                case ".t300":                
                    geometry = DiskGeometry.TridentT300;
                    break;

                default:
                    geometry = DiskGeometry.TridentT80;
                    break;
            }


            IDiskPack newPack;

            if (newImage)
            {
                newPack = FileBackedDiskPack.CreateEmpty(geometry, path);
            }
            else
            {
                newPack = FileBackedDiskPack.Load(geometry, path);
            }

            _tridentController.Drives[drive].LoadPack(newPack);
        }

        public void UnloadTridentDrive(int drive)
        {
            if (drive < 0 || drive > 7)
            {
                throw new InvalidOperationException("drive must be between 0 and 7.");
            }

            //
            // Commit the current disk first
            //
            _tridentController.CommitDisk(drive);

            _tridentController.Drives[drive].UnloadPack();
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

        public TridentController TridentController
        {
            get { return _tridentController; }
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
        private TridentController _tridentController;

        private Scheduler _scheduler;
    }
}
