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
using Contralto.Memory;

namespace Contralto.IO
{
    /// <summary>
    /// Implements the Organ Keyboard interface used by the ST-74
    /// Music System.  Very little is known about the hardware at this time,
    /// so most of this is speculation or based on disassembly/reverse-engineering
    /// of the music system code.
    /// 
    /// This is currently a stub that implements the bare minimum to make the
    /// music system think there's a keyboard attached to the system.
    /// </summary>
    public class OrganKeyboard : IMemoryMappedDevice
    {
        public OrganKeyboard(AltoSystem system)
        {
            _system = system;
            Reset();
        }

        public void Reset()
        {
            //
            // Initialize keyboard registers.
            // Based on disassembly of the Nova code that drives the keyboard
            // interface, the top 6 bits are active low.
            //
            for (int i = 0; i < 16; i++)
            {                
                _keyData[i] = (ushort)(0xfc00);
            }
        }

        /// <summary>
        /// Reads a word from the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="extendedMemory"></param>
        /// <returns></returns>
        public ushort Read(int address, TaskType task, bool extendedMemory)
        {

            Log.Write(LogType.Verbose, LogComponent.Organ, "Organ read from {0} by task {1} (bank {2}), Nova PC {3}",
                Conversion.ToOctal(address),
                task,
                UCodeMemory.GetBank(task),
                Conversion.ToOctal(_system.CPU.R[6]));

            return _keyData[address - 0xfe60];

        }

        /// <summary>
        /// Writes a word to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public void Load(int address, ushort data, TaskType task, bool extendedMemory)
        {

            // The registers are write-only as far as I've been able to ascertain.
            Log.Write(LogType.Verbose, LogComponent.Organ, "Unexpected organ write to {0} ({1}) by task {2} (bank {3})",
                Conversion.ToOctal(address),
                Conversion.ToOctal(data),
                task,
                UCodeMemory.GetBank(task));
        }

        /// <summary>
        /// Specifies the range (or ranges) of addresses decoded by this device.
        /// </summary>
        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }


        /// <summary>
        /// From: http://bitsavers.org/pdf/xerox/alto/memos_1975/Reserved_Alto_Memory_Locations_Jan75.pdf
        /// 
        /// #177140 - #177157: Organ Keyboard (Organ Hardware - Kaehler)
        /// </summary>
        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfe60, 0xfe6f),
        };

        private ushort[] _keyData = new ushort[16];

        private AltoSystem _system;
    }

}