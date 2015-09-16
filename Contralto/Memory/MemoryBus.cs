using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Memory
{
    public enum MemoryOperation
    {
        LoadAddress,
        Read,
        Store
    }

    public class MemoryBus : IClockable
    {
        public MemoryBus()
        {
            _bus = new Dictionary<ushort, IMemoryMappedDevice>(65536);            
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
                            String.Format("Memory mapped address collision for dev {0} at address {1}", dev, OctalHelpers.ToOctal(addr)));
                    }
                    else
                    {
                        _bus.Add(addr, dev);
                    }
                }
            }
        }

        public void Reset()
        {
            _memoryCycle = 0;
            _memoryAddress = 0;
            _memoryData = 0;
            _memoryOperationActive = false;
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
            return ReadFromBus(address);
        }

        public void Clock()
        {
            _memoryCycle++;
            if (_memoryOperationActive)
            {                                
                switch (_memoryCycle)
                {
                    case 3:
                        // Buffered read of single word
                        _memoryData = ReadFromBus(_memoryAddress);
                        break;

                    case 4:
                        // Buffered read of double-word
                        _memoryData2 = ReadFromBus((ushort)(_memoryAddress ^ 1));
                        break;

                    case 5:
                        // End of memory operation
                        _memoryOperationActive = false;
                        _doubleWordStore = false;                        
                        break;
                }
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
                        // Write operations take place on cycles 3 and 4
                        return _memoryCycle > 2;

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

        public void LoadMAR(ushort address)
        {
            if (_memoryOperationActive)
            {
                // This should not happen; CPU should check whether the operation is possible using Ready and stall if not.
                throw new InvalidOperationException("Invalid LoadMAR request during active memory operation.");
            }
            else
            {
                _memoryOperationActive = true;
                _doubleWordStore = false;
                _memoryAddress = address;
                _memoryCycle = 1;
            }
        }

        public ushort ReadMD()
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
                        break;

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
                if (_memoryCycle == 6)
                {
                    return _memoryData2;
                }
                else
                {
                    return _memoryData;
                }
            }
        }

        public void LoadMD(ushort data)
        {
            if (_memoryOperationActive)
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
                        WriteToBus(_memoryAddress, data);
                        _doubleWordStore = true;
                        break;

                    case 4:
                        _memoryData = data; // Only really necessary to show in debugger
                        WriteToBus(_doubleWordStore ? (ushort)(_memoryAddress ^ 1) : _memoryAddress, data);
                        break;
                }       

            }
        }

        /// <summary>
        /// Dispatches reads to memory mapped hardware (RAM, I/O)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private ushort ReadFromBus(ushort address)
        {
            // Look up address in hash; if populated ask the device
            // to return a value otherwise throw.
            if (_bus.ContainsKey(address))
            {
                return _bus[address].Read(address);
            }
            else
            {
                throw new NotImplementedException(String.Format("Read from unimplemented memory-mapped I/O device at {0}.", OctalHelpers.ToOctal(address)));
                //Console.WriteLine("Read from unimplemented memory-mapped I/O device at {0}.", OctalHelpers.ToOctal(address));
                return 0;
            }
        }

        /// <summary>
        /// Dispatches writes to memory mapped hardware (RAM, I/O)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private void WriteToBus(ushort address, ushort data)
        {
            // Look up address in hash; if populated ask the device
            // to store a value otherwise throw.
            if (_bus.ContainsKey(address))
            {
                _bus[address].Load(address, data);
            }
            else
            {
                 throw new NotImplementedException(String.Format("Write to unimplemented memory-mapped I/O device at {0}.", OctalHelpers.ToOctal(address)));
                //Console.WriteLine("Write to unimplemented memory-mapped I/O device at {0}.", OctalHelpers.ToOctal(address));
            }
        }

        /// <summary>
        /// Hashtable used for address-based dispatch to devices on the memory bus.
        /// </summary>
        private Dictionary<ushort, IMemoryMappedDevice> _bus;
        
        private bool _memoryOperationActive;
        private int _memoryCycle;
        private ushort _memoryAddress;
        
        // Buffered read data (on cycles 3 and 4)
        private ushort _memoryData;
        private ushort _memoryData2;

        // Indicates a double-word store (started on cycle 3)
        private bool _doubleWordStore;
    }
}
