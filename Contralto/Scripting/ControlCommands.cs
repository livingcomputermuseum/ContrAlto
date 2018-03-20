using Contralto.SdlUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    public class ControlCommands
    {
        public ControlCommands(AltoSystem system, ExecutionController controller)
        {
            _system = system;
            _controller = controller;
        }

        [DebuggerFunction("quit", "Exits ContrAlto.")]
        private CommandResult Quit()
        {
            _controller.StopExecution();
            return CommandResult.Quit;
        }

        [DebuggerFunction("quit without saving", "Exits ContrAlto without committing changes to Diablo disk packs.")]
        private CommandResult QuitNoSave()
        {
            _controller.StopExecution();
            return CommandResult.QuitNoSave;
        }

        [DebuggerFunction("start", "Starts the emulated Alto normally.")]
        private CommandResult Start()
        {
            if (_controller.IsRunning)
            {
                Console.WriteLine("Alto is already running.");
            }
            else
            {
                _controller.StartExecution(AlternateBootType.None);
                Console.WriteLine("Alto started.");
            }

            return CommandResult.Normal;
        }

        [DebuggerFunction("stop", "Stops the emulated Alto.")]
        private CommandResult Stop()
        {
            _controller.StopExecution();
            Console.WriteLine("Alto stopped.");

            return CommandResult.Normal;
        }

        [DebuggerFunction("reset", "Resets the emulated Alto.")]
        private CommandResult Reset()
        {
            _controller.Reset(AlternateBootType.None);
            Console.WriteLine("Alto reset.");

            return CommandResult.Normal;
        }

        [DebuggerFunction("start with keyboard disk boot", "Starts the emulated Alto with the specified keyboard disk boot address.")]
        private CommandResult StartDisk()
        {
            if (_controller.IsRunning)
            {
                _controller.Reset(AlternateBootType.Disk);
            }
            else
            {
                _controller.StartExecution(AlternateBootType.Disk);
            }

            return CommandResult.Normal;
        }

        [DebuggerFunction("start with keyboard net boot", "Starts the emulated Alto with the specified keyboard ethernet boot number.")]
        private CommandResult StartNet()
        {
            if (_controller.IsRunning)
            {
                _controller.Reset(AlternateBootType.Ethernet);
            }
            else
            {
                _controller.StartExecution(AlternateBootType.Ethernet);
            }

            return CommandResult.Normal;
        }

        [DebuggerFunction("load disk", "Loads the specified drive with the requested disk image.", "<drive> <path>")]
        private CommandResult LoadDisk(ushort drive, string path)
        {
            if (drive > 1)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Load the new pack.
            _system.LoadDiabloDrive(drive, path, false);
            Console.WriteLine("Drive {0} loaded.", drive);

            return CommandResult.Normal;
        }

        [DebuggerFunction("unload disk", "Unloads the specified drive.", "<drive>")]
        private CommandResult UnloadDisk(ushort drive)
        {
            if (drive > 1)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Unload the current pack.
            _system.UnloadDiabloDrive(drive);
            Console.WriteLine("Drive {0} unloaded.", drive);

            return CommandResult.Normal;
        }

        [DebuggerFunction("new disk", "Creates and loads a new image for the specified drive.", "<drive>")]
        private CommandResult NewDisk(ushort drive, string path)
        {
            if (drive > 1)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Unload the current pack.
            _system.LoadDiabloDrive(drive, path, true);
            Console.WriteLine("Drive {0} created and loaded.", drive);

            return CommandResult.Normal;
        }        

        [DebuggerFunction("load trident", "Loads the specified trident drive with the requested disk image.", "<drive> <path>")]
        private CommandResult LoadTrident(ushort drive, string path)
        {
            if (drive > 7)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Load the new pack.
            _system.LoadTridentDrive(drive, path, false);
            Console.WriteLine("Trident {0} loaded.", drive);

            return CommandResult.Normal;
        }

        [DebuggerFunction("unload trident", "Unloads the specified trident drive.", "<drive>")]
        private CommandResult UnloadTrident(ushort drive)
        {
            if (drive > 7)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Unload the current pack.
            _system.UnloadTridentDrive(drive);
            Console.WriteLine("Trident {0} unloaded.", drive);

            return CommandResult.Normal;
        }

        [DebuggerFunction("new trident", "Creates and loads a new image for the specified drive.", "<drive>")]
        private CommandResult NewTrident(ushort drive, string path)
        {
            if (drive > 7)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Unload the current pack.
            _system.LoadTridentDrive(drive, path, true);
            Console.WriteLine("Trident {0} created and loaded.", drive);

            return CommandResult.Normal;
        }

        [DebuggerFunction("set ethernet address", "Sets the Alto's host Ethernet address.")]
        private CommandResult SetEthernetAddress(byte address)
        {
            if (address == 0 || address == 0xff)
            {
                Console.WriteLine("Address {0} is invalid.", Conversion.ToOctal(address));
            }
            else
            {
                Configuration.HostAddress = address;
            }

            return CommandResult.Normal;
        }        

        [DebuggerFunction("set keyboard net boot file", "Sets the boot file used for net booting.")]
        private CommandResult SetKeyboardBootFile(ushort file)
        {
            Configuration.BootFile = file;
            return CommandResult.Normal;
        }

        [DebuggerFunction("set keyboard disk boot address", "Sets the boot address used for disk booting.")]
        private CommandResult SetKeyboardBootAddress(ushort address)
        {
            Configuration.BootFile = address;
            return CommandResult.Normal;
        }


        private AltoSystem _system;
        private ExecutionController _controller;
    }
}
