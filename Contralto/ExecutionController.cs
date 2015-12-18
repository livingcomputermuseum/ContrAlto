using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contralto
{

    public delegate bool StepCallbackDelegate();
    public delegate void ErrorCallbackDelegate(Exception e);


    public class ExecutionController
    {
        public ExecutionController(AltoSystem system)
        {
            _system = system;

            _execAbort = false;            

        }        

        public void StartExecution()
        {
            StartAltoExecutionThread();
        }

        public void StopExecution()
        {
            _execAbort = true;

            if (_execThread != null)
            {
                _execThread.Join();
                _execThread = null;
            }
        }

        public void Reset()
        {
            bool running = IsRunning;

            if (running)
            {
                StopExecution();
            }
            _system.Reset();

            if (running)
            {
                StartExecution();
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

        private void StartAltoExecutionThread()
        {
            if (_execThread != null && _execThread.IsAlive)
            {
                return;
            }

            _execAbort = false;

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

                if (_execAbort)
                {
                    // Halt execution
                    break;
                }
            }            
        }

        // Execution thread and state
        private Thread _execThread;        
        private bool _execAbort;

        private StepCallbackDelegate _stepCallback;
        private ErrorCallbackDelegate _errorCallback;

        private AltoSystem _system;
    }
}
