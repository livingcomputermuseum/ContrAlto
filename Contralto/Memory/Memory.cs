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
            _mem = new ushort[0xffff];
        }

        public ushort Read(int address)
        {
            return _mem[address];
        }

        public void Load(int address, ushort data)
        {
            _mem[address] = data;
        }

        private ushort[] _mem;
    }
}
