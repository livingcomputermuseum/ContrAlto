using System;
using System.IO;

namespace Contralto.CPU
{
    public enum MicrocodeBank
    {
        ROM0 = 0,
        ROM1,
        RAM0,
        RAM1,
        RAM2
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

    static class UCodeMemory
    {
        static UCodeMemory()
        {
            Init();
        }

        private static void Init()
        {     
            //       
            // TODO: this is currently configured for a 2K ROM machine
            // (1K RAM, 2K ROM).  This should be configurable.
            //
            // 1 bank of microcode RAM
            _uCodeRam = new UInt32[1024];
            LoadMicrocode(_uCodeRoms);

            //
            // Cache 3k of instructions: 2K ROM, 1K RAM.
            _decodeCache = new MicroInstruction[1024 * 3];

            // Precache ROM
            CacheMicrocodeROM();

            // Precache (empty) RAM
            for(ushort i=0;i<_uCodeRam.Length;i++)
            {
                UpdateRAMCache(i);
            }

            // Start in ROM0 -- TODO: need to implement reset logic
            _microcodeBank = MicrocodeBank.ROM0;
            _ramAddr = 0;
            _ramBank = 0;
            _ramSelect = true;
            _lowHalfsel = true;
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

        public static MicrocodeBank Bank
        {
            get { return _microcodeBank; }
        }

        public static void LoadControlRAMAddress(ushort address)
        {
            _ramBank = (address & 0x3000) >> 12;
            _ramSelect = (address & 0x0800) == 0;
            _lowHalfsel = (address & 0x0400) == 0;
            _ramAddr = (address & 0x3ff);            
        }

        /// <summary>
        /// Implements the SWMODE F1 logic; selects the proper uCode bank (from
        /// RAM or ROM) based on the supplied NEXT value.
        /// </summary>
        /// <param name="nextAddress"></param>
        public static void SwitchMode(ushort nextAddress)
        {                        
            Logging.Log.Write(Logging.LogComponent.Microcode, "SWMODE: Current Bank {0}", _microcodeBank);
            
            // 2K ROM
            switch(_microcodeBank)
            {
                case MicrocodeBank.ROM0:
                    _microcodeBank = (nextAddress & 0x100) == 0 ? MicrocodeBank.RAM0 : MicrocodeBank.ROM1;
                    break;

                case MicrocodeBank.ROM1:
                    _microcodeBank = (nextAddress & 0x100) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.RAM0;
                    break;

                case MicrocodeBank.RAM0:
                    _microcodeBank = (nextAddress & 0x100) == 0 ? MicrocodeBank.ROM0 : MicrocodeBank.ROM1;
                    break;
            }
            
            // for 1K ROM
            //_microcodeBank = _microcodeBank == MicrocodeBank.ROM0 ? MicrocodeBank.RAM0 : MicrocodeBank.ROM0;

            Logging.Log.Write(Logging.LogComponent.Microcode, "SWMODE: New Bank {0}", _microcodeBank);            
        }

        public static ushort ReadRAM()
        {
            if (!_ramSelect)
            {
                throw new NotImplementedException("Read from microcode ROM not implemented.");
            }

            if (_ramBank > 0)
            {
                //throw new InvalidOperationException("RAM bank > 0, unexpected.");                
                _ramBank = 0;
            }

            // pretend no ram for the moment                
            Logging.Log.Write(Logging.LogComponent.Microcode, "CRAM address for read: Bank {0}, RAM {1}, lowhalf {2} addr {3}",
                _ramBank,
                _ramSelect,
                _lowHalfsel,
                Conversion.ToOctal(_ramAddr));

            UInt32 data = _uCodeRam[_ramAddr + (_ramBank * 1024)];

            // Flip the necessary bits before returning them.
            // (See table in section 8.3 of HWRef.)
            ushort halfWord = (ushort)(_lowHalfsel ? data : (data >> 16));

            Logging.Log.Write(Logging.LogComponent.Microcode, "CRAM data read: {0}-{1}",
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

            if (_ramBank > 0)
            {
                //throw new InvalidOperationException("RAM bank > 0, unexpected.");                
                _ramBank = 0;
            }

            Logging.Log.Write(Logging.LogComponent.Microcode, "CRAM address for write: Bank {0}, addr {1}",
                _ramBank,                
                Conversion.ToOctal(_ramAddr));

            Logging.Log.Write(Logging.LogComponent.Microcode, "CRAM write of low {0}, high {1}",                
                Conversion.ToOctal(low),
                Conversion.ToOctal(high));

            ushort address = (ushort)(_ramAddr + _ramBank * 1024);
            
            _uCodeRam[address] = ((UInt32)(high) << 16) | low;

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
            // Only RAM-enabled tasks can execute from anything other than ROM (right now)
            if (task == TaskType.Emulator)
            {
                // banked
                return _decodeCache[address + (int)_microcodeBank * 1024];
            }
            else
            {
                // ROM only
                return _decodeCache[address];
            }
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

                       
            for(int i=0;i<_uCodeRom.Length;i++)
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

        private static int AddressMap(int address)
        {            
            int  mappedAddress = (~address) & 0x3ff;
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
            _decodeCache[2048 + address] = new MicroInstruction(MapRAMWord(instructionWord));

            //Console.WriteLine(_decodeCache[2048 + address]);
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

        private static UInt32[] _uCodeRom;
        private static UInt32[] _uCodeRam;

        private static MicroInstruction[] _decodeCache;

        private static MicrocodeBank _microcodeBank;

        private static int _ramBank;
        private static bool _ramSelect;
        private static bool _lowHalfsel;
        private static int _ramAddr;

    }
}
