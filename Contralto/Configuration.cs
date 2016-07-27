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

using Contralto.Logging;
using System;
using System.IO;

namespace Contralto
{
    /// <summary>
    /// The configuration of the Alto to emulate
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        /// Alto I System with 1K ROM, 1K RAM
        /// </summary>
        AltoI,

        /// <summary>
        /// Alto II XM System with the standard 1K ROM, 1K RAM
        /// </summary>
        OneKRom,

        /// <summary>
        /// Alto II XM System with 2K ROM, 1K RAM
        /// </summary>
        TwoKRom,

        /// <summary>
        /// Alto II XM System with 3K RAM
        /// </summary>
        ThreeKRam,
    }

    public enum PacketInterfaceType
    {
        /// <summary>
        /// Encapsulate frames inside raw ethernet frames on the host interface.
        /// Requires PCAP.
        /// </summary>
        EthernetEncapsulation,

        /// <summary>
        /// Encapsulate frames inside UDP datagrams on the host interface.
        /// </summary>
        UDPEncapsulation,

        /// <summary>
        /// No encapsulation; sent packets are dropped on the floor and no packets are received.
        /// </summary>
        None,
    }

    public enum AlternateBootType
    {
        None,
        Disk,
        Ethernet,
    }

    public enum PlatformType
    {
        Windows,
        Unix
    }

    /// <summary>
    /// Encapsulates user-configurable settings.  To be enhanced.
    /// </summary>
    public class Configuration
    {
        static Configuration()
        {
            // Initialize things to defaults.            
            HostAddress = 0x22;

            AlternateBootType = AlternateBootType.Disk;
            BootAddress = 0;
            BootFile = 0;

            SystemType = SystemType.TwoKRom;

            InterlaceDisplay = false;

            ThrottleSpeed = true;

            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    Platform = PlatformType.Unix;
                    break;

                default:
                    Platform = PlatformType.Windows;
                    break;
            }

            ReadConfiguration();
        }

        /// <summary>
        /// What kind of system we're running on.  (Not technically configurable.)
        /// </summary>
        public static PlatformType Platform;

        /// <summary>
        /// The type of Alto II to emulate
        /// </summary>
        public static SystemType SystemType;

        /// <summary>
        /// The currently loaded image for Drive 0
        /// </summary>
        public static string Drive0Image;

        /// <summary>
        /// The currently loaded image for Drive 1
        /// </summary>
        public static string Drive1Image;

        /// <summary>
        /// The Ethernet host address for this Alto
        /// </summary>
        public static byte HostAddress;

        /// <summary>
        /// The name of the Ethernet adaptor on the emulator host to use for Ethernet emulation
        /// </summary>
        public static string HostPacketInterfaceName;

        /// <summary>
        /// Whether any packet interfaces are available on the host
        /// </summary>
        public static bool HostRawEthernetInterfacesAvailable;

        /// <summary>
        /// The type of interface to use to host networking.
        /// </summary>
        public static PacketInterfaceType HostPacketInterfaceType;        

        /// <summary>
        /// The type of Alternate Boot to apply
        /// </summary>
        public static AlternateBootType AlternateBootType;

        /// <summary>
        /// The address to boot at reset for a disk alternate boot
        /// </summary>
        public static ushort BootAddress;

        /// <summary>
        /// The file to boot at reset for an ethernet alternate boot
        /// </summary>
        public static ushort BootFile;

        /// <summary>
        /// Whether to render the display "interlaced" or not.
        /// </summary>
        public static bool InterlaceDisplay;

        /// <summary>
        /// Whether to cap execution speed at native execution speed or not.
        /// </summary>
        public static bool ThrottleSpeed;

        public static string GetAltoIRomPath(string romFileName)
        {
            return Path.Combine("ROM", "AltoI", romFileName);               
        }

        public static string GetAltoIIRomPath(string romFileName)
        {
            return Path.Combine("ROM", "AltoII", romFileName);
        }

        public static string GetRomPath(string romFileName)
        {
            return Path.Combine("ROM", romFileName);
        }        

        /// <summary>
        /// Reads the current configuration file from the app's configuration.
        /// 
        /// TODO: use reflection to do this.
        /// </summary>
        public static void ReadConfiguration()
        {
            Drive0Image = (string)Properties.Settings.Default["Drive0Image"];
            Drive1Image = (string)Properties.Settings.Default["Drive1Image"];            
            SystemType = (SystemType)Properties.Settings.Default["SystemType"];
            HostAddress = (byte)Properties.Settings.Default["HostAddress"];
            HostPacketInterfaceName = (string)Properties.Settings.Default["HostPacketInterfaceName"];
            HostPacketInterfaceType = (PacketInterfaceType)Properties.Settings.Default["HostPacketInterfaceType"];
            AlternateBootType = (AlternateBootType)Properties.Settings.Default["AlternateBootType"];
            BootAddress = (ushort)Properties.Settings.Default["BootAddress"];
            BootFile = (ushort)Properties.Settings.Default["BootFile"];
            InterlaceDisplay = (bool)Properties.Settings.Default["InterlaceDisplay"];
            ThrottleSpeed = (bool)Properties.Settings.Default["ThrottleSpeed"];
        }

        /// <summary>
        /// Commits the current configuration to the app's settings.
        /// </summary>
        public static void WriteConfiguration()
        {
            Properties.Settings.Default["Drive0Image"] = Drive0Image;
            Properties.Settings.Default["Drive1Image"] = Drive1Image;
            Properties.Settings.Default["SystemType"] = (int)SystemType;
            Properties.Settings.Default["HostAddress"] = HostAddress;
            Properties.Settings.Default["HostPacketInterfaceName"] = HostPacketInterfaceName;
            Properties.Settings.Default["HostPacketInterfaceType"] = (int)HostPacketInterfaceType;
            Properties.Settings.Default["AlternateBootType"] = (int)AlternateBootType;
            Properties.Settings.Default["BootAddress"] = BootAddress;
            Properties.Settings.Default["BootFile"] = BootFile;
            Properties.Settings.Default["InterlaceDisplay"] = InterlaceDisplay;
            Properties.Settings.Default["ThrottleSpeed"] = ThrottleSpeed;

            Properties.Settings.Default.Save();
        }
    }

}
