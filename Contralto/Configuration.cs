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
using System.Reflection;

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
            
            BootAddress = 0;
            BootFile = 0;

            SystemType = SystemType.OneKRom;

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

            // See if PCap is available.
            TestPCap();

            ReadConfiguration();

            // Special case: On first startup, AlternateBoot will come back as "None" which
            // is an invalid UI setting; default to Ethernet in this case.
            if (AlternateBootType == AlternateBootType.None)
            {
                AlternateBootType = AlternateBootType.Ethernet;
            }
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

        /// <summary>
        /// Whether to enable the DAC used for the Smalltalk music system.
        /// </summary>
        public static bool EnableAudioDAC;

        /// <summary>
        /// Whether to enable capture of the DAC output to file.
        /// </summary>
        public static bool EnableAudioDACCapture;

        /// <summary>
        /// The path to store DAC capture (if enabled).
        /// </summary>
        public static string AudioDACCapturePath;

        /// <summary>
        /// The components to enable debug logging for.
        /// </summary>
        public static LogComponent LogComponents;

        /// <summary>
        /// The types of logging to enable.
        /// </summary>
        public static LogType LogTypes;
        

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
        /// Reads the current configuration file from the appropriate place.
        /// </summary>
        public static void ReadConfiguration()
        {            
            if (Configuration.Platform == PlatformType.Windows
                && Program.StartupArgs.Length == 0)
            {
                //
                // By default, on Windows we use the app Settings functionality
                // to store settings in the registry on a per-user basis.
                // If a configuration file is specified, we will use it instead.
                //
                ReadConfigurationWindows();
            }
            else
            {
                //
                // On UNIX platforms we read in a configuration file.
                // This is mostly because Mono's support for Properties.Settings
                // is broken in inexplicable ways and I'm tired of fighting with it.
                //
                ReadConfigurationUnix();
            }
        }

        /// <summary>
        /// Commits the current configuration to the app's settings.
        /// </summary>
        public static void WriteConfiguration()
        {
            if (Configuration.Platform == PlatformType.Windows)
            {
                WriteConfigurationWindows();
            }
            else
            {
                //
                // At the moment the configuration files are read-only
                // on UNIX platforms.
                //
            }
        }

        private static void ReadConfigurationWindows()
        {
            AudioDACCapturePath = Properties.Settings.Default.AudioDACCapturePath;
            Drive0Image = Properties.Settings.Default.Drive0Image;
            Drive1Image = Properties.Settings.Default.Drive1Image;
            SystemType = (SystemType)Properties.Settings.Default.SystemType;
            HostAddress = Properties.Settings.Default.HostAddress;
            HostPacketInterfaceName = Properties.Settings.Default.HostPacketInterfaceName;
            HostPacketInterfaceType = (PacketInterfaceType)Properties.Settings.Default.HostPacketInterfaceType;
            AlternateBootType = (AlternateBootType)Properties.Settings.Default.AlternateBootType;
            BootAddress = Properties.Settings.Default.BootAddress;
            BootFile = Properties.Settings.Default.BootFile;
            InterlaceDisplay = Properties.Settings.Default.InterlaceDisplay;
            ThrottleSpeed = Properties.Settings.Default.ThrottleSpeed;
            EnableAudioDAC = Properties.Settings.Default.EnableAudioDAC;
            EnableAudioDACCapture = Properties.Settings.Default.EnableAudioDACCapture;
            AudioDACCapturePath = Properties.Settings.Default.AudioDACCapturePath;
        }

        private static void WriteConfigurationWindows()
        {
            Properties.Settings.Default.Drive0Image = Drive0Image;
            Properties.Settings.Default.Drive1Image = Drive1Image;
            Properties.Settings.Default.SystemType = (int)SystemType;
            Properties.Settings.Default.HostAddress = HostAddress;
            Properties.Settings.Default.HostPacketInterfaceName = HostPacketInterfaceName;
            Properties.Settings.Default.HostPacketInterfaceType = (int)HostPacketInterfaceType;
            Properties.Settings.Default.AlternateBootType = (int)AlternateBootType;
            Properties.Settings.Default.BootAddress = BootAddress;
            Properties.Settings.Default.BootFile = BootFile;
            Properties.Settings.Default.InterlaceDisplay = InterlaceDisplay;
            Properties.Settings.Default.ThrottleSpeed = ThrottleSpeed;
            Properties.Settings.Default.EnableAudioDAC = EnableAudioDAC;
            Properties.Settings.Default.EnableAudioDACCapture = EnableAudioDACCapture;
            Properties.Settings.Default.AudioDACCapturePath = AudioDACCapturePath;
            Properties.Settings.Default.AudioDACCapturePath = Properties.Settings.Default.AudioDACCapturePath;
            Properties.Settings.Default.Save();
        }

        private static void ReadConfigurationUnix()
        {
            string configFilePath = null;

            if (Program.StartupArgs.Length > 0)
            {
                configFilePath = Program.StartupArgs[0];
            }
            else
            {
                // No config file specified, default.
                configFilePath = "Contralto.cfg";
            }

            //
            // Check that the configuration file exists.
            // If not, we will warn the user and use default settings.
            //
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Configuration file {0} does not exist or cannot be accessed.  Using default settings.", configFilePath);
                return;
            }

            using (StreamReader configStream = new StreamReader(configFilePath))
            {
                //
                // Config file consists of text lines containing name / value pairs:
                //      <Name>=<Value>
                // Whitespace is ignored.
                //
                int lineNumber = 0;
                while (!configStream.EndOfStream)
                {
                    lineNumber++;
                    string line = configStream.ReadLine().Trim();

                    if (string.IsNullOrEmpty(line))
                    {
                        // Empty line, ignore.
                        continue;
                    }

                    if (line.StartsWith("#"))
                    {
                        // Comment to EOL, ignore.
                        continue;
                    }

                    // Find the '=' separating tokens and ensure there are just two.
                    string[] tokens = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length < 2)
                    {
                        Console.WriteLine(
                            "{0} line {1}: Invalid syntax.", configFilePath, lineNumber);
                        continue;
                    }

                    string parameter = tokens[0].Trim();
                    string value = tokens[1].Trim();

                    // Reflect over the public, static properties in this class and see if the parameter matches one of them
                    // If not, it's an error, if it is then we attempt to coerce the value to the correct type.
                    System.Reflection.FieldInfo[] info = typeof(Configuration).GetFields(BindingFlags.Public | BindingFlags.Static);

                    bool bMatch = false;
                    foreach (FieldInfo field in info)
                    {
                        // Case-insensitive compare.
                        if (field.Name.ToLowerInvariant() == parameter.ToLowerInvariant())
                        {
                            bMatch = true;

                            //
                            // Switch on the type of the field and attempt to convert the value to the appropriate type.
                            // At this time we support only strings and integers.
                            //
                            try
                            {
                                switch (field.FieldType.Name)
                                {
                                    case "Int32":
                                        {
                                            int v = Convert.ToInt32(value, 8);
                                            field.SetValue(null, v);
                                        }
                                        break;

                                    case "UInt16":
                                        {
                                            UInt16 v = Convert.ToUInt16(value, 8);
                                            field.SetValue(null, v);
                                        }
                                        break;

                                    case "Byte":
                                        {
                                            byte v = Convert.ToByte(value, 8);
                                            field.SetValue(null, v);
                                        }
                                        break;

                                    case "String":
                                        {
                                            field.SetValue(null, value);
                                        }
                                        break;

                                    case "Boolean":
                                        {
                                            bool v = bool.Parse(value);
                                            field.SetValue(null, v);
                                        }
                                        break;

                                    case "SystemType":
                                        {
                                            field.SetValue(null, Enum.Parse(typeof(SystemType), value, true));
                                        }
                                        break;

                                    case "PacketInterfaceType":
                                        {
                                            field.SetValue(null, Enum.Parse(typeof(PacketInterfaceType), value, true));
                                        }
                                        break;

                                    case "AlternateBootType":
                                        {
                                            field.SetValue(null, Enum.Parse(typeof(AlternateBootType), value, true));
                                        }
                                        break;

                                    case "LogType":
                                        {
                                            field.SetValue(null, Enum.Parse(typeof(LogType), value, true));
                                        }
                                        break;

                                    case "LogComponent":
                                        {
                                            field.SetValue(null, Enum.Parse(typeof(LogComponent), value, true));
                                        }
                                        break;
                                }
                            }
                            catch
                            {
                                Console.WriteLine(
                                    "{0} line {1}: Value '{2}' is invalid for parameter '{3}'.", configFilePath, lineNumber, value, parameter);
                            }
                        }
                    }

                    if (!bMatch)
                    {
                        Console.WriteLine(
                            "{0} line {1}: Unknown configuration parameter '{2}'.", configFilePath, lineNumber, parameter);
                    }
                }
            }
        }

        private static void TestPCap()
        {         
            // Just try enumerating interfaces, if this fails for any reason we assume
            // PCap is not properly installed.
            try
            {
                SharpPcap.CaptureDeviceList devices = SharpPcap.CaptureDeviceList.Instance;
                Configuration.HostRawEthernetInterfacesAvailable = true;
            }
            catch
            {
                Configuration.HostRawEthernetInterfacesAvailable = false;
            }         
        }


    }

}
