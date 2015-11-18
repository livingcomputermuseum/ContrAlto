using System.Collections.Generic;
using Contralto.CPU;

namespace Contralto.Display
{
    /// <summary>
    /// DisplayController implements hardware controlling the virtual electron beam
    /// as it scans across the screen.  It implements the logic of the display's sync generator
    /// and wakes up the DVT and DHT tasks as necessary during a display field.
    /// </summary>
    public class DisplayController : IClockable
    {
        public DisplayController(AltoSystem system)
        {
            _system = system;            
            Reset();
        }

        public void AttachDisplay(Debugger display)
        {
            _display = display;
        }

        public int Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        public void Reset()
        {
            _evenField = true;
            _clocks = 0;
            _totalClocks = 0;
            _state = DisplayState.VerticalBlank;
            _scanline = 0;
            _word = 0;
            _dwtBlocked = true;
            _dhtBlocked = false;

            _whiteOnBlack = false;
            _lowRes = false;

            // Wakeup DVT
            _system.CPU.WakeupTask(TaskType.DisplayVertical);
            _system.CPU.BlockTask(TaskType.DisplayHorizontal);
            _system.CPU.BlockTask(TaskType.DisplayWord);
        }

        public void Clock()
        {

            //
            // Move the electron beam appropriately and wake up the display tasks
            // as necessary.  Render the display at the end of the field.
            //

            //
            // "The display control hardware also generates wakeup requests to the microprocessor
            //  tasking hardware.  The vertical task DVT is awakened once per field, at the beginning
            //  of vertical retrace.  The display horizontal task is awakened once at the beginning of
            //  each field, and thereafter whenever the display word task blocks.  DHT can block itself,
            //  in which case neither it nor the word task can be awakened until the start of the next
            //  field.  The wakeup request for the display word task (DWT) is controlled by the state
            //  of the 16 word buffer.  If DWT has not executed a BLOCK, if DHT is not blocked, and if
            //  the buffer is not full, DWT wakeups are generated. The hardware sets the buffer empty
            //  and clears the DWT block flip-flop at the beginning of horizontal retrace for every
            //  scan line."
            //

            //
            // Check the BLOCK status of the DWT and DHT tasks for the last executed uInstruction.
            // 

            if (_system.CPU.CurrentTask.Priority == (int)CPU.TaskType.DisplayHorizontal &&
                            _system.CPU.CurrentTask.BLOCK)
            {             
                _dhtBlocked = true;
            }            

            if (_system.CPU.CurrentTask.Priority == (int)CPU.TaskType.DisplayWord &&
                            _system.CPU.CurrentTask.BLOCK)
            {                
                _dwtBlocked = true;

                //
                // Wake up DHT if it has not blocked itself.
                //
                if (!_dhtBlocked)
                {
                    _system.CPU.WakeupTask(TaskType.DisplayHorizontal);
                }
            }
            
            _clocks++;
            _totalClocks++;

            //
            // "If the DWT has not executed a BLOCK, if DHT is not blocked, and if the
            //  buffer is not full, DWT wakeups are generated."
            //
            if (_dataBuffer.Count < 16 &&                
                !_dhtBlocked &&
                !_dwtBlocked)
            {                
                _system.CPU.WakeupTask(TaskType.DisplayWord);
            }                       

            switch (_state)
            {
                case DisplayState.VerticalBlank:
                    // Just killing time
                    if (_clocks > _verticalBlankClocks)
                    {
                        // End of VBlank, start new visible frame
                        _clocks -= _verticalBlankClocks;                        
                        _scanline = _evenField ? 0 : 1;
                        _word = 0;
                        _dataBuffer.Clear();

                        // Wake up DVT, DHT
                        _dwtBlocked = false;
                        _dhtBlocked = false;                        
                        _system.CPU.WakeupTask(TaskType.DisplayHorizontal);                                             
                        
                        _state = DisplayState.HorizontalBlank;                        
                    }
                    break;

                case DisplayState.VisibleScanline:                   
                    //
                    // A scanline is 38 words wide; we clock in each word
                    // from the buffer (if present).
                    // 
                    if (_clocks > _wordClocks)
                    {
                        _clocks -= _wordClocks;

                        ushort displayWord = (ushort)(_whiteOnBlack ? 0 : 0xffff);
                        if (_dataBuffer.Count > 0)
                        {
                            // Dequeue a word and draw it to the screen
                            displayWord = _whiteOnBlack ? _dataBuffer.Dequeue() : (ushort)~_dataBuffer.Dequeue();
                        }

                        _display.DrawDisplayWord(_scanline, _word, displayWord);

                        _word++;
                        if (_word > 37)
                        {
                            // Done with this line
                            _dwtBlocked = false;
                            _dataBuffer.Clear();
                            _state = DisplayState.HorizontalBlank;                            
                        }
                    }
                    break;

                case DisplayState.HorizontalBlank:
                    // Kill time until end of HBlank                   
                    if (_clocks > _horizontalBlankClocks)
                    {
                        _clocks -= _horizontalBlankClocks;

                        // Move to next scanline
                        _scanline += 2;
                        _word = 0;                                                                 

                        if (_scanline > 807)
                        {                            
                            // Done with field, move to vblank, tell display to render
                            _state = DisplayState.VerticalBlank;
                            _evenField = !_evenField;

                            // Wakeup DVT
                            _system.CPU.WakeupTask(TaskType.DisplayVertical);

                            // Block DHT, DWT
                            _system.CPU.BlockTask(TaskType.DisplayHorizontal);
                            _system.CPU.BlockTask(TaskType.DisplayWord);

                            _display.RefreshAltoDisplay();
                            
                            _fields++;
                            //Log.Write(LogComponent.Display, "Display field completed. {0} total clocks elapsed.", _totalClocks);
                            _totalClocks = 0;
                        }
                        else
                        {
                            _state = DisplayState.VisibleScanline;

                            // Run MRT
                            _system.CPU.WakeupTask(TaskType.MemoryRefresh);
                        }
                    }
                    break;
            }
        }

        public void LoadDDR(ushort word)
        {            
            _dataBuffer.Enqueue(word);

            //Console.WriteLine("Enqueue {0}, scanline {1}", word, _scanline);

            // Sanity check: data length should never exceed 16 words.            
            if (_dataBuffer.Count > 16)
            {                
                //_dataBuffer.Dequeue();
                //_system.CPU.BlockTask(TaskType.DisplayWord);
            } 
        }

        public void SETMODE(ushort word)
        {
            _lowRes = (word & 0x8000) != 0;
            _whiteOnBlack = (word & 0x4000) != 0;
        }

        public bool EVENFIELD
        {
            get { return _evenField; }
        }

        private enum DisplayState
        {
            Invalid = 0,
            VerticalBlank,
            VisibleScanline,
            HorizontalBlank,
        }

        private bool _evenField;
        private bool _lowRes;
        private bool _whiteOnBlack;
        private double _clocks;
        private double _totalClocks;
        private int _fields;
        private DisplayState _state;

        // Indicates whether the DWT or DHT blocked itself
        // in which case they cannot be reawakened until the next field.
        private bool _dwtBlocked;
        private bool _dhtBlocked;        

        private int _scanline;
        private int _word;

        private Queue<ushort> _dataBuffer = new Queue<ushort>(16);

        private AltoSystem _system;
        private Debugger _display;

        // Timing constants
        // 38uS per scanline; 4uS for hblank.
        // ~35 scanlines for vblank (1330uS)
        private const double _wordClocks = (34.0 / 38.0) / 0.060;        // uSec to clocks
        private const double _horizontalBlankClocks = 4.0 / 0.060;
        private const double _verticalBlankClocks = 1333.0 / 0.060;


    }
}
