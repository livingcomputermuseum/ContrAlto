using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using System.IO;

namespace Contralto.IO
{

    public struct EthernetInterface
    {
        public EthernetInterface(string name, string description, MacAddress macAddress)
        {
            Name = name;
            Description = description;
            MacAddress = macAddress;
        }

        public static List<EthernetInterface> EnumerateDevices()
        {
            List<EthernetInterface> interfaces = new List<EthernetInterface>();

            foreach (LivePacketDevice device in LivePacketDevice.AllLocalMachine)
            {
                interfaces.Add(new EthernetInterface(device.Name, device.Description, device.GetMacAddress()));
            }

            return interfaces;
        }

        public string Name;
        public string Description;
        public MacAddress MacAddress;
    }

    public delegate void ReceivePacketDelegate(MemoryStream data);

    /// <summary>
    /// Implements the logic for encapsulating a 3mbit ethernet packet into a 10mb packet and sending it over an actual
    /// interface controlled by the host operating system.
    /// 
    /// This uses PCap.NET to do the dirty work.
    /// </summary>
    public class HostEthernet
    {
        public HostEthernet(EthernetInterface iface)
        {
            AttachInterface(iface);           
        }

        public void RegisterReceiveCallback(ReceivePacketDelegate callback)
        {
            _callback = callback;

            // Now that we have a callback we can start receiving stuff.
            Open(false /* not promiscuous */, int.MaxValue);
            BeginReceive();
        }


        /// <summary>
        /// Sends an array of bytes over the ethernet as a 3mbit packet encapsulated in a 10mbit packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="hostId"></param>
        public void Send(ushort[] packet, int length)
        {
            // Sanity check.
            if (length < 1)
            {
                throw new InvalidOperationException("Raw packet data must contain at least two bytes for addressing.");                
            }

            //
            // Do this annoying dance to stuff the ushorts into bytes because this is C#.
            //
            byte[] packetBytes = new byte[length * 2];

            for (int i = 0; i < length; i++)
            {
                packetBytes[i * 2] = (byte)(packet[i] >> 8);
                packetBytes[i * 2 + 1] = (byte)packet[i];
            }

            //
            // Grab the source and destination host addresses from the packet we're sending
            // and build 10mbit versions.
            //
            byte destinationHost = packetBytes[0];
            byte sourceHost = packetBytes[1];

            MacAddress destinationMac = new MacAddress((UInt48)(_10mbitMACPrefix | destinationHost));
            MacAddress sourceMac = new MacAddress((UInt48)(_10mbitMACPrefix | sourceHost));

            // Build the outgoing packet; place the source/dest addresses, type field and the raw data.                
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = sourceMac,
                Destination = destinationMac,
                EtherType = (EthernetType)_3mbitFrameType,
            };

            PayloadLayer payloadLayer = new PayloadLayer
            {
                Data = new Datagram(packetBytes),
            };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, payloadLayer);

            // Send it over the 'net!
            _communicator.SendPacket(builder.Build(DateTime.Now));           
        }

        private void ReceiveCallback(Packet p)
        {
            //
            // Filter out packets intended for the emulator, forward them on, drop everything else.
            //
            if ((int)p.Ethernet.EtherType == _3mbitFrameType &&
                (p.Ethernet.Destination.ToValue() & 0xffffffffff00) == _10mbitMACPrefix )
            {
                _callback(p.Ethernet.Payload.ToMemoryStream());                
            }
            else
            {
                // Not for us, discard the packet.                
            }
        }

        private void AttachInterface(EthernetInterface iface)
        {
            _interface = null;

            // Find the specified device by name
            foreach (LivePacketDevice device in LivePacketDevice.AllLocalMachine)
            {
                if (device.Name == iface.Name && device.GetMacAddress() == iface.MacAddress)
                {
                    _interface = device;
                    break;
                }
            }

            if (_interface == null)
            {
                throw new InvalidOperationException("Requested interface not found.");
            }
        }

        private void Open(bool promiscuous, int timeout)
        {
            _communicator = _interface.Open(0xffff, promiscuous ? PacketDeviceOpenAttributes.Promiscuous : PacketDeviceOpenAttributes.None, timeout);
        }

        /// <summary>
        /// Begin receiving packets, forever.
        /// </summary>
        private void BeginReceive()
        {
            _communicator.ReceivePackets(-1, ReceiveCallback);
        }        

        private LivePacketDevice _interface;
        private PacketCommunicator _communicator;
        private ReceivePacketDelegate _callback;

        private const int _3mbitFrameType = 0xbeef;     // easy to identify, ostensibly unused by anything of any import

        /// <summary>
        /// On output, these bytes are prepended to the Alto's 3mbit (1 byte) address to form a full
        /// 6 byte Ethernet MAC.
        /// On input, ethernet frames are checked for this prefix
        /// </summary>
        private UInt48 _10mbitMACPrefix = 0x0000aa010200;  // 00-00-AA is the old Xerox vendor code, used just to be cute.        
    }   
}
