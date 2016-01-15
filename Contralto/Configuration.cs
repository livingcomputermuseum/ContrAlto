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
            // Initialize things to defaults
            HostAddress = 0x22;

        }

        public static string Drive0Image;
        public static string Drive1Image;
        public static byte HostAddress;
        public static string HostEthernetInterfaceName;                
    }

}
