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

using Contralto.CPU;
using Contralto.Memory;
using System;
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

    [Flags]
    public enum AltoKeysetKey
    {
        None =    0x00,
        Keyset0 = 0x80,     // left-most (bit 8)
        Keyset1 = 0x40,
        Keyset2 = 0x20,
        Keyset3 = 0x10,
        Keyset4 = 0x08,     // right-most (bit 12)
    }

    /// <summary>
    /// Implements the hardware for the standard Alto mouse
    /// and the Keyset, because both share the same memory-mapped
    /// address.  When the Diablo printer is finally emulated,
    /// I'll have to revisit this scheme because it ALSO shares
    /// the same address and that's just silly.
    /// </summary>
    public class MouseAndKeyset : IMemoryMappedDevice
    {
        public MouseAndKeyset()
        {
            _lock = new ReaderWriterLockSlim();
            Reset();
        }

        public void Reset()
        {
            _keyset = 0;
            _buttons = AltoMouseButton.None;
        }

        public ushort Read(int address, TaskType task, bool extendedMemoryReference)
        {
            return (ushort)~((int)_buttons | (int)_keyset);
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemoryReference)
        {
            // nothing
        }

        public void MouseMove(int dx, int dy)
        {
            _lock.EnterWriteLock();
            
            // Calculate number of steps in x and y to be decremented every call to PollMouseBits
            _xSteps = Math.Abs(dx);
            _xDir = Math.Sign(dx);

            _ySteps = Math.Abs(dy);
            _yDir = Math.Sign(dy);
            
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

        public void KeysetDown(AltoKeysetKey key)
        {
            _keyset |= key;
        }

        public void KeysetUp(AltoKeysetKey key)
        {
            _keyset ^= key;
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

        // Keyset switches:
        AltoKeysetKey _keyset;

        /// <summary>
        /// Where the mouse is currently reported to be
        /// </summary>
        private int _mouseX;
        private int _mouseY;

        /// <summary>
        /// Where the mouse is moving to every time PollMouseBits is called.
        /// </summary>       
        private int _xSteps;
        private int _xDir;
        private double _ySteps;
        private int _yDir;

        private ReaderWriterLockSlim _lock;
    }
}

