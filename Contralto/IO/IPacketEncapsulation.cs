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

using System.IO;


namespace Contralto.IO
{
    public delegate void ReceivePacketDelegate(MemoryStream data);

    /// <summary>
    /// Provides a generic interface for host network devices that can encapsulate
    /// Alto ethernet packets.
    /// </summary>
    public interface IPacketEncapsulation
    {
        /// <summary>
        /// Registers a callback delegate to handle packets that are received.
        /// </summary>
        /// <param name="callback"></param>
        void RegisterReceiveCallback(ReceivePacketDelegate callback);

        /// <summary>
        /// Sends the specified word array over the device.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="length"></param>
        void Send(ushort[] packet, int length);

        /// <summary>
        /// Shuts down the encapsulation provider.
        /// </summary>
        void Shutdown();
    }
}
