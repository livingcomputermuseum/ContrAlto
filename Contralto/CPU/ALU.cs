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

using System;

namespace Contralto.CPU
{
    //
    // This implements the stripped-down version of the 74181 ALU 
    // that the Alto exposes to the microcode, and nothing more.
    //
    static class ALU
    {
        static ALU()
        {
            Reset();
        }

        public static void Reset()
        {
            _carry = 0;
        }

        public static int Carry
        {
            get { return _carry; }
            set { _carry = value; }
        }

        public static ushort Execute(AluFunction fn, ushort bus, ushort t, int skip)
        {
            int r;
            switch (fn)
            {
                case AluFunction.Bus:
                    _carry = 0;     // M = 1
                    r = bus;
                    break;

                case AluFunction.T:
                    _carry = 0;     // M = 1
                    r = t;
                    break;

                case AluFunction.BusOrT:
                    _carry = 0;     // M = 1
                    r = (bus | t);
                    break;

                case AluFunction.BusAndT:
                case AluFunction.AluBusAndT:
                    _carry = 0;     // M = 1
                    r = (bus & t);
                    break;

                case AluFunction.BusXorT:
                    _carry = 0;     // M = 1
                    r = (bus ^ t);
                    break;

                case AluFunction.BusPlus1:
                    r = bus + 1;
                    _carry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusMinus1:
                    r = bus - 1;

                    // Just for clarification; the datasheet specifies:
                    // "Because subtraction is actually performed by complementary
                    //  addition (1s complement), a carry out means borrow; thus,
                    //  a carry is generated when there is no underflow and no carry
                    //  is generated when there is underflow."
                    _carry = (r < 0) ? 0 : 1;
                    break;

                case AluFunction.BusPlusT:
                    r = bus + t;
                    _carry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusMinusT:
                    r = bus - t;
                    _carry = (r < 0) ? 0 : 1;
                    break;

                case AluFunction.BusMinusTMinus1:
                    r = bus - t - 1;
                    _carry = (r < 0) ? 0 : 1;
                    break;

                case AluFunction.BusPlusTPlus1:
                    r = bus + t + 1;
                    _carry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusPlusSkip:
                    r = bus + skip;
                    _carry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusAndNotT:
                    r = bus & (~t);
                    _carry = 0;
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unhandled ALU function {0}", fn));
            }     
       
            return (ushort)r;
        }

        private static int _carry;
    }
}
