using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Memory
{
    /// <summary>
    /// Specifies a range of memory from Start to End, inclusive.
    /// </summary>
    public struct MemoryRange
    {
        public MemoryRange(ushort start, ushort end)
        {
            if (!(end > start))
            {
                throw new ArgumentOutOfRangeException("end must be greater than start.");
            }

            Start = start;
            End = end;
        }

        public bool Overlaps(MemoryRange other)
        {
            return ((other.Start >= this.Start && other.Start <= this.End) ||
                    (other.End >= this.Start && other.End <= this.End));
        }

        public ushort Start;
        public ushort End;
    }

    /// <summary>
    /// Specifies an interfaces for devices that appear in mapped memory.  This includes
    /// RAM as well as regular I/O devices.
    /// </summary>
    public interface IMemoryMappedDevice
    {
        /// <summary>
        /// Reads a word from the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ushort Read(int address);

        /// <summary>
        /// Writes a word to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        void Load(int address, ushort data);

        /// <summary>
        /// Specifies the range (or ranges) of addresses decoded by this device.
        /// </summary>
        MemoryRange[] Addresses { get; }
    }
}
