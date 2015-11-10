using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Contralto.Logging;

namespace Contralto.Display
{
    /// <summary>
    /// DisplayController implements hardware controlling the virtual electron beam
    /// as it scans across the screen.  It implements the logic of the display's sync generator
    /// and wakes up the DVT and DHT tasks as necessary during a display field.
    /// </summary>
    public class DisplayController : IClockable
    {
        public DisplayController(AltoSystem system)
        {
            _system = system;
            Reset();
        }

        public void Reset()
        {
            _evenField = true;
        }

        public void Clock()
        {
            // TODO: Move the electron beam appropriately and wake up the display tasks
            // as necessary.
        }


        bool _evenField;


        private AltoSystem _system;
    }
}
