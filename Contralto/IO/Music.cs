using Contralto.CPU;
using Contralto.Logging;
using Contralto.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
