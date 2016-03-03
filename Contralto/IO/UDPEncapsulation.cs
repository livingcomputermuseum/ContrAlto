using System;
using System.Net;
using System.Net.Sockets;

using Contralto.Logging;
using System.Threading;
using System.Net.NetworkInformation;

namespace Contralto.IO
{    
    /// <summary>
    /// Implements the logic for encapsulating a 3mbit ethernet packet into/out of UDP datagrams.
    /// Sent packets are broadcast to the subnet.        
    /// </summary>
    public class UDPEncapsulation : IPacketEncapsulation
    {
        public UDPEncapsulation(string interfaceName)
        {
            // Try to set up UDP client.
            try
            {
                _udpClient = new UdpClient(_udpPort, AddressFamily.InterNetwork);
                _udpClient.EnableBroadcast = true;
                _udpClient.Client.Blocking = true;
                _udpClient.Client.MulticastLoopback = false;

                //
                // Grab the broadcast address for the interface so that we know what broadcast address to use
                // for our UDP datagrams.
                //
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

                IPInterfaceProperties props = null;
                foreach(NetworkInterface nic in nics)
                {
                    if (nic.Description == interfaceName)
                    {
                        props = nic.GetIPProperties();
                        break;
                    }
                }

                if (props == null)
                {
                    throw new InvalidOperationException(String.Format("No interface matching description '{0}' was found.", interfaceName));
                }

                foreach(UnicastIPAddressInformation unicast in props.UnicastAddresses)
                {
                    // Find the first InterNetwork address for this interface and 
                    // go with it.
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _thisIPAddress = unicast.Address;
                        _broadcastEndpoint = new IPEndPoint(GetBroadcastAddress(_thisIPAddress, unicast.IPv4Mask), _udpPort);
                        break;
                    }
                }
                
                if (_broadcastEndpoint == null)
                {
                    throw new InvalidOperationException(String.Format("No IPV4 network information was found for interface '{0}'.", interfaceName));
                }
                   
            }
            catch(Exception e)
            {
                Log.Write(LogType.Error, LogComponent.EthernetPacket,
                    "Error configuring UDP socket {0} for use with ContrAlto on interface {1}.  Ensure that the selected network interface is valid, configured properly, and that nothing else is using this port.",
                    _udpPort,
                    interfaceName);

                Log.Write(LogType.Error, LogComponent.EthernetPacket,
                    "Error was '{0}'.",
                    e.Message);

                _udpClient = null;
            }
        }        

        public void RegisterReceiveCallback(ReceivePacketDelegate callback)
        {   
            // UDP connection could not be configured, can't receive anything.                     
            if (_udpClient == null)
            {
                return;
            }

            // Set up input
            _callback = callback;
            BeginReceive();
        }


        /// <summary>
        /// Sends an array of bytes over the ethernet as a 3mbit packet encapsulated in a 10mbit packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="hostId"></param>
        public void Send(ushort[] packet, int length)
        {
            // UDP could not be configured, drop the packet.
            if (_udpClient == null)
            {
                return;
            }

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

            _udpClient.Send(packetBytes, packetBytes.Length, _broadcastEndpoint);            

            Log.Write(LogComponent.HostNetworkInterface, "Encapsulated 3mbit packet sent via UDP.");
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
            Log.Write(LogComponent.HostNetworkInterface, "UDP Receiver thread started.");
            
            IPEndPoint groupEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
            //return;

            while (true)
            {                
                byte[] data = _udpClient.Receive(ref groupEndPoint);

                // TODO: sanitize data before handing it off.

                // Drop our own UDP packets.
                if (!groupEndPoint.Address.Equals(_thisIPAddress))
                {
                    Log.Write(LogComponent.HostNetworkInterface, "Received UDP-encapsulated 3mbit packet.");
                    _callback(new System.IO.MemoryStream(data));
                }
            }
        }

        
        private IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }

            return new IPAddress(broadcastAddress);
        } 

        // Callback delegate for received data.
        private ReceivePacketDelegate _callback;

        // Thread used for receive
        private Thread _receiveThread;

        // UDP port (TODO: make configurable?)
        private const int _udpPort = 42424;
        private UdpClient _udpClient;
        private IPEndPoint _broadcastEndpoint;

        // The IP address (unicast address) of the interface we're using to send UDP datagrams.
        private IPAddress _thisIPAddress;
    }   
}
