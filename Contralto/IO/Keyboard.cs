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

using System.Collections.Generic;

using Contralto.Memory;
using Contralto.CPU;

namespace Contralto.IO
{
    /// <summary>
    /// The keys on the Alto keyboard.
    /// </summary>
    public enum AltoKey
    {
        None = 0,
        A,
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

    /// <summary>
    /// Specifies the word and bitmask for a given Alto keyboard key.
    /// </summary>
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
    /// Implements the Alto's keyboard and its encodings.  It also provides 
    /// functionality for automatically pressing boot address key combinations.
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
            _bootKeysPressed = false;
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
            //
            // If we had been holding boot keys, release them now that a real user is pressing a key.
            if (_bootKeysPressed)
            {
                Reset();                
            }

            AltoKeyBit bits = _keyMap[key];
            _keyWords[bits.Word] |= bits.Bitmask;
        }

        public void KeyUp(AltoKey key)
        {
            AltoKeyBit bits = _keyMap[key];
            _keyWords[bits.Word] &= (ushort)~bits.Bitmask;
        }

        public void PressBootKeys(ushort bootAddress, bool netBoot)
        {                       
            for (int i = 0; i < 16; i++)
            {
                if ((bootAddress & (0x8000 >> i)) != 0)
                {
                    KeyDown(_bootKeys[i]);
                }
            }

            if (netBoot)
            {
                // BS is held for netbooting
                KeyDown(AltoKey.BS);
            }

            _bootKeysPressed = true;
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

        /// <summary>
        /// The keys used to specify a 16-bit boot address, from MSB to LSB
        /// </summary>
        private AltoKey[] _bootKeys = new AltoKey[]
        {
            AltoKey.D3, AltoKey.D2, AltoKey.W, AltoKey.Q, AltoKey.S, AltoKey.A, AltoKey.D9, AltoKey.I,
            AltoKey.X, AltoKey.O, AltoKey.L, AltoKey.Comma, AltoKey.Quote, AltoKey.RBracket, AltoKey.BlankMiddle, AltoKey.BlankTop
        };       

        private bool _bootKeysPressed;        
    }
}
