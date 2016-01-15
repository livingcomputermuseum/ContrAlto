#define LOGGING_ENABLED

using System;
using System.IO;

namespace Contralto.Logging
{
    /// <summary>
    /// Specifies a component to specify logging for
    /// </summary>
    [Flags]
    public enum LogComponent
    {
        None = 0,
        EmulatorTask = 0x1,
        DiskSectorTask = 0x2,
        DiskWordTask = 0x4,
        DiskController = 0x8, 
        Alu = 0x10,       
        Memory = 0x20,
        Keyboard = 0x40,
        Display = 0x80,
        Microcode = 0x100,
        CPU = 0x200,
        EthernetController = 0x400,
        EthernetTask = 0x800,
        TaskSwitch = 0x1000,
        HostEthernet = 0x2000,

        Debug = 0x40000000,
        All =   0x7fffffff
    }

    /// <summary>
    /// Specifies the type (or severity) of a given log message
    /// </summary>
    [Flags]
    public enum LogType
    {
        None = 0,
        Normal = 0x1,
        Warning = 0x2,
        Error = 0x4,
        Verbose = 0x8,
        All = 0x7fffffff
    }

    /// <summary>
    /// Provides basic functionality for logging messages of all types.
    /// </summary>
    public static class Log
    {
        static Log()
        {
            // TODO: make configurable
            _components = LogComponent.HostEthernet | LogComponent.EthernetController; // LogComponent.DiskController | LogComponent.DiskSectorTask | LogComponent.Debug | LogComponent.CPU; // LogComponent.EthernetController; // | LogComponent.Microcode | LogComponent.Memory | LogComponent.CPU;
            _type = LogType.Normal | LogType.Warning | LogType.Error | LogType.Verbose;

            _logStream = new StreamWriter("log.txt");
        }

        public static LogComponent LogComponents
        {
            get { return _components; }
            set { _components = value; }
        }

#if LOGGING_ENABLED
        /// <summary>
        /// Logs a message without specifying type/severity for terseness;
        /// will not log if Type has been set to None.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Write(LogComponent component, string message, params object[] args)
        {
            Write(LogType.Normal, component, message, args);            
        }

        public static void Write(LogType type, LogComponent component, string message, params object[] args)
        {
            if ((_type & type) != 0 &&
                (_components & component) != 0)
            {
                //
                // My log has something to tell you...
                // TODO: color based on type, etc.
                Console.WriteLine(component.ToString() + ": " + message, args);

                if (_logStream != null)
                {
                    _logStream.WriteLine(component.ToString() + ": " + message, args);
                }
            }
        }
#else
        public static void Write(LogComponent component, string message, params object[] args)
        {
            
        }

        public static void Write(LogType type, LogComponent component, string message, params object[] args)
        {

        }

#endif

        private static LogComponent _components;
        private static LogType _type;
        private static StreamWriter _logStream;
    }
}
