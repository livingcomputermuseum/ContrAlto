using Contralto.CPU;
using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Memory
{
    public class Memory : IMemoryMappedDevice
    {
        public Memory()
        {
            Reset();
        }

        public void Reset()
        {
            // 4 64K banks
            _mem = new ushort[0x40000];
            _xmBanks = new ushort[16];
        }

        public ushort Read(int address, TaskType task, bool extendedMemory)
        {
            // Check for XM registers; this occurs regardless of XM flag since it's in the I/O page.
            if (address >= _xmBanksStart && address < _xmBanksStart + 16)
            {
                return _xmBanks[address - _xmBanksStart];
            }
            else
            {
                address += 0x10000 * GetBankNumber(task, extendedMemory);
                ushort data = _mem[address];

                if (extendedMemory)
                {
                    Log.Write(LogComponent.Memory, "extended memory read from {0} - {1}", Conversion.ToOctal(address), Conversion.ToOctal(data));
                }

                return data;
            }
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemory)
        {
            // Check for XM registers; this occurs regardless of XM flag since it's in the I/O page.
            if (address >= _xmBanksStart && address < _xmBanksStart + 16)
            {
                _xmBanks[address - _xmBanksStart] = data;
                Log.Write(LogComponent.Memory, "XM register for task {0} set to bank {1} (normal), {2} (xm)",
                    (TaskType)(address - _xmBanksStart),
                    (data & 0xc) >> 2,
                    (data & 0x3));
            }
            else
            {
                address += 0x10000 * GetBankNumber(task, extendedMemory);
                _mem[address] = data;

                if (extendedMemory)
                {
                    Log.Write(LogComponent.Memory, "extended memory write to {0} of {1}", Conversion.ToOctal(address), Conversion.ToOctal(data));
                }
            }
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        private int GetBankNumber(TaskType task, bool extendedMemory)
        {
            return extendedMemory ? _xmBanks[(int)task] & 0x3 : (_xmBanks[(int)task] & 0xc) >> 2;
        }

        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0, _memTop),         // Main bank of RAM to 176777; IO page above this.
            new MemoryRange(_xmBanksStart, _xmBanksStart + 16),    // Memory bank registers
        };

        private const int _memTop = 0xfdff;         // 176777
        private const int _xmBanksStart = 0xffe0;   // 177740

        private ushort[] _mem;

        private ushort[] _xmBanks;
    }
}
