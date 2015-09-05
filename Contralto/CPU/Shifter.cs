using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{
    public enum ShifterOp
    {
        Invalid = 0,
        None,
        ShiftLeft,
        ShiftRight,
        RotateLeft,
        RotateRight,          
    }

    //NOTE: FOR NOVA (NOVEL) SHIFTS (from aug '76 manual):
    // The emulator has two additional bits of state, the SKIP and CARRY flip flops.CARRY is identical
    // to the Nova carry bit, and is set or cleared as appropriate when the DNS+- (do Nova shifts)
    // function is executed.DNS also addresses R from(1R[3 - 4] XOR 3), and sets the SKIP flip flop if 
    // appropriate.The PC is incremented by 1 at the beginning of the next emulated instruction if
    // SKIP is set, using ALUF DB.IR4- clears SKIP.

    public static class Shifter
    {
        static Shifter()
        {
            _op = ShifterOp.Invalid;
            _count = 0;
            _magic = false;
        }

        public static void SetOperation(ShifterOp op, int count)
        {
            _op = op;
            _count = count;
        }

        /// <summary>
        /// TODO: this is kind of clumsy.
        /// </summary>
        /// <param name="magic"></param>
        public static void SetMagic(bool magic)
        {
            _magic = magic;            
        }

        /// <summary>
        /// Does the last specified operation to the specified inputs
        /// </summary>
        /// <param name="input">Normal input to be shifted</param>
        /// <param name="t">CPU t register, for MAGIC shifts only</param>
        /// <returns></returns>
        public static ushort DoOperation(ushort input, ushort t)
        {
            ushort output = 0;
            switch(_op)
            {
                case ShifterOp.Invalid:
                    throw new InvalidOperationException("Shifter op has not been set.");

                case ShifterOp.None:
                    output = input;
                    break;

                case ShifterOp.ShiftLeft:
                    output = (ushort)(input << _count);

                    if (_magic)
                    {
                        // "MAGIC places the high order bit of T into the low order bit of the
                        // shifter output on left shifts..."
                        output |= (ushort)((t & 0x8000) >> 15);
                    }
                    break;

                case ShifterOp.ShiftRight:
                    output = (ushort)(input >> _count);

                    if (_magic)
                    {
                        // "...and places the low order bit of T into the high order bit position 
                        // of the shifter output on right shifts."
                        output |= (ushort)((t & 0x1) << 15);
                    }
                    break;

                case ShifterOp.RotateLeft:
                    // TODO: optimize, this is stupid
                    output = input;
                    for (int i = 0; i < _count; i++)
                    {
                        int c = (output & 0x8000) >> 15;
                        output = (ushort)((output << 1) | c);
                    }
                    break;

                case ShifterOp.RotateRight:
                    // TODO: optimize, this is still stupid
                    output = input;
                    for (int i = 0; i < _count; i++)
                    {
                        int c = (output & 0x1) << 15;
                        output = (ushort)((output >> 1) | c);
                    }
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unhandled shift operation {0}", _op));
            }

            return output;
        }

        private static ShifterOp _op;
        private static int _count;
        private static bool _magic;
    }
}
