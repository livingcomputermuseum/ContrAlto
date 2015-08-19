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
        RotateRight
    }

    public static class Shifter
    {
        static Shifter()
        {
            _op = ShifterOp.Invalid;
        }

        public static void SetOperation(ShifterOp op, int count)
        {
            _op = op;
        }

        public static ushort DoOperation(ushort input)
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
                    break;

                case ShifterOp.ShiftRight:
                    output = (ushort)(input >> _count);
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
    }
}
