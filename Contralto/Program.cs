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

            for(int i=0;i<2048;i++)
            {
                MicroInstruction inst = new MicroInstruction(UCodeMemory.UCodeROM[i]);

                Console.WriteLine("{0}: {1}", OctalHelpers.ToOctal(i), Disassembler.DisassembleInstruction(inst, TaskType.Emulator));
            }

            while(true)
            {
                MemoryBus.Clock();
                cpu.ExecuteNext();
            }

        }
    }
}
