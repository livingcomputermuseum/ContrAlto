using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Memory;
using Contralto.CPU;
using Contralto.Logging;

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

        public ushort Read(int address, TaskType task, bool extendedMemoryReference)
        {
            // TODO: implement; return nothing pressed for any address now.
            Log.Write(LogComponent.Keyboard, "Keyboard read; unimplemented.");
            return 0xffff;
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemoryReference)
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
