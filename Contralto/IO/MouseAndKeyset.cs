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
using Contralto.Scripting;
using System;
using System.Collections.Generic;
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
        public MouseAndKeyset(AltoSystem system)
        {
            _system = system;
            _lock = new ReaderWriterLockSlim();            
            Reset();
        }

        public void Reset()
        {
            _keyset = 0;
            _buttons = AltoMouseButton.None;
            _moves = new Queue<MouseMovement>();
            _currentMove = null;
            _pollCounter = 0;
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
            // Calculate number of steps in x and y to be decremented every call to PollMouseBits
            MouseMovement nextMove = new MouseMovement(Math.Abs(dx), Math.Abs(dy), Math.Sign(dx), Math.Sign(dy));

            _lock.EnterWriteLock();
            
            _moves.Enqueue(nextMove);

            if (ScriptManager.IsRecording)
            {
                ScriptManager.Recorder.MouseMoveRelative(dx, dy);
            }

            _lock.ExitWriteLock();
        }

        public void MouseDown(AltoMouseButton button)
        {
            _buttons |= button;

            if (ScriptManager.IsRecording)
            {
                //
                // Record the absolute position of the mouse (as held in MOUSELOC in system memory).
                // All other mouse movements in the script will be recorded relative to this point.
                //
                //int x = _system.Memory.Read(0x114, CPU.TaskType.Ethernet, false);
                //int y = _system.Memory.Read(0x115, CPU.TaskType.Ethernet, false);
                //ScriptManager.Recorder.MouseMoveAbsolute(x, y);

                ScriptManager.Recorder.MouseDown(button);
            }
        }

        public void MouseUp(AltoMouseButton button)
        {
            _buttons ^= button;

            if (ScriptManager.IsRecording)
            {
                ScriptManager.Recorder.MouseUp(button);
            }
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

            if (_currentMove == null && _moves.Count > 0)
            {
                _currentMove = _moves.Dequeue();
            }

            //
            // <-MOUSE is invoked by the Memory Refresh Task once per scanline (including during vblank) which
            // works out to about 13,000 times a second.  To more realistically simulate the movement of a mouse
            // across a desk, we return actual mouse movement data only periodically.
            //
            if (_currentMove != null && (_pollCounter % _currentMove.PollRate) == 0)
            {

                //
                // Choose a direction.  We do not provide movements in both X and Y at the same time;
                // this is solely to avoid a microcode bug that causes erroneous movements in such cases
                // (which then plays havoc with scripting and absolute coordinates.)
                // (It is also the case that on the real hardware, such movements are extremely rare due to
                // the nature of the hardware involved).
                //
                int dx = _currentMove.DX;
                int dy = _currentMove.DY;

                if (dx != 0 && dy != 0)
                {
                    // Choose just one of the two directions to move in.
                    if (_currentDirection)
                    {
                        dx = 0;
                    }
                    else
                    {
                        dy = 0;
                    }

                    _currentDirection = !_currentDirection;
                }


                if (dy == -1 && dx == 0)
                {
                    bits = 1;
                }
                else if (dy == 1 && dx == 0)
                {
                    bits = 2;
                }
                else if (dy == 0 && dx == -1)
                {
                    bits = 3;
                }
                else if (dy == -1 && dx == -1)
                {
                    bits = 4;
                }
                else if (dy == 1 && dx == -1)
                {
                    bits = 5;
                }
                else if (dy == 0 && dx == 1)
                {
                    bits = 6;
                }
                else if (dy == -1 && dx == 1)
                {
                    bits = 7;
                }
                else if (dy == 1 && dx == 1)
                {
                    bits = 8;
                }

                //
                // Move the mouse closer to its destination in either X or Y
                // (but not both)
                if (_currentMove.XSteps > 0 && dx != 0)
                {
                    _currentMove.XSteps--;

                    if (_currentMove.XSteps == 0)
                    {
                        _currentMove.DX = 0;
                    }
                }

                if (_currentMove.YSteps > 0 && dy != 0)
                {
                    _currentMove.YSteps--;

                    if (_currentMove.YSteps == 0)
                    {
                        _currentMove.DY = 0;
                    }
                }

                if (_currentMove.XSteps == 0 && _currentMove.YSteps == 0)
                {
                    _currentMove = null;
                }
            }
            
            _lock.ExitReadLock();
            _pollCounter++;

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

        AltoSystem _system;

        // Mouse buttons:
        AltoMouseButton _buttons;

        // Keyset switches:
        AltoKeysetKey _keyset;

        private ReaderWriterLockSlim _lock;

        // Used to control the rate of mouse movement data
        //
        public int _pollCounter;        

        /// <summary>
        /// Where the mouse is moving to every time PollMouseBits is called.
        /// </summary> 
        private Queue<MouseMovement> _moves;
        private MouseMovement _currentMove;
        private bool _currentDirection;

        private class MouseMovement
        {
            public MouseMovement(int xsteps, int ysteps, int dx, int dy)
            {                
                XSteps = xsteps;
                YSteps = ysteps;
                DX = dx;
                DY = dy;

                //
                // Calculate the rate at which mouse data should be returned in PollMouseBits,
                // this is a function of the distance moved in this movement.  We assume that the
                // movement occurred in 1/60th of a second; PollMouseBits is invoked (via <-MOUSE)
                // by the MRT approximately every 1/13000th of a second.
                // This is all approximate and not expected to be completely accurate.
                //                
                double distance = Math.Sqrt(Math.Pow(xsteps, 2) + Math.Pow(ysteps, 2));

                PollRate = (int)((13000.0 / 120.0) / (distance + 1));

                if (PollRate == 0)
                {
                    PollRate = 1;
                }
            }

            public int XSteps;
            public int YSteps;
            public int DX;
            public int DY;
            public int PollRate;
        }

    }
}

