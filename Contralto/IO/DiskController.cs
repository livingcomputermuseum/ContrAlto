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

using System;
using System.IO;
using Contralto.Logging;
using Contralto.CPU;

namespace Contralto.IO
{
    public enum DiskActivityType
    {
        Idle,
        Read,
        Write,
        Seek,
    }

    public delegate void DiskActivity(DiskActivityType activity);

    /// <summary>
    /// DiskController provides an implementation for the logic in the standard Alto disk controller,
    /// which talks to a Diablo model 31 or 44 removable pack drive.
    /// </summary>
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

                //
                // Select disk from bit 14 of KDATA.
                // The HW reference claims that the drive is selected by bit 14 of KDATA XOR'd with bit 15
                // of KADR but I can find no evidence in the schematics that this is actually so. 
                // Page 18 of the controller schematic ("DISK ADDRESSING") shows that the current DATA(14) (KDATA bit 14) 
                // value is gated into the DISK select lines (running to the drive) whenever a KADR<- F1 is executed.
                // It is possible that the HW ref is telling the truth but the XORing is done by the Sector Task uCode
                // and not the hardware, but where this is actually occurring is not obvious.
                // At any rate: The below behavior appears to work correctly, so I'm sticking with it.
                //
                _disk = ((_kDataWrite & 0x2) >> 1);

                Log.Write(LogComponent.DiskController, "KADR set to {0} (Header {1}, Label {2}, Data {3}, Xfer {4}, Drive {5})",
                    Conversion.ToOctal(_kAdr),
                    Conversion.ToOctal((_kAdr & 0xc0) >> 6),
                    Conversion.ToOctal((_kAdr & 0x30) >> 4),
                    Conversion.ToOctal((_kAdr & 0xc) >> 2),
                    _dataXfer,
                    _disk);

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
                    _restore = true;
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
                    _seeking = false;
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

        public bool Ready
        {
            get
            {
                // This is the SRWREADY signal, generated by the drive itself.
                // This is true if the drive is:
                //   - powered on, loaded with a disk, spun up
                //   - not actively seeking                
                return _drives[_disk].IsLoaded && !_seeking;
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
                       (_kStat & NOTREADY) != 0 ||
                       (!Ready);
            }
        }

        public DiabloDrive[] Drives
        {
            get { return _drives; }
        }     
        
        /// <summary>
        /// Exposes the last activity the disk controller undertook.
        /// This is used exclusively so the UI can show a tiny disk
        /// access icon, it is not necessary to the functioning of the
        /// emulation.
        /// </summary>
        public DiskActivityType LastDiskActivity
        {
            get { return _lastDiskActivity; }
        }

        public void CommitDisk(int driveId)
        {           
            DiabloDrive drive = _drives[driveId];
            if (drive.IsLoaded)
            {
                drive.Pack.Save();
            }
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
            _restore = false;

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

        /// <summary>
        /// Called back 12 times per rotation of the disk to kick off a new disk sector.
        /// </summary>
        /// <param name="timeNsec"></param>
        /// <param name="skewNsec"></param>
        /// <param name="context"></param>
        private void SectorCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            //
            // Next sector; move to next sector and wake up Disk Sector task.
            //            
            _sector = (_sector + 1) % 12;            

            _kStat = (ushort)((_kStat & 0x0fff) | (_sector << 12));

            // Clear the NOTREADY flag depending on whether the selected drive is loaded or not
            if (_drives[_disk].IsLoaded)
            {
                _kStat &= (ushort)~NOTREADY;
            }
            else
            {
                _kStat |= NOTREADY;
            }

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
                Log.Write(LogType.Verbose, LogComponent.DiskController, "KADR is {0}", Conversion.ToOctal(_kAdr));
                Log.Write(LogType.Verbose, LogComponent.DiskController, "KDATA is {0}", Conversion.ToOctal(_kDataWrite));                
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

                _lastDiskActivity = DiskActivityType.Idle;
            }
            else
            {
                // Schedule next sector pulse
                _sectorEvent.TimestampNsec = _sectorDuration - skewNsec;
                _system.Scheduler.Schedule(_sectorEvent);
            }
        }

        /// <summary>
        /// Called back for every word time in this sector.
        /// </summary>
        /// <param name="timeNsec"></param>
        /// <param name="skewNsec"></param>
        /// <param name="context"></param>
        private void WordCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            SpinDisk();

            // Schedule next word if this wasn't the last word this sector.
            if (_sectorWordIndex < DiabloDrive.SectorWordCount)
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

        /// <summary>
        /// Called back if the sector task doesn't start in time, providing SECLATE
        /// semantics.
        /// </summary>
        /// <param name="timeNsec"></param>
        /// <param name="skewNsec"></param>
        /// <param name="context"></param>
        private void SeclateCallback(ulong timeNsec, ulong skewNsec, object context)
        {            
            if (_seclateEnable)
            {
                _seclate = true;                
                _kStat |= SECLATE;                
                Log.Write(LogComponent.DiskSectorTask, "SECLATE for sector {0}.", _sector);
            }
        }

        public void ClearStatus()
        {
            // "Causes all error latches in disk controller hardware to reset, clears clears KSTAT[13]." (chksum error flag)
            _kStat &= 0xff4b;
            _seclate = false;
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

        /// <summary>
        /// Starts a seek operation.
        /// </summary>
        public void Strobe()
        {
            //
            // "Initiates a disk seek [or restore] operation.  The KDATA register must have been loaded previously,
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

            if (_restore)
            {
                InitSeek(0);
            }
            else
            {
                InitSeek((_kDataWrite & 0x0ff8) >> 3);
            }
        }

        private void InitSeek(int destCylinder)
        {
            //
            // Set "seek fail" bit based on selected cylinder (if out of bounds) and do not
            // commence a seek if so.            
            if (!SelectedDrive.IsLoaded || destCylinder > SelectedDrive.Pack.Geometry.Cylinders - 1)
            {
                _kStat |= SEEKFAIL;

                Log.Write(LogComponent.DiskController, "Seek failed, specified cylinder {0} is out of range.", destCylinder);
                _seeking = false;
                _lastDiskActivity = DiskActivityType.Idle;
            }
            else if (destCylinder != SelectedDrive.Cylinder)
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
                _lastDiskActivity = DiskActivityType.Seek;
            }
            else
            {
                // Clear the fail bit.
                _kStat &= (ushort)~SEEKFAIL;
                Log.Write(LogComponent.DiskController, "Seek is a no op ({0} to {1}).", destCylinder, SelectedDrive.Cylinder);
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
            // drive itself when actual data is present).  For our purposes, the two clocks
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
            // and read in the data if transfers are not inhibited.
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

                        _lastDiskActivity = DiskActivityType.Read;
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
                            _lastDiskActivity = DiskActivityType.Write;
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
            // (not the sync word) is actually read.
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
                        _sectorWordIndex = DiabloDrive.HeaderOffset;
                        break;

                    case 1:
                        _sectorWordIndex = DiabloDrive.LabelOffset;
                        break;

                    case 2:
                        _sectorWordIndex = DiabloDrive.DataOffset;
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
                _restore = false;

                Log.Write(LogComponent.DiskController, "Seek to {0} completed.", SelectedDrive.Cylinder);
            }
            else
            {
                // Nope.
                // Schedule next seek step.
                _seekEvent.TimestampNsec = _seekDuration - skewNsec;
                _system.Scheduler.Schedule(_seekEvent);

                _lastDiskActivity = DiskActivityType.Seek;
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
            //double seekTimeMsec = 15.0 + 8.6 * Math.Sqrt(dt);
            double seekTimeMsec = 1.0;      // why not just have this be fast for now.

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
        private bool _restore;

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
        private static double _scale = 1.0;
        private static ulong _sectorDuration = (ulong)((40.0 / 12.0) * Conversion.MsecToNsec * _scale);      // time in nsec for one sector            
        private static ulong _wordDuration = (ulong)((_sectorDuration / (ulong)(DiabloDrive.SectorWordCount)) * _scale);  // time in nsec for one word
        private int _sectorWordIndex;                                                               // current word being read

        private Event _sectorEvent;
        private Event _wordEvent;      

        //
        // SECLATE data.
        // 86uS for SECLATE delay (approx. 505 clocks)
        // This is based on the R/C network connected to the 74123 (monostable vibrator) at 31 on the disk control board.
        // R = 30K, C = .01uF (10000pF).  T(nsec) = .28 * R * C * (1 + (0.7/R)) => .28 * 30 * 10000 * (1 + (0.7/30)) = 85959.9999nsec; 86usec.
        //
        private static ulong _seclateDuration = (ulong)(86.0 * Conversion.UsecToNsec * _scale);
        private bool _seclateEnable;
        private bool _seclate;      
        private Event _seclateEvent;

        // Attached drives
        private DiabloDrive[] _drives;

        private AltoSystem _system;

        private bool _debugRead;

        private DiskActivityType _lastDiskActivity;

        //
        // KSTAT bitfields.
        // Note that in reality the SECLATE status bit (bit 11) is a bit more nuanced; it's actually the OR of two signals:
        // 1) SECLATE while the Sector Task is enabled (meaning that the sector task missed the beginning of a sector and that's bad).
        //    This is emulated.
        // 2) CARRY while the Word Task is enabled.  CARRY in this case is the carry out from the disk word shift register, signaling
        //    a completed word.  If the Word Task is still running while a word is completed, this indicates that the word task missed that
        //    word and that's a fault.  This is not currently emulated.
        //
        public static readonly ushort SECLATE   = 0x10;     // status bit 11
        public static readonly ushort NOTREADY  = 0x20;     // 10
        public static readonly ushort STROBE    = 0x40;     // 9
        public static readonly ushort SEEKFAIL  = 0x80;     // 8
        
    }
}
