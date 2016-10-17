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

using Contralto.CPU;
using Contralto.Logging;
using System;
using System.Collections.Generic;

namespace Contralto.Memory
{
    public enum MemoryOperation
    {
        None,        
        LoadAddress,
        Read,
        Store
    }

    /// <summary>
    /// Implements the memory bus and memory timings for the Alto system.
    /// This implements timings for both Alto I and Alto II systems.
    /// </summary>
    public sealed class MemoryBus : IClockable
    {
        public MemoryBus()
        {
            _bus = new Dictionary<ushort, IMemoryMappedDevice>(65536);
            _systemType = Configuration.SystemType;           

            Reset();
        }       

        public void AddDevice(IMemoryMappedDevice dev)
        {
            //
            // Add the new device to the hash; this is done by adding
            // one entry for every address claimed by the device.  Since we have only 64K of address
            // space, this isn't too awful.
            //
            foreach(MemoryRange range in dev.Addresses)
            {
                for(ushort addr = range.Start; addr <= range.End; addr++)
                {
                    if (_bus.ContainsKey(addr))
                    {
                        throw new InvalidOperationException(
                            String.Format("Memory mapped address collision for dev {0} at address {1} with {2}", dev, Conversion.ToOctal(addr), _bus[addr]));
                    }
                    else
                    {
                        _bus.Add(addr, dev);
                    }
                }
            }

            if (dev is Memory)
            {
                _mainMemory = (Memory)dev;
            }
        }

        public void Reset()
        {
            _memoryCycle = 0;
            _memoryAddress = 0;
            _memoryDataLow = 0;
            _memoryDataHigh = 0;
            _firstWordStored = false;
            _firstWordRead = false;
            _memoryOperationActive = false;
            _extendedMemoryReference = false;            
        }

        public ushort MAR
        {
            get { return _memoryAddress; }
        }

        public ushort MDLow
        {
            get { return _memoryDataLow; }
        }

        public ushort MDHigh
        {
            get { return _memoryDataHigh; }
        }

        public ushort MDWrite
        {
            get { return _memoryDataWrite; }
        }

        public int Cycle
        {
            get { return _memoryCycle; }
        }

        public bool Active
        {
            get { return _memoryOperationActive; }
        }

        /// <summary>
        /// Used for debugging only -- returns the (correctly mapped)
        /// word at the specified address
        /// </summary>
        public ushort DebugReadWord(ushort address)
        {
            // TODO: allow debug reads from any bank.
            // probably add special debug calls to IMemoryMappedDevice iface.
            return ReadFromBus(address, TaskType.Emulator, false);
        }

        public ushort DebugReadWord(TaskType task, ushort address)
        {
            // TODO: allow debug reads from any bank.
            // probably add special debug calls to IMemoryMappedDevice iface.
            return ReadFromBus(address, task, false);
        }

        public void Clock()
        {
            _memoryCycle++;
            if (_memoryOperationActive)
            {                  
                if (_systemType == SystemType.AltoI)
                {
                    ClockAltoI();
                }
                else
                {
                    ClockAltoII();
                }                
            }            
        }
         
        private void ClockAltoI()
        {
            switch (_memoryCycle)
            {
                case 4:
                    // Buffered read of single word
                    _memoryDataLow = ReadFromBus(_memoryAddress, _task, _extendedMemoryReference);
                    break;

                case 5:
                    // Buffered read of double-word
                    _memoryDataHigh = ReadFromBus((ushort)(_memoryAddress | 1), _task, _extendedMemoryReference);
                    break;

                case 7:
                    // End of memory operation
                    _memoryOperationActive = false;                    
                    break;
            }
        }

        private void ClockAltoII()
        {
            switch (_memoryCycle)
            {
                case 3:
                    // Buffered read of single word
                    _memoryDataLow = ReadFromBus(_memoryAddress, _task, _extendedMemoryReference);
                    break;

                case 4:
                    // Buffered read of double-word
                    _memoryDataHigh = ReadFromBus((ushort)(_memoryAddress ^ 1), _task, _extendedMemoryReference);
                    break;

                case 6:
                    // End of memory operation
                    _memoryOperationActive = false;                    
                    break;
            }
        }

        public bool Ready(MemoryOperation op)
        {
            if (_memoryOperationActive)
            {
                switch (op)
                {
                    case MemoryOperation.LoadAddress:
                        // Can't start a new Load operation until the current one is finished.
                        return false;

                    case MemoryOperation.Read:
                        // Read operations take place on cycles 5 and 6
                        return _memoryCycle > 4;

                    case MemoryOperation.Store:
                        if (_systemType == SystemType.AltoI)
                        {
                            // Store operations take place on cycles 5 and 6
                            return _memoryCycle > 4;
                        }
                        else
                        {
                            // Store operations take place on cycles 3 and 4
                            return _memoryCycle > 2;
                        }

                    default:
                        throw new InvalidOperationException(String.Format("Unexpected memory operation {0}", op));
                }
            }
            else
            {
                // Nothing running right now, we're ready for anything.
                return true;
            }
        }

        public void LoadMAR(ushort address, TaskType task, bool extendedMemoryReference)
        {
            //
            // This seems like as good a place as any to point out an oddity --
            // The Hardware Reference (Section 2, page 7) notes:
            //   "MAR<- cannot be invoked in the same instruction as <-MD of a previous access."
            // This rule is broken by the Butte microcode used by BravoX.  It appears to expect that
            // the MAR load takes place after MD has been read, which makes sense.
            //            
            if (_memoryOperationActive)
            {
                // This should not happen; CPU implementation should check whether the operation is possible 
                // using Ready and stall if not.
                throw new InvalidOperationException("Invalid LoadMAR request during active memory operation.");
            }
            else
            {
                _memoryOperationActive = true;
                _firstWordStored = false;
                _firstWordRead = false;
                _memoryAddress = address;
                _extendedMemoryReference = extendedMemoryReference;
                _task = task;
                _memoryCycle = 1;
            }                                 
            
        }        

        public ushort ReadMD()
        {           
            if (_systemType == SystemType.AltoI)
            {
                return ReadMDAltoI();
            }
            else
            {
                return ReadMDAltoII();
            }
        }

        private ushort ReadMDAltoI()
        {
            if (_memoryOperationActive)
            {
                switch (_memoryCycle)
                {
                    case 1:
                    case 2:

                        // TODO: good microcode should never do this
                        throw new InvalidOperationException("Unexpected microcode behavior -- ReadMD too soon after start of memory cycle.");
                    case 3:
                    case 4:
                        // This should not happen; CPU should check whether the operation is possible using Ready and stall if not.
                        throw new InvalidOperationException("Invalid ReadMD request during cycle 3 or 4 of memory operation.");

                    case 5:
                        // Single word read                       
                        return _memoryDataLow;

                    case 6:
                        // Double word read, return other half of double word.
                        return _memoryDataHigh;                                         

                    default:
                        // Invalid state.
                        throw new InvalidOperationException(string.Format("Unexpected memory cycle {0} in memory state machine.", _memoryCycle));
                }
            }
            else
            {
                // The Alto I does not latch memory contents, an <-MD operation returns undefined results
                return 0xffff;
            }
        }

        private ushort ReadMDAltoII()
        {
            if (_memoryOperationActive)
            {
                switch (_memoryCycle)
                {
                    case 1:
                    case 2:
                        // TODO: good microcode should never do this
                        throw new InvalidOperationException("Unexpected microcode behavior -- ReadMD too soon after start of memory cycle.");
                    case 3:
                    case 4:
                        // This should not happen; CPU should check whether the operation is possible using Ready and stall if not.
                        throw new InvalidOperationException("Invalid ReadMD request during cycle 3 or 4 of memory operation.");

                    case 5:
                        // Single word read.
                        //   If this is memory cycle 5 of a double-word *store* (started in cycle 3) then the second word can be *read* here.
                        //   (An example of this is provided in the hardware ref, section 2, pg 8. and is done by the Ethernet microcode on 
                        //   the Alto II, see the code block starting at address 0224 -- EPLOC (600) is loaded with the interface status, 
                        //   EBLOC (601) is read and OR'd with NWW in the same memory op.)                
                        _firstWordRead = true;                        
                        return _firstWordStored ? _memoryDataHigh : _memoryDataLow;

                    // ***
                    // NB: Handler for double-word read (cycle 6) is in the "else" clause below; this is kind of a hack.
                    // ***

                    default:
                        // Invalid state.
                        throw new InvalidOperationException(string.Format("Unexpected memory cycle {0} in memory state machine.", _memoryCycle));
                }
            }
            else
            {
                //
                // Memory state machine not running, just return last latched contents.
                // ("Because the Alto II latches memory contents, it is possible to execute _MD anytime after
                // cycle 5 of a reference and obtain the results of the read operation")
                // We will return the last half of the doubleword to complete a double-word read.
                // (If the first half hasn't yet been read, we return the first half instead...)
                //                
                return _firstWordRead ? _memoryDataHigh : _memoryDataLow;                
            }            
        }

        public void LoadMD(ushort data)
        {
            if (_memoryOperationActive)
            {
                if (_systemType == SystemType.AltoI)
                {
                    LoadMDAltoI(data);
                }
                else
                {
                    LoadMDAltoII(data);
                }                
            }
            else
            {
                //
                // This is illegal behavior but we won't throw here;
                // ST80 will cause this, for example, if you run it in a 1K CRAM configuration.
                // Because it expects 3K CRAM, it jumps into the middle of ROM1 and weird things happen.
                // We don't want to kill the emulator when this happens, but we will log the result.
                //
                Log.Write(LogType.Warning, LogComponent.Memory,
                    "Unexpected microcode behavior -- LoadMD while memory inactive (cycle {0}).", _memoryCycle);
            }
        }

        private void LoadMDAltoI(ushort data)
        {
            switch (_memoryCycle)
            {
                case 1:
                case 2:
                case 3:
                case 4:                
                    // Good microcode should never do this
                    throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD during incorrect memory cycle.");

                case 5:

                    _memoryDataWrite = data;
                    // Start of doubleword write:
                    WriteToBus(_memoryAddress, data, _task, _extendedMemoryReference);
                    _firstWordStored = true;
                    break;

                case 6:
                    if (!_firstWordStored)
                    {
                        throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD on cycle 6, no LoadMD on cycle 5.");
                    }

                    _memoryDataWrite = data;
                    ushort actualAddress = (ushort)(_memoryAddress | 1);

                    WriteToBus(actualAddress, data, _task, _extendedMemoryReference);
                    break;
            }

        }

        private void LoadMDAltoII(ushort data)
        {
            switch (_memoryCycle)
            {
                case 1:
                case 2:                
                    // Good microcode should never do this
                    throw new InvalidOperationException(
                        String.Format("Unexpected microcode behavior -- LoadMD during incorrect memory cycle {0}.", _memoryCycle));

                case 3:
                    _memoryDataWrite = data;
                    // Start of doubleword write:
                    WriteToBus(_memoryAddress, data, _task, _extendedMemoryReference);
                    _firstWordStored = true;                    
                    break;

                case 4:
                    _memoryDataWrite = data;
                    ushort actualAddress = _firstWordStored ? (ushort)(_memoryAddress ^ 1) : _memoryAddress;                    
                    WriteToBus(actualAddress, data, _task, _extendedMemoryReference);
                    break;

                case 5:
                    //
                    // This case is not documented in the HW ref. The ALU portion of MADTEST executes an instruction
                    // including an <-MD Bus Source (ANDed with Constant value) and an MD<- F2, which makes
                    // very little sense.  Since the read can't be accomplished until cycle 5, the instruction
                    // is blocked until then.  MADTEST doesn't seem to care what the result is, and I can't find
                    // any other code that uses microcode in this way.
                    // For now, this is a no-op.
                    //
                    Log.Write(LogType.Warning, LogComponent.Memory,
                        "Unexpected microcode behavior -- LoadMD during cycle 5.");
                    break;
            }

        }

        /// <summary>
        /// Dispatches reads to memory mapped hardware (RAM, I/O)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private ushort ReadFromBus(ushort address, TaskType task, bool extendedMemoryReference)
        {
            if (address <= Memory.RamTop)
            {
                // Main memory access; shortcut hashtable lookup for performance reasons.
                return _mainMemory.Read(address, task, extendedMemoryReference);
            }
            else
            {
                // Memory-mapped device access:
                // Look up address in hash; if populated ask the device
                // to return a value otherwise return 0.
                IMemoryMappedDevice memoryMappedDevice = null;
                if (_bus.TryGetValue(address, out memoryMappedDevice))
                {
                    return memoryMappedDevice.Read(address, task, extendedMemoryReference);
                }    
                else
                {                    
                    return 0;
                }           
            }
        }

        /// <summary>
        /// Dispatches writes to memory mapped hardware (RAM, I/O)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private void WriteToBus(ushort address, ushort data, TaskType task, bool extendedMemoryReference)
        {
            if (address <= Memory.RamTop)
            {
                // Main memory access; shortcut hashtable lookup for performance reasons.
                _mainMemory.Load(address, data, task, extendedMemoryReference);
            }
            else
            {
                // Memory-mapped device access:
                // Look up address in hash; if populated ask the device
                // to store a value otherwise do nothing.
                IMemoryMappedDevice memoryMappedDevice = null;
                if (_bus.TryGetValue(address, out memoryMappedDevice))
                {                    
                    memoryMappedDevice.Load(address, data, task, extendedMemoryReference);
                }            
            }
        }

        /// <summary>
        /// Hashtable used for address-based dispatch to devices on the memory bus.
        /// </summary>
        private Dictionary<ushort, IMemoryMappedDevice> _bus;

        /// <summary>
        /// Cache the system type since we rely on it        
        /// </summary>
        private SystemType _systemType;

        //
        // Optimzation: keep reference to main memory; since 99.9999% of accesses go directly there,
        // we can avoid the hashtable overhead using a simple address check.
        //
        private Memory _mainMemory;

        private bool _memoryOperationActive;
        private int _memoryCycle;
        private ushort _memoryAddress;
        private bool _extendedMemoryReference;
        private TaskType _task;

        // Buffered read data (on cycles 3 and 4)
        private ushort _memoryDataLow;
        private ushort _memoryDataHigh;

        // Write data (used for debugger UI only)
        private ushort _memoryDataWrite;

        // Indicates that the first word of a double-word store has taken place (started on cycle 3)
        private bool _firstWordStored;

        // Indicates tghat the first word of a double-word read has taken place (started on cycle 5)
        private bool _firstWordRead;
    }
}
