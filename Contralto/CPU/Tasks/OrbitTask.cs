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
using Contralto.Logging;
using System;

namespace Contralto.CPU
{
    public partial class AltoCPU
    {
        /// <summary>
        /// OrbitTask provides the implementation of the Orbit (printer rasterizer) controller
        /// specific functions.
        /// </summary>
        private sealed class OrbitTask : Task
        {
            public OrbitTask(AltoCPU cpu) : base(cpu)
            {
                _taskType = TaskType.Orbit;
                _wakeup = false;
            }

            public override void OnTaskSwitch()
            {
                // We put ourselves back to sleep immediately once we've started running.
                //_wakeup = false;
            }

            protected override void ExecuteBlock()
            {
                //_wakeup = false;
                _cpu._system.OrbitController.Stop();
            }

            protected override InstructionCompletion ExecuteInstruction(MicroInstruction instruction)
            {
                // TODO: get rid of polling.
                //_wakeup = _cpu._system.OrbitController.Wakeup;
                return base.ExecuteInstruction(instruction);
            }

            protected override ushort GetBusSource(MicroInstruction instruction)
            {
                //
                // The Orbit task is wired to be a RAM-enabled task so it can use
                // S registers.
                // This code is stolen from the Emulator task; we should refactor this...
                //
                EmulatorBusSource ebs = (EmulatorBusSource)instruction.BS;

                switch (ebs)
                {
                    case EmulatorBusSource.ReadSLocation:
                        if (instruction.RSELECT != 0)
                        {
                            return _cpu._s[_rb][instruction.RSELECT];
                        }
                        else
                        {
                            // "...when reading data from the S registers onto the processor bus,
                            //  the RSELECT value 0 causes the current value of the M register to
                            //  appear on the bus..."
                            return _cpu._m;
                        }

                    case EmulatorBusSource.LoadSLocation:
                        // "When an S register is being loaded from M, the processor bus receives an
                        // undefined value rather than being set to zero."
                        _loadS = true;
                        return 0x0;       // Technically this is an "undefined value," we're defining it as -1.

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled bus source {0}", instruction.BS));
                }

                
            }

            protected override void ExecuteSpecialFunction1Early(MicroInstruction instruction)
            {
                OrbitF1 of1 = (OrbitF1)instruction.F1;
                switch (of1)
                { 

                    case OrbitF1.OrbitDeltaWC:
                        _busData &= _cpu._system.OrbitController.GetDeltaWC();
                        break;

                    case OrbitF1.OrbitDBCWidthRead:
                        _busData &= _cpu._system.OrbitController.GetDBCWidth();
                        break;

                    case OrbitF1.OrbitOutputData:
                        _busData &= _cpu._system.OrbitController.GetOutputDataAlto();
                        break;

                    case OrbitF1.OrbitStatus:
                        _busData &= _cpu._system.OrbitController.GetOrbitStatus();

                        // branch:
                        // "OrbitStatus sets NEXT[7] of IACS os *not* on, i.e. if Orbit is
                        //  not in a character segment."
                        //
                        if (!_cpu._system.OrbitController.IACS)
                        {
                            _nextModifier |= 0x4;
                        }
                        break;
                }
            }

            protected override void ExecuteSpecialFunction2(MicroInstruction instruction)
            {
                OrbitF2 of2 = (OrbitF2)instruction.F2;
                switch (of2)
                {
                    case OrbitF2.OrbitDBCWidthSet:
                        _cpu._system.OrbitController.SetDBCWidth(_busData);
                        break;

                    case OrbitF2.OrbitXY:
                        _cpu._system.OrbitController.SetXY(_busData);
                        break;

                    case OrbitF2.OrbitHeight:
                        _cpu._system.OrbitController.SetHeight(_busData);

                        // branch:
                        // "OrbitHeight sets NEXT[7] if the refresh timer has expired, i.e.
                        //  if the image buffer needs refreshing."
                        //
                        if (_cpu._system.OrbitController.RefreshTimerExpired)
                        {                            
                            _nextModifier |= 0x4;
                        }
                        break;

                    case OrbitF2.OrbitFontData:
                        _cpu._system.OrbitController.WriteFontData(_busData);
                        break;

                    case OrbitF2.OrbitInk:
                        _cpu._system.OrbitController.WriteInkData(_busData);
                        break;

                    case OrbitF2.OrbitControl:
                        _cpu._system.OrbitController.Control(_busData);
                        break;

                    case OrbitF2.OrbitROSCommand:
                        _cpu._system.OrbitController.SendROSCommand(_busData);
                        break;

                    default:
                        throw new InvalidOperationException(String.Format("Unhandled orbit F2 {0}.", of2));
                }
            }
        }
    }
}
