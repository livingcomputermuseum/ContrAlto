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

using SharpPcap;
using SharpPcap.WinPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using PacketDotNet;

using System;
using System.Net.NetworkInformation;


using Contralto.Logging;


namespace Contralto.IO
{
    /// <summary>
    /// Represents a host ethernet interface.
    /// </summary>
    public struct EthernetInterface
    {
        public EthernetInterface(string name, string description)
        {
            Name = name;            
            Description = description;
        }        

        public override string ToString()
        {
            return String.Format("{0} ({1})", Name, Description);
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
        public HostEthernetEncapsulation(string name)
        {            
            // Find the specified device by name
            foreach (ICaptureDevice device in CaptureDeviceList.Instance)
            {
                if (device is WinPcapDevice)
                {
                    //
                    // We use the friendly name to make it easier to specify in config files.
                    //
                    if (((WinPcapDevice)device).Interface.FriendlyName.ToLowerInvariant() == name.ToLowerInvariant())
                    {
                        AttachInterface(device);
                        break;
                    }
                }
                else
                {
                    if (device.Name.ToLowerInvariant() == name.ToLowerInvariant())
                    {
                        AttachInterface(device);
                        break;
                    }
                }
            }

            if (_interface == null)
            {
                Log.Write(LogComponent.HostNetworkInterface, "Specified ethernet interface does not exist or is not compatible with ContrAlto.");
                throw new InvalidOperationException("Specified ethernet interface does not exist or is not compatible with ContrAlto.");
            }
        }

        public void RegisterReceiveCallback(ReceivePacketDelegate callback)
        {
            _callback = callback;

            // Now that we have a callback we can start receiving stuff.
            Open(false /* not promiscuous */, 0);
            BeginReceive();
        }

        public void Shutdown()
        {
            if (_interface != null)
            {
                try
                {
                    if (_interface.Started)
                    {
                        _interface.StopCapture();
                    }
                }
                catch
                {
                    // Eat exceptions.  The Pcap libs seem to throw on StopCapture on
                    // Unix platforms, we don't really care about them (since we're shutting down anyway)
                    // but this prevents debug spew from appearing on the console.
                }
                finally
                {
                    _interface.Close();
                }
            }
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
            packetBytes[0] = (byte)((length) >> 8);
            packetBytes[1] = (byte)(length);

            //
            // Do this annoying dance to stuff the ushorts into bytes because this is C#.
            //
            for (int i = 0; i < length; i++)
            {                
                packetBytes[i * 2 + 2] = (byte)(packet[i] >> 8);
                packetBytes[i * 2 + 3] = (byte)(packet[i]);
            }

            //
            // Grab the source and destination host addresses from the packet we're sending
            // and build 10mbit versions as necessary.
            //
            byte destinationHost = packetBytes[2];
            byte sourceHost = packetBytes[3];

            Log.Write(LogComponent.HostNetworkInterface, "Sending packet; source {0} destination {1}, length {2} words.",
                Conversion.ToOctal(sourceHost),
                Conversion.ToOctal(destinationHost),
                length);

            _10mbitMACPrefix[5] = Configuration.HostAddress;    // Stuff our current Alto host address into the 10mbit MAC

            EthernetPacket p = new EthernetPacket(
                new PhysicalAddress(_10mbitMACPrefix),          // Source address
                _10mbitBroadcast,                               // Destnation (broadcast)
                (EthernetPacketType)_3mbitFrameType);

            p.PayloadData = packetBytes;

            // Send it over the 'net!
            _interface.SendPacket(p);

            Log.Write(LogComponent.HostNetworkInterface, "Encapsulated 3mbit packet sent.");
        }

        private void ReceiveCallback(object sender, CaptureEventArgs e)
        {
            //
            // Filter out packets intended for the emulator, forward them on, drop everything else.
            //
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
            {
                EthernetPacket packet = (EthernetPacket)PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Ethernet, e.Packet.Data);

                _10mbitMACPrefix[5] = Configuration.HostAddress;

                if ((int)packet.Type == _3mbitFrameType &&                                                                  // encapsulated 3mbit frames
                    (packet.SourceHwAddress != new System.Net.NetworkInformation.PhysicalAddress(_10mbitMACPrefix)))        // and not sent by this emulator
                {
                    Log.Write(LogComponent.HostNetworkInterface, "Received encapsulated 3mbit packet.");
                    _callback(new System.IO.MemoryStream(packet.PayloadData));
                }
                else
                {
                    // Not for us, discard the packet.
                }
            }
        }

        private void AttachInterface(ICaptureDevice iface)
        {
            _interface = iface;

            if (_interface == null)
            {
                throw new InvalidOperationException("Requested interface not found.");
            }

            Log.Write(LogComponent.HostNetworkInterface, "Attached to host interface {0}", iface.Name);
        }

        private void Open(bool promiscuous, int timeout)
        {
            if (_interface is WinPcapDevice)
            {
                ((WinPcapDevice)_interface).Open(promiscuous ? OpenFlags.MaxResponsiveness | OpenFlags.Promiscuous : OpenFlags.MaxResponsiveness, timeout);
            }
            else if (_interface is LibPcapLiveDevice)
            {
                ((LibPcapLiveDevice)_interface).Open(promiscuous ? DeviceMode.Promiscuous : DeviceMode.Normal, timeout);
            }
            else if (_interface is AirPcapDevice)
            {
                ((AirPcapDevice)_interface).Open(promiscuous ? OpenFlags.MaxResponsiveness | OpenFlags.Promiscuous : OpenFlags.MaxResponsiveness, timeout);
            }            

            Log.Write(LogComponent.HostNetworkInterface, "Host interface opened and receiving packets.");
        }

        /// <summary>
        /// Begin receiving packets, forever.
        /// </summary>
        private void BeginReceive()
        {
            // Kick off receiver.
            _interface.OnPacketArrival += ReceiveCallback;
            _interface.StartCapture();
        }

        private ICaptureDevice _interface;
        private ReceivePacketDelegate _callback;

        private const int _3mbitFrameType = 0xbeef;     // easy to identify, ostensibly unused by anything of any import        

        /// <summary>
        /// On output, these bytes are prepended to the Alto's 3mbit (1 byte) address to form a full
        /// 6 byte Ethernet MAC.
        /// On input, ethernet frames are checked for this prefix.
        /// </summary>
        private byte[] _10mbitMACPrefix = { 0x00, 0x00, 0xaa, 0x01, 0x02, 0x00 };  // 00-00-AA is the Xerox vendor code, used just to be cute.  

        private PhysicalAddress _10mbitBroadcast = new PhysicalAddress(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }); 
    }   
}
