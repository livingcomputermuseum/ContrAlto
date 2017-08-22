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
using System.Text;

namespace Contralto.CPU
{    
    /// <summary>
    /// Provides a facility for doing a (crude) disassembly of microcode.
    /// </summary>
    public static class UCodeDisassembler
    {       
        
        /// <summary>
        /// Disassembles the specified microinstruction for the specified Task type.
        /// </summary>
        /// <param name="instruction">The microinstruction to disassemble</param>
        /// <param name="task">The task to interpret the microinstruction for</param>
        public static string DisassembleInstruction(MicroInstruction instruction, TaskType task)
        {            
            StringBuilder disassembly = new StringBuilder();

            uint rSelect = instruction.RSELECT;
            bool loadR = false;
            bool loadS = false;
            string source = string.Empty;
            string operation = string.Empty;
            string f1 = string.Empty;
            string f2 = string.Empty;            
            string load = string.Empty;

            // Select BUS data.
            if (instruction.F1 != SpecialFunction1.Constant &&
                instruction.F2 != SpecialFunction2.Constant)
            {
                // Normal BUS data (not constant ROM access).
                switch (instruction.BS)
                {
                    case BusSource.ReadR:
                        source = String.Format("$R{0} ", Conversion.ToOctal((int)rSelect));
                        break;

                    case BusSource.LoadR:
                        source = "L ";
                        loadR = true;
                        break;

                    case BusSource.None:
                        // nothing
                        break;

                    case BusSource.TaskSpecific1:
                    case BusSource.TaskSpecific2:
                        source = DisassembleBusSource(instruction, task, out loadS);        // task specific -- call into specific implementation
                        break;

                    case BusSource.ReadMD:
                        source = "←MD ";
                        break;

                    case BusSource.ReadMouse:
                        source = "←MOUSE ";
                        break;

                    case BusSource.ReadDisp:
                        source = "←DISP ";
                        break;
                }
            }           

            if ((int)instruction.BS > 4 ||
                instruction.F1 == SpecialFunction1.Constant ||
                instruction.F2 == SpecialFunction2.Constant)
            {
                source += String.Format("C({0})",
                    Conversion.ToOctal(ControlROM.ConstantROM[(instruction.RSELECT << 3) | ((uint)instruction.BS)]));
            }

            switch (instruction.ALUF)
            {
                case AluFunction.Bus:
                    operation = source;
                    break;

                case AluFunction.T:
                    operation = "T ";
                    break;

                case AluFunction.BusOrT:
                    operation = String.Format("{0} or T ", source);
                    break;

                case AluFunction.BusAndT:
                    operation = String.Format("{0} and T ", source);
                    break;

                case AluFunction.BusXorT:
                    operation = String.Format("{0} xor T ", source);
                    break;

                case AluFunction.BusPlus1:
                    operation = String.Format("{0} + 1 ", source);
                    break;

                case AluFunction.BusMinus1:
                    operation = String.Format("{0} - 1 ", source);
                    break;

                case AluFunction.BusPlusT:
                    operation = String.Format("{0} + T ", source);
                    break;

                case AluFunction.BusMinusT:
                    operation = String.Format("{0} - T ", source);
                    break;

                case AluFunction.BusMinusTMinus1:
                    operation = String.Format("{0} - T - 1 ", source);
                    break;

                case AluFunction.BusPlusTPlus1:
                    operation = String.Format("{0} + T + 1 ", source);
                    break;

                case AluFunction.BusPlusSkip:
                    operation = String.Format("{0} + SKIP ", source);
                    break;

                case AluFunction.AluBusAndT:
                    operation = String.Format("{0}.T ", source);
                    break;

                case AluFunction.BusAndNotT:
                    operation = String.Format("{0} and not T ", source);
                    break;

                default:
                    operation = "Undefined ALU operation ";
                    break;
            }

            switch (instruction.F1)
            {
                case SpecialFunction1.None:
                    f1 = string.Empty;
                    break;

                case SpecialFunction1.LoadMAR:

                    if (instruction.F2 == SpecialFunction2.StoreMD)
                    {
                        f1 = "XMAR← ";
                    }
                    else
                    {
                        f1 = "MAR← ";
                    }
                    break;

                case SpecialFunction1.Task:
                    f1 = "TASK ";
                    break;

                case SpecialFunction1.Block:                    
                    f1 = "BLOCK ";
                    break;

                case SpecialFunction1.LLSH1:
                    f1 = "←L LSH 1 ";
                    break;

                case SpecialFunction1.LRSH1:
                    f1 = "←L RSH 1 ";
                    break;

                case SpecialFunction1.LLCY8:
                    f1 = "←L LCY 8 ";
                    break;

                case SpecialFunction1.Constant:
                    // Ignored here; handled by Constant ROM access logic above.
                    break;

                default:
                    // Let the specific task implementation take a crack at this.
                    f1 = DisassembleSpecialFunction1(instruction, task);
                    break;
            }

            switch (instruction.F2)
            {
                case SpecialFunction2.None:
                    f2 = string.Empty;
                    break;

                case SpecialFunction2.BusEq0:
                    f2 = "BUS=0 ";
                    break;

                case SpecialFunction2.ShLt0:
                    f2 = "SH<0 ";
                    break;

                case SpecialFunction2.ShEq0:
                    f2 = "SH=0 ";
                    break;

                case SpecialFunction2.Bus:
                    f2 = "BUS ";
                    break;

                case SpecialFunction2.ALUCY:
                    f2 = "ALUCY ";
                    break;

                case SpecialFunction2.StoreMD:
                    // Special case for Trident
                    if ((task == TaskType.TridentInput || task == TaskType.TridentOutput) &&
                        instruction.BS == BusSource.None)
                    {
                        f2 = "MD← KDTA ";
                    }
                    else if (instruction.F1 != SpecialFunction1.LoadMAR)
                    {
                        f2 = "MD← ";
                    }
                    break;

                case SpecialFunction2.Constant:
                    // Ignored here; handled by Constant ROM access logic above.
                    break;

                default:
                    // Let the specific task implementation take a crack at this.
                    f2 = DisassembleSpecialFunction2(instruction, task);
                    break;
            }

            //
            // Write back to registers:
            //

            // Load T
            bool loadTFromALU = false;
            if (instruction.LoadT)
            {
                // Does this operation change the source for T?                
                switch (instruction.ALUF)
                {
                    case AluFunction.Bus:
                    case AluFunction.BusOrT:
                    case AluFunction.BusPlus1:
                    case AluFunction.BusMinus1:
                    case AluFunction.BusPlusTPlus1:
                    case AluFunction.BusPlusSkip:
                    case AluFunction.AluBusAndT:
                        loadTFromALU = true;
                        break;
                }

                load = String.Format("T← {0}", loadTFromALU ? operation : source);
            }

            // Load L (and M) from ALU
            if (instruction.LoadL)
            {
                if (string.IsNullOrEmpty(load))
                {
                    // T not loaded at all, L loaded from ALU
                    load = String.Format("L← {0}", operation);
                }
                else if (loadTFromALU)
                {
                    // T loaded from ALU, L loaded from ALU
                    load = String.Format("L← {0}", load);
                }
                else
                {
                    // T loaded from bus source, L loaded from ALU
                    load = String.Format("L← {0}, {1}", operation, load);
                }
            }

            // Do writeback to selected R register from shifter output
            if (loadR)
            {
                load = String.Format("$R{0}← {1}", 
                    Conversion.ToOctal((int)rSelect),
                    !string.IsNullOrEmpty(load) ? load : operation);
            }

            // Do writeback to selected S register from M
            if (loadS)
            {
                if (string.IsNullOrEmpty(load))
                {
                    load = String.Format("$S{0}← M",
                         Conversion.ToOctal((int)rSelect));
                }
                else
                {
                    load = String.Format("$S{0}← M, {1}",
                        Conversion.ToOctal((int)rSelect),
                        load);
                }
            }

            // Test for a NOP-like instruction.
            if (!instruction.LoadL && 
                !instruction.LoadT && 
                !loadR && 
                !loadS && 
                instruction.F1 == SpecialFunction1.None &&
                instruction.F2 == SpecialFunction2.None &&
                instruction.ALUF == AluFunction.Bus)
            {
                disassembly.AppendFormat("NOP :{0}", Conversion.ToOctal(instruction.NEXT));
            }
            else if (!string.IsNullOrEmpty(load))
            {
                disassembly.AppendFormat("{0}{1}{2} :{3}", 
                    f1, 
                    f2, 
                    load,
                    Conversion.ToOctal(instruction.NEXT));
            }
            else
            {
                disassembly.AppendFormat("{0}{1}{2} :{3}", 
                    f1, 
                    f2, 
                    operation, 
                    Conversion.ToOctal(instruction.NEXT));
            }


            return disassembly.ToString();
        }        

        private static string DisassembleBusSource(MicroInstruction instruction, TaskType task, out bool loadS)
        {
            switch(task)
            {
                case TaskType.Emulator:
                case TaskType.Orbit:
                case TaskType.TridentInput:
                case TaskType.TridentOutput:
                    return DisassembleEmulatorBusSource(instruction, out loadS);

                default:
                    loadS = false;
                    return String.Format("BS {0}", Conversion.ToOctal((int)instruction.BS));
            }
        }

        private static string DisassembleSpecialFunction1(MicroInstruction instruction, TaskType task)
        {
            switch (task)
            {
                case TaskType.Emulator:
                    return DisassembleEmulatorSpecialFunction1(instruction);

                case TaskType.Orbit:
                    return DisassembleOrbitSpecialFunction1(instruction);

                default:
                    return String.Format("F1 {0}", Conversion.ToOctal((int)instruction.F1));
            }
        }

        private static string DisassembleSpecialFunction2(MicroInstruction instruction, TaskType task)
        {
            switch (task)
            {
                case TaskType.Emulator:
                    return DisassembleEmulatorSpecialFunction2(instruction);

                case TaskType.Orbit:
                    return DisassembleOrbitSpecialFunction2(instruction);

                case TaskType.TridentInput:
                case TaskType.TridentOutput:
                    return DisassembleTridentSpecialFunction2(instruction);

                default:
                    return String.Format("F2 {0}", Conversion.ToOctal((int)instruction.F2));
            }
        }

        private static string DisassembleEmulatorBusSource(MicroInstruction instruction, out bool loadS)
        {
            EmulatorBusSource bs = (EmulatorBusSource)instruction.BS;

            switch(bs)
            {
                case EmulatorBusSource.ReadSLocation:
                    loadS = false;

                    if (instruction.RSELECT == 0)
                    {
                        return "M";
                    }
                    else
                    {
                        return String.Format("$S{0}", Conversion.ToOctal((int)instruction.RSELECT));
                    }

                case EmulatorBusSource.LoadSLocation:
                    loadS = true;
                    return String.Empty;

                default:
                    loadS = false;
                    throw new InvalidOperationException(String.Format("Unhandled Emulator BS {0}", bs));
                    
            }

        }

        private static string DisassembleEmulatorSpecialFunction1(MicroInstruction instruction)
        {
            EmulatorF1 ef1 = (EmulatorF1)instruction.F1;

            switch(ef1)
            {
                case EmulatorF1.SWMODE:
                    return "SWMODE ";

                case EmulatorF1.WRTRAM:
                    return "WRTRAM ";

                case EmulatorF1.RDRAM:
                    return "RDRAM ";

                case EmulatorF1.LoadRMR:
                    return "RMR← ";

                case EmulatorF1.LoadESRB:
                    return "ESRB← ";

                case EmulatorF1.RSNF:
                    return "RSNF ";

                case EmulatorF1.STARTF:
                    return "STARTF ";

                default:
                    return String.Format("Emulator F1 {0}", Conversion.ToOctal((int)ef1));
            }

        }

        private static string DisassembleEmulatorSpecialFunction2(MicroInstruction instruction)
        {
            EmulatorF2 ef2 = (EmulatorF2)instruction.F2;

            switch (ef2)
            {
                case EmulatorF2.ACDEST:
                    return "ACDEST ";

                case EmulatorF2.ACSOURCE:
                    return "ACSOURCE ";

                case EmulatorF2.MAGIC:
                    return "MAGIC ";

                case EmulatorF2.LoadDNS:
                    return "DNS← ";

                case EmulatorF2.BUSODD:
                    return "BUSODD ";

                case EmulatorF2.LoadIR:
                    return "IR← ";

                case EmulatorF2.IDISP:
                    return "IDISP ";

                default:
                    return String.Format("Emulator F2 {0}", Conversion.ToOctal((int)ef2));
            }
        }

        private static string DisassembleOrbitSpecialFunction1(MicroInstruction instruction)
        {
            OrbitF1 of1 = (OrbitF1)instruction.F1;

            switch(of1)
            {
                case OrbitF1.OrbitBlock:
                    return "OrbitBlock ";

                case OrbitF1.OrbitDeltaWC:
                    return "←OrbitDeltaWC ";

                case OrbitF1.OrbitDBCWidthRead:
                    return "←OrbitDBCWidthRead ";

                case OrbitF1.OrbitStatus:
                    return "←OrbitStatus ";

                default:
                    return String.Format("Orbit F1 {0}", Conversion.ToOctal((int)of1));
            }
        }

        private static string DisassembleOrbitSpecialFunction2(MicroInstruction instruction)
        {
            OrbitF2 of2 = (OrbitF2)instruction.F2;

            switch (of2)
            {
                case OrbitF2.OrbitDBCWidthSet:
                    return "OrbitDBCWidthSet← ";

                case OrbitF2.OrbitXY:
                    return "OrbitXY← ";

                case OrbitF2.OrbitHeight:
                    return "OrbitHeight← ";

                case OrbitF2.OrbitFontData:
                    return "OrbitFontData← ";

                case OrbitF2.OrbitInk:
                    return "OrbitInk← ";

                case OrbitF2.OrbitControl:
                    return "OrbitControl← ";

                case OrbitF2.OrbitROSCommand:
                    return "OrbitROSCommand← ";                

                default:
                    return String.Format("Orbit F2 {0}", Conversion.ToOctal((int)of2));
            }
        }

        private static string DisassembleTridentSpecialFunction2(MicroInstruction instruction)
        {
            TridentF2 tf2 = (TridentF2)instruction.F2;

            switch (tf2)
            {
                case TridentF2.EMPTY:
                    return "EMPTY ";

                case TridentF2.KTAG:
                    return "KTAG← ";

                case TridentF2.ReadKDTA:
                    return "←KDTA ";

                case TridentF2.RESET:
                    return "RESET ";

                case TridentF2.STATUS:
                    return "STATUS ";

                case TridentF2.WAIT:
                case TridentF2.WAIT2:
                    return "WAIT ";

                case TridentF2.WriteKDTA:
                    return "KDTA← ";

                default:
                    return String.Format("Trident F2 {0}", Conversion.ToOctal((int)tf2));
            }
        }
    }
}
