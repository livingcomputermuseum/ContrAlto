using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

namespace Contralto.IO
{
    public class DiskController
    {
        public DiskController(AltoSystem system)
        {
            _system = system;
        }        

        public ushort KDATA
        {
            get { return _kData; }
            set { _kData = value; }
        }

        public ushort KADR
        {
            get { return _kAdr; }
            set
            {
                _kAdr = value;
                _recNo = 0;

                // "In addition, it causes the head address bit to be loaded from KDATA[13]."
                _head = (_kData & 0x4) >> 2;
            }
        }

        public ushort KCOM
        {
            get { return _kCom; }
            set
            {
                _kCom = value;

                //
            }
        }

        public ushort KSTAT
        {
            get { return _kStat; }
            set { _kStat = value; }
        }

        public ushort RECNO
        {
            get { return _recMap[_recNo];  }
        }

        public void Reset()
        {
            ClearStatus();
            _recNo = 0;
            _elapsedSectorTime = 0.0;
            _cylinder = 0;
            _sector = 0;
            _head = 0;
            _kStat = 0;
        }

        public void Clock()
        {
            _elapsedSectorTime++;

            // TODO: only signal sector changes if disk is loaded, etc.
            if (_elapsedSectorTime > _sectorClocks)
            {
                //
                // Next sector; save fractional part of elapsed time (to more accurately keep track of time), move to next sector
                // and wake up sector task.
                //
                _elapsedSectorTime -= _sectorClocks;

                _sector = (_sector + 1) % 12;

                _kStat = (ushort)((_kStat & 0x0fff) | (_sector << 12));

                _system.CPU.WakeupTask(CPU.TaskType.DiskSector);
            }

            // If seek is in progress, move closer to the desired cylinder...
            // TODO: move bitfields to enums / constants, this is getting silly.
            if ((_kStat & 0x0040) != 0)
            {
                _elapsedSeekTime++;
                if (_elapsedSeekTime > _seekClocks)
                {
                    _elapsedSectorTime -= _seekClocks;

                    if (_cylinder < _destCylinder)
                    {
                        _cylinder++;
                    }
                    else
                    {
                        _cylinder--;
                    }

                    // Are we *there* yet?
                    if (_cylinder == _destCylinder)
                    {
                        // clear Seek bit
                        _kStat &= 0xffbf;
                    }
                }
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
            if ((_kCom & 0x1) != 1)
            {
                throw new InvalidOperationException("STROBE while SENDADR bit of KCOMM not 1.  Unexpected.");
            }
            
            _destCylinder = (_kData & 0x0ff8) >> 3;

            // set "seek fail" bit based on selected cylinder (if out of bounds) and do not
            // commence a seek if so.
            if (_destCylinder < 203)
            {
                _kStat |= 0x0080;
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

            return seekTimeMsec / AltoSystem.ClockInterval;
        }

        private ushort _kData;
        private ushort _kAdr;
        private ushort _kCom;
        private ushort _kStat;

        private int _recNo;
        private ushort[] _recMap =
        {
            0, 2, 3, 1
        };

        // Current disk position
        private int _cylinder;
        private int _destCylinder;
        private int _head;
        private int _sector;

        // Sector timing.  Based on table on pg. 43 of the Alto Hardware Manual
        private double _elapsedSectorTime;     // elapsed time in this sector (in clocks)
        private const double _sectorDuration = (40.0 / 12.0); // time in msec for one sector
        private readonly double _sectorClocks = _sectorDuration / AltoSystem.ClockInterval;     // number of clock cycles per sector time.

        // Cylinder seek timing.  Again, see the manual.
        // Timing varies based on how many cylinders are being traveled during a seek; see
        // CalculateSeekTime() for more.
        private double _elapsedSeekTime;
        private double _seekClocks;


        private AltoSystem _system;
    }
}
