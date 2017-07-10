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
    /// The NullPageSink:  All pages go to the bit-bucket, for the true paperless office.
    /// </summary>
    public class NullPageSink : IPageSink
    {
        public NullPageSink()
        {
            
        }

        public void StartDoc()
        {

        }

        public void AddPage(byte[] rasters, int width, int height)
        {
            
        }

        public void EndDoc()
        {
            
        }
    }
}
