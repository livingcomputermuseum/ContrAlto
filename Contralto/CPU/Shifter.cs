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
            Reset();
        }

        public static void Reset()
        {
            _op = ShifterOp.Invalid;
            _count = 0;
            _output = 0;
            _magic = false;
            _dns = false;
            _dnsCarry = 0;
        }

        /// <summary>
        /// Returns the result of the last Shifter operation (via DoOperation).
        /// </summary>
        public static ushort Output
        {
            get { return _output; }
        }

        /// <summary>
        /// Returns the last DNS-style Carry bit from the last operation (via DoOperation).
        /// </summary>
        public static int DNSCarry
        {
            get { return _dnsCarry; }
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
        /// TODO: this is still kind of clumsy.
        /// </summary>
        /// <param name="dns"></param>
        public static void SetDNS(bool dns, int carry)
        {
            // Sanity check
            if (carry != 0 && carry != 1)
            {
                throw new InvalidOperationException("carry can only be 0 or 1.");
            }

            _dns = dns;
            _dnsCarry = carry;
        }

        /// <summary>
        /// Does the last specified operation to the specified inputs;  the result
        /// can be read from Output.
        /// </summary>
        /// <param name="input">Normal input to be shifted</param>
        /// <param name="t">CPU t register, for MAGIC shifts only</param>        
        public static ushort DoOperation(ushort input, ushort t)
        {
            // Sanity check: MAGIC and DNS cannot be set at the same time.
            if (_magic && _dns)
            {
                throw new InvalidOperationException("Both MAGIC and DNS bits are set.");
            }
            
            switch(_op)
            {
                case ShifterOp.Invalid:
                    throw new InvalidOperationException("Shifter op has not been set.");

                case ShifterOp.None:
                    _output = input;
                    break;

                case ShifterOp.ShiftLeft:
                    _output = (ushort)(input << _count);

                    if (_magic)
                    {
                        // "MAGIC places the high order bit of T into the low order bit of the
                        // shifter output on left shifts..."
                        _output |= (ushort)((t & 0x8000) >> 15);
                    }
                    else if (_dns)
                    {
                        //
                        // "Rotate the 17 input bits left by one bit.  This has the effect of rotating
                        // bit 0 left into the carry position and the carry bit into bit 15."
                        //

                        // Put input carry into bit 15.
                        _output = (ushort)(_output | _dnsCarry);

                        // update carry
                        _dnsCarry = ((input & 0x8000) >> 15);
                    }
                    break;

                case ShifterOp.ShiftRight:
                    _output = (ushort)(input >> _count);

                    if (_magic)
                    {
                        // "...and places the low order bit of T into the high order bit position 
                        // of the shifter output on right shifts."
                        _output |= (ushort)((t & 0x1) << 15);
                    }
                    else if (_dns)
                    {
                        //
                        // "Rotate the 17 bits right by one bit.  Bit 15 is rotated into the carry position
                        // and the carry bit into bit 0."
                        //

                        // Put input carry into bit 0.
                        _output |= (ushort)(_output | (_dnsCarry << 15));

                        // update carry
                        _dnsCarry = input & 0x1;
                    }
                    break;

                case ShifterOp.RotateLeft:
                    // TODO: optimize, this is stupid
                    _output = input;
                    for (int i = 0; i < _count; i++)
                    {
                        int c = (_output & 0x8000) >> 15;
                        _output = (ushort)((_output << 1) | c);
                    }

                    if (_dns)
                    {
                        //
                        // "Swap the 8-bit halves of the 16-bit result.  The carry is not affected."
                        //
                        _output = (ushort)(((input & 0xff00) >> 8) | ((input & 0x00ff) << 8));
                    }
                    break;

                case ShifterOp.RotateRight:
                    // TODO: optimize, this is still stupid
                    _output = input;
                    for (int i = 0; i < _count; i++)
                    {
                        int c = (_output & 0x1) << 15;
                        _output = (ushort)((_output >> 1) | c);
                    }
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Unhandled shift operation {0}", _op));
            }

            return _output;
        }

        private static ShifterOp _op;
        private static ushort _output;
        private static int _count;
        private static bool _magic;
        private static bool _dns;
        private static int _dnsCarry;
    }
}
