using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    public class ScriptPlayback
    {
        public ScriptPlayback(string scriptFile, AltoSystem system, ExecutionController controller)
        {
            _scriptReader = new ScriptReader(scriptFile);
            _system = system;
            _controller = controller;

            _currentAction = null;

            _stopPlayback = false;
        }

        /// <summary>
        /// Fired when playback of the script has completed or is stopped.
        /// </summary>
        public event EventHandler PlaybackCompleted;

        public void Start()
        {
            _stopPlayback = false;

            // Schedule first event.
            ScheduleNextEvent(0);
        }

        public void Stop()
        {
            // We will stop after the next event is fired (if any)
            _stopPlayback = true;
        }

        private void ScheduleNextEvent(ulong skewNsec)
        {
            //
            // Grab the next action if the current one is done.
            //
            if (_currentAction == null || _currentAction.Completed)
            {
                _currentAction = _scriptReader.ReadNext();
            }            
            
            if (_currentAction != null)
            {
                // We have another action to queue up.
                Event scriptEvent = new Event(_currentAction.Timestamp, _currentAction, OnEvent);
                ScriptManager.ScriptScheduler.Schedule(scriptEvent);

                Log.Write(LogComponent.Scripting, "Queueing script action {0}", _currentAction);
            }
            else 
            {
                //
                // Playback is complete.
                //
                Log.Write(LogComponent.Scripting, "Playback completed.");
                PlaybackCompleted(this, null);
            }
        }

        private void OnEvent(ulong skewNsec, object context)
        {
            // Replay the action.
            if (!_stopPlayback)
            {
                ScriptAction action = (ScriptAction)context;
                Log.Write(LogComponent.Scripting, "Invoking action {0}", action);

                action.Replay(_system, _controller);

                // Special case for Wait -- this causes the script to stop here until the
                // Alto itself tells things to start up again.
                //
                if (action is WaitAction)
                {
                    Log.Write(LogComponent.Scripting, "Playback paused, awaiting wakeup from Alto.");
                }
                else
                {
                    // Kick off the next action in the script.
                    ScheduleNextEvent(skewNsec);
                }
            }
            else
            {
                Log.Write(LogComponent.Scripting, "Playback stopped.");
                PlaybackCompleted(this, null);
            }
        }

        private AltoSystem _system;
        private ExecutionController _controller;
        private ScriptReader _scriptReader;

        private ScriptAction _currentAction;

        private bool _stopPlayback;
    }
}
