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

using Contralto.Memory;
using System;

namespace Contralto.CPU
{
    //
    // From Alto Hardware Manual, Section 2.1.
    // These are the non-task specific definitions.
    //
    public enum BusSource
    {
        ReadR = 0,
        LoadR = 1,
        None = 2,
        TaskSpecific1 = 3,
        TaskSpecific2 = 4,
        ReadMD = 5,
        ReadMouse = 6 ,
        ReadDisp = 7,
    }

    public enum SpecialFunction1
    {
        None = 0,
        LoadMAR = 1,
        Task = 2,
        Block = 3,
        LLSH1 = 4,
        LRSH1 = 5,
        LLCY8 = 6,
        Constant = 7,
    }

    public enum SpecialFunction2
    {
        None = 0,
        BusEq0 = 1,
        ShLt0 = 2,
        ShEq0 = 3,
        Bus = 4,
        ALUCY = 5,
        StoreMD = 6,
        Constant = 7,
    }

    public enum AluFunction
    {
        Bus = 0,
        T = 1,
        BusOrT = 2,
        BusAndT = 3,
        BusXorT = 4,
        BusPlus1 = 5,
        BusMinus1 = 6,
        BusPlusT = 7,
        BusMinusT = 8 ,
        BusMinusTMinus1 = 9,
        BusPlusTPlus1 = 10,
        BusPlusSkip = 11,
        AluBusAndT = 12,
        BusAndNotT = 13,
        Undefined1 = 14,
        Undefined2 = 15,
    }

    //
    // Task-specific enumerations follow
    //

    //
    // Emulator
    //
    enum EmulatorF1
    {
        SWMODE = 8,
        WRTRAM = 9,
        RDRAM = 10,
        LoadRMR = 11,
        Unused = 12,
        LoadESRB = 13,
        RSNF = 14,
        STARTF = 15,
    }

    enum EmulatorF2
    {
        BUSODD = 8,
        MAGIC = 9,
        LoadDNS = 10,
        ACDEST = 11,
        LoadIR = 12,
        IDISP = 13,
        ACSOURCE = 14,
        Unused = 15,
    }

    enum EmulatorBusSource
    {
        ReadSLocation = 3,  // <-SLOCATION: read from S reg into M
        LoadSLocation = 4,  // SLOCATION<- store to S reg from M
    }


    //
    // Disk (both sector and word tasks)
    //
    enum DiskF1
    {
        STROBE = 9,
        LoadKSTAT = 10,
        INCRECNO = 11,
        CLRSTAT = 12,
        LoadKCOMM = 13,
        LoadKADR = 14,
        LoadKDATA = 15,
    }

    enum DiskF2
    {
        INIT = 8,
        RWC = 9,
        RECNO = 10,
        XFRDAT = 11,
        SWRNRDY = 12,
        NFER = 13,
        STROBON = 14,
    }

    enum DiskBusSource
    {
        ReadKSTAT = 3,
        ReadKDATA = 4,
    }

    enum DisplayWordF2
    {
        LoadDDR = 8,
    }

    enum DisplayHorizontalF2
    {
        EVENFIELD = 8,
        SETMODE = 9,
    }

    enum DisplayVerticalF2
    {
        EVENFIELD = 8,
    }

    enum CursorF2
    {
        LoadXPREG = 8,
        LoadCSR = 9,
    }

    enum EthernetBusSource
    {
        EIDFCT = 4,
    }

    enum EthernetF1
    {
        EILFCT = 11,
        EPFCT = 12,
        EWFCT = 13,
    }

    enum EthernetF2
    {
        EODFCT = 8,
        EOSFCT = 9,
        ERBFCT = 10,
        EEFCT = 11,
        EBFCT = 12,
        ECBFCT = 13,
        EISFCT = 14,
    }

    /// <summary>
    /// MicroInstruction encapsulates the decoding of a microinstruction word.
    /// It also caches precomputed metadata related to the microinstruction that
    /// help speed microcode execution.
    /// </summary>
    public class MicroInstruction
    {
        public MicroInstruction(UInt32 code)
        {
            // Parse fields
            RSELECT = (code & 0xf8000000) >> 27;
            ALUF =    (AluFunction)((code & 0x07800000) >> 23);
            BS =      (BusSource)((code &        0x00700000) >> 20);
            F1 =      (SpecialFunction1)((code & 0x000f0000) >> 16);
            F2 =      (SpecialFunction2)((code & 0x0000f000) >> 12);
            LoadT =   ((code & 0x00000800) >> 11) == 0 ? false : true;
            LoadL =   ((code & 0x00000400) >> 10) == 0 ? false : true;
            NEXT =    (ushort)(code & 0x3ff);

            // Parse metadata:

            // Whether this instruction references constant memory
            ConstantAccess =
                       F1 == SpecialFunction1.Constant ||
                       F2 == SpecialFunction2.Constant;

            ConstantAccessOrBS4 = ConstantAccess || (int)BS > 4;

            // Constant ROM access:
            // "The constant memory is gated to the bus by F1=7, F2=7, or BS>4.  The constant memory is addressed by the
            // (8 bit) concatenation of RSELECT and BS.  The intent in enabling constants with BS>4 is to provide a masking
            // facility, particularly for the <-MOUSE and <-DISP bus sources.  This works because the processor bus ANDs if
            // more than one source is gated to it.  Up to 32 such mask contans can be provided for each of the four bus sources
            // > 4."
            // NOTE also:
            // "Note that the [emulator task F2] functions which replace the low bits of RSELECT with IR affect only the 
            // selection of R; they do not affect the address supplied to the constant ROM."
            // Hence this can be statically cached without issue.
            ConstantValue = ControlROM.ConstantROM[(RSELECT << 3) | ((uint)BS)];            

            // Whether this instruction needs the Shifter output
            // This is the only task-specific thing we cache, even if this isn't
            // the right task, worst-case we'll do an operation we didn't need to.
            NeedShifterOutput = (EmulatorF2)F2 == EmulatorF2.LoadDNS ||
                               F2 == SpecialFunction2.ShEq0 ||
                               F2 == SpecialFunction2.ShLt0;

            // Whether this instruction accesses memory
            MemoryAccess = 
                (BS == BusSource.ReadMD && !ConstantAccess) ||        // ReadMD only occurs if not reading from constant ROM.
                F1 == SpecialFunction1.LoadMAR ||
                F2 == SpecialFunction2.StoreMD;

            // What kind of memory access this instruction performs, if any.
            if (MemoryAccess)
            {
                if (F1 == SpecialFunction1.LoadMAR)
                {
                    MemoryOperation = MemoryOperation.LoadAddress;
                }
                else if (BS == BusSource.ReadMD)
                {
                    MemoryOperation = MemoryOperation.Read;
                }
                else
                {
                    MemoryOperation = MemoryOperation.Store;
                }
            }
            else
            {
                MemoryOperation = MemoryOperation.None;
            }

            // Whether to load T from the ALU or the bus.
            switch (ALUF)
            {
                case AluFunction.Bus:
                case AluFunction.BusOrT:
                case AluFunction.BusPlus1:
                case AluFunction.BusMinus1:
                case AluFunction.BusPlusTPlus1:
                case AluFunction.BusPlusSkip:
                case AluFunction.AluBusAndT:
                    LoadTFromALU = true;
                    break;
            }
        }

        public override string ToString()
        {
            return String.Format("RSELECT={0} ALUF={1} BS={2} F1={3} F2={4} LoadT={5} LoadL={6} NEXT={7}",
                Conversion.ToOctal((int)RSELECT),
                ALUF,
                BS,
                F1,
                F2,
                LoadT,
                LoadL,
                Conversion.ToOctal(NEXT));
        }

        public UInt32 RSELECT;
        public AluFunction ALUF;
        public BusSource BS;
        public SpecialFunction1 F1;
        public SpecialFunction2 F2;
        public bool LoadT;
        public bool LoadL;
        public bool NeedShifterOutput;
        public ushort NEXT;

        // Metadata about the instruction that can be precalculated and used during execution
        public bool ConstantAccess;
        public bool ConstantAccessOrBS4;
        public ushort ConstantValue;
        public bool MemoryAccess;
        public MemoryOperation MemoryOperation;
        public bool LoadTFromALU;
    }
}
