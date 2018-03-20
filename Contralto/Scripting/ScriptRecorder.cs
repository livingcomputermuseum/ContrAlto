using Contralto.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    /// <summary>
    /// Records actions.
    /// </summary>
    public class ScriptRecorder
    {
        public ScriptRecorder(AltoSystem system, string scriptFile)
        {
            _script = new ScriptWriter(scriptFile);
            _system = system;
            _lastTimestamp = 0;

            _firstTime = true;
        }

        public void End()
        {
            _script.End();
        }

        public void KeyDown(AltoKey key)
        {
            _script.AppendAction(
                new KeyAction(
                    GetRelativeTimestamp(),
                    key,
                    true));
        }

        public void KeyUp(AltoKey key)
        {
            _script.AppendAction(
                new KeyAction(
                    GetRelativeTimestamp(),
                    key,
                    false));
        }

        public void MouseDown(AltoMouseButton button)
        {
            _script.AppendAction(
                new MouseButtonAction(
                    GetRelativeTimestamp(),
                    button,
                    true));
        }

        public void MouseUp(AltoMouseButton button)
        {
            _script.AppendAction(
                new MouseButtonAction(
                    GetRelativeTimestamp(),
                    button,
                    false));
        }

        public void MouseMoveRelative(int dx, int dy)
        {
            _script.AppendAction(
                new MouseMoveAction(
                    GetRelativeTimestamp(),
                    dx,
                    dy,
                    false));
        }

        public void MouseMoveAbsolute(int dx, int dy)
        {
            _script.AppendAction(
                new MouseMoveAction(
                    GetRelativeTimestamp(),
                    dx,
                    dy,
                    true));
        }

        public void Command(string command)
        {
            _script.AppendAction(
                new CommandAction(
                    GetRelativeTimestamp(),
                    command));
        }

        private ulong GetRelativeTimestamp()
        {
            if (_firstTime)
            {
                _firstTime = false;
                //
                // First item recorded, occurs at relative timestamp 0.
                //
                _lastTimestamp = ScriptManager.ScriptScheduler.CurrentTimeNsec;
                return 0;
            }
            else
            {
                //
                // relative time is delta between current system timestamp and the last
                // recorded entry.
                ulong relativeTimestamp = ScriptManager.ScriptScheduler.CurrentTimeNsec - _lastTimestamp;
                _lastTimestamp = ScriptManager.ScriptScheduler.CurrentTimeNsec;

                return relativeTimestamp;
            }
        }

        private bool _enabled;

        private AltoSystem _system;
        private ulong _lastTimestamp;
        private bool _firstTime;
        private ScriptWriter _script;
    }
}
