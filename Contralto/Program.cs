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

            AltoSystem system = new AltoSystem();

            
            /*
            for(int address=0; address < 1024; address++)
            { 

                MicroInstruction inst = new MicroInstruction(UCodeMemory.UCodeROM[address]);

                Console.WriteLine("{0}: {1} - RSEL:{2} ALUF:{3} BS:{4} F1:{5} F2:{6} T:{7} L:{8} NEXT:{9}",
                    OctalHelpers.ToOctal(address, 4),
                    OctalHelpers.ToOctal((int)UCodeMemory.UCodeROM[address], 11),
                    OctalHelpers.ToOctal((int)inst.RSELECT, 2),
                    OctalHelpers.ToOctal((int)inst.ALUF, 2),
                    OctalHelpers.ToOctal((int)inst.BS),
                    OctalHelpers.ToOctal((int)inst.F1, 2),
                    OctalHelpers.ToOctal((int)inst.F2, 2),
                    inst.LoadT ? 1 : 0,
                    inst.LoadL ? 1 : 0,
                    OctalHelpers.ToOctal((int)inst.NEXT, 4));
            } */
            

            // for now everything is driven through the debugger            
            Debugger d = new Debugger(system);
            d.LoadSourceCode("Disassembly\\altoIIcode3.mu");
            d.ShowDialog();                       

        }
    }
}
