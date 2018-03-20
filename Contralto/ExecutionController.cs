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

namespace Contralto
{

    public delegate bool StepCallbackDelegate();
    public delegate void ErrorCallbackDelegate(Exception e);
    public delegate void ShutdownCallbackDelegate(bool commitDisks);

    public class ShutdownException : Exception
    {
        public ShutdownException(bool commitDisks) : base()
        {
            _commitDisks = commitDisks;
        }

        public bool CommitDisks
        {
            get { return  _commitDisks; }
        }

        private bool _commitDisks;
    }


    public class ExecutionController
    {
        public ExecutionController(AltoSystem system)
        {
            _system = system;

            _execAbort = false;
            _userAbort = false;

        }        

        public void StartExecution(AlternateBootType bootType)
        {
            StartAltoExecutionThread();
            _system.PressBootKeys(bootType);
        }

        public void StopExecution()
        {
            _userAbort = true;

            if (System.Threading.Thread.CurrentThread !=
               _execThread)
            {
                //
                // Call is asynchronous, we will wait for the
                // execution thread to finish.
                //
                if (_execThread != null)
                {
                    _execThread.Join();
                    _execThread = null;
                }
            }
        }

        public void Reset(AlternateBootType bootType)
        {
            if (System.Threading.Thread.CurrentThread ==
                _execThread)
            {
                //
                // Call is from within the execution thread
                // so we can just reset the system without worrying
                // about synchronization.
                //
                _system.Reset();
                _system.PressBootKeys(bootType);
            }
            else
            {
                //
                // Call is asynchronous, we need to stop the
                // execution thread and restart it after resetting
                // the system.
                //
                bool running = IsRunning;

                if (running)
                {
                    StopExecution();
                }
                _system.Reset();
                _system.PressBootKeys(bootType);

                if (running)
                {
                    StartExecution(AlternateBootType.None);
                }
            }
        }

        public bool IsRunning
        {
            get { return (_execThread != null && _execThread.IsAlive); }
        }
        
        public StepCallbackDelegate StepCallback
        {
            get { return _stepCallback; }
            set { _stepCallback = value; }
        }

        public ErrorCallbackDelegate ErrorCallback
        {
            get { return _errorCallback; }
            set { _errorCallback = value; }
        }

        public ShutdownCallbackDelegate ShutdownCallback
        {
            get { return _shutdownCallback; }
            set { _shutdownCallback = value; }
        }

        private void StartAltoExecutionThread()
        {
            if (_execThread != null && _execThread.IsAlive)
            {
                return;
            }

            _execAbort = false;
            _userAbort = false;

            _execThread = new Thread(new System.Threading.ThreadStart(ExecuteProc));
            _execThread.Start();
        }

        private void ExecuteProc()
        {
            while (true)
            {
                // Execute a single microinstruction
                try
                {
                    _system.SingleStep();

                    if (ScriptManager.IsPlaying ||
                        ScriptManager.IsRecording)
                    {
                        ScriptManager.ScriptScheduler.Clock();
                    }
                }
                catch(ShutdownException s)
                {
                    //
                    // We will only actually shut down if someone
                    // is listening to this event.
                    //
                    if (_shutdownCallback != null)
                    {
                        _shutdownCallback(s.CommitDisks);
                        _execAbort = true;
                    }
                }
                catch (Exception e)
                {
                    if (_errorCallback != null)
                    {
                        _errorCallback(e);
                    }
                    _execAbort = true;
                }

                if (_stepCallback != null)
                {
                    _execAbort = _stepCallback();
                }

                if (_execAbort || _userAbort)
                {
                    // Halt execution
                    break;
                }
            }            
        }

        // Execution thread and state
        private Thread _execThread;        
        private bool _execAbort;
        private bool _userAbort;

        private StepCallbackDelegate _stepCallback;
        private ErrorCallbackDelegate _errorCallback;
        private ShutdownCallbackDelegate _shutdownCallback;

        private AltoSystem _system;
    }
}
