using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.CPU;

namespace Contralto
{
    class Program
    {
        static void Main(string[] args)
        {
            AltoCPU cpu = new AltoCPU();

            while(true)
            {
                cpu.ExecuteNext();
            }

        }
    }
}
