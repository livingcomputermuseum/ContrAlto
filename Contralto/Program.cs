using Contralto.CPU;
using System;

namespace Contralto
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AltoSystem system = new AltoSystem();

            if (args.Length > 0)
            {
                system.LoadDrive(0, args[0]);
            }

            AltoWindow mainWindow = new AltoWindow();

            mainWindow.AttachSystem(system);
            
            /*
            Debugger d = new Debugger(system);
            system.AttachDisplay(d);
            d.LoadSourceCode(MicrocodeBank.ROM0, "Disassembly\\altoIIcode3.mu");
            d.LoadSourceCode(MicrocodeBank.ROM1, "Disassembly\\MesaROM.mu");
            d.ShowDialog();                       
            */
            mainWindow.ShowDialog();


        }
    }
}
