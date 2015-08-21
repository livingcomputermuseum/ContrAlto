using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.CPU;
using Contralto.Memory;

namespace Contralto
{
    class Program
    {
        static void Main(string[] args)
        {
            AltoCPU cpu = new AltoCPU();

            while(true)
            {
                MemoryBus.Clock();
                cpu.ExecuteNext();
            }

        }
    }
}
