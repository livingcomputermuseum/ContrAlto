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

using Contralto.Logging;
using System;
using System.IO;

namespace Contralto.CPU
{
    public enum MicrocodeBank
    {
        ROM0 = 0,
        ROM1 = 1,
        RAM0 = 2,
        RAM1 = 3,
        RAM2 = 4
    }

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

    /// <summary>
    /// UCodeMemory maintains a set of Microcode ROM images and provides facilities
    /// for accessing them.
    /// </summary>
    static class UCodeMemory
    {
        static UCodeMemory()
        {
            Init();
        }

        public static void Reset()
        {
            Init();
        }

        private static void Init()
        {     
            //                               
            // Max 3 banks of microcode RAM            
            _uCodeRam = new UInt32[1024 * 3];

            if (Configuration.SystemType == SystemType.AltoI)
            {
                LoadAltoIMicrocode(_uCodeRomsAltoI);
            }
            else
            {
                LoadAltoIIMicrocode(_uCodeRomsAltoII);
            }

            //
            // Cache 5k of instructions: max 2K ROM, 3K RAM.
            _decodeCache = new MicroInstruction[1024 * 5];

            // Precache ROM
            CacheMicrocodeROM();

            // Precache (empty) RAM
            for(ushort i=0;i<_uCodeRam.Length;i++)
            {
                UpdateRAMCache(i);
            }

            // Start in ROM0
            _microcodeBank = new MicrocodeBank[16];
            _ramAddr = 0;
            _ramBank = 0;
            _ramSelect = true;
            _lowHalfsel = true;

            // Cache the system type from the configuration
            _systemType = Configuration.SystemType;
        }

        public static void LoadBanksFromRMR(ushort rmr)
        {
            for(int i=0;i<16;i++)
            {                
                _microcodeBank[i] = (rmr & (1 << i)) == 0 ? MicrocodeBank.RAM0 : MicrocodeBank.ROM0;
            }
        }

        /// <summary>
        /// Exposes the raw contents of the Microcode ROM
        /// </summary>
        public static UInt32[] UCodeROM
        {
            get { return _uCodeRom; }
        }

        /// <summary>
        /// Exposes the raw contents of the Microcode RAM
        /// </summary>
        public static UInt32[] UCodeRAM
        {
            get { return _uCodeRam; }
        }

        public static MicrocodeBank GetBank(TaskType task)
        {            
            return _microcodeBank[(int)task];
        }

        public static void LoadControlRAMAddress(ushort address)
        {            
            _ramSelect = (address & 0x0800) == 0;
            _lowHalfsel = (address & 0x0400) == 0;
            _ramAddr = (address & 0x3ff);

            // Clip RAM bank into range, it's always 0 unless we have a 3K uCode RAM system
            if (_systemType != SystemType.ThreeKRam)
            {
                _ramBank = 0;
            }
            else
            {
                _ramBank = (address & 0x3000) >> 12;
            }
        }

        /// <summary>
        /// Implements the SWMODE F1 logic; selects the proper uCode bank (from
        /// RAM or ROM) based on the supplied NEXT value.
        /// Technically this is only supported for the Emulator task.
        /// </summary>
        /// <param name="nextAddress"></param>
        public static void SwitchMode(ushort nextAddress, TaskType task)
        {
            // Log.Write(Logging.LogComponent.Microcode, "SWMODE: Current Bank {0}", _microcodeBank[(int)task]);

            switch (_systemType)
            {
                case SystemType.AltoI:
                case SystemType.OneKRom:                
                    _microcodeBank[(int)task] = _microcodeBank[(int)task] == MicrocodeBank.ROM0 ? MicrocodeBank.RAM0 : MicrocodeBank.ROM0;
                    break;

                case SystemType.TwoKRom:
                    switch (_microcodeBank[(int)task])
                    {
                        case MicrocodeBank.ROM0:
                            _microcodeBank[(int)task] = (nextAddress & 0x100) == 0 ? MicrocodeBank.RAM0 : MicrocodeBank.ROM1;
                            break;

                        case MicrocodeBank.ROM1:
                            _microcodeBank[(int)task] = (nextAddress & 0x100) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.RAM0;
                            break;

                        case MicrocodeBank.RAM0:
                            _microcodeBank[(int)task] = (nextAddress & 0x100) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.ROM1;
                            break;
                    }
                    break;

                case SystemType.ThreeKRam:                    
                    if ((nextAddress & 0x100) == 0)
                    {
                        switch(_microcodeBank[(int)task])
                        {
                            case MicrocodeBank.ROM0:
                                _microcodeBank[(int)task] = (nextAddress & 0x80) == 0 ? MicrocodeBank.RAM0 : MicrocodeBank.RAM2;
                                break;

                            case MicrocodeBank.RAM0:
                            case MicrocodeBank.RAM1:
                                _microcodeBank[(int)task] = (nextAddress & 0x80) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.RAM2;
                                break;

                            case MicrocodeBank.RAM2:
                                _microcodeBank[(int)task] = (nextAddress & 0x80) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.RAM1;
                                break;
                        }
                    }
                    else
                    {
                        switch (_microcodeBank[(int)task])
                        {
                            case MicrocodeBank.ROM0:
                                _microcodeBank[(int)task] = (nextAddress & 0x80) == 0 ? MicrocodeBank.RAM1 : MicrocodeBank.RAM0;
                                break;

                            case MicrocodeBank.RAM0:
                                _microcodeBank[(int)task] = MicrocodeBank.RAM1;
                                break;

                            case MicrocodeBank.RAM1:
                            case MicrocodeBank.RAM2:
                                _microcodeBank[(int)task] = MicrocodeBank.RAM0;
                                break;
                        }
                    }                    
                    break;
            }

            // Log.Write(Logging.LogComponent.Microcode, "SWMODE: New Bank {0} for Task {1}", _microcodeBank[(int)task], task);            
        }

        public static ushort ReadRAM()
        {
            if (!_ramSelect)
            {
                throw new NotImplementedException("Read from microcode ROM not implemented.");
            }            
                         
            Log.Write(Logging.LogComponent.Microcode, "CRAM address for read: Bank {0}, RAM {1}, lowhalf {2} addr {3}",
                _ramBank,
                _ramSelect,
                _lowHalfsel,
                Conversion.ToOctal(_ramAddr));

            UInt32 data = MapRAMWord(_uCodeRam[_ramAddr + (_ramBank * 1024)]);

            // Flip the necessary bits before returning them.
            // (See table in section 8.3 of HWRef.)
            ushort halfWord = (ushort)(_lowHalfsel ? data : (data >> 16));

            Log.Write(Logging.LogComponent.Microcode, "CRAM data read: {0}-{1}",
                _lowHalfsel ? "low" : "high",
                Conversion.ToOctal(halfWord));

            return halfWord; 
        }

        public static void WriteRAM(ushort low, ushort high)
        {
            if (!_ramSelect)
            {
                // No-op, can't write to ROM.
                return;
            }           
            
            /* 
            Log.Write(Logging.LogComponent.Microcode, "CRAM address for write: Bank {0}, addr {1}",
                _ramBank,                
                Conversion.ToOctal(_ramAddr));

            Log.Write(Logging.LogComponent.Microcode, "CRAM write of low {0}, high {1}",                
                Conversion.ToOctal(low),
                Conversion.ToOctal(high));
            */

            ushort address = (ushort)(_ramAddr + _ramBank * 1024);
            
            _uCodeRam[address] = MapRAMWord(((UInt32)(high) << 16) | low);

            UpdateRAMCache(address);
        }

        /// <summary>
        /// Retrieve the microinstruction for the given address using the currently
        /// selected memory bank.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static MicroInstruction GetInstruction(ushort address, TaskType task)
        {            
            return _decodeCache[address + (int)_microcodeBank[(int)task] * 1024];            
        }

        private static void LoadAltoIIMicrocode(RomFile[] romInfo)
        {
            _uCodeRom = new UInt32[2048];

            foreach(RomFile file in romInfo)
            {
                //
                // Each file contains 1024 bytes, each byte containing one nybble in the low 4 bits.
                //
                using(FileStream fs = new FileStream(file.Filename, FileMode.Open, FileAccess.Read))
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
                        _uCodeRom[file.StartingAddress + i] |= (uint)((data[AddressMapAltoII(i)] & 0xf) << file.BitPosition);
                    }
                }
            }
                       
            for(int i=0;i<_uCodeRom.Length;i++)
            {               
                _uCodeRom[i] = MapWord(_uCodeRom[i]);
            } 
        }

        private static void LoadAltoIMicrocode(RomFile[] romInfo)
        {
            _uCodeRom = new UInt32[1024];

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
                        _uCodeRom[file.StartingAddress + i] |= (uint)((data[AddressMapAltoI(i)] & 0xf) << file.BitPosition);
                    }
                }
            }

            for (int i = 0; i < _uCodeRom.Length; i++)
            {
                _uCodeRom[i] = MapWord(_uCodeRom[i]);
            }
        }

        private static UInt32 MapWord(UInt32 word)
        {
            // Invert the requisite bits just to make things easier; the high bits of F1 and F2 and the Load L bit are inverted
            // already; we leave those alone.
            const UInt32 invertedBitMask = 0xfff77bff;

            UInt32 masked = word & ~invertedBitMask;
            word = ((~word) & invertedBitMask) | masked;

            return word;
        }

        private static UInt32 MapRAMWord(UInt32 word)
        {
            // Invert the requisite bits just to make things easier; the high bits of F1 and F2 and the Load L bit are inverted
            // already; we leave those alone.
            const UInt32 bitMask = 0x00088400;
            
            word ^= bitMask;

            return word;
        }

        private static int AddressMapAltoII(int address)
        {            
            int  mappedAddress = (~address) & 0x3ff;
            return mappedAddress;
        }

        private static int AddressMapAltoI(int address)
        {
            int mappedAddress = (~address) & 0xff;
            return mappedAddress;
        }

        private static void CacheMicrocodeROM()
        {
            for(int i=0;i<_uCodeRom.Length;i++)
            {
                _decodeCache[i] = new MicroInstruction(_uCodeRom[i]);
            }
        }

        private static void UpdateRAMCache(ushort address)
        {
            UInt32 instructionWord = _uCodeRam[address];
            _decodeCache[2048 + address] = new MicroInstruction(instructionWord);
        }       

        private static RomFile[] _uCodeRomsAltoI =
        {
            // 0 to 377
            new RomFile(Configuration.GetAltoIRomPath("00_23.BIN"), 0x000, 28),
            new RomFile(Configuration.GetAltoIRomPath("01_23.BIN"), 0x000, 24),
            new RomFile(Configuration.GetAltoIRomPath("02_23.BIN"), 0x000, 20),
            new RomFile(Configuration.GetAltoIRomPath("03_23.BIN"), 0x000, 16),
            new RomFile(Configuration.GetAltoIRomPath("04_23.BIN"), 0x000, 12),
            new RomFile(Configuration.GetAltoIRomPath("05_23.BIN"), 0x000, 8),
            new RomFile(Configuration.GetAltoIRomPath("06_23.BIN"), 0x000, 4),
            new RomFile(Configuration.GetAltoIRomPath("07_23.BIN"), 0x000, 0),

            // 400 to 777
            new RomFile(Configuration.GetAltoIRomPath("10_23.BIN"), 0x100, 28),
            new RomFile(Configuration.GetAltoIRomPath("11_23.BIN"), 0x100, 24),
            new RomFile(Configuration.GetAltoIRomPath("12_23.BIN"), 0x100, 20),
            new RomFile(Configuration.GetAltoIRomPath("13_23.BIN"), 0x100, 16),
            new RomFile(Configuration.GetAltoIRomPath("14_23.BIN"), 0x100, 12),
            new RomFile(Configuration.GetAltoIRomPath("15_23.BIN"), 0x100, 8),
            new RomFile(Configuration.GetAltoIRomPath("16_23.BIN"), 0x100, 4),
            new RomFile(Configuration.GetAltoIRomPath("17_23.BIN"), 0x100, 0),

            // 1000 to 1377
            new RomFile(Configuration.GetAltoIRomPath("20_23.BIN"), 0x200, 28),
            new RomFile(Configuration.GetAltoIRomPath("21_23.BIN"), 0x200, 24),
            new RomFile(Configuration.GetAltoIRomPath("22_23.BIN"), 0x200, 20),
            new RomFile(Configuration.GetAltoIRomPath("23_23.BIN"), 0x200, 16),
            new RomFile(Configuration.GetAltoIRomPath("24_23.BIN"), 0x200, 12),
            new RomFile(Configuration.GetAltoIRomPath("25_23.BIN"), 0x200, 8),
            new RomFile(Configuration.GetAltoIRomPath("26_23.BIN"), 0x200, 4),
            new RomFile(Configuration.GetAltoIRomPath("27_23.BIN"), 0x200, 0),

            // 1400 to 1777
            new RomFile(Configuration.GetAltoIRomPath("30_23.BIN"), 0x300, 28),
            new RomFile(Configuration.GetAltoIRomPath("31_23.BIN"), 0x300, 24),
            new RomFile(Configuration.GetAltoIRomPath("32_23.BIN"), 0x300, 20),
            new RomFile(Configuration.GetAltoIRomPath("33_23.BIN"), 0x300, 16),
            new RomFile(Configuration.GetAltoIRomPath("34_23.BIN"), 0x300, 12),
            new RomFile(Configuration.GetAltoIRomPath("35_23.BIN"), 0x300, 8),
            new RomFile(Configuration.GetAltoIRomPath("36_23.BIN"), 0x300, 4),
            new RomFile(Configuration.GetAltoIRomPath("37_23.BIN"), 0x300, 0),
        };

        private static RomFile[] _uCodeRomsAltoII =
       {
            // first K (standard uCode)
            new RomFile(Configuration.GetAltoIIRomPath("U55"), 0x000, 28),
            new RomFile(Configuration.GetAltoIIRomPath("U64"), 0x000, 24),
            new RomFile(Configuration.GetAltoIIRomPath("U65"), 0x000, 20),
            new RomFile(Configuration.GetAltoIIRomPath("U63"), 0x000, 16),
            new RomFile(Configuration.GetAltoIIRomPath("U53"), 0x000, 12),
            new RomFile(Configuration.GetAltoIIRomPath("U60"), 0x000, 8),
            new RomFile(Configuration.GetAltoIIRomPath("U61"), 0x000, 4),
            new RomFile(Configuration.GetAltoIIRomPath("U62"), 0x000, 0),

            // second K (MESA 5.0)
            new RomFile(Configuration.GetAltoIIRomPath("U54"), 0x400, 28),
            new RomFile(Configuration.GetAltoIIRomPath("U74"), 0x400, 24),
            new RomFile(Configuration.GetAltoIIRomPath("U75"), 0x400, 20),
            new RomFile(Configuration.GetAltoIIRomPath("U73"), 0x400, 16),
            new RomFile(Configuration.GetAltoIIRomPath("U52"), 0x400, 12),
            new RomFile(Configuration.GetAltoIIRomPath("U70"), 0x400, 8),
            new RomFile(Configuration.GetAltoIIRomPath("U71"), 0x400, 4),
            new RomFile(Configuration.GetAltoIIRomPath("U72"), 0x400, 0)
        };


        private static UInt32[] _uCodeRom;
        private static UInt32[] _uCodeRam;

        private static MicroInstruction[] _decodeCache;

        private static MicrocodeBank[] _microcodeBank;

        private static int _ramBank;
        private static bool _ramSelect;
        private static bool _lowHalfsel;
        private static int _ramAddr;

        private static SystemType _systemType;

    }
}
