using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto
{
    public static class Conversion
    {
        public static string ToOctal(int i)
        {
            return Convert.ToString(i, 8);            
        }

        public static string ToOctal(sbyte s)
        {
            if (s < 0)
            {
                return "-" + Convert.ToString(-s, 8);
            }
            else
            {
                return Convert.ToString(s, 8);
            }
        }

        public static string ToOctal(int i, int digits)
        {
            string octalString = Convert.ToString(i, 8);            
            return new String('0', digits - octalString.Length) + octalString;
        }
    }
}
