using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    public class ScriptReader
    {
        public ScriptReader(string scriptPath)
        {
            _scriptReader = new StreamReader(scriptPath);
        }

        public ScriptAction ReadNext()
        {
            if (_scriptReader == null)
            {
                return null;
            }

            //
            // Read the next action from the script file,
            // skipping over comments and empty lines.
            //
            while (true)
            {
                if (_scriptReader.EndOfStream)
                {
                    // End of the stream, return null to indicate this,
                    // and close the stream.
                    _scriptReader.Close();
                    _scriptReader = null;
                    return null;
                }

                string line = _scriptReader.ReadLine().Trim();

                // Skip empty or comment lines.
                if (string.IsNullOrWhiteSpace(line) ||
                    line.StartsWith("#"))
                {
                    continue;
                }

                try
                {
                    return ScriptAction.Parse(line);
                }
                catch(Exception e)
                {
                    Log.Write(LogComponent.Scripting, "Invalid script; error: {0}.", e.Message);
                    _scriptReader.Close();
                    _scriptReader = null;
                    return null;
                }
            }
        }

        private StreamReader _scriptReader;

    }
}
