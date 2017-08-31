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

namespace Contralto.Memory
{
    /// <summary>
    /// Implements the Alto's main memory, up to 4 banks of 64KW in 16-bit words.
    /// Provides implementation of the IIXM's memory mapping hardware.
    /// </summary>
    public class Memory : IMemoryMappedDevice
    {
        public Memory()
        {
            // Set up handled addresses based on the system type.
            if (Configuration.SystemType == SystemType.AltoI)
            {
                _addresses = new MemoryRange[]
                {
                    new MemoryRange(0, _memTop),                                     // Main bank of RAM to 176777; IO page above this.                    
                };
            }
            else
            {
                _addresses = new MemoryRange[]
                {
                    new MemoryRange(0, _memTop),                                     // Main bank of RAM to 176777; IO page above this.
                    new MemoryRange(_xmBanksStart, (ushort)(_xmBanksStart + 16)),    // Memory bank registers
                };
            }

            Reset();
        }

        /// <summary>
        /// The top address of main memory (above which lies the I/O space)
        /// </summary>
        public static ushort RamTop
        {
            get { return _memTop; }
        }

        /// <summary>
        /// Full reset, clears all memory.
        /// </summary>
        public void Reset()
        {
            // 4 64K banks, regardless of system type.  (Alto Is just won't use the extra memory.)
            _mem = new ushort[0x40000];
            _xmBanks = new ushort[16];
            _xmBanksAlternate = new int[16];
            _xmBanksNormal = new int[16];
        }

        /// <summary>
        /// Soft reset, clears XM bank registers.
        /// </summary>
        public void SoftReset()
        {
            _xmBanks = new ushort[16];
            _xmBanksAlternate = new int[16];
            _xmBanksNormal = new int[16];
        }

        public ushort Read(int address, TaskType task, bool extendedMemory)
        {
            // Check for XM registers; this occurs regardless of XM flag since it's in the I/O page.
            if (address >= _xmBanksStart && address < _xmBanksStart + 16)
            {
                // NB: While not specified in documentatino, some code (IFS in particular) relies on the fact that
                // the upper 12 bits of the bank registers are all 1s.
                return (ushort)(0xfff0 | _xmBanks[address - _xmBanksStart]);
            }
            else
            {                   
                address += 0x10000 * GetBankNumber(task, extendedMemory);

                return _mem[address];
            }
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemory)
        {
            // Check for XM registers; this occurs regardless of XM flag since it's in the I/O page.
            if (address >= _xmBanksStart && address < _xmBanksStart + 16)
            {
                _xmBanks[address - _xmBanksStart] = data;

                // Precalc bank numbers to speed memory accesses that use them
                _xmBanksAlternate[address - _xmBanksStart] = data & 0x3;
                _xmBanksNormal[address - _xmBanksStart] = (data & 0xc) >> 2;

                Log.Write(LogComponent.Memory, "XM register for task {0} set to bank {1} (normal), {2} (xm)",
                    (TaskType)(address - _xmBanksStart),
                    (data & 0xc) >> 2,
                    (data & 0x3));
            }
            else
            {
                address += 0x10000 * GetBankNumber(task, extendedMemory);              
                _mem[address] = data;
            }
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        private int GetBankNumber(TaskType task, bool extendedMemory)
        {
            return extendedMemory ? _xmBanksAlternate[(int)task] : _xmBanksNormal[(int)task];
        }

        private readonly MemoryRange[] _addresses;

        private static readonly ushort _memTop = 0xfdff;         // 176777
        private static readonly ushort _xmBanksStart = 0xffe0;   // 177740

        private ushort[] _mem;

        private ushort[] _xmBanks;        
        private int[] _xmBanksAlternate;
        private int[] _xmBanksNormal;
    }
}
