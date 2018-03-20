using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    public static class ScriptManager
    {
        static ScriptManager()
        {
            _scheduler = new Scheduler();
        }

        public static Scheduler ScriptScheduler
        {
            get { return _scheduler; }
        }

        /// <summary>
        /// Fired when playback of a script has completed or is stopped.
        /// </summary>
        public static event EventHandler PlaybackCompleted;

        public static void StartRecording(AltoSystem system, string scriptPath)
        {
            // Stop any pending actions
            StopRecording();
            StopPlayback();

            _scriptRecorder = new ScriptRecorder(system, scriptPath);

            Log.Write(LogComponent.Scripting, "Starting recording to {0}", scriptPath);

            //
            // Record the absolute position of the mouse (as held in MOUSELOC in system memory).
            // All other mouse movements in the script will be recorded relative to this point.
            //
            int x = system.Memory.Read(0x114, CPU.TaskType.Ethernet, false);
            int y = system.Memory.Read(0x115, CPU.TaskType.Ethernet, false);
            _scriptRecorder.MouseMoveAbsolute(x, y);
        }

        public static void StopRecording()
        {
            if (IsRecording)
            {
                _scriptRecorder.End();
                _scriptRecorder = null;
            }

            Log.Write(LogComponent.Scripting, "Stopped recording.");
        }

        public static void StartPlayback(AltoSystem system, ExecutionController controller, string scriptPath)
        {
            // Stop any pending actions
            StopRecording();
            StopPlayback();

            _scheduler.Reset();

            _scriptPlayback = new ScriptPlayback(scriptPath, system, controller);
            _scriptPlayback.PlaybackCompleted += OnPlaybackCompleted;
            _scriptPlayback.Start();

            Log.Write(LogComponent.Scripting, "Starting playback of {0}", scriptPath);
        }
        
        public static void StopPlayback()
        {
            if (IsPlaying)
            {
                _scriptPlayback.Stop();
                _scriptPlayback = null;

                PlaybackCompleted(null, null);
            }

            Log.Write(LogComponent.Scripting, "Stopped playback.");
        }

        public static void CompleteWait()
        {
            if (IsPlaying)
            {
                _scriptPlayback.Start();                

                Log.Write(LogComponent.Scripting, "Playback resumed after Wait.");
            }
        }

        public static ScriptRecorder Recorder
        {
            get { return _scriptRecorder;  }
        }

        public static ScriptPlayback Playback
        {
            get { return _scriptPlayback; }
        }

        public static bool IsRecording
        {
            get { return _scriptRecorder != null; }
        }

        public static bool IsPlaying
        {
            get { return _scriptPlayback != null; }
        }

        private static void OnPlaybackCompleted(object sender, EventArgs e)
        {
            _scriptPlayback = null;
            PlaybackCompleted(null, null);
        }

        private static ScriptRecorder _scriptRecorder;
        private static ScriptPlayback _scriptPlayback;

        private static Scheduler _scheduler;
    }
}
