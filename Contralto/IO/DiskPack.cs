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
using System.IO;

namespace Contralto.IO
{

    /// <summary>
    /// DiskGeometry encapsulates the geometry of a disk.
    /// </summary>
    public struct DiskGeometry
    {
        public DiskGeometry(int cylinders, int heads, int sectors, SectorGeometry sectorGeometry)
        {
            Cylinders = cylinders;
            Heads = heads;
            Sectors = sectors;
            SectorGeometry = sectorGeometry;
        }

        public int Cylinders;
        public int Heads;
        public int Sectors;
        public SectorGeometry SectorGeometry;

        /// <summary>
        /// Returns the total size (in bytes) of a disk with this geometry.
        /// This includes the extra word-per-sector of the Bitsavers images.
        /// </summary>
        /// <returns></returns>
        public int GetDiskSizeBytes()
        {            
            return SectorGeometry.GetSectorSizeBytes() * (Sectors * Heads * Cylinders);
        }

        //
        // Standard Diablo geometries
        //
        public static readonly DiskGeometry Diablo31 = new DiskGeometry(203, 2, 12, SectorGeometry.Diablo);
        public static readonly DiskGeometry Diablo44 = new DiskGeometry(406, 2, 12, SectorGeometry.Diablo);

        //
        // Standard Trident geometries
        //
        public static readonly DiskGeometry TridentT80 = new DiskGeometry(815, 5, 9, SectorGeometry.Trident);
        public static readonly DiskGeometry TridentT300 = new DiskGeometry(815, 19, 9, SectorGeometry.Trident);
        public static readonly DiskGeometry Shugart4004 = new DiskGeometry(202, 4, 8, SectorGeometry.Trident);
        public static readonly DiskGeometry Shugart4008 = new DiskGeometry(202, 8, 8, SectorGeometry.Trident);
    }
    
    /// <summary>
    /// Describes the geometry of an Alto disk sector in terms of the 
    /// size of the header, label, and data blocks.
    /// </summary>
    public struct SectorGeometry
    {
        /// <summary>
        /// Specifies the layout of a sector on an Alto pack.  Sizes are in bytes.
        /// </summary>        
        public SectorGeometry(int headerSizeBytes, int labelSizeBytes, int dataSizeBytes)
        {
            HeaderSizeBytes = headerSizeBytes;
            LabelSizeBytes = labelSizeBytes;
            DataSizeBytes = dataSizeBytes;

            HeaderSize = HeaderSizeBytes / 2;
            LabelSize = LabelSizeBytes / 2;
            DataSize = DataSizeBytes / 2;
        }

        public int HeaderSize;
        public int LabelSize;
        public int DataSize;

        public int HeaderSizeBytes;
        public int LabelSizeBytes;
        public int DataSizeBytes;

        /// <summary>
        /// Returns the total size (in bytes) of a sector with this geometry.
        /// This includes the extra word-per-sector of the Bitsavers images.
        /// </summary>
        /// <returns></returns>
        public int GetSectorSizeBytes()
        {
            return  DataSizeBytes +
                    LabelSizeBytes +
                    HeaderSizeBytes +
                    2;     // Extra dummy word
        }

        public static readonly SectorGeometry Diablo = new SectorGeometry(4, 16, 512);
        public static readonly SectorGeometry Trident = new SectorGeometry(4, 20, 2048);
    } 

    /// <summary>
    /// DiskSector encapsulates the records contained in a single Alto disk sector
    /// on a disk.  This includes the header, label, and data records.
    /// </summary>
    public class DiskSector
    {
        /// <summary>
        /// Create a new DiskSector populated from the specified stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="inputStream"></param>
        /// <param name="cylinder"></param>
        /// <param name="head"></param>
        /// <param name="sector"></param>
        public DiskSector(SectorGeometry geometry, Stream inputStream, int cylinder, int head, int sector)
        {
            //
            // Read in the sector from the input stream.
            //
            byte[] header = new byte[geometry.HeaderSizeBytes];
            byte[] label = new byte[geometry.LabelSizeBytes];
            byte[] data = new byte[geometry.DataSizeBytes];

            //
            // Bitsavers images have an extra word in the header for some reason.
            // ignore it.
            // TODO: should support different formats ("correct" raw, Alto CopyDisk format, etc.)
            //
            inputStream.Seek(2, SeekOrigin.Current);

            if (inputStream.Read(header, 0, header.Length) != header.Length)
            {
                throw new InvalidOperationException("Short read while reading sector header.");
            }

            if (inputStream.Read(label, 0, label.Length) != label.Length)
            {
                throw new InvalidOperationException("Short read while reading sector label.");
            }

            if (inputStream.Read(data, 0, data.Length) != data.Length)
            {
                throw new InvalidOperationException("Short read while reading sector data.");
            }

            _header = GetUShortArray(header);
            _label = GetUShortArray(label);
            _data = GetUShortArray(data);

            _cylinder = cylinder;
            _head = head;
            _sector = sector;

            _modified = false;
        }

        /// <summary>
        /// Create a new, empty sector.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="cylinder"></param>
        /// <param name="head"></param>
        /// <param name="sector"></param>
        public DiskSector(SectorGeometry geometry, int cylinder, int head, int sector)
        {
            _header = new ushort[geometry.HeaderSize];
            _label = new ushort[geometry.LabelSize];
            _data = new ushort[geometry.DataSize];

            _cylinder = cylinder;
            _head = head;
            _sector = sector;

            _modified = true;
        }

        public int Cylinder
        {
            get { return _cylinder; }
        }

        public int Head
        {
            get { return _head; }
        }

        public int Sector
        {
            get { return _sector; }
        }

        public bool Modified
        {
            get { return _modified; }
        }

        public ushort ReadHeader(int offset)
        {
            return _header[offset];
        }

        public void WriteHeader(int offset, ushort value)
        {
            _header[offset] = value;
            _modified = true;
        }

        public ushort ReadLabel(int offset)
        {
            return _label[offset];
        }

        public void WriteLabel(int offset, ushort value)
        {
            _label[offset] = value;
            _modified = true;
        }

        public ushort ReadData(int offset)
        {
            return _data[offset];
        }

        public void WriteData(int offset, ushort value)
        {
            _data[offset] = value;
            _modified = true;
        }

        public void WriteToStream(Stream s)
        {
            //
            // Bitsavers images have an extra word in the header for some reason.
            // We will follow this standard when writing out.
            // TODO: should support different formats ("correct" raw, Alto CopyDisk format, etc.)
            //
            byte[] dummy = new byte[2];
            s.Write(dummy, 0, 2);

            WriteWordBuffer(s, _header);
            WriteWordBuffer(s, _label);
            WriteWordBuffer(s, _data);

            _modified = false;
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

        private void WriteWordBuffer(Stream imageStream, ushort[] buffer)
        {
            // TODO: this is beyond inefficient
            for (int i = 0; i < buffer.Length; i++)
            {
                imageStream.WriteByte((byte)buffer[i]);
                imageStream.WriteByte((byte)(buffer[i] >> 8));
            }
        }

        private ushort[] _header;
        private ushort[] _label;
        private ushort[] _data;

        private int _cylinder;
        private int _head;
        private int _sector;

        private bool _modified;
    }

    /// <summary>
    /// The IDiskPack interface defines a generic mechanism for creating, loading, storing,
    /// and accessing the sectors of a disk pack.
    /// </summary>
    public interface IDiskPack : IDisposable
    {
        /// <summary>
        /// The geometry of this pack.
        /// </summary>
        DiskGeometry Geometry { get; }

        /// <summary>
        /// The filename of this pack.
        /// </summary>
        string PackName { get; }

        /// <summary>
        /// Commits the current in-memory image back to the file it came from.
        /// </summary>
        void Save();

        /// <summary>
        /// Retrieves the specified sector from storage.
        /// </summary>
        /// <param name="cylinder"></param>
        /// <param name="head"></param>
        /// <param name="sector"></param>
        /// <returns></returns>
        DiskSector GetSector(int cylinder, int head, int sector);

        /// <summary>
        /// Commits this sector back to storage.
        /// </summary>
        /// <param name="sector"></param>
        void CommitSector(DiskSector sector);
    }


    /// <summary>
    /// In-memory implementation of IDiskPack.  Useful for small disks (e.g. Diablo).
    /// Changes to the in-memory copy are only committed back to the disk image
    /// when Save is invoked.
    /// </summary>
    public class InMemoryDiskPack : IDiskPack
    {
        /// <summary>
        /// Creates a new, empty disk pack with the specified geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="path"></param>
        public static InMemoryDiskPack CreateEmpty(DiskGeometry geometry, string path)
        {
            return new InMemoryDiskPack(geometry, path, false);
        }

        /// <summary>
        /// Loads an existing disk pack image.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static InMemoryDiskPack Load(DiskGeometry geometry, string path)
        {
            return new InMemoryDiskPack(geometry, path, true);
        }

        public void Dispose()
        {
            //
            // Nothing to do here.
            //
        }

        private InMemoryDiskPack(DiskGeometry geometry, string path, bool load)
        {
            _packName = path;
            _geometry = geometry;
            _sectors = new DiskSector[_geometry.Cylinders, _geometry.Heads, _geometry.Sectors];            

            if (load)
            {
                //
                // Attempt to load in the specified image file.
                //
                using (FileStream imageStream = new FileStream(_packName, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        for (int cylinder = 0; cylinder < _geometry.Cylinders; cylinder++)
                        {
                            for (int head = 0; head < _geometry.Heads; head++)
                            {
                                for (int sector = 0; sector < _geometry.Sectors; sector++)
                                {
                                    _sectors[cylinder, head, sector] = new DiskSector(_geometry.SectorGeometry, imageStream, cylinder, head, sector);
                                }
                            }
                        }

                        if (imageStream.Position != imageStream.Length)
                        {
                            throw new InvalidOperationException("Extra data at end of image file.");
                        }
                    }
                    finally
                    {
                        imageStream.Close();
                    }
                }
            }
            else
            {
                //
                // Just initialize a new, empty disk.
                //
                for (int cylinder = 0; cylinder < _geometry.Cylinders; cylinder++)
                {
                    for (int head = 0; head < _geometry.Heads; head++)
                    {
                        for (int sector = 0; sector < _geometry.Sectors; sector++)
                        {
                            _sectors[cylinder, head, sector] = new DiskSector(_geometry.SectorGeometry, cylinder, head, sector);
                        }
                    }
                }
            }
        }

        public DiskGeometry Geometry
        {
            get { return _geometry; }
        }

        public string PackName
        {
            get { return _packName; }
        }

        /// <summary>
        /// Commits the current in-memory image back to the file from which it was loaded.
        /// </summary>
        public void Save()
        {
            using (FileStream imageStream = new FileStream(_packName, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    for (int cylinder = 0; cylinder < _geometry.Cylinders; cylinder++)
                    {
                        for (int head = 0; head < _geometry.Heads; head++)
                        {
                            for (int sector = 0; sector < _geometry.Sectors; sector++)
                            {                               
                                _sectors[cylinder, head, sector].WriteToStream(imageStream);
                            }
                        }
                    }
                }
                finally
                {
                    imageStream.Close();
                }
            }
        }

        public DiskSector GetSector(int cylinder, int head, int sector)
        {
            //
            // Return the in-memory sector reference.
            //
            return _sectors[cylinder, head, sector];
        }

        public void CommitSector(DiskSector sector)
        {
            //
            // Update the in-memory sector reference to point to this (possibly new) sector object.
            //
            if (sector.Modified)
            {
                _sectors[sector.Cylinder, sector.Head, sector.Sector] = sector;
            }
        }

        private string _packName;               // The file from whence the data came
        private DiskSector[,,] _sectors;        // All of the sectors on disk
        private DiskGeometry _geometry;         // The geometry of this disk.
    }

    /// <summary>
    /// FileBackedDiskPack provides an implementation of IDiskPack where sectors are read into memory
    /// only when requested, and changes are flushed to disk when use of the sector is complete.
    /// </summary>
    public class FileBackedDiskPack : IDiskPack
    {
        /// <summary>
        /// Creates a new, empty disk pack with the specified geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="path"></param>
        public static FileBackedDiskPack CreateEmpty(DiskGeometry geometry, string path)
        {
            return new FileBackedDiskPack(geometry, path, false);
        }

        /// <summary>
        /// Loads an existing image.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileBackedDiskPack Load(DiskGeometry geometry, string path)
        {
            return new FileBackedDiskPack(geometry, path, true);
        }

        public void Dispose()
        {
            if (_diskStream != null)
            {
                _diskStream.Close();
            }
        }

        private FileBackedDiskPack(DiskGeometry geometry, string path, bool load)
        {
            _packName = path;
            _geometry = geometry;
            
            if (load)
            {
                //
                // Attempt to open an existing stream for read/write access.
                //
                _diskStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);

                //
                // Quick sanity check that the disk image is the right size.
                //
                if (_diskStream.Length != geometry.GetDiskSizeBytes())
                {
                    _diskStream.Close();
                    _diskStream = null;
                    throw new InvalidOperationException("Image size is invalid.");
                }
            }
            else
            {
                //
                // Attempt to initialize a new stream with read/write access.
                //
                _diskStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

                //
                // And set the size of the stream.
                //
                _diskStream.SetLength(geometry.GetDiskSizeBytes());
            }
        }

        public DiskGeometry Geometry
        {
            get { return _geometry; }
        }

        public string PackName
        {
            get { return _packName; }
        }

        /// <summary>
        /// Commits pending changes back to disc.
        /// </summary>
        /// <param name="imageStream"></param>
        public void Save()
        {
            // Nothing here, we expect CommitSector
            // to be called by whoever has a sector checked out before shutdown.
        }

        public DiskSector GetSector(int cylinder, int head, int sector)
        {
            //
            // Retrieve the appropriate sector from disk.
            // Seek to the appropriate position and read.
            //
            _diskStream.Position = GetOffsetForSector(cylinder, head, sector);

            return new DiskSector(_geometry.SectorGeometry, _diskStream, cylinder, head, sector);
        }

        public void CommitSector(DiskSector sector)
        {
            if (sector.Modified)
            {
                //
                // Commit this data back to disk.
                // Seek to the appropriate position and flush.
                //
                _diskStream.Position = GetOffsetForSector(sector.Cylinder, sector.Head, sector.Sector);
                sector.WriteToStream(_diskStream);
            }
        }

        private long GetOffsetForSector(int cylinder, int head, int sector)
        {
            int sectorNumber = (cylinder * _geometry.Heads * _geometry.Sectors) +
                               (head * _geometry.Sectors) +
                               sector;

            return sectorNumber * _geometry.SectorGeometry.GetSectorSizeBytes();
        }

        private string _packName;               // The file from whence the data came
        private FileStream _diskStream;         // The disk image stream containing this disk's contents.
        private DiskGeometry _geometry;         // The geometry of this disk.
    }
}
