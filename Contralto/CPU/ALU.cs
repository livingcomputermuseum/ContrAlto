using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{
    // From Alto Hardware Manual, Section 2.1
    static class ALU
    {
        static ALU()
        {
            _lastCarry = 0;
        }



        public static ushort Execute(AluFunction fn, ushort a, ushort b)
        {

            return 0;
        }

        private static ushort _lastCarry;
    }
}
