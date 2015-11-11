using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;
using System.IO;
using Contralto.Logging;

namespace Contralto.IO
{
    public class DiskController : IClockable
    {
        public DiskController(AltoSystem system)
        {
            _system = system;
            Reset();

            // Load the pack
            _pack = new DiabloPack(DiabloDiskType.Diablo31);

            // TODO: this does not belong here.
            FileStream fs = new FileStream("Disk\\tdisk4.dsk", FileMode.Open, FileAccess.Read);

            _pack.Load(fs);

            fs.Close();                                   
        }        

        /// <summary>
        /// TODO: this is messy; the read and write sides of KDATA are distinct hardware.
        /// According to docs, on a Write, eventually it appears on the Read side during an actual write to the disk
        /// but not right away.  For now, this never happens (since we don't yet support writing).
        /// </summary>
        public ushort KDATA
        {
            get
            {                
                return _kDataRead;
            }
            set
            {
                _kDataWrite = value;
            }
        }

        public ushort KADR
        {
            get { return _kAdr; }
            set
            {
                _kAdr = value;
                _recNo = 0;

                // "In addition, it causes the head address bit to be loaded from KDATA[13]."
                int newHead = (_kDataWrite & 0x4) >> 2;

                Log.Write(LogComponent.DiskController, "At sector time {0}:", _elapsedSectorTime);
                if (newHead != _head)
                {
                    // If we switch heads, we need to reload the sector
                    _head = newHead;
                    LoadSector();
                }

                // "0 normally, 1 if the command is to terminate immediately after the correct cylinder
                // position is reached (before any data is transferred)."
                _dataXfer = (_kAdr & 0x2) != 0x2;

                Log.Write(LogComponent.DiskController, "KADR set to {0} (Header {1}, Label {2}, Data {3}, Xfer {4}, Drive {5})",
                    Conversion.ToOctal(_kAdr),
                    Conversion.ToOctal((_kAdr & 0xc0) >> 6),
                    Conversion.ToOctal((_kAdr & 0x30) >> 4),
                    Conversion.ToOctal((_kAdr & 0xc) >> 2),
                    _dataXfer,
                    _kAdr & 0x1);

                Log.Write(LogComponent.DiskController, "  -Disk Address ({0}) is C/H/S {1}/{2}/{3}, Drive {4} Restore {5}",
                    Conversion.ToOctal(_kDataWrite),
                    (_kDataWrite & 0x0ff8) >> 3,
                    _head,
                    (_kDataWrite & 0xf000) >> 12,
                    (_kDataWrite & 0x2) >> 1,
                    (_kDataWrite & 0x1));

                Log.Write(LogComponent.DiskController, "  -Selected disk is {0}", ((_kDataWrite & 0x2) >> 1) ^ (_kAdr & 0x1));

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

                _diskBitCounterEnable = _wffo;

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
                // Bits 4-7 of KSTAT are always 1s.
                return (ushort)(_kStat | (0x0f00));
            }
            set
            {
                _kStat = value;             
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
            get { return _sectorWordTime < 10; }
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

        public bool Ready
        {
            get
            {
                // TODO: verify if this is correct.
                // Not ready if we're in the middle of a seek.
                return (_kStat & 0x0040) == 0;
            }
        }

        public void Reset()
        {
            ClearStatus();
            _recNo = 0;
            _elapsedSectorTime = 0.0;
            _cylinder = _destCylinder = 0;
            _sector = 0;
            _head = 0;
            _kStat = 0;
            _kDataRead = 0;
            _kDataWrite = 0;
            _sendAdr = false;

            _wdInhib = true;
            _xferOff = true;

            _wdInit = false;

            _diskBitCounterEnable = false;
            _sectorWordIndex = 0;
            _sectorWordTime = 0;

            InitSector();

            // Wakeup the sector task first thing
            _system.CPU.WakeupTask(CPU.TaskType.DiskSector);
        }

        public void Clock()
        {
            _elapsedSectorTime++;            

            // TODO: only signal sector changes if disk is loaded, etc.
            if (_elapsedSectorTime > _sectorClocks )
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
                _sectorWordIndex = 0;
                _sectorWordTime = 0.0;

                _kDataRead = 0;

                // Load new sector in
                LoadSector();

                // Only wake up if not actively seeking.
                if ((_kStat & 0x0040) == 0)
                {
                    Log.Write(LogType.Verbose, LogComponent.DiskController, "Waking up sector task for C/H/S {0}/{1}/{2}", _cylinder, _head, _sector);
                    _system.CPU.WakeupTask(CPU.TaskType.DiskSector);
                }
            }

            // If seek is in progress, move closer to the desired cylinder...
            // TODO: move bitfields to enums / constants, this is getting silly.
            if ((_kStat & 0x0040) != 0)
            {
                _elapsedSeekTime++;
                if (_elapsedSeekTime > _seekClocks)
                {
                    _elapsedSeekTime -= _seekClocks;

                    if (_cylinder < _destCylinder)
                    {
                        _cylinder++;
                    }
                    else if (_cylinder > _destCylinder)
                    {
                        _cylinder--;
                    }

                    Log.Write(LogComponent.DiskController, "Seek progress: cylinder {0} reached.", _cylinder);

                    // Are we *there* yet?
                    if (_cylinder == _destCylinder)
                    {
                        // clear Seek bit
                        _kStat &= 0xffbf;

                        Log.Write(LogComponent.DiskController, "Seek to {0} completed.", _cylinder);
                    }
                }
            }

            //
            // Spin the disk platter and read in words as applicable.
            //
            SpinDisk();

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

            Log.Write(LogComponent.DiskController, "STROBE: Seek initialized.");
            
            _destCylinder = (_kDataWrite & 0x0ff8) >> 3;

            // set "seek fail" bit based on selected cylinder (if out of bounds) and do not
            // commence a seek if so.
            if (_destCylinder > 202)
            {
                _kStat |= 0x0080;

                Log.Write(LogComponent.DiskController, "Seek failed, specified cylinder {0} is out of range.", _destCylinder);
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
                
                Log.Write(LogComponent.DiskController, "Seek to {0} from {1} commencing.  Will take {2} clocks.", _destCylinder, _cylinder, _seekClocks);
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

            return (seekTimeMsec / AltoSystem.ClockInterval) / 100;     // div 100 to make things faster for now
        }

        /// <summary>
        /// "Rotates" the emulated disk platter one clock's worth.
        /// </summary>
        private void SpinDisk()
        {
            //
            // Roughly:  If transfer is enabled:
            //   Select data word based on elapsed time in this sector.
            //   On a new word, wake up the disk word task if not inhibited.
            //
            // If transfer is not enabled BUT the disk word task is enabled,
            // we will still wake up the disk word task if the appropriate clock
            // source is selected.
            //
            // We simulate the movement of a sector under the heads by dividing
            // the sector into word-sized timeslices.  Not all of these slices
            // will actually contain valid data -- some are empty, used by the microcode
            // for lead-in or inter-record delays, but the slices are still used to
            // keep things in line time-wise; the real hardware uses a crystal-controlled clock
            // to generate these slices during these periods (and the clock comes from the
            // disk itself when actual data is present).  For our purposes, the two clocks
            // are one and the same.
            //            

            // Move the disk forward one clock
            _sectorWordTime++;

            // If we have reached a new word timeslice, do something appropriate.
            if (_sectorWordTime > _wordDuration)
            {
                // Save the fractional portion of the timeslice for the next slice
                _sectorWordTime -= _wordDuration;                

                //
                // Pick out the word that just passed under the head.  This may not be
                // actual data (it could be the pre-header delay, inter-record gaps or sync words)
                // and we may not actually end up doing anything with it, but we may
                // need it to decide whether to do anything at all.
                //                
                ushort diskWord = _sectorData[_sectorWordIndex].Data;

                bool bWakeup = false;
                //
                // If the word task is enabled AND the write ("crystal") clock is enabled
                // then we will wake up the word task now.
                // 
                if (!_wdInhib && !_bClkSource)
                {                    
                    bWakeup = true;                    
                }                

                //
                // If the clock is enabled OR the WFFO bit is set (go ahead and run the bit clock)
                // then we will wake up the word task and read in the data if transfers are not
                // inhibited.  TODO: this should only happen on reads.
                //
                if (_wffo || _diskBitCounterEnable)
                {
                    if (!_xferOff)
                    {
                        Log.Write(LogType.Verbose, LogComponent.DiskWordTask, "Sector {0} Word {1} read into KDATA", _sector, Conversion.ToOctal(diskWord));  
                        _kDataRead = diskWord;
                    }

                    if (!_wdInhib)
                    {
                        bWakeup = true;
                    }
                }

                //
                // If the WFFO bit is cleared (wait for the sync word to be read) 
                // then we check the word for a "1" (the sync word) to enable
                // the clock.  This occurs late in the cycle so that the NEXT word
                // (not the sync word) is actually read.  TODO: this should only happen on reads.
                //
                if (!_wffo && diskWord == 1)
                {                    
                    _diskBitCounterEnable = true;
                }

                if (bWakeup)
                {
                    Log.Write(LogType.Verbose, LogComponent.DiskWordTask, "Word task awoken for word {0}.", _sectorWordIndex);
                    _system.CPU.WakeupTask(CPU.TaskType.DiskWord);
                }

                // Last, move to the next word.
                _sectorWordIndex++;
            }
        }

        private void LoadSector()
        {
            //
            // Pull data off disk and pack it into our faked-up sector.
            // Note that this data is packed in in REVERSE ORDER because that's
            // how it gets written out and it's how the Alto expects it to be read back in.
            //
            DiabloDiskSector sector = _pack.GetSector(_cylinder, _head, _sector);

            // debugging
            /*
            if (_cylinder >= 32)
            {
                Console.WriteLine("loading in C/H/S {0}/{1}/{2}", _cylinder, _head, _sector);
                for(int i=0;i<sector.Header.Length;i++)
                {
                    Console.WriteLine("Header {0}, memory {1} (alt {2})",
                        Conversion.ToOctal(sector.Header[i]),
                        Conversion.ToOctal(_system.MemoryBus.DebugReadWord((ushort)(0x30 + i))),
                        Conversion.ToOctal(_system.MemoryBus.DebugReadWord((ushort)(0x1e + i))));
                }

                for (int i = 0; i < sector.Label.Length; i++)
                {
                    Console.WriteLine("Label {0}, memory {1} (alt {2})",
                        Conversion.ToOctal(sector.Label[i]),
                        Conversion.ToOctal(_system.MemoryBus.DebugReadWord((ushort)(0x32 + i))),
                        Conversion.ToOctal(_system.MemoryBus.DebugReadWord((ushort)(0x20 + i))));
                }
            } */

            // Header (2 words data, 1 word cksum)
            for (int i = _headerOffset + 1, j = 1; i < _headerOffset + 3; i++, j--)
            {
                // actual data to be loaded from disk / cksum calculated
                _sectorData[i] = new DataCell(sector.Header[j], CellType.Data);
            }

            ushort checksum = CalculateChecksum(_sectorData, _headerOffset + 1, 2);
            _sectorData[_headerOffset + 3].Data = checksum;
            Log.Write(LogType.Verbose, LogComponent.DiskController, "Header checksum for C/H/S {0}/{1}/{2} is {3}", _cylinder, _head, _sector, Conversion.ToOctal(checksum));
            
            // Label (8 words data, 1 word cksum)
            for (int i = _labelOffset + 1, j = 7; i < _labelOffset + 9; i++, j--)
            {
                // actual data to be loaded from disk / cksum calculated
                _sectorData[i] = new DataCell(sector.Label[j], CellType.Data);
            }

            checksum = CalculateChecksum(_sectorData, _labelOffset + 1, 8);
            _sectorData[_labelOffset + 9].Data = checksum;
            Log.Write(LogType.Verbose, LogComponent.DiskController, "Label checksum for C/H/S {0}/{1}/{2} is {3}", _cylinder, _head, _sector, Conversion.ToOctal(checksum));

            // sector data (256 words data, 1 word cksum)
            for (int i = _dataOffset + 1, j = 255; i < _dataOffset + 257; i++, j--)
            {
                // actual data to be loaded from disk / cksum calculated
                _sectorData[i] = new DataCell(sector.Data[j], CellType.Data);
            }

            checksum = CalculateChecksum(_sectorData, _dataOffset + 1, 256);
            _sectorData[_dataOffset + 257].Data = checksum;
            Log.Write(LogType.Verbose, LogComponent.DiskController, "Data checksum for C/H/S {0}/{1}/{2} is {3}", _cylinder, _head, _sector, Conversion.ToOctal(checksum));

        }

        private void InitSector()
        {
            // Fill in sector with default data (basically, fill in non-data areas).            

            //
            // header delay, 22 words
            for (int i=0; i < _headerOffset; i++)
            {                
                _sectorData[i] = new DataCell(0, CellType.Gap);
            }

            _sectorData[_headerOffset] = new DataCell(1, CellType.Sync);
            // inter-reccord delay between header & label (10 words)
            for (int i = _headerOffset + 4; i < _labelOffset; i++)
            {
                _sectorData[i] = new DataCell(0, CellType.Gap);
            }

            _sectorData[_labelOffset] = new DataCell(1, CellType.Sync);
            // inter-reccord delay between label & data (10 words)
            for (int i = _labelOffset + 10; i < _dataOffset; i++)
            {
                _sectorData[i] = new DataCell(0, CellType.Gap);
            }

            _sectorData[_dataOffset] = new DataCell(1, CellType.Sync);
            // read-postamble
            for (int i = _dataOffset + 257; i < _sectorWords;i++)
            {
                _sectorData[i] = new DataCell(0, CellType.Gap);
            }            
        }

        private ushort CalculateChecksum(DataCell[] sectorData, int offset, int length)
        {
            //
            // From the uCode, the Alto's checksum algorithm is:
            // 1. Load checksum with constant value of 521B (0x151)
            // 2. For each word in the record, cksum <- word XOR cksum
            // 3. Profit
            //
            ushort checksum = 0x151;

            for(int i = offset; i < offset + length;i++)
            {
                // Sanity check that we're checksumming actual data
                if (sectorData[i].Type != CellType.Data)
                {
                    throw new InvalidOperationException("Attempt to checksum non-data area of sector.");
                }

                checksum = (ushort)(checksum ^ sectorData[i].Data);
            }

            return checksum;
        }

        private ushort _kDataRead;
        private ushort _kDataWrite;
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

        // bit clock flag
        private bool _diskBitCounterEnable;

        // WDINIT signal
        private bool _wdInit;

        // Sector timing.  Based on table on pg. 43 of the Alto Hardware Manual
        private double _elapsedSectorTime;     // elapsed time in this sector (in clocks)
        private const double _sectorDuration = (40.0 / 12.0); // time in msec for one sector
        private const double _sectorClocks = _sectorDuration / (0.00017);     // number of clock cycles per sector time.

       
        private int _sectorWordIndex;
        private double _sectorWordTime;


        // From altoconsts23.mu:  [all constants in octal, for reference]
        // $MFRRDL		$177757;	DISK HEADER READ DELAY IS 21 WORDS
        // $MFR0BL		$177744;	DISK HEADER PREAMBLE IS 34 WORDS                <<-- used for writing
        // $MIRRDL		$177774;	DISK INTERRECORD READ DELAY IS 4 WORDS
        // $MIR0BL		$177775;	DISK INTERRECORD PREAMBLE IS 3 WORDS            <<-- writing
        // $MRPAL		$177775;	DISK READ POSTAMBLE LENGTH IS 3 WORDS
        // $MWPAL		$177773;	DISK WRITE POSTAMBLE LENGTH IS 5 WORDS          <<-- writing, clearly.
        private const int _sectorWords = 269 + 22 + 34;                          // Based on : 269 data words (+ cksums) / sector, + X words for delay / preamble / sync
        private const double _wordDuration = (_sectorClocks / (double)_sectorWords);       
        private const double _headerReadDelay = 17;        
        private const double _interRecordDelay = 4;

        // offsets in words for start of data in sector
        private const int _headerOffset = 22;
        private const int _labelOffset = _headerOffset + 14;
        private const int _dataOffset = _labelOffset + 20;

        // The data for the current sector
        private enum CellType
        {
            Data,
            Gap,
            Sync,
        }

        private struct DataCell
        {
            public DataCell(ushort data, CellType type)
            {
                Data = data;
                Type = type;
            }

            public ushort Data;
            public CellType Type;

            public override string ToString()
            {
                return String.Format("{0} {1}", Data, Type);
            }
        }

        private DataCell[] _sectorData = new DataCell[_sectorWords];


        // Cylinder seek timing.  Again, see the manual.
        // Timing varies based on how many cylinders are being traveled during a seek; see
        // CalculateSeekTime() for more.
        private double _elapsedSeekTime;
        private double _seekClocks;

        // The pack loaded into the drive
        DiabloPack _pack;

        private AltoSystem _system;
    }
}
