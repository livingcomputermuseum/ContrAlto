using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _lastCarry = 0;
        }

        public int Carry
        {
            get { return _lastCarry; }
        }

        public static ushort Execute(AluFunction fn, ushort bus, ushort t)
        {
            int r = 0;
            switch (fn)
            {
                case AluFunction.Bus:
                    _lastCarry = 0;     // M = 1
                    r = bus;
                    break;

                case AluFunction.T:
                    _lastCarry = 0;     // M = 1
                    r= t;
                    break;

                case AluFunction.BusOrT:
                    _lastCarry = 0;     // M = 1
                    r = (bus | t);
                    break;

                case AluFunction.BusAndT:
                case AluFunction.AluBusAndT:
                    _lastCarry = 0;     // M = 1
                    r = (bus & t);
                    break;

                case AluFunction.BusXorT:
                    _lastCarry = 0;     // M = 1
                    r = (bus ^ t);
                    break;

                case AluFunction.BusPlus1:
                    r = bus + 1;
                    _lastCarry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusMinus1:
                    r = bus - 1;
                    _lastCarry = (r < 0) ? 1 : 0;
                    break;

                case AluFunction.BusPlusT:
                    r = bus + t;
                    _lastCarry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusMinusT:
                    r = bus - t;
                    _lastCarry = (r < 0) ? 1 : 0;
                    break;

                case AluFunction.BusMinusTMinus1:
                    r = bus - t - 1;
                    _lastCarry = (r < 0) ? 1 : 0;
                    break;

                case AluFunction.BusPlusTPlus1:
                    r = bus + t + 1;
                    _lastCarry = (r > 0xffff) ? 1 : 0;
                    break;

                case AluFunction.BusPlusSkip:
                    throw new NotImplementedException("SKIP?");                    

                case AluFunction.BusAndNotT:
                    r = bus & (~t);
                    _lastCarry = 0;
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unhandled ALU function {0}", fn));
            }     
       
            return (ushort)r;
        }

        private static int _lastCarry;
    }
}
