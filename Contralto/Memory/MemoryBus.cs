using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Memory
{
    public static class MemoryBus
    {
        static MemoryBus()
        {
            _mem = new Memory();
            _memoryCycle = 0;
            _memoryAddress = 0;
            _memoryData = 0;
            _memoryOperationActive = false;
        }

        public static void Clock()
        {
            if (_memoryOperationActive)
            {
                _memoryCycle++;
                
                switch (_memoryCycle)
                {
                    case 3:
                        _memoryData = ReadFromBus(_memoryAddress);
                        break;

                    case 4:
                        _memoryData2 = ReadFromBus(_memoryAddress ^ 1);
                        break;

                    case 6:
                        _memoryOperationActive = false;
                        _doubleWordStore = false;
                        _memoryCycle = 0;
                        break;
                }
            }
        }

        public static void LoadMAR(ushort address)
        {
            if (_memoryOperationActive)
            {
                // TODO: stall CPU
            }
            else
            {
                _memoryOperationActive = true;
                _doubleWordStore = false;
                _memoryAddress = address;
                _memoryCycle = 1;
            }
        }

        public static ushort ReadMD()
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
                        // TODO: not ready yet; need to tell CPU to wait.

                        break;

                    case 5:
                        // Single word read
                        return _memoryData;                        

                    case 6:  // TODO: rectify with timing (doubleword read extends cycle to 6)
                        // Doubleword read:
                        return _memoryData2;                        

                    default:
                        // Invalid state.
                        throw new InvalidOperationException(string.Format("Unexpected memory cycle {0} in memory state machine.", _memoryCycle));
                }
            }
            else
            {                                
                // not running, just return last latched contents
                return _memoryData;
            }
        }

        public static void LoadMD(ushort data)
        {
            if (_memoryOperationActive)
            {
                switch (_memoryCycle)
                {
                    case 1:
                    case 2:
                    case 5:
                        // TODO: good microcode should never do this
                        throw new InvalidOperationException("Unexpected microcode behavior -- LoadMD too soon after start of memory cycle.");
                        break;

                    case 3:
                        // Start of doubleword write:
                        WriteToBus(_memoryAddress, data);
                        _doubleWordStore = true;
                        break;

                    case 4:                        
                        WriteToBus(_doubleWordStore ? _memoryAddress ^ 1 : _memoryAddress, data);
                        break;
                }       

            }
        }


        private static Memory _mem;
        private static bool _memoryOperationActive;
        private static int _memoryCycle;
        private static ushort _memoryAddress;
        
        // Buffered read data (on cycles 3 and 4)
        private static ushort _memoryData;
        private static ushort _memoryData2;

        // Indicates a double-word store (started on cycle 3)
        private static bool _doubleWordStore;
    }
}
