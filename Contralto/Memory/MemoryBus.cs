using Contralto.CPU;
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

                        if (dev is Memory)
                        {
                            _mainMemory = (Memory)dev;
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            _memoryCycle = 0;
            _memoryAddress = 0;
            _memoryData = 0;
            _doubleWordStore = false;
            _doubleWordMixed = false;
            _memoryOperationActive = false;
            _extendedMemoryReference = false;
        }

        public ushort MAR
        {
            get { return _memoryAddress; }
        }

        public ushort MD
        {
            get { return _memoryData; }
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
                    _memoryData = ReadFromBus(_memoryAddress, _task, _extendedMemoryReference);
                    break;

                case 5:
                    // Buffered read of double-word
                    _memoryData2 = ReadFromBus((ushort)(_memoryAddress | 1), _task, _extendedMemoryReference);
                    break;

                case 7:
                    // End of memory operation
                    _memoryOperationActive = false;
                    _doubleWordStore = false;
                    break;
            }
        }

        private void ClockAltoII()
        {
            switch (_memoryCycle)
            {
                case 3:
                    // Buffered read of single word
                    _memoryData = ReadFromBus(_memoryAddress, _task, _extendedMemoryReference);
                    break;

                case 4:
                    // Buffered read of double-word
                    _memoryData2 = ReadFromBus((ushort)(_memoryAddress ^ 1), _task, _extendedMemoryReference);
                    break;

                case 5:
                    // End of memory operation
                    _memoryOperationActive = false;
                    _doubleWordStore = false;
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
                            // // Store operations take place on cycles 5 and 6
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
            if (_memoryOperationActive)
            {
                // This should not happen; CPU implementation should check whether the operation is possible 
                // using Ready and stall if not.
                throw new InvalidOperationException("Invalid LoadMAR request during active memory operation.");
            }
            else
            {
                _memoryOperationActive = true;
                _doubleWordStore = false;
                _doubleWordMixed = false;
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
                        throw new InvalidOperationException("Invalid ReadMR request during cycle 3 or 4 of memory operation.");

                    case 5:
                        // Single word read                       
                        return _memoryData;

                    case 6:
                        // Double word read, return other half of double word.
                        return _memoryData2;                                         

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
                        throw new InvalidOperationException("Invalid ReadMR request during cycle 3 or 4 of memory operation.");

                    case 5:
                        // Single word read                       
                        return _memoryData;

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
                // memory state machine not running, just return last latched contents.
                // ("Because the Alto II latches memory contents, it is possible to execute _MD anytime after
                // cycle 5 of a reference and obtain the results of the read operation")
                // If this is memory cycle 6 we will return the last half of the doubleword to complete a double-word read.
                if (_memoryCycle == 6 || (_memoryCycle == 5 && _doubleWordMixed))
                {                    
                    _doubleWordMixed = false;                 
                    return _memoryData2;
                }
                else
                {
                    _doubleWordMixed = false;                    
                    return _memoryData;
                }
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
        }

        private void LoadMDAltoI(ushort data)
        {
            switch (_memoryCycle)
            {
                case 1:
                case 2:
                case 3:
                case 4:                
                    // TODO: good microcode should never do this
                    throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD during incorrect memory cycle.");

                case 5:

                    _memoryData = data; // Only really necessary to show in debugger
                                        // Start of doubleword write:
                    WriteToBus(_memoryAddress, data, _task, _extendedMemoryReference);
                    _doubleWordStore = true;
                    _doubleWordMixed = true;
                    break;

                case 6:
                    if (!_doubleWordStore)
                    {
                        throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD on cycle 6, no LoadMD on cycle 5.");
                    }

                    _memoryData = data; // Only really necessary to show in debugger                                 
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
                case 5:
                    // TODO: good microcode should never do this
                    throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD during incorrect memory cycle.");

                case 3:

                    _memoryData = data; // Only really necessary to show in debugger
                                        // Start of doubleword write:
                    WriteToBus(_memoryAddress, data, _task, _extendedMemoryReference);
                    _doubleWordStore = true;
                    _doubleWordMixed = true;
                    break;

                case 4:
                    _memoryData = data; // Only really necessary to show in debugger                                 
                    ushort actualAddress = _doubleWordStore ? (ushort)(_memoryAddress ^ 1) : _memoryAddress;

                    WriteToBus(actualAddress, data, _task, _extendedMemoryReference);
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
                // to return a value otherwise throw.
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
                // to store a value otherwise throw.
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
        private ushort _memoryData;
        private ushort _memoryData2;

        // Indicates a double-word store (started on cycle 3)
        private bool _doubleWordStore;

        // Indicates a mixed double-word store/load (started in cycle 3)
        private bool _doubleWordMixed;
    }
}
