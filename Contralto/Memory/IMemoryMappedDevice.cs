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

using System;
using Contralto.CPU;

namespace Contralto.Memory
{
    /// <summary>
    /// Specifies a range of memory from Start to End, inclusive.
    /// </summary>
    public struct MemoryRange
    {
        public MemoryRange(ushort start, ushort end)
        {
            if (!(end >= start))
            {
                throw new ArgumentOutOfRangeException("end must be greater than or equal to start.");
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
        /// <param name="extendedMemory"></param>
        /// <returns></returns>
        ushort Read(int address, TaskType task, bool extendedMemory);

        /// <summary>
        /// Writes a word to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        void Load(int address, ushort data, TaskType task, bool extendedMemory);

        /// <summary>
        /// Specifies the range (or ranges) of addresses decoded by this device.
        /// </summary>
        MemoryRange[] Addresses { get; }
    }
}
