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
using Contralto.Logging;
using System.Threading;

namespace Contralto.IO
{

    public struct EthernetInterface
    {
        public EthernetInterface(string name, string description)
        {
            Name = name;
            Description = description;            
        }

        public static List<EthernetInterface> EnumerateDevices()
        {
            List<EthernetInterface> interfaces = new List<EthernetInterface>();

            foreach (LivePacketDevice device in LivePacketDevice.AllLocalMachine)
            {
                interfaces.Add(new EthernetInterface(device.Name, device.Description));
            }

            return interfaces;
        }

        public override string ToString()
        {
            return Description;
        }

        public string Name;
        public string Description;        
    }    

    /// <summary>
    /// Implements the logic for encapsulating a 3mbit ethernet packet into a 10mb packet and sending it over an actual
    /// ethernet interface controlled by the host operating system.
    /// 
    /// This uses PCap.NET to do the dirty work.
    /// </summary>
    public class HostEthernetEncapsulation : IPacketEncapsulation
    {
        public HostEthernetEncapsulation(EthernetInterface iface)
        {
            AttachInterface(iface);
        }

        public HostEthernetEncapsulation(string name)
        {
            // Find the specified device by name
            List<EthernetInterface> interfaces = EthernetInterface.EnumerateDevices();

            foreach (EthernetInterface i in interfaces)
            {
                if (name == i.Name)
                {
                    AttachInterface(i);
                    return;
                }
            }

            throw new InvalidOperationException("Specified ethernet interface does not exist or is not compatible with WinPCAP.");
        }

        public void RegisterReceiveCallback(ReceivePacketDelegate callback)
        {
            _callback = callback;

            // Now that we have a callback we can start receiving stuff.
            Open(true /* promiscuous */, int.MaxValue);
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
            // Outgoing packet contains 1 extra word (2 bytes) containing
            // the prepended packet length (one word)            
            byte[] packetBytes = new byte[length * 2 + 2];

            //
            // First two bytes include the length of the 3mbit packet; since 10mbit packets have a minimum length of 46 
            // bytes, and 3mbit packets have no minimum length this is necessary so the receiver can pull out the 
            // correct amount of data.
            //
            packetBytes[0] = (byte)(length);
            packetBytes[1] = (byte)((length) >> 8);

            //
            // Do this annoying dance to stuff the ushorts into bytes because this is C#.
            //
            for (int i = 0; i < length; i++)
            {
                packetBytes[i * 2 + 2] = (byte)(packet[i]);
                packetBytes[i * 2 + 3] = (byte)(packet[i] >> 8);
            }

            //
            // Grab the source and destination host addresses from the packet we're sending
            // and build 10mbit versions.
            //
            byte destinationHost = packetBytes[3];
            byte sourceHost = packetBytes[2];

            Log.Write(LogComponent.HostNetworkInterface, "Sending packet; source {0} destination {1}, length {2} words.",
                Conversion.ToOctal(sourceHost),
                Conversion.ToOctal(destinationHost),
                length);

            MacAddress destinationMac = Get10mbitDestinationMacFrom3mbit(destinationHost);
            MacAddress sourceMac = new MacAddress((UInt48)(_10mbitMACPrefix | Configuration.HostAddress));

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

            Log.Write(LogComponent.HostNetworkInterface, "Encapsulated 3mbit packet sent.");
        }

        private void ReceiveCallback(Packet p)
        {
            //
            // Filter out packets intended for the emulator, forward them on, drop everything else.
            //
            if ((int)p.Ethernet.EtherType == _3mbitFrameType &&                                                 // encapsulated 3mbit frames
                ((p.Ethernet.Destination.ToValue() & 0xffffffffff00) == _10mbitMACPrefix ||                     //  addressed to any emulator OR
                 p.Ethernet.Destination.ToValue() == _10mbitBroadcast) &&                                       //  broadcast
                (p.Ethernet.Source.ToValue() != (UInt48)(_10mbitMACPrefix | Configuration.HostAddress)))        //  and not sent by this emulator                
            {
                Log.Write(LogComponent.HostNetworkInterface, "Received encapsulated 3mbit packet.");
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
                if (device.Description == iface.Name)
                {
                    _interface = device;
                    break;
                }
            }

            if (_interface == null)
            {
                throw new InvalidOperationException("Requested interface not found.");
            }

            Log.Write(LogComponent.HostNetworkInterface, "Attached to host interface {0}", iface.Name);
        }

        private void Open(bool promiscuous, int timeout)
        {
            _communicator = _interface.Open(65536, promiscuous ? PacketDeviceOpenAttributes.MaximumResponsiveness | PacketDeviceOpenAttributes.Promiscuous : PacketDeviceOpenAttributes.MaximumResponsiveness, timeout);

            // Set this to 1 so we'll get packets as soon as they arrive, no buffering.
            _communicator.SetKernelMinimumBytesToCopy(1);

            Log.Write(LogComponent.HostNetworkInterface, "Host interface opened and receiving packets.");
        }

        /// <summary>
        /// Begin receiving packets, forever.
        /// </summary>
        private void BeginReceive()
        {
            // Kick off receive thread.   
            _receiveThread = new Thread(ReceiveThread);
            _receiveThread.Start();
        }

        private void ReceiveThread()
        {
            // Just call ReceivePackets, that's it.  This will never return.
            // (probably need to make this more elegant so we can tear down the thread
            // properly.)
            Log.Write(LogComponent.HostNetworkInterface, "Receiver thread started.");
            _communicator.ReceivePackets(-1, ReceiveCallback);
        }

        private MacAddress Get10mbitDestinationMacFrom3mbit(byte destinationHost)
        {
            MacAddress destinationMac;
            
            if (destinationHost == _3mbitBroadcast)
            {
                // 3mbit broadcast gets translated to 10mbit broadcast
                destinationMac = new MacAddress(_10mbitBroadcast);
            }
            else
            {
                // Check addressing table for external (non emulator) addresses;
                // otherwise just address other emulators
                // TODO: implement table.  Currently hardcoded address 1 to test IFS on dev machine
                //
                if (destinationHost == 1)
                {
                    destinationMac = new MacAddress((UInt48)(_ifsTestMAC));
                }
                else
                {
                    destinationMac = new MacAddress((UInt48)(_10mbitMACPrefix | destinationHost));       // emulator destination address
                }
            
            }            

            return destinationMac;
        }    

        private LivePacketDevice _interface;
        private PacketCommunicator _communicator;
        private ReceivePacketDelegate _callback;


        // Thread used for receive
        private Thread _receiveThread;

        private const int _3mbitFrameType = 0xbeef;     // easy to identify, ostensibly unused by anything of any import        

        /// <summary>
        /// On output, these bytes are prepended to the Alto's 3mbit (1 byte) address to form a full
        /// 6 byte Ethernet MAC.
        /// On input, ethernet frames are checked for this prefix
        /// </summary>
        private UInt48 _10mbitMACPrefix = 0x0000aa010200;  // 00-00-AA is the Xerox vendor code, used just to be cute.  

        private UInt48 _10mbitBroadcast = (UInt48)0xffffffffffff;
        private const int _3mbitBroadcast = 0;

        // Temporary; to be replaced with an external address mapping table
        private UInt48 _ifsTestMAC = (UInt48)0x001060b88e3e;
    }   
}
