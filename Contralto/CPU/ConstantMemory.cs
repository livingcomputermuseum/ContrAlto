using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.CPU
{
    static class ControlROM
    {
        static ControlROM()
        {
            Init();
        }

        private static void Init()
        {
            LoadConstants(_constantRoms);
            LoadACSource(_acSourceRoms);
        }

        public static ushort[] ConstantROM
        {
            get { return _constantRom; }
        }

        public static byte[] ACSourceROM
        {
            get { return _acSourceRom; }
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
                        _constantRom[file.StartingAddress + i] |= (ushort)((DataMapConstantRom(data[AddressMapConstantRom(i)]) & 0xf) << file.BitPosition);
                    }
                }
            }

            // And invert all bits
            for (int i = 0; i < _constantRom.Length; i++)
            {
               _constantRom[i] = (ushort)((~_constantRom[i]) & 0xffff);
            } 
        }

        private static void LoadACSource(RomFile romInfo)
        {
            _acSourceRom = new byte[256];
            
            using (FileStream fs = new FileStream(Path.Combine("ROM", romInfo.Filename), FileMode.Open, FileAccess.Read))
            {
                int length = (int)fs.Length;
                if (length != 256)
                {
                    throw new InvalidOperationException("ROM file should be 256 bytes in length");
                }
                byte[] data = new byte[fs.Length];                
                fs.Read(data, 0, (int)fs.Length);

                // Copy in the data, modifying the address as required.
                for (int i = 0; i < length; i++)
                {
                    _acSourceRom[i] = (byte)((~data[AddressMapACSourceRom(i)]) & 0xf);
                }
            }
        }

        private static int AddressMapConstantRom(int address)
        {            
            // Descramble the address bits as they are in no sane order.    
            // (See 05a_AIM.pdf, pg. 5 (Page 9 of the orginal docs))
            int[] addressMapping = { 7, 2, 1, 0, 3, 4, 5, 6 };            
  
            int mappedAddress = 0;

            for (int i = 0; i < addressMapping.Length; i++)
            {
                if ((address & (1 << i)) != 0)
                {
                    mappedAddress |= (1 << (addressMapping[i]));
                }
            }            
            return mappedAddress;
        }

        private static int DataMapConstantRom(int data)
        {            
            // Reverse bits 0-4.
            int mappedData = 0;

            for (int i = 0; i < 4; i++)
            {
                if ((data & (1 << i)) != 0)
                {
                    mappedData |= (1 << (3-i));
                }
            }

            return mappedData;
        }

        private static int AddressMapACSourceRom(int data)
        {
            // Reverse bits 0-7.
            int mappedData = 0;

            for (int i = 0; i < 8; i++)
            {
                if ((data & (1 << i)) != 0)
                {
                    mappedData |= (1 << (7 - i));
                }
            }

            // And invert.
            return (~mappedData) & 0xff;
        }

        private static RomFile[] _constantRoms =
            {               
                new RomFile("c0", 0x000, 12),
                new RomFile("c1", 0x000, 8),
                new RomFile("c2", 0x000, 4),
                new RomFile("c3", 0x000, 0),                
            };

        private static RomFile _acSourceRoms = new RomFile("2kctl.u3", 0x000, 0);

        private static ushort[] _constantRom;
        private static byte[] _acSourceRom;
    }
}
