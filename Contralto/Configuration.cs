using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto
{
    /// <summary>
    /// Encapsulates user-configurable settings.  To be enhanced.
    /// </summary>
    public class Configuration
    {
        static Configuration()
        {
            // Initialize things to defaults.
            // TODO: Load from config file.
            HostAddress = 0x22;

            EthernetBootEnabled = false;
            EthernetBootFile = 0;
        }

        public static string Drive0Image;
        public static string Drive1Image;
        public static byte HostAddress;
        public static string HostEthernetInterfaceName;  
        public static bool HostEthernetAvailable;

        public static bool EthernetBootEnabled;
        public static ushort EthernetBootFile;
    }

}
