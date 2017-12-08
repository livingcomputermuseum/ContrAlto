using Contralto.CPU;
using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.IO
{
    /// <summary>
    /// Implements the Alto's Trident disk controller ("Tricon").
    /// This can talk to up to 8 (technically 16, but the software doesn't
    /// appear to support more than 8) T-80 (80mb) or T-300 (300mb) drives.
    /// 
    /// TODO: This implementation is preliminary and still experimental.
    ///       There are a few major issues:
    ///       - The state machine for the Output FIFO is a tangled mess.
    ///       - TFU refuses to talk to more than drive 0.  Unsure if this is
    ///         a bug in the Trident emulation, a bug elsewhere in Contralto
    ///         or user error.  TriEx seems to be fine.
    ///       - TriEx gets many errors of status "00000" (meaning that TriEx
    ///         didn't get a response from the controller when it expected to.)
    ///         TFU works fine and can certify, erase, and exercise packs all day.
    ///         The TriEx issue seems to be related in some way to the timing of
    ///         the output FIFO.  Probably a subtle issue with microcode wakeups.
    ///         Untangle the output FIFO state machine and revisit this.
    ///       - There is at this time no way to toggle the Read Only switch
    ///         on the emulated drive.  (All drives are read/write).
    ///       - The Trident sector wakeup signal needs to go away when the TriCon
    ///         is put to sleep (for performance reasons, no sense keeping the
    ///         scheduler busy for no reason.)
    /// </summary>
    public class TridentController
    {
        public TridentController(AltoSystem system)
        {
            _system = system;

            //
            // We initialize 16 drives even though the
            // controller only technically supports 8.
            // TODO: detail
            //
            _drives = new TridentDrive[16];

            for(int i=0;i<_drives.Length;i++)
            {
                _drives[i] = new TridentDrive(system);
            }

            Reset();
        }

        public void Reset()
        {
            _runEnable = false;

            _seekIncomplete = false;
            _headOverflow = false;
            _deviceCheck = false;
            _sectorOverflow = false;
            _outputLate = false;
            _inputLate = false;
            _compareError = false;
            _readOnly = false;
            _offset = false;

            _selectedDrive = 0;

            _sector = 0;
            _checkDone = false;
            _waitForEmpty = false;
            _pauseOutputProcessing = false;
            _commandState = CommandState.Command;
            _readState = ReadState.Idle;
            _writeState = WriteState.Idle;

            _outputFifo = new Queue<int>();
            _inputFifo = new Queue<ushort>();

            _sectorEvent = new Event(0, null, SectorCallback);
            _outputFifoEvent = new Event(0, null, OutputFifoCallback);
            _readWordEvent = new Event(0, null, ReadWordCallback);

            // And schedule the first sector pulse.
            _system.Scheduler.Schedule(_sectorEvent);
        }

        /// <summary>
        /// "RESET INPUT FIFO AND CLEAR ERRORS"
        /// </summary>
        public void ControllerReset()
        {
            _seekIncomplete = false;
            _headOverflow = false;
            _deviceCheck = false;
            _sectorOverflow = false;
            _outputLate = false;
            _inputLate = false;
            _compareError = false;
            _readOnly = false;
            _offset = false;

            _commandState = CommandState.Command;
            _writeState = WriteState.Idle;
            _readState = ReadState.Idle;

            _waitForEmpty = false;

            ClearInputFIFO();
        }

        public void CommitDisk(int drive)
        {
            TridentDrive d = _drives[drive];
            if (d.IsLoaded)
            {
                d.Pack.Save();
            }
        }

        public TridentDrive[] Drives
        {
            get { return _drives; }
        }

        public void STARTF(ushort value)
        {
            //
            // "An SIO with bit 10 set will cause Run-Enable to be set.
            //  An SIO with bit 11 set to one will cause Run-Enable to be
            //  reset."
            //
            if ((value & 0x10) != 0)
            {
                _runEnable = false;
                _system.CPU.BlockTask(TaskType.TridentInput);
                _system.CPU.BlockTask(TaskType.TridentOutput);
            }
            
            if ((value & 0x20) != 0)
            {
                _runEnable = true;

                //
                // "Issuing an SIO with bit 10 set will wake up the microcode
                //  once and thus may report status in the absence of sector
                //  pulses from the disk."
                // So do that now.
                // Based on a reading of the microcode and schematics it looks like only the
                // Write (Output) task is woken up here.
                //

                //
                // Clear error flags.
                //
                _inputLate = false;
                _outputLate = false;
                _sectorOverflow = false;

                //
                // Clear the FIFOs
                //
                ClearOutputFIFO();
                ClearInputFIFO();
                
                _system.CPU.WakeupTask(TaskType.TridentOutput);
            }

            Log.Write(LogComponent.TridentController, "Trident STARTF {0}", Conversion.ToOctal(value));
        }

        public void Stop()
        {
            _runEnable = false;
            _system.CPU.BlockTask(TaskType.TridentInput);
            _system.CPU.BlockTask(TaskType.TridentOutput);
        }

        public ushort KDTA
        {
            get
            {
                Log.Write(LogComponent.TridentController, "Trident KDTA read");
                return DequeueInputFIFO();
            }

            set
            {
                Log.Write(LogComponent.TridentController, "Trident KDTA queued {0}", Conversion.ToOctal(value));
                WriteOutputFifo(value);
            }
        }

        public ushort STATUS
        {
            get
            {
                //
                // The status bits from the controller are:
                // 0 - Seek Incomplete  : drive was unable to correctly position the heads, a Rezero
                //                       must be issued to clear this error.
                // 1 - Head Overflow    : head address given to the drive is invalid.
                // 2 - Device check     : One of the following errors occurred
                //     a) Head select or Cylinder select or Write commands and disk not ready
                //     b) An illegal cylinder address.
                //     c) Offset active and cylinder select command.
                //     d) Read-Only and write
                //     e) Certain errors during writing -- multiple heads selected, various read errors)
                // 3 - Not selected     : The selected drive is off-line or not powered up.
                // 4 - Not Online       : The drive is in test mode or the heads are not loaded.
                // 5 - Not Ready        : There is a cylinder seek in progress or the heads are not loaded
                // 6 - Sector Overflow  : The controller detected a write command was active when the next
                //                        sector pulse occurred.
                // 7 - Output Late      : The 16 word output buffer became empty while a read or write command
                //                        was in progress.
                // 8 - Input Late       : The 16 word input buffer became full.
                // 9 - Compare Error    : The data read during a "Read and Compare" operation did not match
                //                        the data read off the disk.
                // 10 - Read Only       : The "Read-Only" switch on the disk drive is on.
                // 11 - Offset          : The cylinder position is currently offset.  This is a mode used for
                //                        recovery of bad data.  (NOTE: Not emulated.)
                // 12-15 Sector count   : A value from 0 to count-1, where count is the number of sectors 
                //                        implemented in the disk drive.  The value returned here is the sector
                //                        count for the *next sector* on the disk.

                ushort status = (ushort)(
                    (_seekIncomplete ?          0x8000 : 0) |
                    (_headOverflow ?            0x4000 : 0) |
                    (_deviceCheck ?             0x2000 : 0) |
                    (NotSelected() ?            0x1000 : 0) |
                    (NotOnline() ?              0x0800 : 0) |
                    (NotReady() ?               0x0400 : 0) |
                    (_sectorOverflow ?          0x0200 : 0) |
                    (_outputLate ?              0x0100 : 0) |
                    (_inputLate ?               0x0080 : 0) |
                    (_compareError ?            0x0040 : 0) |
                    (SelectedDrive.ReadOnly ?   0x0020 : 0) |
                    (_offset ?                  0x0010 : 0) |
                    ((_sector) &                0x000f));

                Log.Write(LogComponent.TridentController, "STATUS word is {0}", Conversion.ToOctal(status));
                return status;
            }
        }

        public void TagInstruction(ushort tag)
        {
            Log.Write(LogComponent.TridentController, "Trident tag instruction {0} queued", Conversion.ToOctal(tag));

            //
            // Add tag to the output FIFO with bit 16 set, identifying it as a tag.
            //
            WriteOutputFifo(0x10000 | tag);
        }

        public void WaitForEmpty()
        {
            // Block the Output task; it will be awoken when the output FIFO is emptied.
            if (_outputFifo.Count > 0)
            {
                _system.CPU.BlockTask(TaskType.TridentOutput);
                _waitForEmpty = true;
                Log.Write(LogComponent.TridentController, "Trident output task blocked until Output FIFO emptied.");
            }
        }

        private bool NotReady()
        {
            return SelectedDrive.NotReady |         // The drive is not ready (i.e. seeking)
                    !SelectedDrive.IsLoaded |       // No pack is loaded
                    _selectedDrive > 7;             // The drive doesn't exist
        }

        private bool NotOnline()
        {
            //
            // A drive is considered online if (a) it exists, and (b) it has a pack loaded.
            //
            return _selectedDrive < 8 ? !SelectedDrive.IsLoaded : true;
        }

        private bool NotSelected()
        {
            //
            // Only drives 0-7 exist and are running.
            //
            return _selectedDrive > 7;
        }

        private void WriteOutputFifo(int value)
        {
            EnqueueOutputFIFO(value);

            //
            // If this is the first word written to an empty FIFO, and we aren't waiting for the next
            // sector pulse, queue up the fifo callback to pull words and process them.
            //
            if (!_pauseOutputProcessing && _outputFifo.Count == 1)
            {
                _outputFifoEvent.TimestampNsec = _outputFifoDuration;
                _system.Scheduler.Schedule(_outputFifoEvent);
            }
        }

        private void OutputFifoCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            switch (_commandState)
            {
                case CommandState.Command:
                    
                    if (_outputFifo.Count > 0)
                    {
                        int fifoWord = DequeueOutputFIFO();

                        if ((fifoWord & 0x10000) != 0)
                        {
                            //
                            // This is a Tag word; process it accordingly.
                            //
                            Log.Write(LogComponent.TridentController, "Tag word {0} pulled from Output FIFO.", Conversion.ToOctal((ushort)fifoWord));
                            ProcessTagWord(fifoWord);
                        }
                        else
                        {
                            //
                            // This is a data word, unexpected in this state.
                            //
                            Log.Write(LogType.Error, LogComponent.TridentController, "Unexepected Data word {0} in Command state.", Conversion.ToOctal((ushort)fifoWord));                                
                        }

                        //
                        // Schedule next FIFO wakeup if there are more words left to be processed.
                        //
                        RescheduleOutputEvent();
                    }
                    break;

                case CommandState.Read:
                    //
                    // Once a Read operation has commenced, we do not process the output FIFO via this callback, instead we let
                    // the Input callback dequeue words as necessary to do Compare operations with incoming data.
                    //
                    if (_outputFifo.Count > 0 && _readState != ReadState.Reading)
                    {
                        int fifoWord = DequeueOutputFIFO();

                        if ((fifoWord & 0x10000) != 0)
                        {
                            //
                            // This is a Tag word, unexpected in this state.
                            //
                            Log.Write(LogComponent.TridentController, "Unexpected tag word {0} in Read state.", Conversion.ToOctal((ushort)fifoWord));
                        }
                        else
                        {
                            switch (_readState)
                            {
                                case ReadState.Initialized:
                                    _readWordCount = fifoWord - 1;
                                    _readState = ReadState.Reading;

                                    Log.Write(LogType.Verbose, LogComponent.TridentController, "Read initializing {0} words (max) to read.", _readWordCount);
                                    break;

                                default:
                                    throw new InvalidOperationException(
                                        String.Format("Unexpected ReadState {0} in OutputFIFOCallback.", _readState));

                            }
                        }

                        //
                        // Schedule next FIFO wakeup if there are more words left to be processed.
                        //
                        RescheduleOutputEvent();
                    }
                    
                    break;

                case CommandState.Write:
                    if (_outputFifo.Count == 0)
                    {
                        //
                        // We have run out of data on a write.
                        //
                        _outputLate = true;

                        Log.Write(LogComponent.TridentController, "Output FIFO underflowed.");
                    }
                    else
                    {
                        int fifoWord = DequeueOutputFIFO();

                        if ((fifoWord & 0x10000) != 0)
                        {
                            //
                            // This is a Tag word, unexpected in this state.
                            //
                            Log.Write(LogComponent.TridentController, "Unexpected tag word {0} in Write state.", Conversion.ToOctal((ushort)fifoWord));
                        }
                        else
                        {
                            switch (_writeState)
                            {
                                case WriteState.Initialized:
                                    // This word should be the MAX number of words to check.
                                    _writeWordCount = fifoWord - 1;
                                    _writeState = WriteState.WaitingForStartBit;
                                    Log.Write(LogComponent.TridentController, "Write initializing {0} words (max) to write.", _writeWordCount);
                                    break;

                                case WriteState.WaitingForStartBit:
                                    // See if we got the "1" bit indicating the start of the output data to be written to disk.
                                    if (fifoWord == 1)
                                    {
                                        Log.Write(LogComponent.TridentController, "Start bit recognized, commencing write operation.");
                                        _writeState = WriteState.Writing;
                                    }
                                    break;

                                case WriteState.Writing:
                                    ProcessDiskWrite(fifoWord);
                                    break;

                                default:
                                    throw new InvalidOperationException(
                                        String.Format("Unexpected WriteState {0} in OutputFIFOCallback.", _writeState));
                            }
                        }

                        //
                        // Schedule next FIFO wakeup if there are more words left to be processed.
                        //
                        RescheduleOutputEvent();
                        
                    }
                    break;
            }
        }

        private void RescheduleOutputEvent()
        {
            if (!_pauseOutputProcessing && _outputFifo.Count > 0)
            {
                _outputFifoEvent.TimestampNsec = _writeState == WriteState.Writing ? _writeWordDuration : _outputFifoDuration;
                _system.Scheduler.Schedule(_outputFifoEvent);
            }
        }

        private void ProcessTagWord(int tagWord)
        {
            //
            // From the TRICON schematic (page 16):
            // Bit 3 is the Enable bit, which enables selection of one of four commands
            // to the drive, specified in bits 1 and 2:
            // (and as usual, these are in the backwards Nova/Alto bit ordering scheme)
            // Bits 1 and 2 decode to:
            // 0 0 - Control
            // 0 1 - Set Head
            // 1 0 - Set Cylinder
            // 1 1 - Set Drive
            //
            // Head, Cylinder, Drive are straightforward -- the lower bits of the tag
            // word contain the data for the command.
            //
            // The Control bits are described in the Trident T300 Theory of Operations manual,
            // Page 3-13 and are the lower 10 bits of the tag command:
            //
            // 0 - Strobe Late  : Skews read data detection 4ns late for attempted read-error recovery.
            // 1 - Strobe Early : Same as above, but early.
            // 2 - Write        : Turns on circuits to write data,
            // 3 - Read         : Turns on read circuits and resets Attention interrupts.
            // 4 - Address Mark : Commands an address mark to be generated, if writing; or 
            //                    enables the address mark detector, if reading.
            // 5 - Reset Head Register : Resets HAR to 0
            // 6 - Device Check Reset : Resets most types of Device Check errors unless an error
            //                          condition is still present.
            // 7 - Head Select  : Tuns on the head-selection circuits.  Head select must be active 5
            //                    or 15 microseconds before Write or Read is commanded, respectively.
            // 8 - Rezero       : Repositions the heads to cylinder 000, selects Head Address 0, and resets
            //                    some types of Device Checks.
            // 9 - Head Advance : Increases Head Address count by one.
            //
            //
            // Bit 0 of the Tag word, if set, tells the controller to hold off Output FIFO processing
            // until the next sector pulse.
            //
            if ((tagWord & 0x8000) != 0)
            {
                _pauseOutputProcessing = true;
                Log.Write(LogComponent.TridentController, "Output FIFO processing paused until next sector pulse.");
            }

            //
            // See if the enable bit (3) is set, in which case this is a command to the drive.
            //
            if ((tagWord & 0x1000) != 0)
            {
                //
                // Switch on the specific command
                switch ((TagCommand)((tagWord & 0x6000) >> 13))
                {
                    case TagCommand.Control:
                        Log.Write(LogComponent.TridentController, "Control word.");
                        ControlFlags control = (ControlFlags)tagWord;

                        if ((control & ControlFlags.HeadAdvance) != 0)
                        {
                            if (!SelectedDrive.IsLoaded)
                            {
                                _deviceCheck = true;
                            }
                            else
                            {
                                if (SelectedDrive.Head + 1 >= SelectedDrive.Pack.Geometry.Heads)
                                {
                                    _headOverflow = true;
                                    _deviceCheck = true;

                                    Log.Write(LogComponent.TridentController, "Head {0} is out of range on Head Advance.", SelectedDrive.Head + 1);
                                }
                                else
                                {
                                    SelectedDrive.Head++;
                                    Log.Write(LogComponent.TridentController, "Control: Head Advance.  Head is now {0}", SelectedDrive.Head);
                                }
                            }
                        }

                        if ((control & ControlFlags.Rezero) != 0)
                        {
                            _deviceCheck = false;
                            SelectedDrive.Head = 0;

                            InitSeek(0);

                            Log.Write(LogComponent.TridentController, "Control: Rezero.");
                        }

                        if ((control & ControlFlags.HeadSelect) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Head Select.");

                            if (!SelectedDrive.IsLoaded)
                            {
                                _deviceCheck = true;
                            }

                            // TODO: technically this needs to be active before a write or read is selected.  Do I care?
                        }

                        if ((control & ControlFlags.DeviceCheckReset) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Device Check Reset.");
                            _deviceCheck = false;
                        }

                        if ((control & ControlFlags.ResetHeadRegister) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Reset Head Register.");
                            SelectedDrive.Head = 0;
                        }

                        if ((control & ControlFlags.AddressMark) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Address mark.");

                            // Not much to do here, emulation-wise.
                        }

                        if ((control & ControlFlags.Read) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Read.");
                            //
                            // Commence reading -- start reading a word at a time into the input FIFO,
                            // Waking up the Input task as necessary.
                            //
                            if (NotReady())
                            {
                                _deviceCheck = true;
                            }
                            else
                            {
                                InitRead();
                            }
                        }

                        if ((control & ControlFlags.Write) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Write.");

                            //
                            // Commence writing -- start pulling a word at a time out of the output FIFO,
                            // Waking up the Output task as necessary.
                            //
                            if (NotReady())
                            {
                                _deviceCheck = true;
                            }
                            else
                            {
                                InitWrite();
                            }
                        }

                        if ((control & ControlFlags.StrobeEarly) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Strobe Early.");

                            // Not going to emulate this, as fun as it sounds.
                        }

                        if ((control & ControlFlags.StrobeLate) != 0)
                        {
                            Log.Write(LogComponent.TridentController, "Control: Strobe Late.");

                            // Not going to emulate this either.
                        }

                        break;

                    case TagCommand.SetHead:
                        int head = tagWord & 0x1f; // low 5 bits
                        Log.Write(LogComponent.TridentController, "Command is Set Head {0}", head);

                        if (!SelectedDrive.IsLoaded)
                        {
                            _deviceCheck = true;
                        }
                        else
                        {
                            if (head >= SelectedDrive.Pack.Geometry.Heads)
                            {
                                _headOverflow = true;
                                _deviceCheck = true;

                                Log.Write(LogComponent.TridentController, "Head {0} is out of range.", head);
                            }
                            else
                            {
                                SelectedDrive.Head = head;
                            }
                        }
                        break;

                    case TagCommand.SetCylinder:
                        int cyl = tagWord & 0x3ff; // low 10 bits
                        Log.Write(LogComponent.TridentController, "Command is Set Cylinder {0}.", cyl);

                        if (NotReady())
                        {
                            _deviceCheck = true;
                        }
                        else
                        {
                            InitSeek(cyl);
                        }
                        break;

                    case TagCommand.SetDrive:
                        //
                        // We take all four drive-select bits even though only 8 drives are actually supported.
                        // The high bit is used by many trident utilities to select an invalid drive to test for
                        // the presence of the 8-drive multiplexer.
                        // (In the absence of the multiplexer, selecting any drive selects drive 0.)
                        //
                        _selectedDrive = tagWord & 0xf;

                        Log.Write(LogComponent.TridentController, "Command is Set Drive {0}", _selectedDrive);
                        break;
                }
            }
        }       

        private void InitSeek(int destCylinder)
        {
            _deviceCheck = !SelectedDrive.Seek(destCylinder);
        }

        private void InitRead()
        {
            if (_readState == ReadState.Idle)
            {
                _readState = ReadState.Initialized;
                _commandState = CommandState.Read;
                _checkedWordCount = 0;
                _readIndex = 0;
                _readWordCount = 0;
                _checkDone = false;

                _readWordEvent.TimestampNsec = _readWordDuration;
                _system.Scheduler.Schedule(_readWordEvent);
            }
            else
            {
                // Unexpected behavior.
                throw new InvalidOperationException("Unexpected Read command while read active.");
            }
        }

        private void ReadWordCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            if (_readWordCount > 0)
            {
                // Enqueue data from disk.
                ushort dataWord;

                switch (_sectorBlock)
                {
                    case SectorBlock.Header:
                        dataWord = SelectedDrive.ReadHeader(_readIndex);
                        break;

                    case SectorBlock.Label:
                        dataWord = SelectedDrive.ReadLabel(_readIndex);
                        break;

                    case SectorBlock.Data:
                        dataWord = SelectedDrive.ReadData(_readIndex);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unexpected Sector Block of {0} on read.", _sectorBlock));
                }

                Log.Write(LogType.Verbose, LogComponent.TridentController, "Read word {0}:{1}", _readIndex, Conversion.ToOctal(dataWord));

                EnqueueInputFIFO(dataWord);

                //
                // Compare data with check data in output fifo, if any.
                // From the microcode comments:
                // "Note that the first two words of a block are always checked,
                //  followed by additional words until a zero word or the end of the block."
                //
                // If we hit a tag word, the check is also completed.
                // TODO: verify this w/schematic & microcode.

                if (_outputFifo.Count > 0)
                {
                    int checkWord = _outputFifo.Peek();
                    if ((checkWord & 0x10000) == 0 && (!_checkDone || _checkedWordCount < 2))
                    {
                        // Actually pull the word off
                        checkWord = (ushort)DequeueOutputFIFO();

                        Log.Write(LogType.Verbose, LogComponent.TridentController, "Pulled checkword {0} from output FIFO", Conversion.ToOctal(checkWord));

                        // A zero word indicates the check is complete (we will
                        // still read the minimum two words in)
                        if (checkWord == 0)
                        {
                            _checkDone = true;
                        }
                        // Compare didn't.
                        else if (checkWord != dataWord)
                        {
                            _compareError = true;
                        }
                        _checkedWordCount++;
                    }
                }
            }
            else
            {
                // Last four words (0 through -3) are checksum and ECC words.
                // The checksum words are ignored by the microcode, and
                // since we're not simulating faulty disks, these are always zero.
                EnqueueInputFIFO(0);
                Log.Write(LogComponent.TridentController, "Read ECC/checksum word 0");
            }

            _readIndex++;
            _readWordCount--;

            if (_readWordCount > -4)
            {
                // More words to read, queue up the next.
                _readWordEvent.TimestampNsec = _readWordDuration;
                _system.Scheduler.Schedule(_readWordEvent);
            }
            else
            {
                Log.Write(LogComponent.TridentController, "CHS {0}/{1}/{2} {3} read from drive {4} complete.", SelectedDrive.Cylinder, SelectedDrive.Head, _sector, _sectorBlock, _selectedDrive);
                _readState = ReadState.Idle;
                _commandState = CommandState.Command;

                _sectorBlock++;

                //
                // Kick the output event if necessary.
                //
                RescheduleOutputEvent();
            }
        }        

        private void InitWrite()
        {
            if (_writeState == WriteState.Idle)
            {
                Log.Write(LogComponent.TridentController, "Write primed, waiting for start bit.");
                _writeState = WriteState.Initialized;
                _commandState = CommandState.Write;
                _writeIndex = 0;
            }
            else
            {
                // Unexpected, throw for now.
                throw new InvalidOperationException("Unexpected Write command while write active.");
            }
        }

        private void ProcessDiskWrite(int dataWord)
        {
            //
            // Sanity check: if there's a tag bit set, we've picked up some
            // invalid data...
            //
            if ((dataWord & 0x10000) != 0)
            {
                Log.Write(LogType.Error, LogComponent.TridentController, "Tag bit set in data word during Write ({0}).", Conversion.ToOctal(dataWord));
            }

            Log.Write(LogComponent.TridentController, "Write word {0}:{1}", _writeIndex, Conversion.ToOctal(dataWord));

            //
            // Commit to the proper block in the sector:
            //
            switch(_sectorBlock)
            {
                case SectorBlock.Header:
                    SelectedDrive.WriteHeader(_writeIndex, (ushort)dataWord);
                    break;

                case SectorBlock.Label:
                    SelectedDrive.WriteLabel(_writeIndex, (ushort)dataWord);
                    break;

                case SectorBlock.Data:
                    SelectedDrive.WriteData(_writeIndex, (ushort)dataWord);
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unexpected Sector Block of {0} on write.", _sectorBlock));
            }
            
            _writeIndex++;

            _writeWordCount--;
            if (_writeWordCount <= 0)
            {
                Log.Write(LogType.Verbose, LogComponent.TridentController, "CHS {0}/{1}/{2} {3} write to drive {4} complete. {5} words written.", SelectedDrive.Cylinder, SelectedDrive.Head, _sector, _sectorBlock, _selectedDrive, _writeIndex);
                _writeState = WriteState.Idle;
                _commandState = CommandState.Command;
                _writeIndex = 0;

                // Move to the next block
                _sectorBlock++;
            }
        }

        private void SectorCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            // Move to the next sector if the controller is running
            // and the disk is ready.
            if (_runEnable && !NotReady())
            {
                _sector = (_sector + 1) % 9;
                SelectedDrive.Sector = _sector;

                // Reset to the first block (header) in the sector.
                _sectorBlock = SectorBlock.Header;

                // Wake up the Output task
                _system.CPU.WakeupTask(TaskType.TridentOutput);
            }
            else
            {
                // Keep the output task asleep.
                _system.CPU.BlockTask(TaskType.TridentOutput);
            }

            //
            // Schedule the next (potential) sector pulse
            //            
            _sectorEvent.TimestampNsec = _sectorDuration - skewNsec;
            _system.Scheduler.Schedule(_sectorEvent);

            //
            // If output fifo processing was paused and there's data in the FIFO
            // to be dealt with, wake up the callback now.
            //
            if (_pauseOutputProcessing && _outputFifo.Count > 0)
            {
                _pauseOutputProcessing = false;
             
                _outputFifoEvent.TimestampNsec = _outputFifoDuration;
                _system.Scheduler.Schedule(_outputFifoEvent);
            }
        }

        private TridentDrive SelectedDrive
        {
            get { return _drives[_selectedDrive]; }
        }
        

        //
        // Input FIFO semantics
        //
        private void ClearInputFIFO()
        {
            _inputFifo.Clear();
            _system.CPU.BlockTask(TaskType.TridentInput);
        }

        private void EnqueueInputFIFO(ushort word)
        {
            if (_inputFifo.Count == 16)
            {
                // We have overflowed the input FIFO.  Set the requisite error
                // flags and shut this thing down.
                _inputLate = true;

                Log.Write(LogComponent.TridentController, "Input FIFO overflowed.");
            }
            else
            {
                _inputFifo.Enqueue(word);

                if (_inputFifo.Count >= 4)
                {
                    _system.CPU.WakeupTask(TaskType.TridentInput);
                }
                else
                {
                    _system.CPU.BlockTask(TaskType.TridentInput);
                }
            }
        }

        private ushort DequeueInputFIFO()
        {
            ushort word = 0;
            if (_inputFifo.Count == 0)
            {
                Log.Write(LogType.Warning, LogComponent.TridentController, "Input FIFO underflowed, returning 0.");
            }
            else
            {
                word = _inputFifo.Dequeue();
            }

            if (_inputFifo.Count < 4)
            {
                _system.CPU.BlockTask(TaskType.TridentInput);
            }
            else
            {
                _system.CPU.WakeupTask(TaskType.TridentInput);
            }
            return word;
        }

        //
        // Output FIFO semantics
        //
        private void ClearOutputFIFO()
        {
            _outputFifo.Clear();
            _system.CPU.WakeupTask(TaskType.TridentOutput);
        }

        private void EnqueueOutputFIFO(int word)
        {
            if (_outputFifo.Count == 16)
            {
                Log.Write(LogComponent.TridentController, "Output FIFO overflowed, dropping word.");             
            }
            else
            {
                _outputFifo.Enqueue(word);

                Log.Write(LogComponent.TridentController, "Output FIFO enqueued, queue depth is now {0}", _outputFifo.Count);
            }

            if (_outputFifo.Count <= 12)
            {
                _system.CPU.WakeupTask(TaskType.TridentOutput);
            }
            else
            {
                _system.CPU.BlockTask(TaskType.TridentOutput);
            }
            
        }

        private int DequeueOutputFIFO()
        {
            int word = 0;
            if (_outputFifo.Count == 0)
            {
                Log.Write(LogType.Warning, LogComponent.TridentController, "Output FIFO underflowed, returning 0.");                
            }
            else
            {
                word = _outputFifo.Dequeue();
                Log.Write(LogComponent.TridentController, "Output FIFO dequeued, queue depth is now {0}", _outputFifo.Count);
            }

            if (_waitForEmpty)
            {
                // Only wake up the Output task when the output FIFO goes completely empty.
                if (_outputFifo.Count == 0)
                {
                    _waitForEmpty = false;
                    _system.CPU.WakeupTask(TaskType.TridentOutput);
                    Log.Write(LogComponent.TridentController, "Output FIFO emptied, waking Output task.");
                }
                else
                {
                    _system.CPU.BlockTask(TaskType.TridentOutput);
                }
            }
            else
            {
                if (_outputFifo.Count <= 12)
                {
                    _system.CPU.WakeupTask(TaskType.TridentOutput);
                }
                else
                {
                    _system.CPU.BlockTask(TaskType.TridentOutput);
                }
            }
            return word;
        }

        // Wakeup signals
        private bool _runEnable;

        // Status bits
        private bool _seekIncomplete;
        private bool _headOverflow;
        private bool _deviceCheck;
        private bool _sectorOverflow;
        private bool _outputLate;
        private bool _inputLate;
        private bool _compareError;
        private bool _readOnly;
        private bool _offset;     

        // Current sector
        private int _sector;

        /// <summary>
        /// Drive timings
        /// </summary>
        private static ulong _sectorDuration = (ulong)((16.67 / 9.0) * Conversion.MsecToNsec);      // time in nsec for one sector -- 9 sectors, 16.66ms per rotation
        private Event _sectorEvent;

        //
        // Command state
        //
        private CommandState _commandState;

        //
        // Output FIFO.  This is actually 17-bits wide (the 17th bit is the Tag bit).
        //
        private Queue<int> _outputFifo;
        private Event _outputFifoEvent;
        private bool _pauseOutputProcessing;
        private bool _waitForEmpty;
        private static ulong _outputFifoDuration = (ulong)(_sectorDuration / 2240.0);   // time to pull one word from fifo during command processing -- this is made up at the moment.
        private static ulong _writeWordDuration = (ulong)(_sectorDuration / 2240.0);    // time in nsec for one word time on disk -- 1120 words per sector.
                                                                                        // TODO: when a write isn't in process, how is the output fifo clocked?

        // Input FIFO.
        private Queue<ushort> _inputFifo;

        //
        // Read timings
        //
        private static ulong _readWordDuration = (ulong)(_sectorDuration / 1120.0);     // time in nsec for one word time on disk -- 1120 words per sector.
        private Event _readWordEvent;
        private int _readIndex;
        private int _readWordCount;
        private bool _checkDone;
        private int _checkedWordCount;

        //
        // Write data
        //
        private int _writeIndex;
        private int _writeWordCount;

        private enum TagCommand
        {
            Control = 0,
            SetHead = 1,
            SetCylinder = 2,
            SetDrive = 3,
        }

        [Flags]
        private enum ControlFlags
        {
            HeadAdvance         = 0x001,       // 9
            Rezero              = 0x002,
            HeadSelect          = 0x004,
            DeviceCheckReset    = 0x008,
            ResetHeadRegister   = 0x010,
            AddressMark         = 0x020,
            Read                = 0x040,
            Write               = 0x080,
            StrobeEarly         = 0x100,
            StrobeLate          = 0x200,       // 0
        }

        private enum CommandState
        {
            Command,
            Read,
            Write
        }

        private enum WriteState
        {
            Idle,
            Initialized,
            WaitingForStartBit,
            Writing,
        }
        private WriteState _writeState;

        private enum ReadState
        {
            Idle,
            Initialized,
            Reading,
        }
        private ReadState _readState;

        private enum SectorBlock
        {
            Header,
            Label,
            Data,
        }
        private SectorBlock _sectorBlock;
        
        //
        // Attached drives
        //
        private TridentDrive[] _drives;
        private int _selectedDrive;

        private AltoSystem _system;
    }
}
