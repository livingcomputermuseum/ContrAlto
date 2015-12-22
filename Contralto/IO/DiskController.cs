using System;
using System.IO;
using Contralto.Logging;
using Contralto.CPU;

namespace Contralto.IO
{
    public class DiskController
    {
        public DiskController(AltoSystem system)
        {
            _system = system;

            // Load the drives
            _drives = new DiabloDrive[2];
            _drives[0] = new DiabloDrive(_system);
            _drives[1] = new DiabloDrive(_system);           

            Reset();
        }

        /// <summary>
        /// TODO: this is messy; the read and write sides of KDATA are distinct hardware.
        /// According to docs, on a Write, eventually it appears on the Read side during an actual write to the disk
        /// but not right away.  
        /// </summary>
        public ushort KDATA
        {
            get
            {
                _debugRead = false;
                return _kDataRead;
            }
            set
            {
                _kDataWrite = value;
                _kDataWriteLatch = true;
            }
        }

        public ushort KADR
        {
            get { return _kAdr; }
            set
            {
                _kAdr = value;
                _recNo = 0;
                _syncWordWritten = false;

                // "In addition, it causes the head address bit to be loaded from KDATA[13]."
                int newHead = (_kDataWrite & 0x4) >> 2;

                SelectedDrive.Head = newHead;

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
                    newHead,
                    (_kDataWrite & 0xf000) >> 12,
                    (_kDataWrite & 0x2) >> 1,
                    (_kDataWrite & 0x1));
            
                Log.Write(LogComponent.DiskController, "  -Selected disk is {0}", _disk);

                if ((_kDataWrite & 0x1) != 0)
                {
                    // Restore operation to cyl. 0:
                    InitSeek(0);
                }
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

                
                if (_sendAdr & (_kDataWrite & 0x2) != 0)
                {
                    // Select disk if _sendAdr is true
                    _disk = (_kAdr & 0x1);
                    _seeking = false;

                    /*
                    
                    // Clear the NOTREADY flag depending on whether the drive is loaded or not
                    if (_drives[_disk].IsLoaded)
                    {
                        _kStat &= (ushort)~NOTREADY;
                    }
                    else
                    {
                        _kStat |= NOTREADY;
                    } */
                }

            }
        }

        /// <summary>
        /// Used by the DiskTask code to check the WDINIT signal for dispatch.
        /// </summary>
        public bool WDINIT
        {
            get { return _wdInit; }
            set { _wdInit = value; }
        }

        public ushort KSTAT
        {
            get
            {
                // Bits 4-7 of KSTAT are always 1s (it's a shortcut allowing the disk microcode to write
                // "-1" to bits 4-7 of the disk status word at 522 without extra code.)
                return (ushort)(_kStat | (0x0f00));
            }
            set
            {
                _kStat = value;
            }
        }

        public ushort RECNO
        {
            get { return _recMap[_recNo]; }
        }

        public bool DataXfer
        {
            get { return _dataXfer; }
        }

        public int Cylinder
        {
            get { return SelectedDrive.Cylinder; }
        }

        public int SeekCylinder
        {
            get { return _destCylinder; }
        }

        public int Head
        {
            get { return SelectedDrive.Head; }
        }

        public int Sector
        {
            get { return SelectedDrive.Sector; }
        }

        public int Drive
        {
            get { return 0; }
        }

        public double ClocksUntilNextSector
        {
            get { return 0; }  // _sectorClocks - _elapsedSectorTime; }
        }

        public bool Ready
        {
            get
            {
                // This is the SRWREADY signal, generated by the drive itself.
                // This is true if the drive is:
                //   - powered on, loaded with a disk, spun up
                //   - not actively seeking                
                return _drives[0].IsLoaded;
            }
        }

        public bool FatalError
        {
            get
            {
                //
                // A fatal error is signaled when any of:
                //  - SECLATE
                //  - A seek error
                //  - Drive not ready
                // Is true.
                // (In reality the logic is a bit more complicated,
                // but this is sufficient.)
                //
                return (_kStat & SECLATE) != 0 ||
                       (_kStat & SEEKFAIL) != 0 ||
                       (_kStat & NOTREADY) != 0;
            }
        }

        public DiabloDrive[] Drives
        {
            get { return _drives; }
        }

        public void Reset()
        {
            ClearStatus();
            _recNo = 0;
            _sector = 0;
            _disk = 0;
            _kStat = 0;
            _kDataRead = 0;
            _kDataWrite = 0;
            _kDataWriteLatch = false;
            _sendAdr = false;
            _seeking = false;

            _wdInhib = true;
            _xferOff = true;

            _wdInit = false;

            _syncWordWritten = false;

            _diskBitCounterEnable = false;
            _sectorWordIndex = 0;

            // Reset drives
            _drives[0].Reset();
            _drives[1].Reset();            

            // Create events to be reused during execution

            // Schedule the first sector immediately.
            _sectorEvent = new Event(0, null, SectorCallback);
            _wordEvent = new Event(_wordDuration, null, WordCallback);
            _seclateEvent = new Event(_seclateDuration, null, SeclateCallback);
            _seekEvent = new Event(_seekDuration, null, SeekCallback);

            // And schedule the first sector pulse.
            _system.Scheduler.Schedule(_sectorEvent);
        }

        /// <summary>
        /// Allows the Disk Sector task to disable the SECLATE signal.
        /// </summary>
        public void DisableSeclate()
        {
            _seclateEnable = false;
        }

        private void SectorCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            //
            // Next sector; move to next sector and wake up Disk Sector task.
            //            
            _sector = (_sector + 1) % 12;

            _kStat = (ushort)((_kStat & 0x0fff) | (_sector << 12));

            // Reset internal state machine for sector data
            _sectorWordIndex = 0;
            _syncWordWritten = false;

            _kDataRead = 0;

            // Load new sector in
            SelectedDrive.Sector = _sector;

            // Only wake up if not actively seeking.
            if ((_kStat & STROBE) == 0)
            {
                Log.Write(LogType.Verbose, LogComponent.DiskController, "Waking up sector task for C/H/S {0}/{1}/{2}", SelectedDrive.Cylinder, SelectedDrive.Head, _sector);
                _system.CPU.WakeupTask(CPU.TaskType.DiskSector);

                // Reset SECLATE                
                _seclate = false;
                _seclateEnable = true;
                _kStat &= (ushort)~SECLATE;

                // Schedule a disk word wakeup to spin the disk
                _wordEvent.TimestampNsec = _wordDuration;
                _system.Scheduler.Schedule(_wordEvent);

                // Schedule SECLATE trigger
                _seclateEvent.TimestampNsec = _seclateDuration;
                _system.Scheduler.Schedule(_seclateEvent);
            }
            else
            {
                // Schedule next sector pulse
                _sectorEvent.TimestampNsec = _sectorDuration - skewNsec;
                _system.Scheduler.Schedule(_sectorEvent);
            }
        }

        private void WordCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            SpinDisk();

            // Schedule next word if this wasn't the last word this sector.
            if (_sectorWordIndex < _sectorWordCount)
            {
                _wordEvent.TimestampNsec = _wordDuration - skewNsec;
                _system.Scheduler.Schedule(_wordEvent);
            }
            else
            {
                // Schedule next sector pulse immediately
                _sectorEvent.TimestampNsec = skewNsec;
                _system.Scheduler.Schedule(_sectorEvent);
            }
        }

        private void SeclateCallback(ulong timeNsec, ulong skewNsec, object context)
        {            
            if (_seclateEnable)
            {
                _seclate = true;                
                _kStat |= SECLATE;       // TODO: move to constant field!
                Log.Write(LogComponent.DiskSectorTask, "SECLATE for sector {0}.", _sector);
            }
        }

        public void ClearStatus()
        {
            // "...clears KSTAT[13]." (chksum error flag)
            _kStat &= 0xff4b;
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

            _syncWordWritten = false;

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

            InitSeek((_kDataWrite & 0x0ff8) >> 3);
        }

        private void InitSeek(int destCylinder)
        {
            // set "seek fail" bit based on selected cylinder (if out of bounds) and do not
            // commence a seek if so.
            // TODO: handle Model-44 cylinder count (and packs, for that matter)
            if (destCylinder > 202)
            {
                _kStat |= SEEKFAIL;

                Log.Write(LogComponent.DiskController, "Seek failed, specified cylinder {0} is out of range.", destCylinder);
                _seeking = false;
            }
            else
            {
                // Otherwise, start a seek.
                _destCylinder = destCylinder;

                // Clear the fail bit.
                _kStat &= (ushort)~SEEKFAIL;

                // Set seek bit
                _kStat |= STROBE;
                _seeking = true;

                // And figure out how long this will take.
                _seekDuration = (ulong)(CalculateSeekTime() / (ulong)(Math.Abs(_destCylinder - SelectedDrive.Cylinder) + 1));

                _seekEvent.TimestampNsec = _seekDuration;
                _system.Scheduler.Schedule(_seekEvent);

                Log.Write(LogComponent.DiskController, "Seek to {0} from {1} commencing.  Will take {2} nsec.", _destCylinder, SelectedDrive.Cylinder, _seekDuration);
            }
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

            //
            // Pick out the word that just passed under the head.  This may not be
            // actual data (it could be the pre-header delay, inter-record gaps or sync words)
            // and we may not actually end up doing anything with it, but we may
            // need it to decide whether to do anything at all.
            //                                             
            DataCell diskWord = SelectedDrive.ReadWord(_sectorWordIndex);

            bool bWakeup = false;
            //
            // If the word task is enabled AND the write ("crystal") clock is enabled
            // then we will wake up the word task now.
            // 
            if (!_seclate && !_wdInhib && !_bClkSource)
            {
                bWakeup = true;
            }

            //
            // If the clock is enabled OR the WFFO bit is set (go ahead and run the bit clock)
            // and we weren't late reading this sector,  then we will wake up the word task 
            // and read in the data if transfers are not inhibited.  TODO: this should only happen on reads.
            //
            if (!_seclate && (_wffo || _diskBitCounterEnable))
            {
                if (!_xferOff)
                {
                    if (!IsWrite())
                    {
                        // Read operation:
                        // Debugging: on a read/check, if we are overwriting a word that was never read by the
                        // microcode via KDATA, log it.
                        if (_debugRead)
                        {
                            Log.Write(LogType.Warning, LogComponent.DiskController, "--- missed sector word {0}({1}) ---", _sectorWordIndex, _kDataRead);
                        }

                        Log.Write(LogType.Verbose, LogComponent.DiskWordTask, "Sector {0} Word {1} read into KDATA", _sector, Conversion.ToOctal(diskWord.Data));
                        _kDataRead = diskWord.Data;
                        _debugRead = diskWord.Type == CellType.Data;
                    }
                    else
                    {
                        // Write                        
                        Log.Write(LogType.Verbose, LogComponent.DiskController, "Sector {0} Word {1} (rec {2}) to be written with {3} from KDATA", _sector, _sectorWordIndex, _recNo, Conversion.ToOctal(_kDataWrite));

                        if (_kDataWriteLatch)
                        {
                            _kDataRead = _kDataWrite;
                            _kDataWriteLatch = false;
                        }

                        if (_syncWordWritten)
                        {
                            // Commit actual data to disk now that the sync word has been laid down
                            SelectedDrive.WriteWord(_sectorWordIndex, _kDataWrite);
                        }
                    }
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
            if (!IsWrite() && !_wffo && diskWord.Data == 1)
            {
                _diskBitCounterEnable = true;
            }
            else if (IsWrite() && _wffo && _kDataWrite == 1 && !_syncWordWritten)
            {
                Log.Write(LogType.Normal, LogComponent.DiskController, "Sector {0} Sync Word {1} (rec {2}) written.", _sector, _sectorWordIndex, _recNo);
                _syncWordWritten = true;

                // "Adjust" the write index to the start of the data area for the current record.
                // This is cheating.
                switch (_recNo)
                {
                    case 0:
                        _sectorWordIndex = _headerOffset;
                        break;

                    case 1:
                        _sectorWordIndex = _labelOffset;
                        break;

                    case 2:
                        _sectorWordIndex = _dataOffset;
                        break;
                }
            }

            if (bWakeup)
            {
                Log.Write(LogType.Verbose, LogComponent.DiskWordTask, "Word task awoken for word {0}.", _sectorWordIndex);
                _system.CPU.WakeupTask(TaskType.DiskWord);                
            }

            // Last, move to the next word.
            _sectorWordIndex++;

        }

        private bool IsWrite()
        {
            return ((_kAdr & 0x00c0) >> 6) == 2 || ((_kAdr & 0x00c0) >> 6) == 3;
        }

        private void SeekCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            if (SelectedDrive.Cylinder < _destCylinder)
            {
                SelectedDrive.Cylinder++;
            }
            else if (SelectedDrive.Cylinder > _destCylinder)
            {
                SelectedDrive.Cylinder--;
            }

            Log.Write(LogComponent.DiskController, "Seek progress: cylinder {0} reached.", SelectedDrive.Cylinder);

            // Are we *there* yet?
            if (SelectedDrive.Cylinder == _destCylinder)
            {
                // clear Seek bit
                _kStat &= (ushort)~STROBE;
                _seeking = false;

                Log.Write(LogComponent.DiskController, "Seek to {0} completed.", SelectedDrive.Cylinder);
            }
            else
            {
                // Nope.
                // Schedule next seek step.
                _seekEvent.TimestampNsec = _seekDuration - skewNsec;
                _system.Scheduler.Schedule(_seekEvent);
            }
        }

        private ulong CalculateSeekTime()
        {
            // How many cylinders are we moving?
            int dt = Math.Abs(_destCylinder - SelectedDrive.Cylinder);

            //
            // From the Hardware Manual, pg 43:
            // "Seek time (approx.):  15 + 8.6 * sqrt(dt)  (msec)
            //
            double seekTimeMsec = 15.0 + 8.6 * Math.Sqrt(dt);

            return (ulong)(seekTimeMsec * Conversion.MsecToNsec);
        }

        private DiabloDrive SelectedDrive
        {
            get { return _drives[_disk]; }
        }

        private ushort _kDataRead;
        private ushort _kDataWrite;
        private bool _kDataWriteLatch;
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

        // Current sector        
        private int _sector;

        //
        // Seek state
        //
        private int _destCylinder;
        private ulong _seekDuration;
        private Event _seekEvent;
        private bool _seeking;

        // Selected disk
        private int _disk;

        // bit clock flag
        private bool _diskBitCounterEnable;

        // WDINIT signal
        private bool _wdInit;

        private bool _syncWordWritten;

        // Sector timing.  Based on table on pg. 43 of the Alto Hardware Manual                                        

        // From altoconsts23.mu:  [all constants in octal, for reference]
        // $MFRRDL		$177757;	DISK HEADER READ DELAY IS 21 WORDS
        // $MFR0BL		$177744;	DISK HEADER PREAMBLE IS 34 WORDS                <<-- used for writing
        // $MIRRDL		$177774;	DISK INTERRECORD READ DELAY IS 4 WORDS
        // $MIR0BL		$177775;	DISK INTERRECORD PREAMBLE IS 3 WORDS            <<-- writing
        // $MRPAL		$177775;	DISK READ POSTAMBLE LENGTH IS 3 WORDS
        // $MWPAL		$177773;	DISK WRITE POSTAMBLE LENGTH IS 5 WORDS          <<-- writing, clearly.
        private static double _scale = 1.75;
        private static ulong _sectorDuration = (ulong)((40.0 / 12.0) * Conversion.MsecToNsec * _scale);      // time in nsec for one sector    
        private static int _sectorWordCount = 269 + 22 + 34;                                            // Based on : 269 data words (+ cksums) / sector, + X words for delay / preamble / sync
        private static ulong _wordDuration = (ulong)((_sectorDuration / (ulong)(_sectorWordCount)) * _scale);  // time in nsec for one word
        private int _sectorWordIndex;                                                               // current word being read

        private Event _sectorEvent;
        private Event _wordEvent;


        // offsets in words for start of data in sector
        private const int _headerOffset = 22;
        private const int _labelOffset = _headerOffset + 14;
        private const int _dataOffset = _labelOffset + 20;

        // SECLATE data.
        // 8.5uS for seclate delay (approx. 50 clocks)
        private static ulong _seclateDuration = (ulong)(85.0 * Conversion.UsecToNsec * _scale);
        private bool _seclateEnable;
        private bool _seclate;      
        private Event _seclateEvent;

        // Attached drives
        private DiabloDrive[] _drives;

        private AltoSystem _system;

        private bool _debugRead;

        // KSTAT bitfields        
        private readonly ushort SECLATE   = 0x10;
        private readonly ushort NOTREADY  = 0x20;
        private readonly ushort STROBE    = 0x40;
        private readonly ushort SEEKFAIL  = 0x80;
        
    }
}
