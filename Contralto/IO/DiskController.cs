using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

namespace Contralto.IO
{
    public class DiskController : IClockable
    {
        public DiskController(AltoSystem system)
        {
            _system = system;
            Reset();

            // Wakeup the sector task first thing
            _system.CPU.WakeupTask(CPU.TaskType.DiskSector);
        }        

        public ushort KDATA
        {
            get
            {                
                return _kData;
            }
            set { _kData = value; }
        }

        public ushort KADR
        {
            get { return _kAdr; }
            set
            {
                _kAdr = value;
                _recNo = 0;

                // "In addition, it causes the head address bit to be loaded from KDATA[13]."
                _head = (_kData & 0x4) >> 2;

                // "0 normally, 1 if the command is to terminate immediately after the correct cylinder
                // position is reached (before any data is transferred)."
                _dataXfer = (_kAdr & 0x2) != 0x2;
            }
        }

        public ushort KCOM
        {
            get { return _kCom; }
            set
            {
                _kCom = value;

                // Read control bits (pg. 47 of hw manual)
                _xferOff = (_kCom & 0x10) == 0x10;
                _wdInhib = (_kCom & 0x08) == 0x08;
                _bClkSource = (_kCom & 0x04) == 0x04;
                _wffo = (_kCom & 0x02) == 0x02;
                _sendAdr = (_kCom & 0x01) == 0x01;

                Console.WriteLine(
                    "sst {0}, xferOff {1}, wdInhib {2}, bClkSource {3}, wffo {4}, sendAdr {5}",
                    _elapsedSectorStateTime,
                    _xferOff,
                    _wdInhib,
                    _bClkSource,
                    _wffo,
                    _sendAdr);

                // Update WDINIT state based on _wdInhib.
                if (_wdInhib)
                {
                    _wdInit = true;
                }
            }
        }

        /// <summary>
        /// Used by the DiskTask code to check the WDINIT signal for dispatch.
        /// </summary>
        public bool WDINIT
        {
            get { return _wdInit; }
        }

        public ushort KSTAT
        {
            get
            {
                Console.WriteLine("kstat read {0}", _kStat);
                return _kStat;
            }
            set
            {
                _kStat = value;
                Console.WriteLine("kstat write {0}", _kStat);
            }
        }

        public ushort RECNO
        {
            get { return _recMap[_recNo];  }
        }

        public bool DataXfer
        {
            get { return _dataXfer; }
        }

        /// <summary>
        /// This is a hack to see how the microcode expects INIT to work
        /// </summary>
        public bool RecordInit
        {
            get { return _elapsedSectorStateTime < 10; }
        }

        public int Cylinder
        {
            get { return _cylinder; }
        }

        public int SeekCylinder
        {
            get { return _destCylinder;  }
        }

        public int Head
        {
            get { return _head; }
        }

        public int Sector
        {
            get { return _sector; }
        }

        public int Drive
        {
            get { return 0;  }
        }

        public double ClocksUntilNextSector
        {
            get { return _sectorClocks - _elapsedSectorTime; }
        }

        public void Reset()
        {
            ClearStatus();
            _recNo = 0;
            _elapsedSectorTime = 0.0;
            _cylinder = 0;
            _sector = 0;
            _head = 0;
            _kStat = 0;

            _wdInhib = true;
            _xferOff = true;

            _wdInit = false;
        }

        public void Clock()
        {
            _elapsedSectorTime++;

            // TODO: only signal sector changes if disk is loaded, etc.
            if (_elapsedSectorTime > _sectorClocks)
            {
                //
                // Next sector; save fractional part of elapsed time (to more accurately keep track of time), move to next sector
                // and wake up sector task.
                //
                _elapsedSectorTime -= _sectorClocks;

                _sector = (_sector + 1) % 12;

                _kStat = (ushort)((_kStat & 0x0fff) | (_sector << 12));

                // TODO: seclate semantics.  Looks like if the sector task was BLOCKed when a new sector is signaled
                // then the seclate flag is set.

                // Reset internal state machine for sector data
                _sectorState = SectorState.HeaderReadDelay;
                _sectorWordIndex = 0;
                _elapsedSectorStateTime = 0.0;
                Console.WriteLine("New sector ({0}), switching to HeaderReadDelay state.", _sector);
                _kData = 13;

                _system.CPU.WakeupTask(CPU.TaskType.DiskSector);

            }

            // If seek is in progress, move closer to the desired cylinder...
            // TODO: move bitfields to enums / constants, this is getting silly.
            if ((_kStat & 0x0040) != 0)
            {
                _elapsedSeekTime++;
                if (_elapsedSeekTime > _seekClocks)
                {
                    _elapsedSectorTime -= _seekClocks;

                    if (_cylinder < _destCylinder)
                    {
                        _cylinder++;
                    }
                    else if (_cylinder > _destCylinder)
                    {
                        _cylinder--;
                    }

                    // Are we *there* yet?
                    if (_cylinder == _destCylinder)
                    {
                        // clear Seek bit
                        _kStat &= 0xffbf;
                    }
                }
            }

            //
            // Select data word based on elapsed time in this sector, if data transfer is not inhibited.
            // On a new word, wake up the disk word task if not inhibited
            // TODO: the exact mechanics of this are still kind of mysterious.
            // Things to examine the schematics / uCode for:
            //   - Use of WFFO bit -- is this related to the "sync word" that the docs mention?
            //   -   how does WFFO work -- I assume the "1 bit" mentioned in the docs indicates the MFM bit
            //       and indicates the start of the next data word (or is at least used to wait for the next
            //       data word)
            //   - How does the delaying work
            // The microcode word-copying code works basically as:
            //     On wakeup: 
            //              - read word, store into memory.
            //              - block task (remove wakeup)
            //              - task switch (let something else run)
            //              - repeat until all words read
            // that is, the microcode expects to be woken up on a per-word basis, and it only reads in one word
            // per wakeup.
            //


            if (!_wdInhib)
            {
                
                _elapsedSectorStateTime++;        

                switch (_sectorState)
                {
                    case SectorState.HeaderReadDelay:                        
                        if (_sectorWordIndex > 19)
                        {
                            _sectorState = SectorState.Header;
                            _sectorWordIndex = 0;
                            Console.WriteLine("Switching to HeaderPreamble state.");
                            _kData = 1;
                        }
                        else if (_elapsedSectorStateTime > _wordDuration)
                        {
                            _elapsedSectorStateTime -= _wordDuration;

                            _sectorWordIndex++;

                            _kData = 0xfefe;    // unused, just for debugging

                            _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                            Console.WriteLine("delay wakeup");

                        }
                        break;

                    case SectorState.HeaderPreamble:
                        if (_sectorWordIndex > 32)
                        {
                            _sectorState = SectorState.Header;
                            _sectorWordIndex = 0;
                            Console.WriteLine("Switching to Header state.");
                            _kData = 2;
                        }
                        else if (_elapsedSectorStateTime > _wordDuration)
                        {
                            _elapsedSectorStateTime -= _wordDuration;

                            _sectorWordIndex++;

                            _kData = 0xfeff;    // unused, just for debugging
                            _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                            Console.WriteLine("preamble wakeup");
                        }
                        break;

                    case SectorState.Header:
                        if (_sectorWordIndex > 2)   // two words + sync
                        {
                            //_elapsedSectorStateTime -= 2.0 * _wordDuration;
                            _sectorState = SectorState.HeaderInterrecord;
                            _sectorWordIndex = 0;
                            Console.WriteLine("Switching to HeaderGap state.");
                            _kData = 3;
                        }
                        else if (_elapsedSectorStateTime > _wordDuration)
                        {
                            _elapsedSectorStateTime -= _wordDuration;

                            // Put next word into KDATA if not inhibited from doing so.                        
                            if (!_xferOff)
                            {
                                _kData = 0xdead;    // placeholder                     
                                Console.WriteLine("  Header word {0} is {1}", _sectorWordIndex, OctalHelpers.ToOctal(_kData));
                            }
                            _sectorWordIndex++;

                            if (!_wdInhib)
                            {
                                Console.WriteLine("header wakeup");
                                _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                            }
                        }
                        break;

                    case SectorState.HeaderInterrecord:
                        if (_elapsedSectorStateTime > _interRecordDelay)
                        {
                            _elapsedSectorStateTime -= _interRecordDelay;
                            _sectorState = SectorState.Label;
                            Console.WriteLine("Switching to Label state.");
                            _kData = 4;
                        }
                        break;

                    case SectorState.Label:
                        if (_sectorWordIndex > 8)   // eight words + sync
                        {
                            //_elapsedSectorStateTime -= 8.0 * _wordDuration;
                            _sectorState = SectorState.LabelInterrecord;
                            _sectorWordIndex = 0;
                            Console.WriteLine("Switching to LabelGap state.");
                            _kData = 5;
                        }
                        else if (_elapsedSectorStateTime > _wordDuration)
                        {
                            _elapsedSectorStateTime -= _wordDuration;
                            // Put next word into KDATA if not inhibited from doing so.
                            if (!_xferOff)
                            {
                                _kData = 0xbeef;    // placeholder
                                Console.WriteLine("  Label word {0} is {1}", _sectorWordIndex, OctalHelpers.ToOctal(_kData));
                            }
                            _sectorWordIndex++;

                            if (!_wdInhib)
                            {
                                _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                            }
                        }
                        break;

                    case SectorState.LabelInterrecord:
                        if (_elapsedSectorStateTime > _interRecordDelay)
                        {
                            _elapsedSectorStateTime -= _interRecordDelay;
                            _sectorState = SectorState.Data;
                            Console.WriteLine("Switching to Data state.");
                            _kData = 6;
                        }
                        break;

                    case SectorState.Data:
                        if (_sectorWordIndex > 256)   // 256 words + sync
                        {
                            //_elapsedSectorStateTime -= 256.0 * _wordDuration;
                            _sectorState = SectorState.Postamble;
                            _sectorWordIndex = 0;
                            Console.WriteLine("Switching to Leadout state.");
                            _kData = 7;
                        }
                        else if (_elapsedSectorStateTime > _wordDuration)
                        {
                            _elapsedSectorStateTime -= _wordDuration;
                            // Put next word into KDATA if not inhibited from doing so.
                            if (!_xferOff)
                            {
                                _kData = 0xda1a;    // placeholder
                                Console.WriteLine("  Sector word {0} is {1}", _sectorWordIndex, OctalHelpers.ToOctal(_kData));
                            }
                            _sectorWordIndex++;

                            if (!_wdInhib)
                            {
                                _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                            }
                        }
                        break;

                    case SectorState.Postamble:
                        // Just stay here forever.  We will get reset at the start of the next sector.
                        break;

                }
            }

            //
            // Update the WDINIT signal; this is based on WDALLOW (!_wdInhib) which sets WDINIT (this is done
            // in KCOM way above).
            // WDINIT is reset when BLOCK (a BLOCK F1 is being executed) and WDTSKACT (the disk word task is running) are 1.
            //
            if (_system.CPU.CurrentTask.Priority == (int)CPU.TaskType.DiskWord &&
                _system.CPU.CurrentTask.BLOCK)
            {
                _wdInit = false;
            }            
        }

        public void ClearStatus()
        {
            // "...clears KSTAT[13]." (chksum error flag)
            _kStat &= 0xfffb;
        }

        public void IncrementRecord()
        {
            // "Advances the shift registers holding the KADR register so that they present the number and read/write/check status of the
            // next record to the hardware."
            // "RECORD" in this context indicates the sector field corresponding to the 2 bit "action" field in the KADR register 
            // (i.e. one of Header, Label, or Data.)
            // INCRECNO shifts the data over two bits to select from Header->Label->Data.
            _kAdr = (ushort)(_kAdr << 2);
            _recNo++;

            if (_recNo > 3)
            {
                // sanity check for now
                throw new InvalidOperationException("Unexpected INCRECORD past rec 3.");
            }
        }

        public void Strobe()
        {
            //
            // "Initiates a disk seek operation.  The KDATA register must have been loaded previously,
            // and the SENDADR bit of the KCOMM register previously set to 1."
            //            
            
            // sanity check: see if SENDADR bit is set, if not we'll signal an error (since I'm trusting that
            // the official Xerox uCode is doing the right thing, this will help ferret out emulation issues.
            // eventually this can be removed.)
            if (!_sendAdr)
            {
                throw new InvalidOperationException("STROBE while SENDADR bit of KCOM not 1.  Unexpected.");
            }
            
            _destCylinder = (_kData & 0x0ff8) >> 3;

            // set "seek fail" bit based on selected cylinder (if out of bounds) and do not
            // commence a seek if so.
            if (_destCylinder > 202)
            {
                _kStat |= 0x0080;
            }
            else
            {
                // Otherwise, start a seek.

                // Clear the fail bit.
                _kStat &= 0xff7f;

                // Set seek bit
                _kStat |= 0x0040;

                // And figure out how long this will take.
                _seekClocks = CalculateSeekTime();
                _elapsedSeekTime = 0.0;
            }
        }

        private double CalculateSeekTime()
        {
            // How many cylinders are we moving?
            int dt = Math.Abs(_destCylinder - _cylinder);

            //
            // From the Hardware Manual, pg 43:
            // "Seek time (approx.):  15 + 8.6 * sqrt(dt)  (msec)
            //
            double seekTimeMsec = 15.0 + 8.6 * Math.Sqrt(dt);

            return seekTimeMsec / AltoSystem.ClockInterval;
        }

        private ushort _kData;
        private ushort _kAdr;
        private ushort _kCom;
        private ushort _kStat;

        private int _recNo;
        private ushort[] _recMap =
        {
            0, 2, 3, 1
        };

        // KCOM bits
        private bool _xferOff;
        private bool _wdInhib;
        private bool _bClkSource;
        private bool _wffo;
        private bool _sendAdr;

        // Transfer bit
        private bool _dataXfer;

        // Current disk position
        private int _cylinder;
        private int _destCylinder;
        private int _head;
        private int _sector;

        // WDINIT signal
        private bool _wdInit;

        // Sector timing.  Based on table on pg. 43 of the Alto Hardware Manual
        private double _elapsedSectorTime;     // elapsed time in this sector (in clocks)
        private const double _sectorDuration = (40.0 / 12.0); // time in msec for one sector
        private const double _sectorClocks = _sectorDuration / 0.00017;     // number of clock cycles per sector time.

        // Sector data timing and associated state.  Timings based on educated guesses at the moment.
        private enum SectorState
        {
            HeaderReadDelay = 0,     // gap between sector mark and first Header word
            HeaderPreamble,
            Header,           // Header; two words
            HeaderInterrecord,// gap between end of Header and first Label word
            Label,            // Label; 8 words
            LabelInterrecord, // gap betweeen the end of Label and first Data word
            Data,             // Data; 256 words
            Postamble         // gap between the end of Data and the next sector mark
        }
        private SectorState _sectorState;
        private double _elapsedSectorStateTime;
        private int _sectorWordIndex;


        // From altoconsts23.mu:
        // $MFRRDL		$177757;	DISK HEADER READ DELAY IS 21 WORDS
        // $MFR0BL		$177744;	DISK HEADER PREAMBLE IS 34 WORDS
        // $MIRRDL		$177774;	DISK INTERRECORD READ DELAY IS 4 WORDS
        // $MIR0BL		$177775;	DISK INTERRECORD PREAMBLE IS 3 WORDS
        // $MRPAL		$177775;	DISK READ POSTAMBLE LENGTH IS 3 WORDS
        // $MWPAL		$177773;	DISK WRITE POSTAMBLE LENGTH IS 5 WORDS
        private const double _wordDuration = (_sectorClocks / (266.0 + 21 + 34 + (4 + 3) * 3));       // Based on : 266 words / sector, + X words for delay / preamble
        private const double _headerReadDelay = (_wordDuration * 21);
        private const double _headerPreamble = (_wordDuration * 34);
        private const double _interRecordDelay = (_wordDuration * 4);
        private const double _interRecordPreamble = (_wordDuration * 3);




        // Cylinder seek timing.  Again, see the manual.
        // Timing varies based on how many cylinders are being traveled during a seek; see
        // CalculateSeekTime() for more.
        private double _elapsedSeekTime;
        private double _seekClocks;


        private AltoSystem _system;
    }
}
