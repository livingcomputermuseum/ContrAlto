using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU.Nova
{
    /// <summary>
    /// Quick and dirty disassembler for Nova instructions, so we can
    /// see what the emulator task is executing more clearly.
    /// </summary>
    public static class NovaDisassembler
    {

        static NovaDisassembler()
        {
            _altoIOTable = new Dictionary<ushort, string>();

            _altoIOTable.Add(0x6210, "MUL");
            _altoIOTable.Add(0x6211, "DIV");
            _altoIOTable.Add(0x6000, "CYCLE");
            _altoIOTable.Add(0x6900, "JSRII");
            _altoIOTable.Add(0x6a00, "JSRIS");
            _altoIOTable.Add(0x6e00, "CONVERT");
            _altoIOTable.Add(0x6203, "RCLK");
            _altoIOTable.Add(0x6204, "SIO");
            _altoIOTable.Add(0x6205, "BLT");
            _altoIOTable.Add(0x6206, "BLKS");
            _altoIOTable.Add(0x6207, "SIT");
            _altoIOTable.Add(0x6208, "JMPRAM");
            _altoIOTable.Add(0x6209, "RDRAM");
            _altoIOTable.Add(0x620a, "WRTRAM");
            _altoIOTable.Add(0x620c, "VERSION");
            _altoIOTable.Add(0x620d, "DREAD");
            _altoIOTable.Add(0x620e, "DWRITE");
            _altoIOTable.Add(0x620f, "DEXCH");
            _altoIOTable.Add(0x6212, "DIAGNOSE1");
            _altoIOTable.Add(0x6213, "DIAGNOSE2");
            _altoIOTable.Add(0x6214, "BITBLT");
            _altoIOTable.Add(0x6215, "XMLDA");
            _altoIOTable.Add(0x6216, "XMSTA");
        }

        /// <summary>
        /// Disassembles the specified instruction
        /// </summary>
        /// <param name="instructionWord"></param>
        /// <returns></returns>
        public static string DisassembleInstruction(ushort address, ushort instructionWord)
        {
            string disassembly = null;

            
            switch ((InstructionClass)(instructionWord & 0xe000))
            {
                case InstructionClass.MEM:
                    disassembly = DisassembleMem(address, instructionWord);
                    break;

                case InstructionClass.LDA:                    
                case InstructionClass.STA:
                    disassembly = DisassembleLoadStore(address, instructionWord);
                    break;

                case InstructionClass.IO:
                    disassembly = DisassembleIO(instructionWord);
                    break;

                default:
                    // None of the above, must be ALC
                    disassembly = DisassembleALC(instructionWord);
                    break;
            }            

            return disassembly;
        }

        private static string DisassembleMem(ushort address, ushort instructionWord)
        {
            StringBuilder d = new StringBuilder();

            // Function
            MemFunction func = (MemFunction)(instructionWord & 0x1800);

            // Indirect bit
            bool indirect = (instructionWord & 0x400) != 0;

            // Indexing mode
            MemIndex index = (MemIndex)(instructionWord & 0x300);

            // Displacement
            int disp = (instructionWord & 0xff);

            switch (index)
            {
                case MemIndex.PageZero:
                    d.AppendFormat("{0}{1} {2}",
                        func,
                        indirect ? "@" : String.Empty,
                        Conversion.ToOctal(disp));
                    break;

                case MemIndex.PCRelative:
                    d.AppendFormat("{0}{1} .+{2}    ;({3})",
                        func,
                        indirect ? "@" : String.Empty,
                        Conversion.ToOctal((sbyte)disp),
                        Conversion.ToOctal((sbyte)disp + address));
                    break;

                case MemIndex.AC2Relative:
                    d.AppendFormat("{0}{1} AC2+{2}",
                        func,
                        indirect ? "@" : String.Empty,
                        Conversion.ToOctal((sbyte)disp));
                    break;

                case MemIndex.AC3Relative:
                    d.AppendFormat("{0}{1} AC3+{2}",
                        func,
                        indirect ? "@" : String.Empty,
                        Conversion.ToOctal((sbyte)disp));
                    break;

                default:
                    throw new InvalidOperationException("unexpected index type.");
            }

            return d.ToString();
        }

        private static string DisassembleLoadStore(ushort address, ushort instructionWord)
        {
            StringBuilder d = new StringBuilder();

            // Accumulator
            int ac = (instructionWord & 0x1800) >> 11;

            // Indirect bit
            bool indirect = (instructionWord & 0x400) != 0;

            // Indexing mode
            MemIndex index = (MemIndex)(instructionWord & 0x300);

            // Displacement
            int disp = (instructionWord & 0xff);

            // instruction (LDA or STA)
            string inst = (InstructionClass)(instructionWord & 0x6000) == InstructionClass.LDA ? "LDA" : "STA";

            switch(index)
            {
                case MemIndex.PageZero:
                    d.AppendFormat("{0}{1} {2},{3}",
                        inst,
                        indirect ? "@" : String.Empty,
                        ac,
                        Conversion.ToOctal(disp));
                    break;

                case MemIndex.PCRelative:
                    d.AppendFormat("{0}{1} {2},.+{3}    ;({4})",
                        inst,
                        indirect ? "@" : String.Empty,
                        ac,
                        Conversion.ToOctal((sbyte)disp),
                        Conversion.ToOctal((sbyte)disp + address));
                    break;

                case MemIndex.AC2Relative:
                    d.AppendFormat("{0}{1} {2},AC2+{3}",
                        inst,
                        indirect ? "@" : String.Empty,
                        ac,
                        Conversion.ToOctal((sbyte)disp));
                    break;

                case MemIndex.AC3Relative:
                    d.AppendFormat("{0}{1} {2},AC3+{3}",
                        inst,
                        indirect ? "@" : String.Empty,
                        ac,
                        Conversion.ToOctal((sbyte)disp));
                    break;

                default:
                    throw new InvalidOperationException("unexpected index type.");
            }           

            return d.ToString();
        }

        private static string DisassembleIO(ushort instructionWord)
        {
            StringBuilder d = new StringBuilder();

            //
            // First see if this is an Alto-specific instruction; if so
            // use those mnemonics.  Otherwise decode as a Nova I/O instruction.
            //
            if (_altoIOTable.ContainsKey(instructionWord))
            {
                return _altoIOTable[instructionWord];
            }

            // Accumulator
            int ac = (instructionWord & 0x1800) >> 11;

            // Transfer
            IOTransfer trans = (IOTransfer)(instructionWord & 0x700);

            // Control
            IOControl cont = (IOControl)(instructionWord & 0xc0);

            // Device code
            int deviceCode = (instructionWord & 0x3f);

            if (trans != IOTransfer.SKP)
            {
                d.AppendFormat("{0}{1} {2},{3}",
                    trans,
                    cont == IOControl.None ? String.Empty : cont.ToString(),
                    ac,
                    Conversion.ToOctal(deviceCode));
            }
            else
            {
                d.AppendFormat("{0} {1}",
                    (IOSkip)cont,                    
                    Conversion.ToOctal(deviceCode));
            }

            return d.ToString();
        }

        private static string DisassembleALC(ushort instructionWord)
        {
            StringBuilder d = new StringBuilder();

            // Grab source/dest accumulators
            int srcAC = (instructionWord & 0x6000) >> 13;
            int dstAC = (instructionWord & 0x1800) >> 11;

            // Function
            ALCFunctions func = (ALCFunctions)(instructionWord & 0x700);

            // Shift
            ALCShift shift = (ALCShift)(instructionWord & 0xc0);

            // Carry
            ALCCarry carry = (ALCCarry)(instructionWord & 0x30);

            // No load
            bool noLoad = ((instructionWord & 0x8) != 0);

            // Skip
            ALCSkip skip = (ALCSkip)(instructionWord & 0x7);

            // initial format (minus skip):
            // FUNC[shift][carry][noload] src, dest
            d.AppendFormat(
                "{0}{1}{2}{3} {4},{5}",
                func,
                shift == ALCShift.None ? String.Empty : shift.ToString(),
                carry == ALCCarry.None ? String.Empty : carry.ToString(),
                noLoad ? "#" : String.Empty,
                srcAC,
                dstAC);

            // If a skip is specified, tack it on
            if (skip != ALCSkip.None)
            {
                d.AppendFormat(",{0}", skip);
            }


            return d.ToString();
        }

        /// <summary>
        /// Holds a map from opcode to Alto I/O mnemonics
        /// </summary>
        private static Dictionary<ushort, string> _altoIOTable;

        private enum InstructionClass
        {            
            MEM = 0x0000,
            LDA = 0x2000,
            STA = 0x4000,
            IO =  0x6000,
        }

        private enum ALCFunctions
        {
            COM = 0x000,
            NEG = 0x100,
            MOV = 0x200,
            INC = 0x300,
            ADC = 0x400,
            SUB = 0x500,
            ADD = 0x600,
            AND = 0x700,
        }

        private enum ALCShift
        {
            None = 0x00,
            L =    0x40,
            R =    0x80,
            S =    0xc0,
        }

        private enum ALCCarry
        {
            None = 0x00,
            Z =    0x10,
            O =    0x20,
            C =    0x30,
        }

        private enum ALCSkip
        {
            None = 0x0,
            SKP =  0x1,
            SZC =  0x2,
            SNC =  0x3,
            SZR =  0x4,
            SNR =  0x5,
            SEZ =  0x6,
            SBN =  0x7,
        }

        private enum IOTransfer
        {
            NIO = 0x000,
            DIA = 0x100,
            DOA = 0x200,
            DIB = 0x300,
            DOB = 0x400,
            DIC = 0x500,
            DOC = 0x600,
            SKP = 0x700,
        }

        private enum IOControl
        {
            None = 0x00,
            S =    0x40,
            C =    0x80,
            P =    0xc0,
        }

        private enum IOSkip
        {
            SKPBN = 0x00,
            SKPBZ = 0x40,
            SKPDN = 0x80,
            SKPDZ = 0xc0,
        }

        private enum MemFunction
        {
            JMP = 0x0000,
            JSR = 0x0800,
            ISZ = 0x1000,
            DSZ = 0x1800,
        }

        private enum MemIndex
        {
            PageZero =    0x000,
            PCRelative =  0x100,
            AC2Relative = 0x200,
            AC3Relative = 0x300,
        }

    }
}
