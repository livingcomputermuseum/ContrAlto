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

namespace Contralto.CPU
{
    /// <summary>
    /// Maintains a set of Control ROM images dumped from real Alto hardware.
    /// </summary>
    static class ControlROM
    {
        static ControlROM()
        {
            Init();          
        }

        private static void Init()
        {
            if (Configuration.SystemType == SystemType.AltoI)
            {
                LoadConstants(_constantRomsAltoI, true);                
                LoadACSource(_acSourceRoms, true);               
            }
            else
            {
                LoadConstants(_constantRomsAltoII, false);                
                LoadACSource(_acSourceRoms, true);
            }            
        }

        public static ushort[] ConstantROM
        {
            get { return _constantRom; }
        }

        public static byte[] ACSourceROM
        {
            get { return _acSourceRom; }
        }

        private static void LoadConstants(RomFile[] romInfo, bool flip)
        {
            _constantRom = new ushort[256];

            foreach (RomFile file in romInfo)
            {
                //
                // Each file contains 256 bytes, each byte containing one nybble in the low 4 bits.
                //
                using (FileStream fs = new FileStream(file.Filename, FileMode.Open, FileAccess.Read))
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
                        if (flip)
                        {
                            _constantRom[file.StartingAddress + i] |= (ushort)((DataMapConstantRom(~data[AddressMapConstantRom(i)]) & 0xf) << file.BitPosition);
                        }
                        else
                        {
                            _constantRom[file.StartingAddress + i] |= (ushort)((DataMapConstantRom(data[AddressMapConstantRom(i)]) & 0xf) << file.BitPosition);
                        }
                    }
                }
            }

            // And invert all bits
            for (int i = 0; i < _constantRom.Length; i++)
            {
               _constantRom[i] = (ushort)((~_constantRom[i]) & 0xffff);
            } 
        }

        private static void LoadACSource(RomFile romInfo, bool reverseBits)
        {
            _acSourceRom = new byte[256];
            
            using (FileStream fs = new FileStream(romInfo.Filename, FileMode.Open, FileAccess.Read))
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
                    if (reverseBits)
                    {
                        _acSourceRom[i] = (byte)((~DataMapConstantRom(data[AddressMapACSourceRom(i)])) & 0xf);
                    }
                    else
                    {
                        _acSourceRom[i] = (byte)((~data[AddressMapACSourceRom(i)]) & 0xf);
                    }
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

            // And invert data lines
            return (~mappedData) & 0xff;
        }
       
        private static RomFile[] _constantRomsAltoI =
           {
                new RomFile(Configuration.GetAltoIRomPath("c0_23.BIN"), 0x000, 12),
                new RomFile(Configuration.GetAltoIRomPath("c1_23.BIN"), 0x000, 8),
                new RomFile(Configuration.GetAltoIRomPath("c2_23.BIN"), 0x000, 4),
                new RomFile(Configuration.GetAltoIRomPath("c3_23.BIN"), 0x000, 0),
            };

        private static RomFile[] _constantRomsAltoII =
            {
                new RomFile(Configuration.GetAltoIIRomPath("c0"), 0x000, 12),
                new RomFile(Configuration.GetAltoIIRomPath("c1"), 0x000, 8),
                new RomFile(Configuration.GetAltoIIRomPath("c2"), 0x000, 4),
                new RomFile(Configuration.GetAltoIIRomPath("c3"), 0x000, 0),
            };

        private static RomFile _acSourceRoms = new RomFile(Configuration.GetRomPath("ACSOURCE.NEW"), 0x000, 0);

        private static ushort[] _constantRom;
        private static byte[] _acSourceRom;
    }
}
