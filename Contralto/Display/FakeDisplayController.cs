namespace Contralto.Display
{
    /// <summary>
    /// FakeDisplayController draws the display without the aid of the
    /// display microcode tasks (i.e. it cheats).  It reads the displaylist
    /// starting at DASTART and renders the display from there.    
    /// </summary>
    public class FakeDisplayController : IClockable
    {
        public FakeDisplayController(AltoSystem system)
        {
            _system = system;            
            Reset();
        }

        public void AttachDisplay(IAltoDisplay display)
        {
            _display = display;
        }

        public void Reset()
        {
            
        }

        public void Clock()
        {            
            _clocks++;
            
            if (_clocks > _frameClocks)
            {
                _clocks -= _frameClocks;

                RenderDisplay();
                _display.Render();
            }                    
        }

        private void RenderDisplay()
        {
            // pick up DASTART; if zero we render a blank screen.
            ushort daStart = _system.MemoryBus.DebugReadWord(0x110);
            
            if (daStart == 0)
            {
                for (int scanline = 0; scanline < 808; scanline++)
                {
                    for (int word = 0; word < 38; word++)
                    {
                        _display.DrawDisplayWord(scanline, word, 0xffff, false);
                    }
                }

                _display.Render();
                return;
            }            

            DCB dcb = GetNextDCB(daStart);
            int dcbScanline = 0;

            for (int scanline = 0; scanline < 808; scanline++)
            {
                int wordOffset = 0;                

                // fill in HTAB
                for(int htab = 0;htab<dcb.hTab; htab++)
                {
                    _display.DrawDisplayWord(scanline, wordOffset, (ushort)(dcb.whiteOnBlack ? 0x0 : 0xffff), false);
                    wordOffset++;
                }

                for(int word = 0;word<dcb.nWrds;word++)
                {
                    ushort address = (ushort)(dcb.startAddress + dcbScanline * dcb.nWrds + word);
                    ushort data = _system.MemoryBus.DebugReadWord(address);

                    if (!dcb.whiteOnBlack)
                    {
                        data = (ushort)~data;
                    }

                    _display.DrawDisplayWord(scanline, wordOffset, data, false);
                    wordOffset++;
                }

                // erase remainder of line, if any
                for (; wordOffset < 38; wordOffset++)
                {
                    _display.DrawDisplayWord(scanline, wordOffset, (ushort)(dcb.whiteOnBlack ? 0x0 : 0xffff), false);
                }

                _display.Render();

                // decrement scan line counter for this DCB, if < 0, grab next DCB.
                dcb.scanlineCount--;
                dcbScanline++;

                if (dcb.scanlineCount <= 0)
                {
                    if (dcb.daNext != 0)
                    {
                        dcb = GetNextDCB(dcb.daNext);
                        dcbScanline = 0;
                    }
                    else
                    {                        
                        return;
                    }
                }
            }

        }

        private DCB GetNextDCB(ushort address)
        {
            DCB dcb = new DCB();
            dcb.daNext = _system.MemoryBus.DebugReadWord(address);
            
            ushort mode = _system.MemoryBus.DebugReadWord((ushort)(address + 1));

            dcb.lowRes = (mode & 0x8000) != 0;
            dcb.whiteOnBlack = (mode & 0x4000) != 0;
            dcb.hTab = (mode & 0x3f00) >> 8;
            dcb.nWrds = (mode & 0xff);

            dcb.startAddress = _system.MemoryBus.DebugReadWord((ushort)(address + 2));
            dcb.scanlineCount = _system.MemoryBus.DebugReadWord((ushort)(address + 3)) * 2;

            return dcb;
        }


        private struct DCB
        {
            public ushort daNext;
            public bool lowRes;
            public bool whiteOnBlack;
            public int hTab;
            public int nWrds;
            public ushort startAddress;
            public int scanlineCount;
        }
        
        private double _clocks;
                
        private AltoSystem _system;
        private IAltoDisplay _display;

        // Timing constants
        // 38uS per scanline; 4uS for hblank.
        // ~35 scanlines for vblank (1330uS)
        private const double _scanlineClocks = 38.0 / 0.017;
        private const double _frameClocks = _scanlineClocks * 850;  // approx.        
    }
}
