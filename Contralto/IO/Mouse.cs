using Contralto.CPU;
using Contralto.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Contralto.IO
{
    [Flags]
    public enum AltoMouseButton
    {
        None = 0x0,
        Middle = 0x1,
        Right = 0x2,
        Left = 0x4,
    }
        
    public class Mouse : IMemoryMappedDevice
    {
        public Mouse()
        {
            _lock = new ReaderWriterLockSlim();
            Reset();
        }

        public void Reset()
        {
            // cheat for synchronization: this is approximately where the mouse is initialized to
            // at alto boot.
            _mouseY = 80;
            _mouseX = 0;
        }

        public ushort Read(int address, TaskType task, bool extendedMemoryReference)
        {
            return (ushort)(~_buttons);
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemoryReference)
        {
            // nothing
        }

        public void MouseMove(int x, int y)
        {
            _lock.EnterWriteLock();
            _destX = x;
            _destY = y;

            // Calculate number of steps in x and y to be decremented every call to PollMouseBits
            _xSteps = Math.Abs(_destX - _mouseX);
            _xDir = Math.Sign(_destX - _mouseX);

            _ySteps = Math.Abs(_destY - _mouseY);
            _yDir = Math.Sign(_destY - _mouseY);

            //Console.WriteLine("Mouse move from ({0},{1}) to ({2},{3}).", _mouseX, _mouseY, _destX, _destY);
            _lock.ExitWriteLock();
        }

        public void MouseDown(AltoMouseButton button)
        {
            _buttons |= button;
        }

        public void MouseUp(AltoMouseButton button)
        {
            _buttons ^= button;
        }

        /// <summary>
        /// Gets the bits read by the "<-MOUSE" special function, and moves
        /// the pointer one step closer to its final destination (if it has moved at all).
        /// </summary>
        /// <returns></returns>
        public ushort PollMouseBits()
        {
            //
            // The bits returned correspond to the delta incurred by mouse movement in the X and Y direction
            // and map to:
            // 0 : no change in X or Y
            // 1 : dy = -1
            // 2 : dy = 1
            // 3 : dx = -1
            // 4 : dy = -1, dx = -1
            // 5 : dy = 1, dx = -1
            // 6 : dx = 1
            // 7 : dy = -1, dx = 1
            // 8 : dy = 1, dx =1
            ushort bits = 0;

            _lock.EnterReadLock();
            // TODO: optimize this
            if (_yDir == -1 && _xDir == 0)
            {
                bits = 1;
            }
            else if (_yDir == 1 && _xDir == 0)
            {
                bits = 2;
            }
            else if (_yDir == 0 && _xDir == -1)
            {
                bits = 3;
            }
            else if (_yDir == -1 && _xDir == -1)
            {
                bits = 4;
            }
            else if (_yDir == 1 && _xDir == -1)
            {
                bits = 5;
            }
            else if (_yDir == 0 && _xDir == 1)
            {
                bits = 6;
            }
            else if (_yDir == -1 && _xDir == 1)
            {
                bits = 7;
            }
            else if (_yDir == 1 && _xDir == 1)
            {
                bits = 8;
            }

            //Console.WriteLine("Mouse poll: xdir {0}, ydir {1}, bits {2}", _xDir, _yDir, Conversion.ToOctal(bits));

            // Move the mouse closer to its destination
            if (_xSteps > 0)
            {
                _mouseX += _xDir;
                _xSteps--;

                if (_xSteps == 0)
                {
                    _xDir = 0;
                }
            }

            if (_ySteps > 0)
            {
                _mouseY += _yDir;
                _ySteps--;

                if (_ySteps == 0)
                {
                    _yDir = 0;
                }
            }
            _lock.ExitReadLock();

            return bits;
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfe18, 0xfe1b), // UTILIN: 177030-177033
        };

        // Mouse buttons:
        AltoMouseButton _buttons;

        /// <summary>
        /// Where the mouse is currently reported to be
        /// </summary>
        private int _mouseX;
        private int _mouseY;

        /// <summary>
        /// Where the mouse is moving to every time PollMouseBits is called.
        /// </summary>
        private int _destX;
        private int _destY;

        private int _xSteps;
        private int _xDir;
        private double _ySteps;
        private int _yDir;

        private ReaderWriterLockSlim _lock;
    }
}

