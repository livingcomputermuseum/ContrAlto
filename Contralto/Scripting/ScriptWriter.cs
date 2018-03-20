using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptWriter
    {
        public ScriptWriter(string scriptPath)
        {
            _scriptWriter = new StreamWriter(scriptPath);
        }

        /// <summary>
        /// Adds a new ScriptAction to the queue
        /// </summary>
        /// <param name="action"></param>
        public void AppendAction(ScriptAction action)
        {
            if (_scriptWriter == null)
            {
                throw new InvalidOperationException("Cannot write to closed ScriptWriter.");
            }

            _scriptWriter.WriteLine(action.ToString());
        }

        public void End()
        {
            _scriptWriter.Close();
            _scriptWriter = null;
        }

        private StreamWriter _scriptWriter;
    }
}
