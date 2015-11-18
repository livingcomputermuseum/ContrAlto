using System.Collections.Generic;

using Contralto.Memory;
using Contralto.CPU;

namespace Contralto.IO
{

    public enum AltoKey
    {
        A = 0,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        D0,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        D9,
        Space,
        Plus,
        Minus,
        Comma,
        Period,
        Semicolon,
        Quote,
        LBracket,
        RBracket,
        FSlash,
        BSlash,
        Arrow,
        Lock,        
        LShift,
        RShift,
        LF,
        BS,
        DEL,
        ESC,
        TAB,
        CTRL,
        Return,
        BlankTop,
        BlankMiddle,
        BlankBottom,
    }

    public struct AltoKeyBit
    {
        public AltoKeyBit(int word, ushort mask)
        {
            Word = word;
            Bitmask = mask;
        }

        public int Word;
        public ushort Bitmask;
    }


    /// <summary>
    /// Currently just a stub indicating that no keys are being pressed.
    /// </summary>
    public class Keyboard : IMemoryMappedDevice
    {
        public Keyboard()
        {
            InitMap();
            Reset();
        }

        public void Reset()
        {
            _keyWords = new ushort[4];
        }

        public ushort Read(int address, TaskType task, bool extendedMemoryReference)
        {                  
            // keyboard word is inverted      
            return (ushort)~_keyWords[address - 0xfe1c];     // TODO: move to constant.
        }

        public void Load(int address, ushort data, TaskType task, bool extendedMemoryReference)
        {
            // nothing
        }

        public void KeyDown(AltoKey key)
        {
            AltoKeyBit bits = _keyMap[key];
            _keyWords[bits.Word] |= _keyMap[key].Bitmask;
        }

        public void KeyUp(AltoKey key)
        {
            AltoKeyBit bits = _keyMap[key];
            _keyWords[bits.Word] &= (ushort)~_keyMap[key].Bitmask;
        }

        public MemoryRange[] Addresses
        {
            get { return _addresses; }
        }


        private readonly MemoryRange[] _addresses =
        {
            new MemoryRange(0xfe1c, 0xfe1f), // 177034-177037 
        };

        private void InitMap()
        {
            _keyMap = new Dictionary<AltoKey, AltoKeyBit>();
            _keyMap.Add(AltoKey.D5,     new AltoKeyBit(0, 0x8000));
            _keyMap.Add(AltoKey.D4,     new AltoKeyBit(0, 0x4000));
            _keyMap.Add(AltoKey.D6,     new AltoKeyBit(0, 0x2000));
            _keyMap.Add(AltoKey.E,      new AltoKeyBit(0, 0x1000));
            _keyMap.Add(AltoKey.D7,     new AltoKeyBit(0, 0x0800));
            _keyMap.Add(AltoKey.D,      new AltoKeyBit(0, 0x0400));
            _keyMap.Add(AltoKey.U,      new AltoKeyBit(0, 0x0200));
            _keyMap.Add(AltoKey.V,      new AltoKeyBit(0, 0x0100));
            _keyMap.Add(AltoKey.D0,     new AltoKeyBit(0, 0x0080));
            _keyMap.Add(AltoKey.K,      new AltoKeyBit(0, 0x0040));
            _keyMap.Add(AltoKey.Minus,  new AltoKeyBit(0, 0x0020));
            _keyMap.Add(AltoKey.P,      new AltoKeyBit(0, 0x0010));
            _keyMap.Add(AltoKey.FSlash, new AltoKeyBit(0, 0x0008));
            _keyMap.Add(AltoKey.BSlash, new AltoKeyBit(0, 0x0004));
            _keyMap.Add(AltoKey.LF,     new AltoKeyBit(0, 0x0002));
            _keyMap.Add(AltoKey.BS,     new AltoKeyBit(0, 0x0001));

            _keyMap.Add(AltoKey.D3,     new AltoKeyBit(1, 0x8000));
            _keyMap.Add(AltoKey.D2,     new AltoKeyBit(1, 0x4000));
            _keyMap.Add(AltoKey.W,      new AltoKeyBit(1, 0x2000));
            _keyMap.Add(AltoKey.Q,      new AltoKeyBit(1, 0x1000));
            _keyMap.Add(AltoKey.S,      new AltoKeyBit(1, 0x0800));
            _keyMap.Add(AltoKey.A,      new AltoKeyBit(1, 0x0400));
            _keyMap.Add(AltoKey.D9,     new AltoKeyBit(1, 0x0200));
            _keyMap.Add(AltoKey.I,      new AltoKeyBit(1, 0x0100));
            _keyMap.Add(AltoKey.X,      new AltoKeyBit(1, 0x0080));
            _keyMap.Add(AltoKey.O,      new AltoKeyBit(1, 0x0040));
            _keyMap.Add(AltoKey.L,      new AltoKeyBit(1, 0x0020));
            _keyMap.Add(AltoKey.Comma,  new AltoKeyBit(1, 0x0010));
            _keyMap.Add(AltoKey.Quote,  new AltoKeyBit(1, 0x0008));
            _keyMap.Add(AltoKey.RBracket, new AltoKeyBit(1, 0x0004));
            _keyMap.Add(AltoKey.BlankMiddle, new AltoKeyBit(1, 0x0002));
            _keyMap.Add(AltoKey.BlankTop, new AltoKeyBit(1, 0x0001));

            _keyMap.Add(AltoKey.D1,     new AltoKeyBit(2, 0x8000));
            _keyMap.Add(AltoKey.ESC,    new AltoKeyBit(2, 0x4000));
            _keyMap.Add(AltoKey.TAB,    new AltoKeyBit(2, 0x2000));
            _keyMap.Add(AltoKey.F,      new AltoKeyBit(2, 0x1000));
            _keyMap.Add(AltoKey.CTRL,   new AltoKeyBit(2, 0x0800));
            _keyMap.Add(AltoKey.C,      new AltoKeyBit(2, 0x0400));
            _keyMap.Add(AltoKey.J,      new AltoKeyBit(2, 0x0200));
            _keyMap.Add(AltoKey.B,      new AltoKeyBit(2, 0x0100));
            _keyMap.Add(AltoKey.Z,      new AltoKeyBit(2, 0x0080));
            _keyMap.Add(AltoKey.LShift, new AltoKeyBit(2, 0x0040));
            _keyMap.Add(AltoKey.Period, new AltoKeyBit(2, 0x0020));
            _keyMap.Add(AltoKey.Semicolon, new AltoKeyBit(2, 0x0010));
            _keyMap.Add(AltoKey.Return, new AltoKeyBit(2, 0x0008));
            _keyMap.Add(AltoKey.Arrow,  new AltoKeyBit(2, 0x0004));
            _keyMap.Add(AltoKey.DEL,    new AltoKeyBit(2, 0x0002));

            _keyMap.Add(AltoKey.R,      new AltoKeyBit(3, 0x8000));
            _keyMap.Add(AltoKey.T,      new AltoKeyBit(3, 0x4000));
            _keyMap.Add(AltoKey.G,      new AltoKeyBit(3, 0x2000));
            _keyMap.Add(AltoKey.Y,      new AltoKeyBit(3, 0x1000));
            _keyMap.Add(AltoKey.H,      new AltoKeyBit(3, 0x0800));
            _keyMap.Add(AltoKey.D8,     new AltoKeyBit(3, 0x0400));
            _keyMap.Add(AltoKey.N,      new AltoKeyBit(3, 0x0200));
            _keyMap.Add(AltoKey.M,      new AltoKeyBit(3, 0x0100));
            _keyMap.Add(AltoKey.Lock,   new AltoKeyBit(3, 0x0080));
            _keyMap.Add(AltoKey.Space,  new AltoKeyBit(3, 0x0040));
            _keyMap.Add(AltoKey.LBracket, new AltoKeyBit(3, 0x0020));
            _keyMap.Add(AltoKey.Plus,   new AltoKeyBit(3, 0x0010));
            _keyMap.Add(AltoKey.RShift, new AltoKeyBit(3, 0x0008));
            _keyMap.Add(AltoKey.BlankBottom, new AltoKeyBit(3, 0x0004));
        }

        private ushort[] _keyWords;

        private Dictionary<AltoKey, AltoKeyBit> _keyMap;
    }
}
