using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;

namespace Contralto.IO
{
    /// <summary>
    /// Currently just a stub indicating that no keys are being pressed.
    /// </summary>
    public class Keyboard : IMemoryMappedDevice
    {
        public Keyboard()
        {
            
        }

        public ushort Read(int address)
        {
            // TODO: implement; return nothing pressed for any address now.
            return 0xffff;
        }

        public void Load(int address, ushort data)
        {
            // nothing
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }

        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfe1c, 0xfe1f), // 177034-177037 
        };
    }
}
