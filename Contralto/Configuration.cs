using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto
{
    /// <summary>
    /// The configuration of an Alto II to emulate
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        /// System with the standard 1K ROM, 1K RAM
        /// </summary>
        OneKRom,

        /// <summary>
        /// System with 2K ROM, 1K RAM
        /// </summary>
        TwoKRom,

        /// <summary>
        /// System with 3K RAM
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

            ReadConfiguration();
        }

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

        /// <summary>
        /// Reads the current configuration file from disk.
        /// 
        /// TODO: use reflection to do this.
        /// </summary>
        public static void ReadConfiguration()
        {
            try
            {
                using (StreamReader configStream = new StreamReader("contralto.cfg"))
                {
                    //
                    // Config file consists of text lines containing name / value pairs:
                    //      <Name> <Value>
                    // Whitespace is ignored
                    //
                    int lineNumber = 0;
                    while (!configStream.EndOfStream)
                    {
                        lineNumber++;
                        string line = configStream.ReadLine().Trim();

                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // Find whitespace separating tokens
                        int ws = line.IndexOfAny(new char[] { ' ', '\t' });

                        if (ws < 1)
                        {
                            Log.Write(LogType.Warning, LogComponent.Configuration, "Syntax error on line {0}.  Ignoring.", lineNumber);
                            continue;
                        }

                        string parameter = line.Substring(0, ws);
                        string value = line.Substring(ws + 1, line.Length - ws - 1);

                        try
                        {
                            switch (parameter.ToLowerInvariant())
                            {
                                case "drive0image":
                                    Drive0Image = value;
                                    break;

                                case "drive1image":
                                    Drive1Image = value;
                                    break;

                                case "systemtype":
                                    SystemType = (SystemType)Enum.Parse(typeof(SystemType), value, true);
                                    break;

                                case "hostaddress":
                                    HostAddress = Convert.ToByte(value, 8);
                                    break;

                                case "hostpacketinterfacename":
                                    HostPacketInterfaceName = value;
                                    break;

                                case "hostpacketinterfacetype":
                                    HostPacketInterfaceType = (PacketInterfaceType)Enum.Parse(typeof(PacketInterfaceType), value, true);
                                    break;

                                case "alternateboottype":
                                    AlternateBootType = (AlternateBootType)Enum.Parse(typeof(AlternateBootType), value, true);
                                    break;

                                case "bootaddress":
                                    BootAddress = Convert.ToUInt16(value, 8);
                                    break;

                                case "bootfile":
                                    BootFile = Convert.ToUInt16(value, 8);
                                    break;

                                case "interlacedisplay":
                                    InterlaceDisplay = bool.Parse(value);
                                    break;

                                case "throttlespeed":
                                    ThrottleSpeed = bool.Parse(value);
                                    break;

                                default:
                                    Log.Write(LogType.Warning, LogComponent.Configuration, "Invalid parameter on line {0}.  Ignoring.", lineNumber);
                                    break;
                            }
                        }
                        catch
                        {
                            Log.Write(LogType.Warning, LogComponent.Configuration, "Invalid value on line {0}.  Ignoring.", lineNumber);
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Log.Write(LogType.Warning, LogComponent.Configuration, "Configuration file 'contralto.cfg' could not be read; assuming default settings.");
                WriteConfiguration();
            }
        }

        /// <summary>
        /// Commits the current configuration to disk.
        /// </summary>
        public static void WriteConfiguration()
        {
            try
            {
                using (StreamWriter configStream = new StreamWriter("contralto.cfg"))
                {
                    if (!string.IsNullOrEmpty(Drive0Image))
                    {
                        configStream.WriteLine("Drive0Image {0}", Drive0Image);
                    }

                    if (!string.IsNullOrEmpty(Drive1Image))
                    {
                        configStream.WriteLine("Drive1Image {0}", Drive1Image);
                    }

                    configStream.WriteLine("SystemType {0}", SystemType);
                    configStream.WriteLine("HostAddress {0}", Conversion.ToOctal(HostAddress));

                    if (!string.IsNullOrEmpty(HostPacketInterfaceName))
                    {
                        configStream.WriteLine("HostPacketInterfaceName {0}", HostPacketInterfaceName);
                    }

                    configStream.WriteLine("HostPacketInterfaceType {0}", HostPacketInterfaceType);
                    configStream.WriteLine("AlternateBootType {0}", AlternateBootType);
                    configStream.WriteLine("BootAddress {0}", Conversion.ToOctal(BootAddress));
                    configStream.WriteLine("BootFile {0}", Conversion.ToOctal(BootFile));
                    configStream.WriteLine("InterlaceDisplay {0}", InterlaceDisplay);
                    configStream.WriteLine("ThrottleSpeed {0}", ThrottleSpeed);
                }
            }
            catch (Exception)
            {
                Log.Write(LogType.Warning, LogComponent.Configuration, "Configuration file 'contralto.cfg' could not be opened for writing.");
            }
        }
    }

}
