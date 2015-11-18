using System;

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

        /// <summary>
        /// Conversion from millseconds to nanoseconds
        /// </summary>
        public static readonly ulong MsecToNsec = 1000000;

        /// <summary>
        /// Conversion from microseconds to nanoseconds
        /// </summary>
        public static readonly ulong UsecToNsec = 1000;
    }
}
