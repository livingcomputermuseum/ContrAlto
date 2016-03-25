using System.IO;


namespace Contralto.IO
{
    public delegate void ReceivePacketDelegate(MemoryStream data);

    public interface IPacketEncapsulation
    {
        /// <summary>
        /// Registers a callback delegate to handle packets that are received.
        /// </summary>
        /// <param name="callback"></param>
        void RegisterReceiveCallback(ReceivePacketDelegate callback);

        /// <summary>
        /// Sends the specified word array
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="length"></param>
        void Send(ushort[] packet, int length);

        /// <summary>
        /// Shuts down the encapsulation provider
        /// </summary>
        void Shutdown();
    }
}
