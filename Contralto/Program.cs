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

            // for now everything is driven through the debugger            
            Debugger d = new Debugger(system);
            d.LoadSourceCode("Disassembly\\altoIIcode3.mu");
            d.ShowDialog();                       

        }
    }
}
