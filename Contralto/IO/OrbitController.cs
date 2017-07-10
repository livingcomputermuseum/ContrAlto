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

using Contralto.Logging;

namespace Contralto.IO
{
    /// <summary>
    /// Implements the Orbit controller -- hardware for generating
    /// rasters to be sent to a ROS (Raster Output Scanner) device
    /// such as a laser printer.
    /// </summary>
    public class OrbitController
    {
        public OrbitController(AltoSystem system)
        {
            _system = system;
            _ros = new DoverROS(system);
            _refreshEvent = new Event(_refreshInterval, null, RefreshCallback);           

            Reset();            
        }

        public bool RefreshTimerExpired
        {
            get { return _refresh; }
        }

        public bool IACS
        {
            get { return _iacs; }
        }

        public bool SLOTTAKE
        {
            get { return _slottake; }
        }

        public int FA
        {
            get { return _fa; }
        }

        public void Reset()
        {
            //
            // Reset to power-up (or reboot) defaults.
            //
            _fa = 0;
            _slottake = false;

            _image = _a;
            _output = _b;
            _incon = false;

            _outputY = 0;
            _outputX = 0;

            _run = false;
            _iacs = false;

            _refresh = false;
            _goAway = false;
            _behind = false;

            _stableROS = true;
            _badROS = false;            

            Log.Write(LogComponent.Orbit, "Orbit system reset.");
            UpdateWakeup();
        }

        private void OrbitReset()
        {
            //
            // A Reset performs at least the following functions:
            // FA <- 0, SLOTTAKE <- 0, band buffer A is assigned
            // to the image buffer, the status and control dialogs
            // with the adapter are reset.
            //
            _fa = 0;
            _slottake = false;

            _image = _a;
            _output = _b;
            _incon = false;

            _outputY = 0;
            _outputX = 0;
            
            // Cleared "by reseting [sic] Orbit"
            _iacs = false;

            // Cleared by a buffer switch or an Orbit reset
            _goAway = false;

            _refresh = false;

            _behind = false;

            _stableROS = true;
            _badROS = false;

            Log.Write(LogComponent.Orbit, "Orbit reset.");
            UpdateWakeup();
        }

        public bool Wakeup
        {
            get
            {
                return _run &&
                    ((_refresh || !_goAway) &&
                     (true || !_iacs));     // tautology -- the 'true' is "FIFO ready" and since I don't implement the FIFO (yet...)
            }
        }

        private void UpdateWakeup()
        {
            if (_system.CPU == null)
                return;

            if (Wakeup)
            {
                _system.CPU.WakeupTask(CPU.TaskType.Orbit);
                Log.Write(LogComponent.Orbit, "Orbit wakeup.");
            }
            else
            {
                _system.CPU.BlockTask(CPU.TaskType.Orbit);
                Log.Write(LogComponent.Orbit, "Orbit block.");
            }
        }

        public void STARTF(ushort value)
        {
            // Kick off the refresh timer if it's not already pending

            // Wake up task, etc.
            _run = true;

            // "IACS is cleared by StartIO(4)"
            _iacs = false;

            // per microcode, GOAWAY is cleared
            _goAway = false;

            if (!_refreshRunning)
            {
                _refreshRunning = true;
                _refreshEvent.TimestampNsec = _refreshInterval;
                _system.Scheduler.Schedule(_refreshEvent);
            }

            Log.Write(LogComponent.Orbit, "Orbit started.");

            UpdateWakeup();
        }

        public void Stop()
        {
            _run = false;

            Log.Write(LogComponent.Orbit, "Orbit stopped.");
            UpdateWakeup();
        }

        public void Control(ushort value)
        {
            //
            // This function transfers 16 bits of control information to Orbit.
            //

            //
            // This 8-bit field has two different interpretations, depending on
            // the setting of the WHICH field.
            //
            int auxControl = (value & 0xff00) >> 8;

            //
            // This bit, if 1, will reset Orbit entirely.
            //
            if ((value & 0x1) != 0)
            {
                OrbitReset();
            }

            //
            // This bit controls refresh logic, and should normally be 0.
            //
            if ((value & 0x2) != 0)
            {
                _refresh = false;
            }

            //
            // This bit controls the use to which the 8-bit auxControl field is
            // put.  If WHICH=0, auxControl is interpreted as an address (range 0
            // to 63) into the adapter status memory:  when OrbitStatus is next
            // interrogated, 4 status bits (with numbers 4*auxControl to 
            // 4 *auxControl + 3 will be reported.  If WHICH=1, auxControl is used
            // to set FA.
            //
            if ((value & 0x4) != 0)
            {
                _fa = auxControl;

                // "The setting of FA provided by the Alto is examined only when
                // Orbit reads out the last 16 bits of a scanline..."
                // (So we should leave _outputY alone here).
            }
            else
            {
                _statusAddress = auxControl & 0x3f;
            }

            //
            // This bit controls microcode wakeups, and should normally be 0.
            //
            if ((value & 0x8) != 0)
            {
                _goAway = true;
            }

            //
            // This bit clears the BEHIND indicator.
            //
            if ((value & 0x10) != 0)
            {
                _behind = false;
            }

            //
            // ESS - This bit must be 1 to enable changing the SLOTTAKE setting.
            // SLOTTAKE - This bit setting, enabled by ESS above, controls the 
            //  output buffer logic.  Normally (SLOTTAKE=0), Orbit will not honor
            //  video data requests coming from the adapter.  As soon as SLOTTAKE
            //  is set to 1, however, output data will be passed to the adapter when
            //  it demands it.
            //
            if ((value & 0xc0) == 0xc0)
            {
                _slottake = true;
            }            

            Log.Write(LogComponent.Orbit,
               "Set Control: aux {0}, reset {1} refresh {2} which {3} goaway {4} behind {5} slottake {6}",
                    auxControl, (value & 0x1) != 0, (value & 0x2) != 0, (value & 0x4) != 0, _goAway, _behind, _slottake);

            UpdateWakeup();
        }

        public void SetHeight(ushort value)
        {
            // 
            // This command sends to Orbit a 12-bit field (value[4-15]) which is
            // interpreted as the two's complement of the height of the source raster,
            // in bits.
            //
            _height = 4096 - (value & 0xfff);

            Log.Write(LogComponent.Orbit,
               "Set Height: {0}", _height);
        }

        public void SetXY(ushort value)
        {
            //
            // This commands sets x <- value[0-3] and y <- value[4-15].
            // It is therefore used to set the starting scan-line within
            // the band (x) and the vertical position of the bottom of the
            // copy of the source raster (y).
            _y = value & 0x0fff;
            _x = (value & 0xf000) >> 12;

            Log.Write(LogComponent.Orbit,
                "Set XY: X={0}, Y={1}", _x, _y);
        }

        public void SetDBCWidth(ushort value)
        {
            //
            // value[0-3] tells Orbit which bit (0-15) of the first
            // word of raster data is the first bit to be examined.
            //
            _deltaBC = _deltaBCEnd = (value & 0xf000) >> 12;

            //
            // value[4-15] is interpreted as the width of the source
            // raster, minus 1.
            ///
            _width = (value & 0x0fff);

            //
            // Third, executing this function initializes a mess of logic
            // relating to transferring a source raster to Orbit and sets
            // IACS ("in a character segment").  After the function is
            // executed it is wise to issue only OrbitFontData functions until
            // the image-generation for this character terminates (i.e. IACS
            // becomes 0).  IACS is cleared by StartIO(4).
            // Start a new character
            //
            _iacs = true;
            _firstWord = true;
            _bitCount = 0;
            _cx = _x;
            _cy = _y;

            Log.Write(LogComponent.Orbit,
               "Set DBCWidth: DeltaBC {0}, Width {1}", _deltaBC, _width);

            UpdateWakeup();
        }

        public void WriteFontData(ushort value)
        {
            //
            // This function is used to send 16-bit raster data words to Orbit,
            // usually in a very tight loop.  The loop is exited when IACS becomes
            // 0, that is, when the Orbit hardware counts the width counter down to
            // zero, or when the x counter overflows (i.e. when it would count up
            // from 15 to 16).
            //

            if (!_iacs)
            {
                Log.Write(LogType.Error, 
                    LogComponent.Orbit, "Unexpected OrbitFontData while IACS false.");
                return;
            }

            Log.Write(LogComponent.Orbit,
                "Font Data: {0}", Conversion.ToOctal(value));

            //
            // We insert the word one bit at a time; this is more costly
            // computationally but it's a ton simpler than dealing with word
            // and scanline boundaries and merging entire words at a time.
            //
            int startBit = 15;
            if (_firstWord)
            {
                startBit = 15 - _deltaBC;
                _firstWord = false;
            }
            
            for(int i = startBit; i >=0 ;i--)
            {
                int bitValue = (value & (0x1 << i)) == 0 ? 0 : 1;

                if (bitValue != 0 && _cy < 4096)        // clip to end of band
                {
                    SetImageRasterBit(_cx, _cy);
                }

                _cy++;
                _bitCount++;
                _deltaBCEnd++;

                if (_cy > _y + _height - 1)
                {
                    _cy = _y;

                    _cx++;
                    _width--;

                    if (_cx > 15 || _width < 0)
                    {
                        _iacs = false;

                        //
                        // A height of 1024 is used by the refresh microcode to refresh the Orbit memory.
                        // We log this separately to make debugging more clear.
                        //
                        if (_height == 1024)
                        {
                            Log.Write(LogComponent.Orbit, "Image band completed (refresh).");
                        }
                        else
                        {
                            Log.Write(LogComponent.Orbit, "Image band completed.");
                        }

                        UpdateWakeup();
                        break;
                    }
                }
            }
        }

        public void WriteInkData(ushort value)
        {
            //
            // This function sets 16 bits of INK memory: INK[x,0] through INK[x,15],
            // where x has been previously set with OrbitXY.
            //
            _ink[_x] = value;

            Log.Write(LogComponent.Orbit,
               "Ink Data[{0}]: {1}", _x, Conversion.ToOctal(value));
        }

        public void SendROSCommand(ushort value)
        {
            //
            // This function sends a 16-bit command to the ROS.
            // Another ROS command should not be issued until 60 microseconds
            // have elapsed, to allow time for proper transmission of the command
            // to the ROS.
            //
            _ros.RecvCommand(value);

            Log.Write(LogComponent.Orbit,
               "ROS command {0}", Conversion.ToOctal(value));
        }

        public ushort GetOutputDataAlto()
        {

            if (_slottake)
            {
                Log.Write(LogType.Error, LogComponent.Orbit,
                    "Unexpected OrbitOutputData to Alto with SLOTTAKE set.");
            }

            return GetOutputData();
        }

        public ushort GetOutputDataROS()
        {
            if (!_slottake)
            {
                Log.Write(LogType.Error, LogComponent.Orbit,
                    "Unexpected OrbitOutputData to ROS without SLOTTAKE set.");
            }

            return GetOutputData();
        }

        public ushort GetDeltaWC()
        {
            Log.Write(LogComponent.Orbit,
                "Delta WC {0} ({1} bits)", (_bitCount >> 4), _bitCount);

            return (ushort)(_bitCount >> 4);
        }

        public ushort GetDBCWidth()
        {
            Log.Write(LogComponent.Orbit,
                "Delta DBCWidth {0},{1}", _deltaBCEnd % 16, _width);           

            return (ushort)((_deltaBCEnd << 12) | (_width & 0xfff));
        }

        public ushort GetOrbitStatus()
        {
            int result = _ros.ReadStatus(_statusAddress);

            if (_behind)
            {
                result |= 0x10;
            }

            if (_stableROS)
            {
                result |= 0x20;
            }

            if (_incon)
            {
                result |= 0x40;
            }

            if (_badROS)
            {
                result |= 0x80;
            }

            Log.Write(LogComponent.Orbit,
                "OrbitStatus, address {0}, {1}", _statusAddress, Conversion.ToOctal(result));

            return (ushort)result;
        }

        private ushort GetOutputData()
        {
            //
            // "Basically, the function OrbitOutputData reads 16 bits from the output buffer
            // into the Alto.  
            //
            //  The programmer (or emulator author) needs to be aware of two tricky details
            //  concerning readout:
            //  1.  There is a 16-bit buffer (ROB) that sits between the addressing mechanism
            //      and the outside world (the taker of data: the Alto or the ROS adapter).  As
            //      a result, immediately after the buffers "switch" the ROB contains the last
            //      16-bits of data from the previous buffer (x=15, y=4080 through y=4095). 
            //      The usual convention is to read out this one last word.  This leaves the first
            //      word of the new buffer in ROB.
            //
            //  2. The output band buffer will not be refreshed ... [not a concern here]"
            //

            // Return the last value from ROB
            ushort value = _rob;

            // Update ROB
            _rob = _output[_outputX, _outputY];

            // Clear read word
            _output[_outputX, _outputY] = 0;

            //
            // Move to next word
            //
            _outputY++;

            if (_outputY == 256)
            {
                Log.Write(LogComponent.Orbit,
                    "OutputData - scanline {0} completed", _outputX);

                _outputY = _fa;
                _outputX++;

                if (_outputX == 16)
                {
                    // Done reading output band -- swap buffers!

                    Log.Write(LogComponent.Orbit,
                        "OutputData - buffer read completed.");

                    SwapBuffers();
                    _outputX = 0;
                }
            }

            Log.Write(LogComponent.Orbit,
                "OutputData {0}", Conversion.ToOctal(value));

            return value;
        }

        private void SwapBuffers()
        {
            ushort[,] _temp = _image;
            _image = _output;
            _output = _temp;

            _outputY = _fa;
            _outputX = 0;

            //
            // If input is still being written,
            // the Alto is now behind the consumer...
            //
            if (!_goAway)
            {                
                _behind = true;
            }
            else
            {
                // "The buffer switch will turn off GOAWAY and consequently
                //  allow wakeups again."
                _goAway = false;             
                UpdateWakeup();
            }

            // Flip buffer bit
            _incon = !_incon;
        }

        private void SetImageRasterBit(int x, int y)
        {
            // Pick out the word we're interested in
            int wordAddress = y >> 4;
            ushort inputWord = _image[x, wordAddress];

            // grab the ink bit and OR it in.
            int inkBit = (_ink[x] & (0x8000 >> (y % 16)));

            _image[x, wordAddress] = (ushort)(inputWord | inkBit);
        }

        private void RefreshCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            _refresh = true;

            Log.Write(LogComponent.Orbit, "Refresh signal raised.");

            if (_run)
            {
                // do it again
                _refreshEvent.TimestampNsec = _refreshInterval;
                _system.Scheduler.Schedule(_refreshEvent);
            }
            else
            {
                Log.Write(LogComponent.Orbit, "RUN deasserted, Refresh aborted.");
                _refreshRunning = false;
                _refresh = false;
            }

            UpdateWakeup();
        }

        // Run status
        private bool _run;

        // Raster information
        private int _height;    // height (in bits)
        private int _x;         // scanline
        private int _y;         // bit position in scanline
        private int _width;     // raster width of character
        private int _bitCount;  // count of processed bits; used to calculate DeltaWC.
        private int _deltaBC;   // bit of first word to start with
        private int _deltaBCEnd; // number of bits left in last word rasterized        

        // FA -- "First Address" of each scanline -- a value between
        // 0 and 255 indicating the starting word to be read by the
        // output device (either the Alto or the ROS).
        private int _fa;

        // SLOTTAKE - indicates that the ROS takes data (not the Alto)
        private bool _slottake;

        // Address to pull ROS status bits from
        private int _statusAddress;

        // Signals completion of input from Alto
        private bool _goAway;

        // Whether the Alto has fallen behind the ROS's consumption
        private bool _behind;

        // Current raster position
        private bool _firstWord;
        private int _cx;
        private int _cy;

        //
        // IACS ("in a character segment") is set by OrbitDBCWidthSet
        // and remains set until:
        //   - StartIO(4) is invoked
        //   - The Width counter counts down to zero
        //   - The current band is completed (the x counter overflows to 16)
        //
        private bool _iacs;

        //
        // Refresh timer expiry
        //
        private bool _refresh;

        //
        // Output band position and data
        //
        private int _outputX;
        private int _outputY;       // in words, starting at FA
        private ushort _rob;        // Read Output Buffer

        //
        // Buffer status -- if 0, A is the image buffer, B is the
        // output buffer.  If 1, these are swapped.
        //
        private bool _incon;

        // 
        // ROS status
        //
        private bool _badROS;
        private bool _stableROS;

        // Raster bands
        private ushort[,] _a = new ushort[16, 256]; // 16x4096 bits
        private ushort[,] _b = new ushort[16, 256];

        // Ink data
        private ushort[] _ink = new ushort[16];

        // buffer references that are swapped
        private ushort[,] _image;
        private ushort[,] _output;

        //
        // Refresh event and interval
        private bool _refreshRunning;
        private Event _refreshEvent;
        private readonly ulong _refreshInterval = 2 * Conversion.MsecToNsec;   // 2ms in nsec

        //
        // The ROS we talk to to get raster onto a page.
        //
        private DoverROS _ros;

        private AltoSystem _system;
    }
}
