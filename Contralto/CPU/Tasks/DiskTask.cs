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

            protected override ushort GetBusSource(int bs)
            {
                DiskBusSource dbs = (DiskBusSource)bs;

                switch (dbs)
                {
                    case DiskBusSource.ReadKSTAT:
                        return _cpu._system.DiskController.KSTAT;

                    case DiskBusSource.ReadKDATA:
                        return _cpu._system.DiskController.KDATA;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled bus source {0}", bs));
                }
            }

            protected override void ExecuteSpecialFunction1(int f1)
            {
                DiskF1 df1 = (DiskF1)f1;

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

            protected override void ExecuteSpecialFunction2(int f2)
            {
                DiskF2 df2 = (DiskF2)f2;

                switch (df2)
                {
                    case DiskF2.INIT:
                        // "NEXT<-NEXT OR (if WDTASKACT AND WDINIT) then 37B else 0
                        // TODO: figure out how WDTASKACT and WDINIT work.
                        throw new NotImplementedException("INIT not implemented.");

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled disk special function 2 {0}", df2));
                }
            }
        }

    }
}
