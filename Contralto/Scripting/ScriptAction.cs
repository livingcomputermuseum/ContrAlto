using Contralto.IO;
using Contralto.SdlUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{

    /// <summary>
    /// Base class for scripting actions.
    /// "Timestamp" provides a relative timestamp (in nsec) for the action.
    /// "Completed" indicates whether the action completed during the last execution.
    /// Actions can run multiple times by leaving Completed = false and adjusting the
    /// Timestamp appropriately; the playback engine will reschedule it in this case.
    /// </summary>
    public abstract class ScriptAction
    {
        public ScriptAction(ulong timestamp)
        {
            _timestamp = timestamp;
        }

        /// <summary>
        /// Relative timestamp for this action.
        /// </summary>
        public ulong Timestamp
        {
            get { return _timestamp; }
        }

        /// <summary>
        /// Whether the action has completed after the last
        /// Replay action
        /// </summary>
        public bool Completed
        {
            get { return _completed; }
        }

        /// <summary>
        /// Replays a single step of the action.  If the action is completed,
        /// Completed will be true afterwards.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="controller"></param>
        public abstract void Replay(AltoSystem system, ExecutionController controller);

        /// <summary>
        /// Constructs the proper ScriptAction from a given line of text
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static ScriptAction Parse(string line)
        {
            //            
            // An Action consists of a line in the format:
            //  <timestamp> <Action Type> [args]
            // 
            // <timestamp> specifies a time relative to the last action, and may be:
            // - a 64-bit integer indicating a time in nanoseconds
            // - a double-precision floating point integer ending with "ms" indicating time in milliseconds
            // - a "-", indicating a relative time of zero.  (a "0" also works).
            //
            string[] tokens = line.Split(new char[] { ' ', ',' });

            if (tokens.Length < 2)
            {
                throw new InvalidOperationException("Invalid Action format.");
            }

            ulong timestamp = 0;

            if (tokens[0] != "-")
            {
                if (tokens[0].ToLowerInvariant().EndsWith("ms"))
                {
                    // timestamp in msec
                    double fstamp = double.Parse(tokens[0].Substring(0, tokens[0].Length - 2));

                    timestamp = (ulong)(fstamp * Conversion.MsecToNsec);
                }
                else
                {
                    // assume timestamp in nsec
                    timestamp = ulong.Parse(tokens[0]);
                }
            }

            switch(tokens[1])
            {
                case "KeyDown":
                    return KeyAction.Parse(timestamp, true, tokens);

                case "KeyUp":
                    return KeyAction.Parse(timestamp, false, tokens);

                case "MouseDown":
                    return MouseButtonAction.Parse(timestamp, true, tokens);

                case "MouseUp":
                    return MouseButtonAction.Parse(timestamp, false, tokens);

                case "MouseMove":
                    return MouseMoveAction.Parse(timestamp, false, tokens);

                case "MouseMoveAbsolute":
                    return MouseMoveAction.Parse(timestamp, true, tokens);

                case "Command":
                    return CommandAction.Parse(timestamp, tokens);

                case "KeyStroke":
                    return KeyStrokeAction.Parse(timestamp, tokens);

                case "Type":
                    return TypeAction.Parse(timestamp, false, tokens);

                case "TypeLine":
                    return TypeAction.Parse(timestamp, true, tokens);

                case "Wait":
                    return WaitAction.Parse(timestamp, true, tokens);

                default:
                    throw new InvalidOperationException("Invalid Action");

            }
        }

        protected ulong _timestamp;
        protected bool _completed;
    }


    /// <summary>
    /// Injects a single key action (up or down) into the Alto's keyboard.
    /// </summary>
    public class KeyAction : ScriptAction
    {
        public KeyAction(ulong timestamp, AltoKey key, bool keyDown) : base(timestamp)
        {
            _key = key;
            _keyDown = keyDown;
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            if (_keyDown)
            {
                system.Keyboard.KeyDown(_key);
            }
            else
            {
                system.Keyboard.KeyUp(_key);
            }

            _completed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", _timestamp, _keyDown ? "KeyDown" : "KeyUp", _key);
        }

        public static KeyAction Parse(ulong timestamp, bool keyDown, string[] tokens)
        {
            if (tokens.Length != 3)
            {
                throw new InvalidOperationException("Invalid KeyAction syntax.");
            }

            AltoKey key = (AltoKey)Enum.Parse(typeof(AltoKey), tokens[2]);

            return new KeyAction(timestamp, key, keyDown);

        }

        private AltoKey _key;
        private bool _keyDown;
    }

    /// <summary>
    /// Injects a single mouse button action (up or down) into the Alto's Mouse.
    /// </summary>
    public class MouseButtonAction : ScriptAction
    {
        public MouseButtonAction(ulong timestamp, AltoMouseButton buttons, bool mouseDown) : base(timestamp)
        {
            _buttons = buttons;
            _mouseDown = mouseDown;
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            if (_mouseDown)
            {
                system.MouseAndKeyset.MouseDown(_buttons);
            }
            else
            {
                system.MouseAndKeyset.MouseUp(_buttons);
            }

            _completed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", _timestamp, _mouseDown ? "MouseDown" : "MouseUp", _buttons);
        }

        public static MouseButtonAction Parse(ulong timestamp, bool mouseDown, string[] tokens)
        {
            if (tokens.Length != 3)
            {
                throw new InvalidOperationException("Invalid MouseButtonAction syntax.");
            }

            AltoMouseButton button = (AltoMouseButton)Enum.Parse(typeof(AltoMouseButton), tokens[2]);

            return new MouseButtonAction(timestamp, button, mouseDown);

        }

        private AltoMouseButton _buttons;
        private bool _mouseDown;
    }

    /// <summary>
    /// Injects a mouse movement into the Alto's mouse.
    /// </summary>
    public class MouseMoveAction : ScriptAction
    {
        public MouseMoveAction(ulong timestamp, int dx, int dy, bool absolute) : base(timestamp)
        {
            _dx = dx;
            _dy = dy;
            _absolute = absolute;
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            if (_absolute)
            {
                //
                // We stuff the x/y coordinates into the well-defined memory locations for the mouse coordinates.
                //                
                system.Memory.Load(0x114, (ushort)_dx, CPU.TaskType.Emulator, false);
                system.Memory.Load(0x115, (ushort)_dy, CPU.TaskType.Emulator, false);
            }        
            else
            {
                system.MouseAndKeyset.MouseMove(_dx, _dy);
            }

            _completed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2},{3}", _timestamp, _absolute ? "MouseMoveAbsolute" : "MouseMove", _dx, _dy);
        }

        public static MouseMoveAction Parse(ulong timestamp, bool absolute, string[] tokens)
        {
            if (tokens.Length != 4)
            {
                throw new InvalidOperationException("Invalid MouseMoveAction syntax.");
            }

            int dx = int.Parse(tokens[2]);
            int dy = int.Parse(tokens[3]);

            return new MouseMoveAction(timestamp, dx, dy, absolute);
        }

        private int _dx;
        private int _dy;
        private bool _absolute;
    }

    /// <summary>
    /// Injects a command execution to control the Alto system.  See ControlCommands for
    /// the actual commands.
    /// </summary>
    public class CommandAction : ScriptAction
    {
        public CommandAction(ulong timestamp, string commandString) : base(timestamp)
        {
            _commandString = commandString;
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            //
            // Execute the command.
            //
            // TODO: recreating these objects each time through is uncool.
            //
            ControlCommands controlCommands = new ControlCommands(system, controller);
            CommandExecutor executor = new CommandExecutor(controlCommands);
            
            CommandResult res = executor.ExecuteCommand(_commandString);

            if (res == CommandResult.Quit ||
                res == CommandResult.QuitNoSave)
            {
                //
                // Force an exit, commit disks if result was Quit.
                //
                throw new ShutdownException(res == CommandResult.Quit);
            }

            _completed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} Command {1}", _timestamp, _commandString);
        }

        public static CommandAction Parse(ulong timestamp, string[] tokens)
        {
            if (tokens.Length < 3)
            {
                throw new InvalidOperationException("Invalid Command syntax.");
            }

            StringBuilder commandString = new StringBuilder();

            for (int i = 2; i < tokens.Length; i++)
            {
                commandString.AppendFormat("{0} ", tokens[i]);
            }

            return new CommandAction(timestamp, commandString.ToString());

        }

        private string _commandString;
    }

    /// <summary>
    /// Injects one or more simultaneous keystrokes (keydown followed by keyup) into the
    /// Alto's keyboard.
    /// </summary>
    public class KeyStrokeAction : ScriptAction
    {
        public KeyStrokeAction(ulong timestamp, AltoKey[] keys) : base(timestamp)
        {
            _keys = keys;
            _keyDown = true;
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            //
            // Press all requested keys simultaneously, then release them.
            //
            foreach(AltoKey key in _keys)
            {
                if (_keyDown)
                {
                    system.Keyboard.KeyDown(key);
                }
                else
                {
                    system.Keyboard.KeyUp(key);
                }
            }

            if (_keyDown)
            {
                // Delay 50ms, then repeat for keyup
                _keyDown = false;
                _completed = false;
                _timestamp = 50 * Conversion.MsecToNsec;
            }
            else
            {
                _completed = true;
            }
        }

        public override string ToString()
        {
            StringBuilder keyString = new StringBuilder();

            foreach(AltoKey key in _keys)
            {
                keyString.AppendFormat("{0} ", key);
            }

            return String.Format("{0} KeyStroke {1}", _timestamp, keyString.ToString());
        }

        public static KeyStrokeAction Parse(ulong timestamp, string[] tokens)
        {
            if (tokens.Length < 3)
            {
                throw new InvalidOperationException("Invalid KeyStroke syntax.");
            }

            AltoKey[] keys = new AltoKey[tokens.Length - 2];

            for (int i = 2; i < tokens.Length; i++)
            {
                keys[i - 2] = (AltoKey)Enum.Parse(typeof(AltoKey), tokens[i]);
            }

            return new KeyStrokeAction(timestamp, keys);
        }

        private AltoKey[] _keys;
        private bool _keyDown;
    }

    /// <summary>
    /// Injects a sequence of keystrokes corresponding to the keystrokes needed to
    /// type the provided string.
    /// </summary>
    public class TypeAction : ScriptAction
    {
        static TypeAction()
        {
            BuildKeyMap();
        }

        public TypeAction(ulong timestamp, string text, bool cr) : base(timestamp)
        {
            _text = text;
            _cr = cr;
            _currentStroke = 0;

            BuildStrokeList(text);
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            if (_currentStroke >= _strokes.Count)
            {
                _completed = true;
            }
            else
            {
                Keystroke stroke = _strokes[_currentStroke++];

                if (stroke.Type == StrokeType.KeyDown)
                {
                    system.Keyboard.KeyDown(stroke.Key);
                }
                else
                {
                    system.Keyboard.KeyUp(stroke.Key);
                }

                // Delay 50ms before the next key
                _timestamp = 50 * Conversion.MsecToNsec;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", _timestamp, _cr ? "TypeLine" : "Type", _text);
        }

        public static TypeAction Parse(ulong timestamp, bool cr, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                throw new InvalidOperationException("Invalid TypeAction syntax.");
            }

            StringBuilder commandString = new StringBuilder();

            for (int i = 2; i < tokens.Length; i++)
            {
                commandString.AppendFormat(i == tokens.Length - 1 ? "{0}" : "{0} ", tokens[i]);
            }

            return new TypeAction(timestamp, commandString.ToString(), cr);

        }

        private void BuildStrokeList(string text)
        {
            _strokes = new List<Keystroke>();

            foreach (char c in text)
            {
                //
                // For capital letters or shifted symbols, we need to depress Shift first 
                // (and release it when done).
                //
                bool shifted = _shiftedKeyMap.ContainsKey(c);
                AltoKey charKey;
                if (shifted)
                {
                    Keystroke shift = new Keystroke(StrokeType.KeyDown, AltoKey.RShift);
                    _strokes.Add(shift);

                    charKey = _shiftedKeyMap[c];
                }
                else
                {
                    if (_unmodifiedKeyMap.ContainsKey(c))
                    {
                        charKey = _unmodifiedKeyMap[c];
                    }
                    else
                    {
                        // Ignore this keystroke.
                        continue;
                    }
                }

                _strokes.Add(new Keystroke(StrokeType.KeyDown, charKey));
                _strokes.Add(new Keystroke(StrokeType.KeyUp, charKey));

                if (shifted)
                {
                    Keystroke unshift = new Keystroke(StrokeType.KeyUp, AltoKey.RShift);
                    _strokes.Add(unshift);
                }
            }

            if (_cr)
            {
                // Add a Return keystroke to the end
                _strokes.Add(new Keystroke(StrokeType.KeyDown, AltoKey.Return));
                _strokes.Add(new Keystroke(StrokeType.KeyUp, AltoKey.Return));
            }
        }

        private enum StrokeType
        {
            KeyDown,
            KeyUp
        }

        private struct Keystroke
        {
            public Keystroke(StrokeType type, AltoKey key)
            {
                Type = type;
                Key = key;
            }

            public StrokeType Type;
            public AltoKey Key;
        }

        
        private static void BuildKeyMap()
        {
            _unmodifiedKeyMap = new Dictionary<char, AltoKey>();
            _shiftedKeyMap = new Dictionary<char, AltoKey>();

            // characters requiring no modifiers
            _unmodifiedKeyMap.Add('1', AltoKey.D1);
            _unmodifiedKeyMap.Add('2', AltoKey.D2);
            _unmodifiedKeyMap.Add('3', AltoKey.D3);
            _unmodifiedKeyMap.Add('4', AltoKey.D4);
            _unmodifiedKeyMap.Add('5', AltoKey.D5);
            _unmodifiedKeyMap.Add('6', AltoKey.D6);
            _unmodifiedKeyMap.Add('7', AltoKey.D7);
            _unmodifiedKeyMap.Add('8', AltoKey.D8);
            _unmodifiedKeyMap.Add('9', AltoKey.D9);
            _unmodifiedKeyMap.Add('0', AltoKey.D0);
            _unmodifiedKeyMap.Add('-', AltoKey.Minus);
            _unmodifiedKeyMap.Add('=', AltoKey.Plus);
            _unmodifiedKeyMap.Add('\\', AltoKey.BSlash);
            _unmodifiedKeyMap.Add('q', AltoKey.Q);
            _unmodifiedKeyMap.Add('w', AltoKey.W);
            _unmodifiedKeyMap.Add('e', AltoKey.E);
            _unmodifiedKeyMap.Add('r', AltoKey.R);
            _unmodifiedKeyMap.Add('t', AltoKey.T);
            _unmodifiedKeyMap.Add('y', AltoKey.Y);
            _unmodifiedKeyMap.Add('u', AltoKey.U);
            _unmodifiedKeyMap.Add('i', AltoKey.I);
            _unmodifiedKeyMap.Add('o', AltoKey.O);
            _unmodifiedKeyMap.Add('p', AltoKey.P);
            _unmodifiedKeyMap.Add('[', AltoKey.LBracket);
            _unmodifiedKeyMap.Add(']', AltoKey.RBracket);
            _unmodifiedKeyMap.Add('_', AltoKey.Arrow);
            _unmodifiedKeyMap.Add('a', AltoKey.A);
            _unmodifiedKeyMap.Add('s', AltoKey.S);
            _unmodifiedKeyMap.Add('d', AltoKey.D);
            _unmodifiedKeyMap.Add('f', AltoKey.F);
            _unmodifiedKeyMap.Add('g', AltoKey.G);
            _unmodifiedKeyMap.Add('h', AltoKey.H);
            _unmodifiedKeyMap.Add('j', AltoKey.J);
            _unmodifiedKeyMap.Add('k', AltoKey.K);
            _unmodifiedKeyMap.Add('l', AltoKey.L);
            _unmodifiedKeyMap.Add(';', AltoKey.Semicolon);
            _unmodifiedKeyMap.Add('\'', AltoKey.Quote);
            _unmodifiedKeyMap.Add('z', AltoKey.Z);
            _unmodifiedKeyMap.Add('x', AltoKey.X);
            _unmodifiedKeyMap.Add('c', AltoKey.C);
            _unmodifiedKeyMap.Add('v', AltoKey.V);
            _unmodifiedKeyMap.Add('b', AltoKey.B);
            _unmodifiedKeyMap.Add('n', AltoKey.N);
            _unmodifiedKeyMap.Add('m', AltoKey.M);
            _unmodifiedKeyMap.Add(',', AltoKey.Comma);
            _unmodifiedKeyMap.Add('.', AltoKey.Period);
            _unmodifiedKeyMap.Add('/', AltoKey.FSlash);
            _unmodifiedKeyMap.Add(' ', AltoKey.Space);

            // characters requiring a shift modifier
            _shiftedKeyMap.Add('!', AltoKey.D1);
            _shiftedKeyMap.Add('@', AltoKey.D2);
            _shiftedKeyMap.Add('#', AltoKey.D3);
            _shiftedKeyMap.Add('$', AltoKey.D4);
            _shiftedKeyMap.Add('%', AltoKey.D5);
            _shiftedKeyMap.Add('~', AltoKey.D6);
            _shiftedKeyMap.Add('&', AltoKey.D7);
            _shiftedKeyMap.Add('*', AltoKey.D8);
            _shiftedKeyMap.Add('(', AltoKey.D9);
            _shiftedKeyMap.Add(')', AltoKey.D0);            
            _shiftedKeyMap.Add('|', AltoKey.BSlash);
            _shiftedKeyMap.Add('Q', AltoKey.Q);
            _shiftedKeyMap.Add('W', AltoKey.W);
            _shiftedKeyMap.Add('E', AltoKey.E);
            _shiftedKeyMap.Add('R', AltoKey.R);
            _shiftedKeyMap.Add('T', AltoKey.T);
            _shiftedKeyMap.Add('Y', AltoKey.Y);
            _shiftedKeyMap.Add('U', AltoKey.U);
            _shiftedKeyMap.Add('I', AltoKey.I);
            _shiftedKeyMap.Add('O', AltoKey.O);
            _shiftedKeyMap.Add('P', AltoKey.P);
            _shiftedKeyMap.Add('{', AltoKey.LBracket);
            _shiftedKeyMap.Add('}', AltoKey.RBracket);
            _shiftedKeyMap.Add('^', AltoKey.Arrow);
            _shiftedKeyMap.Add('A', AltoKey.A);
            _shiftedKeyMap.Add('S', AltoKey.S);
            _shiftedKeyMap.Add('D', AltoKey.D);
            _shiftedKeyMap.Add('F', AltoKey.F);
            _shiftedKeyMap.Add('G', AltoKey.G);
            _shiftedKeyMap.Add('H', AltoKey.H);
            _shiftedKeyMap.Add('J', AltoKey.J);
            _shiftedKeyMap.Add('K', AltoKey.K);
            _shiftedKeyMap.Add('L', AltoKey.L);
            _shiftedKeyMap.Add(':', AltoKey.Semicolon);
            _shiftedKeyMap.Add('"', AltoKey.Quote);
            _shiftedKeyMap.Add('Z', AltoKey.Z);
            _shiftedKeyMap.Add('X', AltoKey.X);
            _shiftedKeyMap.Add('C', AltoKey.C);
            _shiftedKeyMap.Add('V', AltoKey.V);
            _shiftedKeyMap.Add('B', AltoKey.B);
            _shiftedKeyMap.Add('N', AltoKey.N);
            _shiftedKeyMap.Add('M', AltoKey.M);
            _shiftedKeyMap.Add('<', AltoKey.Comma);
            _shiftedKeyMap.Add('>', AltoKey.Period);
            _shiftedKeyMap.Add('?', AltoKey.FSlash);
        }


        private string _text;
        private List<Keystroke> _strokes;
        private int _currentStroke;
        private bool _cr;

        private static Dictionary<char, AltoKey> _unmodifiedKeyMap;
        private static Dictionary<char, AltoKey> _shiftedKeyMap;
    }

    /// <summary>
    /// Causes the Playback engine to wait until the Alto executes a wakeup STARTF.
    /// </summary>
    public class WaitAction : ScriptAction
    {
        public WaitAction(ulong timestamp) : base(timestamp)
        {
           
        }

        public override void Replay(AltoSystem system, ExecutionController controller)
        {
            // This is a no-op.
            _completed = true;
        }

        public override string ToString()
        {
            return String.Format("{0} Wait", _timestamp);
        }

        public static WaitAction Parse(ulong timestamp, bool keyDown, string[] tokens)
        {
            if (tokens.Length != 2)
            {
                throw new InvalidOperationException("Invalid WaitAction syntax.");
            }            

            return new WaitAction(timestamp);
        }        
    }
}
