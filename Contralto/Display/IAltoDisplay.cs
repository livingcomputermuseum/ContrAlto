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
