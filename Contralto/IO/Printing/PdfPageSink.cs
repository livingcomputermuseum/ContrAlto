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

using System.IO;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System;
using Contralto.Logging;

namespace Contralto.IO.Printing
{
    /// <summary>
    /// PdfPageSink takes output from the ROS and turns it into
    /// PDF documents in the PrintOutputPath folder.
    /// 
    /// This uses the iTextSharp PDF creation libraries to do the hard work.
    /// </summary>
    public class PdfPageSink : IPageSink
    {
        public PdfPageSink()
        {
            _pageImages = new List<Image>();
        }

        public void StartDoc()
        {
            _pageImages.Clear();

            try
            {
                // Start a new document.
                // All output to a Dover printer is letter-sized.
                _pdf = new Document(PageSize.LETTER);

                string path = Path.Combine(
                    Configuration.PrintOutputPath,
                    String.Format("AltoDocument-{0}.pdf", DateTime.Now.ToString("yyyyMMdd-hhmmss")));

                PdfWriter writer = PdfWriter.GetInstance(
                    _pdf,
                    new FileStream(path, FileMode.Create));

                _pdf.Open();

                // Let the Orbit deal with the margins.
                _pdf.SetMargins(0, 0, 0, 0);
            }
            catch(Exception e)
            {
                //
                // Most likely we couldn't create the output file; log the failure.
                // All output will be relegated to the bit bucket.
                //
                _pdf = null;

                Log.Write(LogType.Error, LogComponent.DoverROS, "Failed to create output PDF.  Error {0}", e.Message);
            }
        }

        public void AddPage(byte[] rasters, int width, int height)
        {
            if (_pdf != null)
            {
                Image pageImage = iTextSharp.text.Image.GetInstance(height, width, 1 /* greyscale */, 1 /* 1bpp */, rasters);
                pageImage.SetDpi(375, 375);
                pageImage.SetAbsolutePosition(0, 0);
                pageImage.RotationDegrees = 90;
                pageImage.ScaleToFit(_pdf.PageSize);

                _pageImages.Add(pageImage);
            }
        }

        public void EndDoc()
        {
            if (_pdf != null)
            {
                try
                {
                    // Grab the configuration here so that if some joker changes the configuration
                    // while we're printing we don't do something weird.
                    bool reversePageOrder = Configuration.ReversePageOrder;

                    // Actually write out the pages now, in the proper order.
                    for (int i = 0; i < _pageImages.Count; i++)
                    {
                        _pdf.Add(_pageImages[reversePageOrder ? (_pageImages.Count - 1) - i : i]);
                        _pdf.NewPage();
                    }

                    _pdf.Close();
                }
                catch (Exception e)
                {
                    // Something bad happened during creation, log an error.
                    Log.Write(LogComponent.DoverROS, "Failed to create output PDF.  Error {0}", e.Message);
                }
            }
        }

        /// <summary>
        /// List of page images in this document.
        /// Since Alto software typically prints the document in reverse-page-order due to the way the 
        /// Dover produces output, we need be able to produce the PDF in reverse order.
        /// This uses extra memory, (about 1.7mb per page printed.)
        /// </summary>
        private List<Image> _pageImages;

        private Document _pdf;
    }
}
