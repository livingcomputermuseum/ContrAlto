using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// Encapsulates the logic for both the ROS and the printer portions of the
    /// print pipeline.
    /// </summary>
    public class DoverROS
    {
        public DoverROS(AltoSystem system)
        {
            _system = system;

            _cs5Event = new Event(0, null, ColdStartCallback);
            _innerLoopEvent = new Event(0, null, InnerLoopCallback);
            _pageBuffer = new Bitmap(4096, 4096, PixelFormat.Format1bppIndexed);

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

            _state = PrintState.ColdStart;
            _innerLoopState = InnerLoopState.Idle;
            _coldStartState = ColdStartState.SendVideoLow;
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
                    break;

                case AdapterCommand.SetBitClockRegister:
                    _bitClock = argument;
                    break;

                case AdapterCommand.SetMotorSpeedRegister:
                    _motorSpeed = argument;
                    break;

                case AdapterCommand.SetLineSyncDelayRegister:
                    _lineSyncDelay = argument;
                    break;

                case AdapterCommand.SetPageSyncDelayRegister:
                    _pageSyncDelay = argument;
                    break;

                case AdapterCommand.ExternalCommand1:

                    bool lastPrintRequest = (_externalCommand1 & 0x1) != 0;

                    _externalCommand1 = argument;

                    //
                    // Dover uses the low-order bit to provide the PrintRequest signal.
                    // A 0-to-1 transition of the bit tells the printer to start feeding
                    // sheets.
                    //
                    Log.Write(LogComponent.DoverROS, "ExternalCommand1 written {0}", argument);
                    if (lastPrintRequest && (_externalCommand1 & 0x1) == 0)
                    {
                        PrintRequest();
                    }
                    break;

                case AdapterCommand.ExternalCommand2:
                    _videoGate = argument;
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

                    //Log.Write(LogComponent.DoverROS, "ROS word 0 bits 0-15: {0}", Conversion.ToOctal(value));
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
                    value |= 0x214;

                    Log.Write(LogComponent.DoverROS, "Dover bits 0-15: {0}", Conversion.ToOctal(value));
                    break;

                case 9:
                    // Special Dover status bits 16-31

                    //
                    // OR in status bits that are expected to be "1"
                    // for normal operation (i.e. no malfunctions).
                    // These are:
                    // 5 - ACMonitor
                    // 13 - LS24 & LS31

                    value |= 0x2004;

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
                    break;
            }

            return (ushort)value;
        }

        private void PrintRequest()
        {
            switch(_state)
            {
                case PrintState.ColdStart:

                    if (!_printMode)
                    {
                        _printMode = true;
                        _sendVideo = false;

                        // Queue a 250ms event to fire CS-5(0).
                        // and 990ms item to cancel printing if a second
                        // print-request isn't received.
                        _innerLoopState = InnerLoopState.CS5;
                        _innerLoopEvent.TimestampNsec = (ulong)(120 * Conversion.MsecToNsec);
                        _system.Scheduler.Schedule(_innerLoopEvent);

                        Log.Write(LogComponent.DoverROS, "Cold Start initialized at {0}ms -- CS-5(0) in 250ms.", _system.Scheduler.CurrentTimeNsec * Conversion.NsecToMsec);
                    }
                    else
                    {
                        Log.Write(LogComponent.DoverROS, "PrintRequest received in cold start at {0}ms.", _system.Scheduler.CurrentTimeNsec * Conversion.NsecToMsec);
                        _keepGoing = true;
                    }
                    break;

                case PrintState.InnerLoop:
                    if (!_printMode)
                    {
                        // PrintRequest too late.
                        Log.Write(LogComponent.DoverROS, "PrintRequest too late.  Ignoring.");
                    }
                    else
                    {
                        if (_innerLoopState != InnerLoopState.Idle)
                        {
                            Log.Write(LogComponent.DoverROS, "PrintRequest received in inner loop.");
                            _keepGoing = true;
                        }
                        else
                        {
                            // 
                            // Currently idle:  Kick off the first round of the inner loop.
                            // Queue a PageSyncDelay (250ms) event to pulse SendVideo
                            // after the pulse, gather video raster from Orbit
                            //
                            Log.Write(LogComponent.DoverROS, "PrintRequest received, starting inner loop.");
                            _innerLoopState = InnerLoopState.CS5;
                            _innerLoopEvent.TimestampNsec = 120 * Conversion.MsecToNsec;
                            _system.Scheduler.Schedule(_innerLoopEvent);
                        }
                    }
                    break;

                case PrintState.Runout:
                    Log.Write(LogComponent.DoverROS, "Runout.");
                    break;

            }

        }

        private void InnerLoopCallback(ulong timestampNsec, ulong delta, object context)
        {
            switch(_innerLoopState)
            {
                case InnerLoopState.CS5:
                    _countH = false;
                    _sendVideo = false;

                    // Keep SendVideo low for 125ms
                    _innerLoopState = InnerLoopState.SendVideo;
                    _innerLoopEvent.TimestampNsec = 125 * Conversion.MsecToNsec;
                    _system.Scheduler.Schedule(_innerLoopEvent);

                    Log.Write(LogComponent.DoverROS, "Inner loop: CS5");
                    break;

                case InnerLoopState.SendVideo:
                    _sendVideo = true;

                    _innerLoopState = InnerLoopState.ReadBands;
                    _readBands = 0;

                    // time for one band of 16 scanlines to be read (approx.)
                    _innerLoopEvent.TimestampNsec = (ulong)(0.2 * Conversion.MsecToNsec);
                    _system.Scheduler.Schedule(_innerLoopEvent);

                    Log.Write(LogComponent.DoverROS, "Inner loop: SendVideo");
                    break;

                case InnerLoopState.ReadBands:
                    // Assume 3000 scanlines for an 8.5" sheet of paper at 350dpi.
                    // If the Orbit is allowing us to read the output buffer then
                    // we will do so, otherwise we emit nothing.
                    if (_readBands > 3000)
                    {
                        if (_state == PrintState.ColdStart)
                        {
                            _innerLoopState = InnerLoopState.ColdStartEnd;
                        }
                        else
                        {
                            _innerLoopState = InnerLoopState.CountH;
                        }
                    }
                    else
                    {
                        if (_system.OrbitController.SLOTTAKE)
                        {
                            // Read in 16 scanlines of data -- this is 256x16 words
                            for (int y = 0; y < 16; y++)
                            {
                                for (int x = 0; x < 256 - _system.OrbitController.FA; x++)
                                {
                                    ushort word = _system.OrbitController.GetOutputDataROS();

                                    _pageData[(_readBands + y) * 512 + x * 2] = (byte)(word & 0xff);
                                    _pageData[(_readBands + y) * 512 + x * 2 + 1] = (byte)(word >> 8);

                                 
                                }
                            }

                            Log.Write(LogComponent.DoverROS, "Read bands {0}", _readBands);
                        }
                        else
                        {
                            // Nothing right now
                            Log.Write(LogComponent.DoverROS, "No bands available from Orbit.");
                        }

                        _readBands += 16;

                        if (_readBands > 2500 && !_countH)
                        {
                            _countH = true;
                            Log.Write(LogComponent.DoverROS, "Enabling CountH");
                        }
                    }

                    // time for one band of 16 scanlines to be read (approx.)
                    _innerLoopEvent.TimestampNsec = (ulong)(0.2 * Conversion.MsecToNsec);
                    _system.Scheduler.Schedule(_innerLoopEvent);
                    break;

                case InnerLoopState.CountH:
                    _countH = false;

                    if (_keepGoing)
                    {
                        // PrintRequest during ColdStart -- move to Inner loop
                        _keepGoing = false;
                        _state = PrintState.InnerLoop;
                        _innerLoopState = InnerLoopState.CS5;
                        Log.Write(LogComponent.DoverROS, "PrintRequest during ColdStart -- moving to inner loop.");
                    }
                    else
                    {
                        _innerLoopState = InnerLoopState.Idle;
                        Log.Write(LogComponent.DoverROS, "No PrintRequest during ColdStart -- idling.");
                    }

                   
                    break;

                case InnerLoopState.ColdStartEnd:
                    if (_keepGoing)
                    {
                        //
                        // Got a PrintRequest during cold start, start the inner loop.
                        //
                        _keepGoing = false;
                        _state = PrintState.InnerLoop;
                        _innerLoopState = InnerLoopState.CS5;
                        Log.Write(LogComponent.DoverROS, "Inner loop: CountH -- continuing");
                    }
                    else
                    {
                        //
                        // No Print Request; idle and shut down.
                        //
                        _innerLoopState = InnerLoopState.Idle;
                        Log.Write(LogComponent.DoverROS, "Inner loop: CountH -- idling");
                    }

                    // Debug: Put picture in image
                    /*
                    BitmapData data = _pageBuffer.LockBits(_pageRect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

                    IntPtr ptr = data.Scan0;
                    System.Runtime.InteropServices.Marshal.Copy(_pageData, 0, ptr, _pageData.Length);

                    _pageBuffer.UnlockBits(data);

                    EncoderParameters p = new EncoderParameters(1);
                    p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                    _pageBuffer.Save(String.Format("c:\\temp\\raster{0}.png", _rasterNum++), GetEncoderForFormat(ImageFormat.Png), p);
                    */

                    _innerLoopEvent.TimestampNsec = (ulong)(150 * Conversion.MsecToNsec);
                    _system.Scheduler.Schedule(_innerLoopEvent);

                    _innerLoopEvent.TimestampNsec = (ulong)(150 * Conversion.MsecToNsec);
                    _system.Scheduler.Schedule(_innerLoopEvent);
                    break;

                case InnerLoopState.Idle:
                    _countH = false;
                    _sendVideo = false;
                    _printMode = false;
                    _state = PrintState.ColdStart;
                    Log.Write(LogComponent.DoverROS, "Inner loop: Idle");
                    break;
            }
        }

        private void ColdStartCallback(ulong timestampNsec, ulong delta, object context)
        {
            switch (_coldStartState)
            {
                case ColdStartState.SendVideoLow:
                    _sendVideo = false;

                    // Keep SendVideo low for 125ms
                    _coldStartState = ColdStartState.SendVideoHigh;
                    _cs5Event.TimestampNsec = 125 * Conversion.MsecToNsec;
                    _system.Scheduler.Schedule(_cs5Event);

                    Log.Write(LogComponent.DoverROS, "Cold start: toggle SendVideo low");
                    break;

                case ColdStartState.SendVideoHigh:
                    _sendVideo = true;

                    _coldStartState = ColdStartState.Timeout;
                    _cs5Event.TimestampNsec = 800 * Conversion.MsecToNsec;
                    _system.Scheduler.Schedule(_cs5Event);

                    Log.Write(LogComponent.DoverROS, "Cold start: toggle SendVideo high");
                    break;

                case ColdStartState.Timeout:
                    if (_state == PrintState.ColdStart)
                    {
                        // Never moved from ColdStart, Alto never sent another PageRequest.
                        Log.Write(LogComponent.DoverROS, "Cold start timeout. Aborting.");
                        _sendVideo = false;
                        _printMode = false;
                    }
                    break;
            }
        }

        private ImageCodecInfo GetEncoderForFormat(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private enum PrintState
        {
            ColdStart = 0,
            InnerLoop,
            Runout
        }

        private enum ColdStartState
        {
            SendVideoLow,
            SendVideoHigh,
            Timeout
        }

        private enum InnerLoopState
        {
            Idle = 0,
            CS5,
            SendVideo,
            ReadBands,
            CountH,
            ColdStartEnd,
        }

        private PrintState _state;
        private InnerLoopState _innerLoopState;
        private ColdStartState _coldStartState;

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
        private ushort _id;
        private ushort _serialNumber;

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


        // Events to drive the print state machine
        //
        private Event _cs5Event;        
        private Event _innerLoopEvent;

        private byte[] _pageData = new byte[4096 * 512];
        private Rectangle _pageRect = new Rectangle(0, 0, 4096, 4096);
        private Bitmap _pageBuffer;
        private int _rasterNum;
    }
}
