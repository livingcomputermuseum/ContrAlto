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
            _output = 0;
            _magic = false;
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
                        throw new NotImplementedException("DNS LSH 1");
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
                        throw new NotImplementedException("DNS RSH 1");
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
                        throw new NotImplementedException("DNS LCY");                        
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
