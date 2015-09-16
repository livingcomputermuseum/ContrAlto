using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto
{
    /// <summary>
    /// Used by classes implementing devices that are clocked (i.e. that are dependent
    /// on time passing in units of a single CPU clock.)
    /// </summary>
    public interface IClockable
    {
        void Clock();
    }
}
