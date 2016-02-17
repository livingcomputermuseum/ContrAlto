using Contralto.CPU;
using Contralto.IO;
using System;
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

            ParseCommandLine(args);
           

            AltoSystem system = new AltoSystem();

            if (!String.IsNullOrEmpty(Configuration.Drive0Image))
            {
                system.LoadDrive(0, Configuration.Drive0Image);
            }

            if (!String.IsNullOrEmpty(Configuration.Drive1Image))
            {
                system.LoadDrive(1, Configuration.Drive1Image);
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

        private static void ParseCommandLine(string[] args)
        {
            // At the moment, options start with a "-" and are one of:
            //  "-hostaddress <address>"  : specifies ethernet host address in octal (1-377)
            //  "-hostinterface <name>"   : specifies the name of the host ethernet interface to use
            //  "-listinterfaces"         : lists ethernet interfaces known by pcap
            //  "-drive0 <image>"         : attaches disk image to drive 0
            //  "-drive1 <image>"         : attaches disk image to drive 1

            int index = 0;

            // TODO: this parsing needs to be made not terrible.
            while(index < args.Length)
            {
                switch (args[index++].ToLower())
                {
                    case "-hostaddress":
                        if (index < args.Length)
                        {
                            Configuration.HostAddress = Convert.ToByte(args[index++], 8);
                        }
                        else
                        {
                            PrintUsage();
                        }
                        break;

                    case "-hostinterface":
                        if (index < args.Length)
                        {
                            if (!Configuration.HostEthernetAvailable)
                            {
                                Console.WriteLine("Ethernet functionality is disabled, host interface cannot be specified.");
                            }
                            else
                            {
                                Configuration.HostEthernetInterfaceName = args[index++];
                            }
                        }
                        else
                        {
                            PrintUsage();
                        }
                        break;

                    case "-listinterfaces":
                        if (!Configuration.HostEthernetAvailable)
                        {
                            Console.WriteLine("Ethernet functionality is disabled, interfaces cannot be enumerated.");
                        }
                        else
                        {
                            List<EthernetInterface> interfaces = EthernetInterface.EnumerateDevices();

                            foreach (EthernetInterface i in interfaces)
                            {
                                Console.WriteLine("Name: '{0}'\n  Description: '{1}'\n  MAC '{2}'", i.Name, i.Description, i.MacAddress);
                            }
                        }
                        break;

                    case "-drive0":
                        if (index < args.Length)
                        {
                            Configuration.Drive0Image = args[index++];
                        }
                        else
                        {
                            PrintUsage();
                        }
                        break;

                    case "-drive1":
                        if (index < args.Length)
                        {
                            Configuration.Drive1Image = args[index++];
                        }
                        else
                        {
                            PrintUsage();
                        }
                        break;
                }
            }            
        }

        private static void PrintUsage()
        {
            // TODO: make more useful.
            Console.WriteLine("Something is wrong, try again.");
        }

        private static void TestPCap()
        {
            // Just try enumerating interfaces, if this fails for any reason we assume
            // PCap is not properly installed.
            try
            {
                List<EthernetInterface> interfaces = EthernetInterface.EnumerateDevices();
                Configuration.HostEthernetAvailable = true;
            }
            catch
            {
                Console.WriteLine("WARNING: WinPCAP does not appear to be properly installed.");
                Console.WriteLine("         Ethernet functionality is disabled.");
                Console.WriteLine("         Please install WinPCAP from: http://www.winpcap.org/");

                Configuration.HostEthernetAvailable = false;
            }
        }
    }
}
