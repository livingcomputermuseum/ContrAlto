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
namespace Contralto.IO.Printing
{
    /// <summary>
    /// IPageSink defines the interface between a ROS device and physical output
    /// on the emulated host (be it real paper on a printer or PDF, Postscript, etc).    
    /// </summary>
    public interface IPageSink
    {
        /// <summary>
        /// Starts a new document to be printed.
        /// </summary>        
        void StartDoc();

        /// <summary>
        /// Adds a new page to the output.  This is provided as a byte array containing 'height' scanlines of the specified width.
        /// </summary>
        /// <param name="raster"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void AddPage(byte[] raster, int width, int height);

        /// <summary>
        /// Ends the document being printed.
        /// </summary>
        void EndDoc();
    }
}
