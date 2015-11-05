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
            _mem = new ushort[0x10000];
        }

        public ushort Read(int address)
        {
            return _mem[address];
        }

        public void Load(int address, ushort data)
        {           
            _mem[address] = data;
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0, 0xfdff), // to 176777; IO page above this.
        };

        private ushort[] _mem;
    }
}
