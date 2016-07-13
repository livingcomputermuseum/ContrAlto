/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using Contralto.CPU;
using Contralto.Logging;
using Contralto.Memory;
using System.IO;

namespace Contralto.IO
{
    /// <summary>
    /// Implements the hardware for Ted Kaehler's organ keyboard and DAC
    /// </summary>
    public class Music : IMemoryMappedDevice
    {
        public Music(AltoSystem system)
        {
            //_musicIo = new FileStream("c:\\alto\\mus.snd", FileMode.Create, FileAccess.ReadWrite);
            _system = system;
            Reset();
        }

        ~Music()
        {
            //_musicIo.Close();
        }

        public void Reset()
        {
            _foo = true;
        }

        /// <summary>
        /// Reads a word from the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="extendedMemory"></param>
        /// <returns></returns>
        public ushort Read(int address, TaskType task, bool extendedMemory)
        {            
            // debug for kaehler's music st              
            Log.Write(LogType.Verbose, LogComponent.Music, "MUSIC (I/O) read from {0} by task {1} (bank {2}), Nova PC {3}", 
                Conversion.ToOctal(address), 
                task, 
                UCodeMemory.GetBank(task),
                Conversion.ToOctal(_system.CPU.R[6]));

            if (address == 0xfffe)
            {
                return _lastDac;
            }
            else
            {
                _foo = !_foo;

                if (!_foo)
                {
                    return 0x800;
                }
                else
                {
                    return 0;
                }

                
               
            }
        }

        /// <summary>
        /// Writes a word to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public void Load(int address, ushort data, TaskType task, bool extendedMemory)
        {
            Log.Write(LogType.Verbose, LogComponent.Music, "MUSIC (I/O) write to {0} ({1}) by task {2} (bank {3})", 
                Conversion.ToOctal(address), 
                Conversion.ToOctal(data), 
                task, 
                UCodeMemory.GetBank(task));

            if (address == 0xfffe)
            {
                //_musicIo.WriteByte((byte)(data >> 8));
                //_musicIo.WriteByte((byte)data);
                _lastDac = data;
            }   
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
        /// #177776: Digital-Analog Converter (DAC Hardware - Kaehler)
        /// </summary>
        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfe60, 0xfe6f),        // Organ
            new MemoryRange(0xfffe, 0xfffe)         // DAC
        };

        private ushort _lastDac;

        private AltoSystem _system;

        private FileStream _musicIo;
        private bool _foo;
    }
}
