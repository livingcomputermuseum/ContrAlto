using Contralto.CPU;
using Contralto.IO;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;

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

            _system = new AltoSystem();

            if (!String.IsNullOrEmpty(Configuration.Drive0Image))
            {
                try
                {
                    _system.LoadDrive(0, Configuration.Drive0Image);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Could not load image '{0}' for drive 0.  Error '{1}'.", Configuration.Drive0Image, e.Message);
                    _system.UnloadDrive(0);
                }
            }

            if (!String.IsNullOrEmpty(Configuration.Drive1Image))
            {
                try
                {
                    _system.LoadDrive(1, Configuration.Drive1Image);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not load image '{0}' for drive 1.  Error '{1}'.", Configuration.Drive1Image, e.Message);
                    _system.UnloadDrive(1);
                }
            }

            //
            // Attach handlers so that we can properly flush state if we're terminated.
            //
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            //
            // Invoke the main UI window; this will run until the user closes it, at which
            // point we are done.
            //                        
            using (AltoWindow mainWindow = new AltoWindow())
            {                
                mainWindow.AttachSystem(_system);
                Application.Run(mainWindow);
            }            
        } 

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting...");

            //
            // Save disk contents
            //
            _system.CommitDiskPack(0);
            _system.CommitDiskPack(1);

            //
            // Commit current configuration to disk
            //
            Configuration.WriteConfiguration();

        }

        private static void PrintHerald()
        {
            Console.WriteLine("ContrAlto v1.0 (c) 2015, 2016 Living Computer Museum.");            
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
                Configuration.HostRawEthernetInterfacesAvailable = false;
            }
        }       

        private static AltoSystem _system;
        private static ManualResetEvent _closeEvent;
    }
}
