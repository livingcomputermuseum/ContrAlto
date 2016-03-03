using Contralto.CPU;
using Contralto.IO;
using System;
using System.Net;
using System.Collections.Generic;

namespace Contralto
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Handle command-line args
            PrintHerald();

            // See if WinPCap is installed and working
            TestPCap();                   

            AltoSystem system = new AltoSystem();

            if (!String.IsNullOrEmpty(Configuration.Drive0Image))
            {
                try
                {
                    system.LoadDrive(0, Configuration.Drive0Image);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Could not load image '{0}' for drive 0.  Error '{1}'.", Configuration.Drive0Image, e.Message);
                    system.UnloadDrive(0);
                }
            }

            if (!String.IsNullOrEmpty(Configuration.Drive1Image))
            {
                try
                {
                    system.LoadDrive(1, Configuration.Drive0Image);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not load image '{0}' for drive 1.  Error '{1}'.", Configuration.Drive1Image, e.Message);
                    system.UnloadDrive(1);
                }
            }

            AltoWindow mainWindow = new AltoWindow();

            mainWindow.AttachSystem(system);
            
            mainWindow.ShowDialog();


        }

        private static void PrintHerald()
        {
            Console.WriteLine("ContrAlto v0.1 (c) 2015, 2016 Living Computer Museum.");            
            Console.WriteLine("Bug reports to joshd@livingcomputermuseum.org");
            Console.WriteLine();
        }      

        private static void TestPCap()
        {
            // Just try enumerating interfaces, if this fails for any reason we assume
            // PCap is not properly installed.
            try
            {
                List<EthernetInterface> interfaces = EthernetInterface.EnumerateDevices();
                Configuration.HostRawEthernetInterfacesAvailable = true;
            }
            catch
            {
                Console.WriteLine("WARNING: WinPCAP does not appear to be properly installed.");
                Console.WriteLine("         Raw Ethernet functionality will be disabled.");
                Console.WriteLine("         Please install WinPCAP from: http://www.winpcap.org/");

                Configuration.HostRawEthernetInterfacesAvailable = false;
            }
        }
    }
}
