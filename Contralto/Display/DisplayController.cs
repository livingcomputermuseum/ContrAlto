using System.Collections.Generic;
using Contralto.CPU;

namespace Contralto.Display
{
    /// <summary>
    /// DisplayController implements hardware controlling the virtual electron beam
    /// as it scans across the screen.  It implements the logic of the display's sync generator
    /// and wakes up the DVT and DHT tasks as necessary during a display field.
    /// </summary>
    public class DisplayController
    {
        public DisplayController(AltoSystem system)
        {
            _system = system;            
            Reset();
        }

        public void AttachDisplay(IAltoDisplay display)
        {
            _display = display;
        }

        public void DetachDisplay()
        {
            _display = null;
        }

        public int Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        public bool DWTBLOCK
        {
            get { return _dwtBlocked; }
            set
            {
                _dwtBlocked = value;
                CheckWordWakeup();
            }
        }

        public bool DHTBLOCK
        {
            get { return _dhtBlocked; }
            set
            {
                _dhtBlocked = value;
                CheckWordWakeup();
            }
        }

        public bool FIFOFULL
        {
            get
            {
                return _dataBuffer.Count >= 15;
            }
        }

        public void Reset()
        {
            _evenField = false;           
            _scanline = 0;
            _word = 0;
            _dwtBlocked = true;
            _dhtBlocked = false;
            _dataBuffer.Clear();

            if (_system.CPU != null)
            {
                CheckWordWakeup();
            }

            _whiteOnBlack = _whiteOnBlackLatch = false;            
            _lowRes = _lowResLatch = false;
            _swModeLatch = false;

            _cursorReg = 0;
            _cursorX = 0;
            _cursorRegLatch = false;
            _cursorXLatch = false;

            _verticalBlankScanlineWakeup = new Event(_verticalBlankDuration, null, VerticalBlankScanlineCallback);
            _horizontalWakeup = new Event(_horizontalBlankDuration, null, HorizontalBlankEndCallback);
            _wordWakeup = new Event(_wordDuration, null, WordCallback);

            // Kick things off
            _system.Scheduler.Schedule(_verticalBlankScanlineWakeup);
        }

        private void FieldStart()
        {
            // Start of Vertical Blanking (end of last field).  This lasts for 34 scanline times or so.
            _evenField = !_evenField;

            // Wakeup DVT
            _system.CPU.WakeupTask(TaskType.DisplayVertical);

            // Block DHT, DWT
            _system.CPU.BlockTask(TaskType.DisplayHorizontal);
            _system.CPU.BlockTask(TaskType.DisplayWord);            

            _fields++;

            _scanline = _evenField ? 0 : 1;

            _vblankScanlineCount = 0;
            

            _dataBuffer.Clear();

            // Schedule wakeup for first scanline of vblank
            _verticalBlankScanlineWakeup.TimestampNsec = _verticalBlankScanlineDuration;
            _system.Scheduler.Schedule(_verticalBlankScanlineWakeup);            
        }

        private void VerticalBlankScanlineCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            // End of VBlank scanline.         
            _vblankScanlineCount++;

            // Run MRT
            _system.CPU.WakeupTask(TaskType.MemoryRefresh);

            // Run Ethernet if a countdown wakeup is in progress
            if (_system.EthernetController.CountdownWakeup)
            {                
                _system.CPU.WakeupTask(TaskType.Ethernet);
            }

            if (_vblankScanlineCount > (_evenField ? 33 : 34))
            {
                // End of vblank:
                // Wake up DHT
                _system.CPU.WakeupTask(TaskType.DisplayHorizontal);

                _dataBuffer.Clear();

                DWTBLOCK = false;
                DHTBLOCK = false;              

                // Run CURT
                _system.CPU.WakeupTask(TaskType.Cursor);

                // Schedule HBlank wakeup for end of first HBlank            
                _horizontalWakeup.TimestampNsec = _horizontalBlankDuration - skewNsec;
                _system.Scheduler.Schedule(_horizontalWakeup);                
            }
            else
            {                
                // Do the next vblank scanline
                _verticalBlankScanlineWakeup.TimestampNsec = _verticalBlankScanlineDuration;
                _system.Scheduler.Schedule(_verticalBlankScanlineWakeup);
            }
        }        

        private void HorizontalBlankEndCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            // Reset scanline word counter
            _word = 0;

            // Deal with cursor latches for this scanline
            if (_cursorRegLatch)
            {
                _cursorRegLatched = _cursorReg;
                _cursorRegLatch = false;
            }

            if (_cursorXLatch)
            {
                _cursorXLatched = _cursorX;
                _cursorXLatch = false;
            }

            // Schedule wakeup for first word on this scanline
            // TODO: the delay below is chosen to reduce flicker on first scanline;
            // investigate.
            _wordWakeup.TimestampNsec = _lowRes ? 0 : _wordDuration * 3;
            _system.Scheduler.Schedule(_wordWakeup);
        }

        private void WordCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            if (_display == null)
            {
                return;
            }

            // Dequeue a word (if available) and draw it to the screen.
            ushort displayWord = (ushort)(_whiteOnBlack ? 0 : 0xffff);
            if (_dataBuffer.Count > 0)
            {                
                displayWord = _whiteOnBlack ? _dataBuffer.Dequeue() : (ushort)~_dataBuffer.Dequeue();
                CheckWordWakeup();
            }

            _display.DrawDisplayWord(_scanline, _word, displayWord, _lowRes);

            // Merge in cursor word:
            // Calculate X offset of current word
            int xOffset = _word * (_lowRes ? 32 : 16);

            _word++;

            if (_word >= (_lowRes ? _scanlineWords / 2 : _scanlineWords))
            {
                // End of scanline.
                // Move to next.    

                // Draw cursor for this scanline first    
                if (_cursorXLatched < 606)
                {
                    _display.DrawCursorWord(_scanline, _cursorXLatched, _whiteOnBlack, _cursorRegLatched);
                }

                _scanline += 2;                

                if (_scanline >= 808)
                {
                    // Done with field.                    

                    // Draw the completed field to the emulated display.
                    _display.Render();

                    // And start over
                    FieldStart();
                }
                else
                {
                    // More scanlines to do.      
                                                     
                    // Run CURT and MRT at end of scanline
                    _system.CPU.WakeupTask(TaskType.Cursor);                    
                    _system.CPU.WakeupTask(TaskType.MemoryRefresh);

                    // Schedule HBlank wakeup for end of next HBlank                    
                    _horizontalWakeup.TimestampNsec = _horizontalBlankDuration - skewNsec;
                    _system.Scheduler.Schedule(_horizontalWakeup);
                    DWTBLOCK = false;
                    _dataBuffer.Clear();

                    // Deal with SWMODE latches for the scanline we're about to draw                    
                    if (_swModeLatch)
                    {
                        _lowRes = _lowResLatch;
                        _whiteOnBlack = _whiteOnBlackLatch;
                        _swModeLatch = false;
                    }

                }
            }
            else
            {
                // More words to do.
                // Schedule wakeup for next word
                if (_lowRes)
                {
                    _wordWakeup.TimestampNsec = _wordDuration * 2 - skewNsec;
                }
                else
                {
                    _wordWakeup.TimestampNsec = _wordDuration - skewNsec;
                }
                _system.Scheduler.Schedule(_wordWakeup);
            }
        }

        private void CheckWordWakeup()
        {
            if (FIFOFULL ||
                DHTBLOCK ||
                DWTBLOCK)
            {
                // If the fifo is full or either the horizontal or word tasks have blocked,
                // the word task must be blocked.
                _system.CPU.BlockTask(TaskType.DisplayWord);
            }
            else if (!FIFOFULL &&
                !DHTBLOCK &&
                !DWTBLOCK)
            {
                //
                // "If the DWT has not executed a BLOCK, if DHT is not blocked, and if the
                //  buffer is not full, DWT wakeups are generated."
                //
                _system.CPU.WakeupTask(TaskType.DisplayWord);
            }
        }
       
        public void LoadDDR(ushort word)
        {        
            _dataBuffer.Enqueue(word);            
            
            // Sanity check: data length should never exceed 16 words.            
            if (_dataBuffer.Count > 16)
            {                
                _dataBuffer.Dequeue();                
            }

            CheckWordWakeup();
        }

        public void LoadXPREG(ushort word)
        {
            if (!_cursorXLatch)
            {
                _cursorXLatch = true;
                _cursorX = (ushort)(~word);         
            }
        }

        public void LoadCSR(ushort word)
        {            
            if (!_cursorRegLatch)
            {
                _cursorRegLatch = true;
                _cursorReg = (ushort)word;               
            }                        
        }

        public void SETMODE(ushort word)
        {
            // These take effect at the beginning of the next scanline.            
            _lowResLatch = (word & 0x8000) != 0;
            _whiteOnBlackLatch = (word & 0x4000) != 0;
            _swModeLatch = true;            
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

        // MODE data
        private bool _evenField;
        private bool _lowRes;
        private bool _lowResLatch;
        private bool _whiteOnBlack;
        private bool _whiteOnBlackLatch;
        private bool _swModeLatch;

        // Cursor data
        private bool _cursorRegLatch;
        private ushort _cursorReg;
        private ushort _cursorRegLatched;
        private bool _cursorXLatch;
        private ushort _cursorX;
        private ushort _cursorXLatched;

        // Indicates whether the DWT or DHT blocked itself
        // in which case they cannot be reawakened until the next field.
        private bool _dwtBlocked;
        private bool _dhtBlocked;        

        private int _scanline;
        private int _word;
        private const int _scanlineWords = 38;

        private Queue<ushort> _dataBuffer = new Queue<ushort>(16);

        private AltoSystem _system;
        private IAltoDisplay _display;

        private int _fields;

        // Timing constants
        // 38uS per scanline; 6uS for hblank.
        // ~35 scanlines for vblank (1330uS)
        private const double _scale = 1.0;        
        private const ulong _verticalBlankDuration = (ulong)(665000.0 * _scale);              // 665uS
        private const ulong _verticalBlankScanlineDuration = (ulong)(38080.0 * _scale);       // 38uS
        private const ulong _horizontalBlankDuration = (ulong)(6084.0 * _scale);              // 6uS
        private const ulong _wordDuration = (ulong)(842.0 * _scale);                          // 32/38uS

        private int _vblankScanlineCount;
        
        //
        // Scheduler events
        //        
        private Event _verticalBlankScanlineWakeup;
        private Event _horizontalWakeup;
        private Event _wordWakeup;
    }
}
