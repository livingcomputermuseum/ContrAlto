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
        /// Conversion from nanoseconds to milliseconds
        /// </summary>
        public static readonly double NsecToMsec = 0.000001;

        /// <summary>
        /// Conversion from microseconds to nanoseconds
        /// </summary>
        public static readonly ulong UsecToNsec = 1000;
    }
}
