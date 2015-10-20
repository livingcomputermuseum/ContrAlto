using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// DiskTask provides implementation for disk-specific special functions
        /// (for both Disk Sector and Disk Word tasks, since the special functions are
        /// identical between the two)
        /// </summary>
        private class DiskTask : Task
        {
            public DiskTask(AltoCPU cpu, bool diskSectorTask) : base(cpu)
            {
                _taskType = diskSectorTask ? TaskType.DiskSector : TaskType.DiskWord;
                _wakeup = false;
            }

            public override void WakeupTask()
            {
                base.WakeupTask();
            }

            protected override bool ExecuteInstruction(MicroInstruction instruction)
            {
                bool task = base.ExecuteInstruction(instruction);                

                return task;
            }

            protected override ushort GetBusSource(int bs)
            {
                DiskBusSource dbs = (DiskBusSource)bs;

                switch (dbs)
                {
                    case DiskBusSource.ReadKSTAT:
                        return _cpu._system.DiskController.KSTAT;

                    case DiskBusSource.ReadKDATA:
                        Console.WriteLine("kdata read");
                        return _cpu._system.DiskController.KDATA;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled bus source {0}", bs));
                }
            }

            protected override void ExecuteSpecialFunction1(MicroInstruction instruction)
            {
                DiskF1 df1 = (DiskF1)instruction.F1;

                switch (df1)
                {
                    case DiskF1.LoadKDATA:
                        // "The KDATA register is loaded from BUS[0-15]."
                        _cpu._system.DiskController.KDATA = _busData;
                        break;

                    case DiskF1.LoadKADR:
                        // "This causes the KADR register to be loaded from BUS[8-14].
                        //  in addition, it causes the head address bit to be loaded from KDATA[13]."
                        // (the latter is done by DiskController)
                        _cpu._system.DiskController.KADR = (ushort)((_busData & 0xfe) >> 1);
                        break;

                    case DiskF1.LoadKCOMM:
                        _cpu._system.DiskController.KCOM = (ushort)((_busData & 0x7c00) >> 10);
                        break;

                    case DiskF1.CLRSTAT:
                        _cpu._system.DiskController.ClearStatus();
                        break;

                    case DiskF1.INCRECNO:
                        _cpu._system.DiskController.IncrementRecord();
                        break;

                    case DiskF1.LoadKSTAT:
                        // "KSTAT[12-15] are loaded from BUS[12-15].  (Actually BUS[13] is ORed onto
                        // KSTAT[13].)"                        

                        // OR in BUS[12-15] after masking in KSTAT[13] so it is ORed in properly.
                        _cpu._system.DiskController.KSTAT = (ushort)(((_cpu._system.DiskController.KSTAT & 0xfff4)) | (_busData & 0xf));
                        break;

                    case DiskF1.STROBE:
                        _cpu._system.DiskController.Strobe();
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled disk special function 1 {0}", df1));
                }
            }

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                DiskF2 df2 = (DiskF2)instruction.F2;

                switch (df2)
                {
                    case DiskF2.INIT:                        
                        _nextModifier |= GetInitModifier(instruction);                        
                        break;                                          

                    case DiskF2.RWC:
                        // "NEXT<-NEXT OR (IF current record to be written THEN 3 ELSE IF
                        // current record to be checked THEN 2 ELSE 0.")
                        // Current record is in bits 8-9 of the command register; this is shifted
                        // by INCREC by the microcode to present the next set of bits.
                        int command = (_cpu._system.DiskController.KADR & 0x00c0) >> 6;

                        _nextModifier |= GetInitModifier(instruction);

                        switch (command)
                        {
                            case 0:
                                // read, no modification.
                                break;

                            case 1:
                                // check, OR in 2
                                _nextModifier |= 0x2;
                                break;

                            case 2:
                            case 3:
                                // write, OR in 3
                                _nextModifier |= 0x3;
                                break;
                        }
                        break;

                    case DiskF2.XFRDAT:
                        // "NEXT <- NEXT OR (IF current command wants data transfer THEN 1 ELSE 0)
                        _nextModifier |= GetInitModifier(instruction);

                        if (_cpu._system.DiskController.DataXfer)
                        {
                            _nextModifier |= 0x1;
                        }
                        break;

                    case DiskF2.RECNO:
                        _nextModifier |= GetInitModifier(instruction);
                        _nextModifier |= _cpu._system.DiskController.RECNO;
                        break;

                    case DiskF2.NFER:
                        // "NEXT <- NEXT OR (IF fatal error in latches THEN 0 ELSE 1)"
                        // We assume success for now...
                        _nextModifier |= GetInitModifier(instruction);
                        _nextModifier |= 0x1;
                        break;

                    case DiskF2.STROBON:
                        // "NEXT <- NEXT OR (IF seek strobe still on THEN 1 ELSE 0)"
                        _nextModifier |= GetInitModifier(instruction);
                        if ((_cpu._system.DiskController.KSTAT & 0x0040) == 0x0040)
                        {
                            _nextModifier |= 0x1;
                        }
                        break;

                    case DiskF2.SWRNRDY:
                        // "NEXT <- NEXT OR (IF disk not ready to accept command THEN 1 ELSE 0)
                        // for now, always zero (not sure when this would be 1 yet)
                        _nextModifier |= GetInitModifier(instruction);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled disk special function 2 {0}", df2));
                }
            }

            /// <summary>
            /// The status of the INIT flag
            /// </summary>
            /// <returns></returns>
            private ushort GetInitModifier(MicroInstruction instruction)
            {
                //
                // "NEXT<-NEXT OR (if WDTASKACT AND WDINIT) then 37B else 0."
                //                

                //
                // A brief discussion of the INIT signal since it isn't really covered in the Alto Hardware docs in any depth
                // (and in fact is completely skipped over in the description of RWC, a rather important detail!)
                // This is where the Alto ref's suggestion to have the uCode *and* the schematic on hand is really quite a
                // valid recommendation.
                //
                // WDINIT is initially set whenever the WDINHIB bit (set via KCOM<-) is cleared (this is the WDALLOW signal).
                // This signals that the microcode is "INITializing" a data transfer (so to speak).  During this period,
                // INIT or RWC instructions in the Disk Word task will OR in 37B to the Next field, causing the uCode to jump 
                // to the requisite initialization paths.  WDINIT is cleared whenever a BLOCK instruction occurs during the Disk Word task,
                // causing INIT to OR in 0 and RWC to or in 0, 2 or 3 (For read, check, or write respectively.)
                //                

                return (_taskType == TaskType.DiskWord && _cpu._system.DiskController.WDINIT) ? (ushort)0x1f : (ushort)0x0;                
            }            
        }        
    }
}
