/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;

namespace Contralto.IO
{

    /// <summary>
    /// DiskGeometry encapsulates the geometry of a disk.
    /// </summary>
    public struct DiskGeometry
    {
        public DiskGeometry(int cylinders, int tracks, int sectors)
        {
            Cylinders = cylinders;
            Tracks = tracks;
            Sectors = sectors;
        }

        public int Cylinders;
        public int Tracks;
        public int Sectors;
    }

    public enum DiabloDiskType
    {
        Diablo31,
        Diablo44
    }

    /// <summary>
    /// DiabloDiskSector encapsulates the records contained in a single Alto disk sector
    /// on a Diablo disk.  This includes the header, label, and data records.
    /// </summary>
    public class DiabloDiskSector
    {
        public DiabloDiskSector(byte[] header, byte[] label, byte[] data)
        {
            if (header.Length != 4 ||
                label.Length != 16 ||
                data.Length != 512)
            {
                throw new InvalidOperationException("Invalid sector header/label/data length.");
            }

            Header = GetUShortArray(header);
            Label = GetUShortArray(label);
            Data = GetUShortArray(data);
        }

        private ushort[] GetUShortArray(byte[] data)
        {
            if ((data.Length % 2) != 0)
            {
                throw new InvalidOperationException("Array length must be even.");
            }

            ushort[] array = new ushort[data.Length / 2];

            int offset = 0;
            for(int i=0;i<array.Length;i++)
            {
                array[i] = (ushort)((data[offset]) | (data[offset + 1] << 8));
                offset += 2;
            }

            return array;
        }

        public ushort[] Header;
        public ushort[] Label;
        public ushort[] Data;
    }

    /// <summary>
    /// Encapsulates disk image data for all disk packs used with the
    /// standard Alto Disk Controller (i.e. the 31 and 44, which differ
    /// only in the number of cylinders)
    /// </summary>
    public class DiabloPack
    {
        public DiabloPack(DiabloDiskType type)
        {
            _diskType = type;
            _packName = null;
            _geometry = new DiskGeometry(type == DiabloDiskType.Diablo31 ? 203 : 406, 2, 12);
            _sectors = new DiabloDiskSector[_geometry.Cylinders, _geometry.Tracks, _geometry.Sectors];           
        }

        public DiskGeometry Geometry
        {
            get { return _geometry; }
        }

        public string PackName
        {
            get { return _packName; }
        }

        public void Load(Stream imageStream, string path, bool reverseByteOrder)
        {
            _packName = path;
            for(int cylinder = 0; cylinder < _geometry.Cylinders; cylinder++)
            {
                for(int track = 0; track < _geometry.Tracks; track++)
                {
                    for(int sector = 0; sector < _geometry.Sectors; sector++)
                    {
                        byte[] header = new byte[4];        // 2 words
                        byte[] label = new byte[16];        // 8 words
                        byte[] data = new byte[512];        // 256 words

                        //
                        // Bitsavers images have an extra word in the header for some reason.
                        // ignore it.
                        // TODO: should support different formats ("correct" raw, Alto CopyDisk format, etc.)
                        //                        
                        imageStream.Seek(2, SeekOrigin.Current);
                        

                        if (imageStream.Read(header, 0, header.Length) != header.Length)
                        {                            
                            throw new InvalidOperationException("Short read while reading sector header.");
                        }                        

                        if (imageStream.Read(label, 0, label.Length) != label.Length)
                        {
                            throw new InvalidOperationException("Short read while reading sector label.");
                        }

                        if (imageStream.Read(data, 0, data.Length) != data.Length)
                        {
                            throw new InvalidOperationException("Short read while reading sector data.");
                        }

                        if (reverseByteOrder)
                        {
                            SwapBytes(header);
                            SwapBytes(label);
                            SwapBytes(data);
                        }

                        _sectors[cylinder, track, sector] = new DiabloDiskSector(header, label, data);                        
                    }
                }
            }

            if (imageStream.Position != imageStream.Length)
            {
                throw new InvalidOperationException("Extra data at end of image file.");
            }
        }

        public void Save(Stream imageStream)
        {
            for (int cylinder = 0; cylinder < _geometry.Cylinders; cylinder++)
            {
                for (int track = 0; track < _geometry.Tracks; track++)
                {
                    for (int sector = 0; sector < _geometry.Sectors; sector++)
                    {
                        byte[] header = new byte[4];        // 2 words
                        byte[] label = new byte[16];        // 8 words
                        byte[] data = new byte[512];        // 256 words

                        //
                        // Bitsavers images have an extra word in the header for some reason.
                        // We will follow this 'standard' when writing out.
                        // TODO: should support different formats ("correct" raw, Alto CopyDisk format, etc.)
                        //
                        byte[] dummy = new byte[2];
                        imageStream.Write(dummy, 0, 2);

                        DiabloDiskSector s = _sectors[cylinder, track, sector];

                        WriteWordBuffer(imageStream, s.Header);
                        WriteWordBuffer(imageStream, s.Label);
                        WriteWordBuffer(imageStream, s.Data);                        
                    }
                }
            }
        }

        public DiabloDiskSector GetSector(int cylinder, int track, int sector)
        {
            return _sectors[cylinder, track, sector];
        }

        private void WriteWordBuffer(Stream imageStream, ushort[] buffer)
        {
            // TODO: this is beyond inefficient
            for(int i=0;i<buffer.Length;i++)
            {
                imageStream.WriteByte((byte)buffer[i]);
                imageStream.WriteByte((byte)(buffer[i] >> 8));
            }
        }

        private void SwapBytes(byte[] data)
        {
            for(int i=0;i<data.Length;i+=2)
            {
                byte t = data[i];
                data[i] = data[i + 1];
                data[i + 1] = t;
            }
        }

        private string _packName;               // The file from whence the data came
        private DiabloDiskSector[,,] _sectors;
        private DiabloDiskType _diskType;
        private DiskGeometry _geometry;
    }
}
