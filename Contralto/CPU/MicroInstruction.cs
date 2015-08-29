using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class MicroInstruction
    {
        public MicroInstruction(UInt32 code)
        {
            RSELECT = (code & 0xf8000000) >> 27;
            ALUF =    (AluFunction)((code & 0x07800000) >> 23);
            BS =      (BusSource)((code &        0x00700000) >> 20);
            F1 =      (SpecialFunction1)((code & 0x000f0000) >> 16);
            F2 =      (SpecialFunction2)((code & 0x0000f000) >> 12);
            LoadT =   ((code & 0x00000800) >> 11) == 0 ? false : true;
            LoadL =   ((code & 0x00000400) >> 10) == 0 ? false : true;
            NEXT =    (ushort)(code & 0x3ff);
        }

        public UInt32 RSELECT;
        public AluFunction ALUF;
        public BusSource BS;
        public SpecialFunction1 F1;
        public SpecialFunction2 F2;
        public bool LoadT;
        public bool LoadL;
        public ushort NEXT;
    }
}
