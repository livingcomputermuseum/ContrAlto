using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.IO
{

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
            _geometry = new DiskGeometry(type == DiabloDiskType.Diablo31 ? 203 : 406, 2, 12);
            _sectors = new DiabloDiskSector[_geometry.Cylinders, _geometry.Tracks, _geometry.Sectors];           
        }

        public DiskGeometry Geometry
        {
            get { return _geometry; }
        }

        public void Load(Stream imageStream)
        {
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

                        _sectors[cylinder, track, sector] = new DiabloDiskSector(header, label, data);                        
                    }
                }
            }

            if (imageStream.Position != imageStream.Length)
            {
                throw new InvalidOperationException("Extra data at end of image file.");
            }
        }

        public DiabloDiskSector GetSector(int cylinder, int track, int sector)
        {
            return _sectors[cylinder, track, sector];
        }

        private DiabloDiskSector[,,] _sectors;
        private DiabloDiskType _diskType;
        private DiskGeometry _geometry;
    }
}
