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
            d.LoadSourceCode("Disassembly\\altoIIcode3.mu");
            d.ShowDialog();                       

        }
    }
}
