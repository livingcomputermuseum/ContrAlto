/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.Display
{
    /// <summary>
    /// IAltoDisplay defines the interface necessary for generating and rendering the Alto's
    /// bitmapped display.
    /// </summary>
    public interface IAltoDisplay
    {
        /// <summary>
        /// Renders a word's worth of data to the specified scanline and word offset.
        /// </summary>
        /// <param name="scanline"></param>
        /// <param name="wordOffset"></param>
        /// <param name="dataWord"></param>
        /// <param name="lowRes"></param>
        void DrawDisplayWord(int scanline, int wordOffset, ushort dataWord, bool lowRes);

        /// <summary>
        /// Renders the cursor word for the specified scanline.
        /// </summary>
        /// <param name="scanline"></param>
        /// <param name="wordOffset"></param>
        /// <param name="dataWord"></param>
        /// <param name="lowRes"></param>
        void DrawCursorWord(int scanline, int xOffset, bool whiteOnBlack, ushort cursorWord);

        /// <summary>
        /// Indicates that an entire frame is ready for display and should be rendered.
        /// </summary>
        void Render();
    }
}
