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

using Contralto.Scripting;
using System;
using System.Threading;

namespace Contralto.SdlUI
{ 
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
            _controller.ShutdownCallback += OnShutdown;

            _commitDisksAtShutdown = true;

            ScriptManager.PlaybackCompleted += OnScriptPlaybackCompleted;
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

            if (!string.IsNullOrWhiteSpace(StartupOptions.ScriptFile))
            {
                Console.WriteLine("Starting playback of script {0}", StartupOptions.ScriptFile);                
                ScriptManager.StartPlayback(_system, _controller, StartupOptions.ScriptFile);
                _controller.StartExecution(AlternateBootType.None);
            }
        }

        /// <summary>
        /// The CLI thread
        /// </summary>
        private void RunCliThread()
        {
            ControlCommands controlCommands = new ControlCommands(_system, _controller);
            CommandExecutor executor = new CommandExecutor(this, controlCommands);
            DebuggerPrompt prompt = new DebuggerPrompt(executor.CommandTreeRoot);

            CommandResult state = CommandResult.Normal;

            while (state != CommandResult.Quit)
            {
                state = CommandResult.Normal;
                try
                {
                    // Get the command string from the prompt.
                    string command = prompt.Prompt().Trim();

                    if (command != String.Empty)
                    {
                        state = executor.ExecuteCommand(command);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
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

            _system.Shutdown(_commitDisksAtShutdown);

            //
            // The Alto window was closed, shut down the CLI.
            //
            _cliThread.Abort();
        }

        private void OnShutdown(bool commitDisks)
        {
            _commitDisksAtShutdown = commitDisks;

            // Close the main window, this will cause everything else to shut down.
            _mainWindow.Close();
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

        private void OnScriptPlaybackCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Script playback completed.");
        }

        //
        // Console commands
        //
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

        [DebuggerFunction("start recording", "Starts recording of inputs to script file")]
        private CommandResult StartRecording(string scriptPath)
        {
            if (ScriptManager.IsPlaying ||
                ScriptManager.IsRecording)
            {
                Console.WriteLine("{0} is already in progress.", ScriptManager.IsPlaying ? "Playback" : "Recording");
            }
            else
            {
                Console.WriteLine("Recording to {0} starting.", scriptPath);
                ScriptManager.StartRecording(_system, scriptPath);                
            }
            return CommandResult.Normal;
        }

        [DebuggerFunction("stop recording", "Stops script recording")]
        private CommandResult StopRecording()
        {
            if (!ScriptManager.IsRecording)
            {
                Console.WriteLine("No recording currently in progress.");
            }
            else
            {                
                ScriptManager.StopRecording();
                Console.WriteLine("Recording stopped.");
            }

            return CommandResult.Normal;
        }

        [DebuggerFunction("start playback", "Starts playback of script file")]
        private CommandResult StartPlayback(string scriptPath)
        {
            if (ScriptManager.IsPlaying ||
                ScriptManager.IsRecording)
            {
                Console.WriteLine("{0} is already in progress.", ScriptManager.IsPlaying ? "Playback" : "Recording");
            }
            else
            {
                Console.WriteLine("Playback of {0} starting.", scriptPath);
                //
                // Start the script.  We need to pause the emulation while doing so,
                // in order to avoid concurrency issues with the Scheduler (which is
                // not thread-safe).
                //
                _controller.StopExecution();
                ScriptManager.StartPlayback(_system, _controller, scriptPath);
                _controller.StartExecution(AlternateBootType.None);
            }
            return CommandResult.Normal;
        }

        [DebuggerFunction("stop playback", "Stops script playback")]
        private CommandResult StopPlayback()
        {
            if (!ScriptManager.IsPlaying)
            {
                Console.WriteLine("No playback currently in progress.");
            }
            else
            {
                ScriptManager.StopPlayback();
                Console.WriteLine("Playback stopped.");
            }

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
        private bool _commitDisksAtShutdown;
    }
}
