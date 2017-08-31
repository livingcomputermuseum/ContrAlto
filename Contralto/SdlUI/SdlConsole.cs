/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;

namespace Contralto.SdlUI
{
    public enum CommandResult
    {        
        Normal,
        Quit
    }

    /// <summary>
    /// Provides a command-line interface to ContrAlto controls,
    /// as a substitute for the GUI interface of the Windows version.
    /// </summary>
    public class SdlConsole
    {
        public SdlConsole(AltoSystem system)
        {
            _system = system;

            _controller = new ExecutionController(_system);
            _controller.ErrorCallback += OnExecutionError;           
        }


        /// <summary>
        /// Invoke the CLI loop in a separate thread.
        /// </summary>
        public void Run(SdlAltoWindow mainWindow)
        {
            Console.WriteLine("You are at the ContrAlto console.  Type 'show commands' to see");
            Console.WriteLine("a list of possible commands, and hit Tab to see possible command completions.");

            _mainWindow = mainWindow;
            _mainWindow.OnClosed += OnMainWindowClosed;

            _cliThread = new Thread(RunCliThread);
            _cliThread.Start();
        }

        /// <summary>
        /// The CLI thread
        /// </summary>
        private void RunCliThread()
        {
            ConsoleExecutor executor = new ConsoleExecutor(this);
            CommandResult state = CommandResult.Normal;

            while (state != CommandResult.Quit)
            {
                state = executor.Prompt();
            }

            //
            // Ensure the emulator is stopped.
            //
            _controller.StopExecution();            

            //
            // Ensure the main window is closed.
            //
            _mainWindow.Close();
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            //
            // Make sure the emulator is stopped.
            //
            _controller.StopExecution();

            _system.Shutdown(true /* commit disk contents */);

            //
            // The Alto window was closed, shut down the CLI.
            //
            _cliThread.Abort();
        }

        /// <summary>
        /// Error handling
        /// </summary>
        /// <param name="e"></param>
        private void OnExecutionError(Exception e)
        {
            Console.WriteLine("Execution error: {0} - {1}", e.Message, e.StackTrace);
            System.Diagnostics.Debugger.Break();
        }

        [DebuggerFunction("quit", "Exits ContrAlto.")]
        private CommandResult Quit()
        {
            _controller.StopExecution();
            return CommandResult.Quit;
        }

        //
        // Console commands
        //
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

        [DebuggerFunction("show disk", "Displays the contents of the specified drive.", "<drive>")]
        private CommandResult ShowDisk(ushort drive)
        {
            if (drive > 1)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Save current drive contents.
            if (_system.DiskController.Drives[drive].IsLoaded)
            {
                Console.WriteLine("Drive {0} contains image {1}", 
                    drive, 
                    _system.DiskController.Drives[drive].Pack.PackName);
            }
            else
            {
                Console.WriteLine("Drive {0} is not loaded.", drive);
            }

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

        [DebuggerFunction("show trident", "Displays the contents of the specified trident drive.", "<drive>")]
        private CommandResult ShowTrident(ushort drive)
        {
            if (drive > 7)
            {
                throw new InvalidOperationException("Drive specification out of range.");
            }

            // Save current drive contents.
            if (_system.TridentController.Drives[drive].IsLoaded)
            {
                Console.WriteLine("Trident {0} contains image {1}",
                    drive,
                    _system.TridentController.Drives[drive].Pack.PackName);
            }
            else
            {
                Console.WriteLine("Trident {0} is not loaded.", drive);
            }

            return CommandResult.Normal;
        }

        [DebuggerFunction("show system type", "Displays the Alto system type.")]
        private CommandResult ShowSystemType()
        {
            Console.WriteLine("System type is {0}", Configuration.SystemType);
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

        [DebuggerFunction("show ethernet address", "Displays the Alto's host Ethernet address.")]
        private CommandResult ShowEthernetAddress()
        {
            Console.WriteLine("Ethernet address is {0}", Conversion.ToOctal(Configuration.HostAddress));
            return CommandResult.Normal;
        }

        [DebuggerFunction("show host network interface name", "Displays the host network interface used for Ethernet emulation")]
        private CommandResult ShowHostNetworkInterfaceName()
        {
            Console.WriteLine("Network interface is '{0}'", Configuration.HostPacketInterfaceName);
            return CommandResult.Normal;
        }

        [DebuggerFunction("show host network interface type", "Displays the host network interface type (RAW or UDP)")]
        private CommandResult ShowHostNetworkInterfaceType()
        {
            Console.WriteLine("Network interface type is '{0}'", Configuration.HostPacketInterfaceType);
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

        // Not yet supported on non-Windows platforms
        /*
        [DebuggerFunction("enable display interlacing", "Enables interlaced display.")]
        private CommandResult EnableDisplayInterlacing()
        {
            Configuration.InterlaceDisplay = true;
            return CommandResult.Normal;
        }

        [DebuggerFunction("disable display interlacing", "Disables interlaced display.")]
        private CommandResult DisableDisplayInterlacing()
        {
            Configuration.InterlaceDisplay = false;
            return CommandResult.Normal;
        }

        [DebuggerFunction("enable speed throttling", "Limits execution speed to 60 fields/sec.")]
        private CommandResult EnableSpeedThrottling()
        {
            Configuration.ThrottleSpeed = true;
            return CommandResult.Normal;
        }

        [DebuggerFunction("disable speed throttling", "Removes speed limits.")]
        private CommandResult DisableSpeedThrottling()
        {
            Configuration.ThrottleSpeed = false;
            return CommandResult.Normal;
        }

        [DebuggerFunction("enable audio dac", "Enables the Audio DAC.")]
        private CommandResult EnableAudioDAC()
        {
            Configuration.EnableAudioDAC = true;
            return CommandResult.Normal;
        }

        [DebuggerFunction("disable audio dac", "Disables the Audio DAC.")]
        private CommandResult DisableAudioDAC()
        {
            Configuration.EnableAudioDAC = false;
            return CommandResult.Normal;
        }

        [DebuggerFunction("enable audio capture", "Enables capture of DAC output.")]
        private CommandResult EnableAudioDACCapture()
        {
            Configuration.EnableAudioDACCapture = true;
            return CommandResult.Normal;
        }

        [DebuggerFunction("disable audio capture", "Disables capture of DAC output.")]
        private CommandResult DisableAudioDACCapture()
        {
            Configuration.EnableAudioDACCapture = false;
            return CommandResult.Normal;
        }
        
        [DebuggerFunction("set audio capture path", "Configures the path for capture output.")]
        private CommandResult SetAudioCapturePath(string path)
        {
            Configuration.AudioDACCapturePath = path;
            return CommandResult.Normal;
        }
        */

        private AltoSystem _system;
        private ExecutionController _controller;
        private SdlAltoWindow _mainWindow;
        private Thread _cliThread;
    }
}
