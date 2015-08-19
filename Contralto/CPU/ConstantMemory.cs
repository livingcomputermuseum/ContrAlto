using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{


    static class ConstantMemory
    {
        static ConstantMemory()
        {
            Init();
        }

        private static void Init()
        {
            LoadConstants(_constantRoms);
        }

        public static ushort[] ConstantROM
        {
            get { return _constantRom; }
        }

        private static void LoadConstants(RomFile[] romInfo)
        {
            _constantRom = new ushort[256];

            foreach (RomFile file in romInfo)
            {
                //
                // Each file contains 256 bytes, each byte containing one nybble in the low 4 bits.
                //
                using (FileStream fs = new FileStream(Path.Combine("ROM", file.Filename), FileMode.Open, FileAccess.Read))
                {
                    int length = (int)fs.Length;
                    if (length != 256)
                    {
                        throw new InvalidOperationException("ROM file should be 256 bytes in length");
                    }

                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);

                    // OR in the data
                    for (int i = 0; i < length; i++)
                    {
                        _constantRom[file.StartingAddress + i] |= (ushort)((data[AddressMap(i)] & 0xf) << file.BitPosition);
                    }
                }
            }
        }

        private static int AddressMap(int address)
        {
            int mappedAddress = (~address) & 0xff;
            return mappedAddress;
        }

        private static RomFile[] _constantRoms =
            {               
                new RomFile("c0", 0x000, 12),
                new RomFile("c1", 0x000, 8),
                new RomFile("c2", 0x000, 4),
                new RomFile("c3", 0x000, 0),                
            };

        private static UInt16[] _constantRom;
    }
}
