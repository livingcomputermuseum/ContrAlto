using Contralto.CPU;

namespace Contralto
{
    class Program
    {
        static void Main(string[] args)
        {
            AltoSystem system = new AltoSystem();                            

            // for now everything is driven through the debugger            
            Debugger d = new Debugger(system);
            system.AttachDisplay(d);
            d.LoadSourceCode(MicrocodeBank.ROM0, "Disassembly\\altoIIcode3.mu");
            d.LoadSourceCode(MicrocodeBank.ROM1, "Disassembly\\MesaROM.mu");
            d.ShowDialog();                       

        }
    }
}
