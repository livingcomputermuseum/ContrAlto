using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{
    struct RomFile
    {
        public RomFile(string filename, ushort addr, int bitPosition)
        {
            Filename = filename;
            StartingAddress = addr;
            BitPosition = bitPosition;
        }

        public string Filename;
        public ushort StartingAddress;
        public int BitPosition;
    }

    static class UCodeMemory
    {
        static UCodeMemory()
        {
            Init();
        }

        private static void Init()
        {            
            _uCodeRam = new UInt32[1024];
            LoadMicrocode(_uCodeRoms);
            //_constantRom = LoadMicrocode(_constantRoms);
        }

        public static UInt32[] UCodeROM
        {
            get { return _uCodeRom; }
        }

        private static void LoadMicrocode(RomFile[] romInfo)
        {
            _uCodeRom = new UInt32[2048];

            foreach(RomFile file in romInfo)
            {
                //
                // Each file contains 1024 bytes, each byte containing one nybble in the low 4 bits.
                //
                using(FileStream fs = new FileStream(Path.Combine("ROM", file.Filename), FileMode.Open, FileAccess.Read))
                {
                    int length = (int)fs.Length;
                    if (length != 1024)
                    {
                        throw new InvalidOperationException("ROM file should be 1024 bytes in length");
                    }

                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);

                    // OR in the data
                    for(int i=0;i<length;i++)
                    {
                        _uCodeRom[file.StartingAddress + i] |= (uint)((data[AddressMap(i)] & 0xf) << file.BitPosition);
                    }
                }
            }

            // Invert the requisite bits just to make things easier; the high bits of F1 and F2 and the Load L bit are inverted
            // normally; we leave those alone.
            const UInt32 invertedBitMask = 0xfff77bff;

            
            for(int i=0;i<_uCodeRom.Length;i++)
            {
                UInt32 masked = _uCodeRom[i] & ~invertedBitMask;
                _uCodeRom[i] = ((~_uCodeRom[i]) & invertedBitMask) | masked;
            } 

        }

        private static int AddressMap(int address)
        {
            //
            // For reasons lost to time, the address bits on the ucode PROMs are flipped
            // (that is, address bit 9 on the PROM chip is connected to address bit 0 on the Alto's
            // microcode address lines, and so on).  The address bits are also inverted, so that's fun too.
            // We need to translate the 10-bit address appropriately by swapping the bits around.
            //
           int  mappedAddress = (~address) & 0x3ff;
            /*
            int mappedAddress = 0;
            for(int i=0;i<10;i++)
            {
                if ((address & (1 << i)) != 0)
                {
                    mappedAddress |= (1 << (9 - i));
                }
            } */

            return mappedAddress;
        }

        private static RomFile[] _uCodeRoms =
        {
            // first K
            new RomFile("u55", 0x000, 28),
            new RomFile("u64", 0x000, 24),
            new RomFile("u65", 0x000, 20),
            new RomFile("u63", 0x000, 16),
            new RomFile("u53", 0x000, 12),
            new RomFile("u60", 0x000, 8),
            new RomFile("u61", 0x000, 4),
            new RomFile("u62", 0x000, 0),

            // second K
            new RomFile("u54", 0x400, 28),
            new RomFile("u74", 0x400, 24),
            new RomFile("u75", 0x400, 20),
            new RomFile("u73", 0x400, 16),
            new RomFile("u52", 0x400, 12),
            new RomFile("u70", 0x400, 8),
            new RomFile("u71", 0x400, 4),
            new RomFile("u72", 0x400, 0)
        };

        private static RomFile[] _constantRoms =
        {

        };

        private static UInt32[] _uCodeRom;
        private static UInt32[] _uCodeRam;
        private static UInt16[] _constantRom;


    }
}
