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


using Contralto.IO.Printing;
using Contralto.Logging;
using System;

namespace Contralto.IO
{
    public enum AdapterCommand
    {
        BufferReset = 0,
        SetScales = 1,
        SetBitClockRegister = 2,
        SetMotorSpeedRegister = 3,
        SetLineSyncDelayRegister = 4,
        SetPageSyncDelayRegister = 5,
        ExternalCommand1 = 6,
        ExternalCommand2 = 7,
    }

    /// <summary>
    /// Encapsulates the logic for both the ROS (Raster Output Scanner) hardware and
    /// the Dover print engine.
    /// 
    /// These two should probably be separated out in the event that we want to support
    /// other kinds of printers.
    /// 
    /// The Dover print engine support is currently very hand-wavy as the documentation
    /// isn't extremely specific on some details and we're emulating a mechanical construct
    /// moving papers past sensors, etc.  It mostly works but it has some rough edges.
    /// In particular the "Cold Start" mechanic is not well understood.  Many parameters
    /// are currently ignored (some for good reason; we don't need to emulate the actual
    /// laser scanning and polygon rotation in order to create a bitmap, for example) but
    /// some of them might be put to good use.
    /// 
    /// None of the diagnostic switches or options are implemented.
    /// </summary>
    public class DoverROS
    {
        public DoverROS(AltoSystem system)
        {
            _system = system;            
            _printEngineEvent = new Event(0, null, PrintEngineCallback);

            Reset();
        }

        public void Reset()
        {
            _testMode = false;
            _commandBeamOn = false;
            _commandLocal = false;
            _testPageSync = false;
            _extendVideo = false;
            _motorScale = 0;
            _bitScale = 0;
            _bitClock = 0;
            _motorSpeed = 0;
            _lineSyncDelay = 0;
            _pageSyncDelay = 0;

            _packetsOK = true;

            _state = PrintState.Idle;
            _runoutCount = 0;
        }

        public void RecvCommand(ushort commandWord)
        {
            //
            // Control of the adapter, ROS, and printer is accomplished with 16-bit commands.
            // The high-order 4 bits of the command give a command code; the remaining 12 bits are
            // used as an argument to the command.
            //
            _lastCommand = commandWord;
            AdapterCommand command = (AdapterCommand)(commandWord >> 12);
            int argument = commandWord & 0xfff;

            switch (command)
            {
                case AdapterCommand.BufferReset:
                    break;

                case AdapterCommand.SetScales:
                    _testMode = (argument & 0x1) != 0;
                    _commandBeamOn = (argument & 0x2) != 0;
                    _commandLocal = (argument & 0x4) != 0;
                    _testPageSync = (argument & 0x8) != 0;
                    _extendVideo = (argument & 0x20) != 0;
                    _motorScale = (argument & 0x1c0) >> 6;
                    _bitScale = (argument & 0xe00) >> 9;

                    Log.Write(LogComponent.DoverROS, "TestMode {0} CommandBeamOn {1} CommandLocal {2} TestPageSync {3} ExtendVideo {4} MotorScale {5} BitScale {6}",
                        _testMode,
                        _commandBeamOn,
                        _commandLocal,
                        _testPageSync,
                        _extendVideo,
                        _motorScale,
                        _bitScale);
                    break;

                case AdapterCommand.SetBitClockRegister:
                    _bitClock = argument;
                    Log.Write(LogComponent.DoverROS, "BitClock set to {0}", argument);
                    break;

                case AdapterCommand.SetMotorSpeedRegister:
                    _motorSpeed = argument;
                    Log.Write(LogComponent.DoverROS, "MotorSpeed set to {0}", argument);
                    break;

                case AdapterCommand.SetLineSyncDelayRegister:
                    _lineSyncDelay = argument;
                    Log.Write(LogComponent.DoverROS, "LineSyncDelay set to {0}", argument);
                    break;

                case AdapterCommand.SetPageSyncDelayRegister:
                    _pageSyncDelay = argument;
                    Log.Write(LogComponent.DoverROS, "PageSyncDelay set to {0}", argument);
                    break;

                case AdapterCommand.ExternalCommand1:

                    bool lastPrintRequest = (_externalCommand1 & 0x1) == 0;

                    _externalCommand1 = argument;

                    //
                    // Dover uses the low-order bit to provide the PrintRequest signal.
                    // A 0-to-1 transition of the bit tells the printer to start feeding
                    // sheets.
                    //
                    Log.Write(LogComponent.DoverROS, "ExternalCommand1 written {0}", argument);
                    if (lastPrintRequest && (_externalCommand1 & 0x1) != 0)
                    {
                        PrintRequest();
                    }
                    break;

                case AdapterCommand.ExternalCommand2:
                    _videoGate = argument;
                    Log.Write(LogComponent.DoverROS, "VideoGate set to {0}", argument);
                    break;

                default:
                    Log.Write(LogType.Error, LogComponent.DoverROS,
                        String.Format("Unhandled ROS command {0}", command));
                    break;
            }
        }

        public int ReadStatus(int address)
        {
            //
            // Address is a value from 0-63 (as specified by the Orbit's OrbitControl function)
            // specifying the address of 4 status bits in the ROS's status memory.
            // 4 status bits -- 4*address to 4*address+3 inclusive are returned.
            //
            int bitAddress = (address * 4);
            ushort statusWord = ReadStatusWord(bitAddress >> 4);
            int bitOffset = 12 - (bitAddress & 0xf);

            return (statusWord & ((0xf) << bitOffset)) >> bitOffset;
        }

        private ushort ReadStatusWord(int wordNumber)
        {
            int value = 0;
            switch (wordNumber)
            {
                case 0:
                    //
                    // Special status from the ROS:
                    // bit 
                    //  0 - SendVideo
                    //  1 - PrintMode
                    //  2 - Local
                    //  3 - BeamEnable
                    //  4 - StatusBeamOn
                    //
                    value |= (_sendVideo ? 0x8000 : 0);
                    value |= (_printMode ? 0x4000 : 0);
                    value |= (_local ? 0x2000 : 0);
                    value |= (_beamEnable ? 0x1000 : 0);
                    value |= (_statusBeamOn ? 0x0800 : 0);
                    break;

                case 1:
                    // A copy of the command most recently received by the adapter.
                    value = _lastCommand;
                    break;

                case 2:
                    //
                    // Bit clock:
                    // bit
                    //  0 - VideoPolarity
                    //  1-3 - BitScale
                    //  4-15 - BitClock
                    //
                    value |= (_videoPolarity ? 0x8000 : 0);
                    value |= (_bitScale << 12);
                    value |= _bitClock;
                    break;

                case 3:
                    //
                    // Motor speed
                    // bit
                    //  0 - SelectLeadEdge
                    //  1-3 - MotorScale
                    //  4-15 - MotorSpeed
                    value |= (_selectLeadEdge ? 0x8000 : 0);
                    value |= (_motorScale << 12);
                    value |= _motorSpeed;
                    break;

                case 4:
                    // 
                    // Line sync delay
                    // bit
                    //  0 - Switch3
                    //  2 - ExtendVideo
                    //  3 - TestPageSync
                    //  4-15 - LineSyncDelay
                    //
                    value |= (_switch3 ? 0x8000 : 0);
                    value |= (_extendVideo ? 0x2000 : 0);
                    value |= (_testPageSync ? 0x1000 : 0);
                    value |= _lineSyncDelay;
                    break;

                case 5:
                    //
                    // Page sync delay
                    // bit
                    //  0 - Switch4
                    //  1 - CommandLocal
                    //  2 - CommandBeamOn
                    //  3 - TestMode
                    //  4-15 - PageSyncDelay
                    //
                    value |= (_switch4 ? 0x8000 : 0);
                    value |= (_commandLocal ? 0x4000 : 0);
                    value |= (_commandBeamOn ? 0x2000 : 0);
                    value |= (_testMode ? 0x1000 : 0);
                    value |= _pageSyncDelay;
                    break;

                case 6:
                    //
                    // External Command 1
                    // bit
                    //  0 - LineNoise
                    //  1 - CompareError
                    //  2 - BufferUnderflow
                    //  3 - PacketsOK
                    //  4-12 - ExternalCommand1
                    //
                    value |= (_lineNoise ? 0x8000 : 0);
                    value |= (_compareError ? 0x4000 : 0);
                    value |= (_bufferUnderflow ? 0x2000 : 0);
                    value |= (_packetsOK ? 0x1000 : 0);
                    value |= _externalCommand1;
                    break;

                case 7:
                    //
                    // bit
                    //  0-3 - LineCount
                    //  4-15 - VideoGate
                    //
                    value |= (_lineCount << 12);
                    value |= _videoGate;

                    //
                    // LineCount is not well documented anywhere in the hardware documentation
                    // available.  It is related to the motion of the laser as it sweeps across
                    // the page.  The source code for Spruce (SprucePrinDover.bcpl)
                    // has this to say:
                    // "A check, designed chiefly for Dover, verifies that the laser is on and the polygon is
                    //  scanning (SOS/EOS are seeing things), by making sure that the low four bits of the line
                    //  count (indicated in status word) are  changing. The code reads the line count, waits
                    //  approximately the time needed for four scan lines (generous margin), then reads the
                    //  count again, reporting a problem if the values are equal."
                    //

                    //
                    // Indeed, if this value does not increment, Spruce fails with a
                    // "Laser appears to be off" error.
                    // We fudge this here.
                    _lineCount++;
                    break;

                case 8:
                    // Special Dover status bits 0-15

                    // Count-H -- indicates that a page is ready to be printed.
                    value |= (_countH ? 0x1000 : 0);

                    //
                    // OR in status bits that are expected to be "1"
                    // for normal operation (i.e. no malfunctions).
                    // These are:
                    // 8 - LS4 (adequate paper in tray)
                    // 11 - LaserOn
                    // 13 - ReadyTemp
                    value |= 0x0094;
                    break;

                case 9:
                    // Special Dover status bits 16-31

                    //
                    // OR in status bits that are expected to be "1"
                    // for normal operation (i.e. no malfunctions).
                    // These are:
                    // 5 - ACMonitor
                    // 13 - LS24 & LS31
                    value |= 0x0404;

                    Log.Write(LogComponent.DoverROS, "Dover bits 16-31: {0}", Conversion.ToOctal(value));
                    break;

                case 10:
                    value = _id;
                    Log.Write(LogComponent.DoverROS, "Device ID {0}", Conversion.ToOctal(value));
                    break;

                case 11:
                    value = _serialNumber;
                    Log.Write(LogComponent.DoverROS, "Device Serial: {0}", Conversion.ToOctal(value));
                    break;

                default:
                    Log.Write(LogComponent.DoverROS, "Unhandled ROS status word {0}", wordNumber);
                    value = 0xffff;
                    break;
            }

            return (ushort)value;
        }

        private void PrintRequest()
        {
            switch (_state)
            {
                case PrintState.Idle:
                    if (!_printMode)
                    {
                        _state = PrintState.ColdStart;
                        _printMode = true;
                        _sendVideo = true;
                        _keepGoing = false;
                        _runoutCount = 0;

                        ClearPageRaster();

                        //
                        // Calculate a few things based on ROS parameters
                        //
                        // PageSyncDelay is (4096-n/i) where n is the number of
                        // scan-lines to pass up after receiving PageSync from the engine
                        // before starting SendVideo.  i is 1 for Dover II, and 4 for older
                        // ones.
                        // We assume a Dover I here.
                        _sendVideoStartScanline = 4 * (4096 - _pageSyncDelay);

                        //
                        // VideoGate is (4096-n/4) where n is the number of scan-lines to
                        // pass up after SendVideo starts before stopping SendVideo.
                        _sendVideoEndScanline = 4 * (4096 - _videoGate) + _sendVideoStartScanline;

                        Log.Write(LogComponent.DoverROS, "SendVideo start {0}, end {1}",
                            _sendVideoStartScanline,
                            _sendVideoEndScanline);

                        //
                        // Start printing engine running.
                        //
                        _printEngineEvent.TimestampNsec = _printEngineTimestepInterval;
                        _system.Scheduler.Schedule(_printEngineEvent);
                        _printEngineTimestep = -375;        // Start 250ms before the first CS-5

                        //
                        // Start output
                        //
                        InitializePrintOutput();                        

                        Log.Write(LogComponent.DoverROS, "Cold Start initialized.  Engine started.");
                    }
                    else
                    {
                        Log.Write(LogComponent.DoverROS, "Unexpected PrintRequest with PrintMode active during Idle.");
                    }

                    break;

                case PrintState.ColdStart:
                    Log.Write(LogComponent.DoverROS, "PrintRequest received in cold start.");
                    _keepGoing = true;
                    break;

                case PrintState.InnerLoop:
                    if (!_printMode)
                    {
                        // PrintRequest too late.
                        Log.Write(LogComponent.DoverROS, "PrintRequest too late during Inner Loop.  Ignoring.");
                    }
                    else
                    {
                        Log.Write(LogComponent.DoverROS, "PrintRequest during inner loop.  Continuing.");
                        _keepGoing = true;
                    }
                    break;

                case PrintState.Runout:
                    if (!_printMode)
                    {
                        // PrintRequest too late.
                        Log.Write(LogComponent.DoverROS, "PrintRequest too late during Runout.  Ignoring.");
                    }
                    else
                    {
                        Log.Write(LogComponent.DoverROS, "PrintRequest during Runout.  Moving to Inner Loop.");
                        _state = PrintState.InnerLoop;                        
                        _keepGoing = true;
                        _runoutCount = 0;
                    }
                    break;
            }
        }

        private void InitializePrintOutput()
        {
            //
            // Select the appropriate output based on the configuration.
            // For now it's either PDF or nothing.
            //
            if (Configuration.EnablePrinting)
            {
                _pageOutput = new PdfPageSink();
            }
            else
            {
                _pageOutput = new NullPageSink();
            }

            _pageOutput.StartDoc();
        }

        /// <summary>
        /// This is invoked for every scanline that passes under the virtual print path.
        /// At each scanline flags are updated and raster data is pulled from the Orbit and
        /// copied to the page as necessary.
        /// </summary>
        /// <param name="timestampNsec"></param>
        /// <param name="delta"></param>
        /// <param name="context"></param>
        private void PrintEngineCallback(ulong delta, object context)
        {
            Log.Write(LogComponent.DoverROS, "Scanline {0} (sendvideo {1})", _printEngineTimestep, _sendVideo);
            switch (_state)
            {
                case PrintState.ColdStart:
                    {
                        // Go through the motions but don't rasterize anything
                        //
                        // The Cold-start loop starts 250ms before the first CS-5 signal.                        
                        if (_printEngineTimestep >= 0 && _printEngineTimestep < _sendVideoStartScanline)
                        {
                            _sendVideo = false;
                        }
                        else
                        {
                            _sendVideo = true;
                        }


                        if (_printEngineTimestep >= 0 && 
                            _printEngineTimestep < _sendVideoEndScanline)
                        {
                            // Pull rasters one scanline at a time out of Orbit,
                            // since we're in cold-start, these are discarded.
                            if (_system.OrbitController.SLOTTAKE)
                            {
                                // Read in one scanline of data -- this is 256 words
                                for (int x = 0; x < 256 - _system.OrbitController.FA; x++)
                                {
                                    ushort word = _system.OrbitController.GetOutputDataROS();

                                    // The assumption is that during this phase the Alto will just
                                    // be sending zeroes, log if this assumption does not hold...
                                    //
                                    if (word != 0)
                                    {
                                        Log.Write(LogComponent.DoverROS, "Read non-zero orbit data during cold-start {0}", Conversion.ToOctal(word));
                                    }
                                }

                                Log.Write(LogComponent.DoverROS, "Read cold-start band {0}", _readBands);
                            }
                            else
                            {
                                // Nothing right now
                                Log.Write(LogComponent.DoverROS, "No bands available from Orbit.");
                            }

                            _readBands++;
                        }

                        if (_printEngineTimestep >= 3136)
                        {
                            // After appx. 896ms (3136 scanlines) the first Count-H is raised.
                            _countH = true;
                        }

                        _printEngineTimestep++;

                        if (_printEngineTimestep >= 3500)
                        {
                            // End of cold-start "page."
                            _readBands = 0;
                            _printEngineTimestep = 0;
                            _countH = false;
                            ClearPageRaster();

                            if (_keepGoing)
                            {
                                // 
                                // We got a PrintRequest during ColdStart, so we'll continue to the next page.
                                //
                                Log.Write(LogComponent.DoverROS, "End of Cold Start, switching to Inner Loop.");
                                _state = PrintState.InnerLoop;
                            }
                            else
                            {
                                //
                                // No PrintRequest, we will switch to Runout and begin shutting down.
                                //
                                Log.Write(LogComponent.DoverROS, "End of Cold Start, switching to Runout.");
                                _state = PrintState.Runout;
                            }

                            _keepGoing = false;
                        }

                        _printEngineEvent.TimestampNsec = _printEngineTimestepInterval;
                        _system.Scheduler.Schedule(_printEngineEvent);
                    }
                    break;

                case PrintState.InnerLoop:
                    {
                        //
                        // Inner loop starts with CS-5 (timestep 0).
                        // After which there is a delay (PageSyncDelay) before
                        // SendVideo goes high and we can start reading raster data.
                        if (_printEngineTimestep < _sendVideoStartScanline)
                        {
                            _sendVideo = false;
                        }
                        else
                        {
                            _sendVideo = true;
                        }

                        if (_printEngineTimestep >= 0 && 
                            _printEngineTimestep < _sendVideoEndScanline)
                        {
                            // Video gate : pull rasters one scanline at a time out of Orbit.
                            if (_system.OrbitController.SLOTTAKE)
                            {
                                // Read in one scanline's worth of data
                                int scanlineWordCount = 256 - _system.OrbitController.FA;
                                for (int x = 0; x < scanlineWordCount; x++)
                                {
                                    ushort word = _system.OrbitController.GetOutputDataROS();
                                    
                                    int pageDataIndex = _readBands * scanlineWordCount * 2 + x * 2;
                                    _pageData[pageDataIndex] = (byte)(~word >> 8);
                                    _pageData[pageDataIndex + 1] = (byte)(~word & 0xff);
                                }

                                Log.Write(LogComponent.DoverROS, "Read band {0}", _readBands);
                            }
                            else
                            {
                                // Nothing right now
                                Log.Write(LogComponent.DoverROS, "No bands available from Orbit.");
                            }

                            _readBands++;
                        }

                        if (_printEngineTimestep >= 3136)
                        {
                            // After appx. 896ms (3136 scanlines) Count-H is raised.
                            _countH = true;
                        }

                        _printEngineTimestep++;

                        if (_printEngineTimestep >= 3500)
                        {
                            //
                            // Send page rasters to the print output.
                            //
                            _pageOutput.AddPage(_pageData, _readBands, (256 - _system.OrbitController.FA) * 16);

                            ClearPageRaster();

                            // End of page.
                            _countH = false;
                            _readBands = 0;
                            _printEngineTimestep = 0;

                            if (_keepGoing)
                            {
                                //
                                // A PrintRequest was recieved during the Inner Loop, so we'll keep going to the next page.
                                Log.Write(LogComponent.DoverROS, "End of Page, continuing in Inner Loop.");
                                _state = PrintState.InnerLoop;
                            }
                            else
                            {
                                Log.Write(LogComponent.DoverROS, "End of Page, switching to Runout.");
                                _state = PrintState.Runout;
                            }

                            _keepGoing = false;
                        }

                        _printEngineEvent.TimestampNsec = _printEngineTimestepInterval;
                        _system.Scheduler.Schedule(_printEngineEvent);

                    }
                    break;

                case PrintState.Runout:
                    {
                        //
                        // SendVideo still gets toggled during Runout.  CountH is
                        // toggled by paper moving past a sensor, which does not happen
                        // during runout.
                        //
                        if (_printEngineTimestep < _sendVideoStartScanline)
                        {
                            _sendVideo = false;
                        }
                        else
                        {
                            _sendVideo = true;
                        }

                        _printEngineTimestep++;

                        if (_printEngineTimestep >= 3500)
                        {

                            // End of runout cycle.
                            _countH = false;
                            _printEngineTimestep = 0;

                            if (_keepGoing)
                            {
                                Log.Write(LogComponent.DoverROS, "End of Runout cycle {0}, continuing in Inner Loop.", _runoutCount);
                                _state = PrintState.InnerLoop;
                            }
                            else
                            {
                                Log.Write(LogComponent.DoverROS, "End of Runout cycle {0}.", _runoutCount);
                                _state = PrintState.Runout;
                            }

                            _keepGoing = false;

                            _runoutCount++;
                        }

                        if (_runoutCount > 7)
                        {
                            // Just shut off.
                            _state = PrintState.Idle;
                            _printMode = false;
                            _countH = false;
                            _sendVideo = false;

                            //
                            // Finish the output.
                            //
                            _pageOutput.EndDoc();

                            Log.Write(LogComponent.DoverROS, "Runout: shutting down, switching to Idle state.  Output completed.");
                        }
                        else
                        {
                            //
                            // Go around for another Runout cycle.
                            _printEngineEvent.TimestampNsec = _printEngineTimestepInterval;
                            _system.Scheduler.Schedule(_printEngineEvent);
                        }
                    }
                    break;
            }
        }

        private void ClearPageRaster()
        {
            //
            // reset to white
            //
            for(int i=0;i<_pageData.Length;i++)
            {
                _pageData[i] = 0xff;
            }
        }

        private enum PrintState
        {
            Idle = 0,
            ColdStart,
            InnerLoop,
            Runout
        }

        private PrintState _state;

        private bool _keepGoing;
        private int _readBands;

        private AltoSystem _system;

        // Last command sent to us
        private ushort _lastCommand;

        // Command registers
        private bool _testMode;
        private bool _commandBeamOn;
        private bool _commandLocal;
        private bool _testPageSync;
        private bool _extendVideo;
        private int _motorScale;
        private int _bitScale;

        private int _bitClock;

        private int _motorSpeed;
        private int _lineSyncDelay;
        private int _pageSyncDelay;

        private int _externalCommand1;
        private int _videoGate;

        // Status registers
        private bool _sendVideo;
        private bool _printMode;
        private bool _local;
        private bool _beamEnable = true;
        private bool _statusBeamOn;

        private bool _lineNoise;
        private bool _compareError;
        private bool _bufferUnderflow;
        private bool _packetsOK = true;

        private int _lineCount;

        // Physical switches (test and otherwise)
        private bool _videoPolarity;
        private bool _selectLeadEdge;
        private bool _switch3;
        private bool _switch4;

        // ID and serial number
        private ushort _id = 0;
        private ushort _serialNumber = 0;

        //
        // Dover specific status that we care to report.
        // TODO: if we're printing to a real printer, we could choose
        // to raise status (malfunction bits, etc.) that correspond to
        // real printer status...
        //

        //
        // This signal is raised if a sheet has been successfully fed to
        // receive the image for the page being printed.
        // Count-H comes on 896ms after CS-5 and persists until the next
        // CS-5.   (Note -- CS-5 is transmitted as the PageSync signal)
        //
        private bool _countH;

        /// <summary>
        /// Number of passes through the Runout state.
        /// </summary>
        private int _runoutCount;


        // Events to drive the print state machine
        //          
        private Event _printEngineEvent;

        private int _printEngineTimestep;
        private int _sendVideoStartScanline;
        private int _sendVideoEndScanline;


        /// <summary>
        /// The timeslice for a single engine step (one scanline).
        /// There are (in theory) 2975 scanlines per 8.5" page (at 350dpi) which moves
        /// through the engine in 850ms.  
        /// 
        /// The cycle time for an entire page is 1000ms, which we pretend
        /// is 3500 scanline times.
        /// 
        /// </summary>
        private ulong _printEngineTimestepInterval = (ulong)((1000.0 / 3500.0) * Conversion.MsecToNsec);

        private byte[] _pageData = new byte[3500 * 512];
        private int _rasterNum;

        private IPageSink _pageOutput;
    }
}
