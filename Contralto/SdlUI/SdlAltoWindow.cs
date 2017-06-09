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
using System.Runtime.InteropServices;
using System.Collections.Generic;

using SDL2;
using Contralto.Display;
using Contralto.IO;

namespace Contralto.SdlUI
{
    /// <summary>
    /// AltoWindow provides a wrapper around SDL2-CS, which is in itself an incredibly thin
    /// wrapper around SDL 2.0.  It presents a window capable of displaying the Alto's video
    /// and handles keyboard and mouse input.
    /// 
    /// This is intended for use on non-Windows platforms but will work on Windows as well.
    /// </summary>
    public class SdlAltoWindow : IAltoDisplay, IDisposable
    {
        public SdlAltoWindow()
        {
            InitKeymap();
        }

        public event EventHandler OnClosed;

        public void Dispose()
        {

        }

        public void AttachSystem(AltoSystem system)
        {
            _system = system;
            _system.AttachDisplay(this);
        }

        public void Run()
        {
            InitSDL();

            SDL.SDL_Event e;
            bool running = true;

            while(running)
            {
                //
                // Run main message loop
                //
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch(e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            OnClosed(null, null);
                            break;

                        case SDL.SDL_EventType.SDL_USEREVENT:
                            // This should always be the case since we only define one
                            // user event, but just to be truly pedantic...
                            if (e.user.type == _renderEventType)
                            {
                                RenderDisplay();
                            }
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            MouseMove(e.motion.x, e.motion.y);
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            MouseDown(e.button.button, e.button.x, e.button.y);
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            MouseUp(e.button.button);
                            break;

                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            KeyDown(e.key.keysym.sym);
                            break;

                        case SDL.SDL_EventType.SDL_KEYUP:
                            KeyUp(e.key.keysym.sym);
                            break;

                        default:
                            break;
                    }
                }

                SDL.SDL_Delay(0);
            }

            //
            // Shut things down nicely.
            //
            if (_sdlRenderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(_sdlRenderer);
                _sdlRenderer = IntPtr.Zero;
            } 

            if (_sdlWindow != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(_sdlWindow);
                _sdlWindow = IntPtr.Zero;
            }
            
            SDL.SDL_Quit();
        }

        public void Close()
        {
            SDL.SDL_Event closeEvent = new SDL.SDL_Event();
            closeEvent.type = SDL.SDL_EventType.SDL_QUIT;

            SDL.SDL_PushEvent(ref closeEvent);
        }

        public void DrawDisplayWord(int scanline, int wordOffset, ushort word, bool lowRes)
        {
            if (lowRes)
            {
                // Low resolution; double up pixels.
                int address = scanline * 76 + wordOffset * 4;

                if (address > _1bppDisplayBuffer.Length)
                {
                    throw new InvalidOperationException("Display word address is out of bounds.");
                }

                UInt32 stretched = StretchWord(word);

                _1bppDisplayBuffer[address] = (byte)(stretched >> 24);
                _1bppDisplayBuffer[address + 1] = (byte)(stretched >> 16);
                _1bppDisplayBuffer[address + 2] = (byte)(stretched >> 8);
                _1bppDisplayBuffer[address + 3] = (byte)(stretched);
            }
            else
            {
                int address = scanline * 76 + wordOffset * 2;

                if (address > _1bppDisplayBuffer.Length)
                {
                    throw new InvalidOperationException("Display word address is out of bounds.");
                }

                _1bppDisplayBuffer[address] = (byte)(word >> 8);
                _1bppDisplayBuffer[address + 1] = (byte)(word);
            }
            
        }

        /// <summary>
        /// Invoked by the DisplayController to draw the cursor at the specified position on the given
        /// scanline.
        /// </summary>
        /// <param name="scanline">The scanline (Y)</param>
        /// <param name="xOffset">X offset (in pixels)</param>
        /// <param name="cursorWord">The word to be drawn</param>
        public void DrawCursorWord(int scanline, int xOffset, bool whiteOnBlack, ushort cursorWord)
        {
            
            int address = scanline * 76 + xOffset / 8;

            //
            // Grab the 32 bits straddling the cursor from the display buffer
            // so we can merge the 16 cursor bits in.
            //
            UInt32 displayWord = (UInt32)((_1bppDisplayBuffer[address] << 24) |
                                        (_1bppDisplayBuffer[address + 1] << 16) |
                                        (_1bppDisplayBuffer[address + 2] << 8) |
                                        _1bppDisplayBuffer[address + 3]);

            // Slide the cursor word to the proper X position
            UInt32 adjustedCursorWord = (UInt32)(cursorWord << (16 - (xOffset % 8)));

            if (!whiteOnBlack)
            {
                displayWord &= ~adjustedCursorWord;
            }
            else
            {
                displayWord |= adjustedCursorWord;
            }

            _1bppDisplayBuffer[address] = (byte)(displayWord >> 24);
            _1bppDisplayBuffer[address + 1] = (byte)(displayWord >> 16);
            _1bppDisplayBuffer[address + 2] = (byte)(displayWord >> 8);
            _1bppDisplayBuffer[address + 3] = (byte)(displayWord);
            
        }

        /// <summary>
        /// "Stretches" a 16 bit word into a 32-bit word (for low-res display purposes).
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private UInt32 StretchWord(ushort word)
        {
            UInt32 stretched = 0;

            for (int i = 0x8000, j = 15; j >= 0; i = i >> 1, j--)
            {
                uint bit = (uint)(word & i) >> j;

                stretched |= (bit << (j * 2 + 1));
                stretched |= (bit << (j * 2));
            }

            return stretched;
        }

        /// <summary>
        /// Transmogrify 1bpp display buffer to 32-bits.
        /// </summary>
        private void ExpandBitmapToARGB()
        {
            int rgbIndex = 0;

            for (int i = 0; i < _32bppDisplayBuffer.Length / 8; i++)
            {
                byte b = _1bppDisplayBuffer[i];
                for (int bit = 7; bit >= 0; bit--)
                {
                    byte color = (byte)((b & (1 << bit)) == 0 ? 0x00 : 0xff);

                    _32bppDisplayBuffer[rgbIndex++] = (int)((color == 0) ? 0xff000000 : 0xffffffff); 
                }
            }
        }

        /// <summary>
        /// Invoked when the Alto is done with a field.  Tells SDL to redraw the Alto display.
        /// This is called on the Alto emulation thread.
        /// </summary>
        public void Render()
        {
            //
            // Send a render event to the SDL message loop so that things
            // will get rendered.
            //
            SDL.SDL_PushEvent(ref _renderEvent); 
        }

        private void RenderDisplay()
        {
            //
            // Stuff the display data into the display texture
            //
            ExpandBitmapToARGB();

            IntPtr textureBits = IntPtr.Zero;
            int pitch = 0;
            SDL.SDL_LockTexture(_displayTexture, IntPtr.Zero, out textureBits, out pitch);

            Marshal.Copy(_32bppDisplayBuffer, 0, textureBits, _32bppDisplayBuffer.Length);

            SDL.SDL_UnlockTexture(_displayTexture);

            //
            // Render the display texture to the renderer
            //
            SDL.SDL_RenderCopy(_sdlRenderer, _displayTexture, IntPtr.Zero, IntPtr.Zero);

            //
            // And show it to us.
            //
            SDL.SDL_RenderPresent(_sdlRenderer);
        }

        private void MouseMove(int x, int y)
        {
            if (!_mouseCaptured)
            {
                return;
            }

            if (_skipNextMouseMove)
            {
                _skipNextMouseMove = false;
                return;
            }

            //
            // As with the Winforms mouse capture code...

            int mx = _windowWidth / 2;
            int my = _windowHeight / 2;

            int dx = x - mx;
            int dy = y - my;

            if (dx != 0 || dy != 0)
            {
                _system.Mouse.MouseMove(dx, dy);

                // Don't handle the very next Mouse Move event (which will just be the motion we caused in the
                // below line...)
                _skipNextMouseMove = true;

                //
                // Move the mouse pointer to the middle of the window.
                //
                SDL.SDL_WarpMouseInWindow(_sdlWindow, mx, my);
            }
        }

        private void KeyDown(SDL.SDL_Keycode code)
        {
            if (!_mouseCaptured)
            {
                return;
            }

            if (_keyMap.ContainsKey(code))
            {
                _system.Keyboard.KeyDown(_keyMap[code]);
            }
        }

        private void KeyUp(SDL.SDL_Keycode code)
        {
            //
            // Check for Alt key to release mouse
            //
            if (code == SDL.SDL_Keycode.SDLK_LALT ||
                code == SDL.SDL_Keycode.SDLK_RALT)
            {
                ReleaseMouse();
            }

            if (!_mouseCaptured)
            {
                return;
            }

            if (_keyMap.ContainsKey(code))
            {
                _system.Keyboard.KeyUp(_keyMap[code]);
            }
        }

        private void MouseDown(byte button, int x, int y)
        {
            //
            // OS X Sierra issue: we get mousedown events when the window title
            // is clicked, sometimes.  These always show up with a Y coordinate
            // of zero.  So ignore those only for mouse-capture purposes as
            // a workaround.
            //
            if (!_mouseCaptured && (x <= 0 || y <= 0))
            {
                return;
            }

            if (!_mouseCaptured)
            {
                CaptureMouse();
            }

            AltoMouseButton altoButton = GetMouseButton(button);

            if (altoButton != AltoMouseButton.None)
            {
                _system.Mouse.MouseDown(altoButton);
            }
        }       

        private void MouseUp(byte button)
        {
            if (!_mouseCaptured)
            {
                return;
            }

            AltoMouseButton altoButton = GetMouseButton(button);

            if (altoButton != AltoMouseButton.None)
            {
                _system.Mouse.MouseUp(altoButton);
            }
        }

        private void CaptureMouse()
        {
            //
            // Turn off the mouse cursor and ensure the mouse is trapped
            // within our window.
            //
            _mouseCaptured = true;
            SDL.SDL_ShowCursor(0);
            SDL.SDL_SetWindowGrab(_sdlWindow, SDL.SDL_bool.SDL_TRUE);

            UpdateWindowTitle();
        }

        private void ReleaseMouse()
        {
            //
            // Turn the mouse cursor back on, and release the mouse.
            //
            _mouseCaptured = false;
            SDL.SDL_ShowCursor(1);
            SDL.SDL_SetWindowGrab(_sdlWindow, SDL.SDL_bool.SDL_FALSE);

            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            SDL.SDL_SetWindowTitle(
                _sdlWindow,
                String.Format("ContrAlto{0}",
                    _mouseCaptured ? " - Mouse captured.  Press 'Alt' to release." : ""));
        }

        private void InitSDL()
        {
            int retVal;

            // Get SDL humming
            if ((retVal = SDL.SDL_Init(SDL.SDL_INIT_VIDEO)) < 0)
            {
                throw new InvalidOperationException(String.Format("SDL_Init failed.  Error {0:x}", retVal));
            }

            // 
            if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1") == SDL.SDL_bool.SDL_FALSE)
            {
                throw new InvalidOperationException("SDL_SetHint failed to set scale quality.");
            }
            
            _sdlWindow = SDL.SDL_CreateWindow(
                "ContrAlto",
                SDL.SDL_WINDOWPOS_UNDEFINED,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                _windowWidth,
                _windowHeight,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (_sdlWindow == IntPtr.Zero)
            {
                throw new InvalidOperationException("SDL_CreateWindow failed.");
            }
            
            _sdlRenderer = SDL.SDL_CreateRenderer(_sdlWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (_sdlRenderer == IntPtr.Zero)
            {
                throw new InvalidOperationException("SDL_CreateRenderer failed.");
            }

            _displayTexture = SDL.SDL_CreateTexture(
                _sdlRenderer, 
                SDL.SDL_PIXELFORMAT_ARGB8888, 
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 
                608, 
                _windowHeight);

            if (_displayTexture == IntPtr.Zero)
            {
                throw new InvalidOperationException("SDL_CreateTexture failed.");
            }

            SDL.SDL_SetRenderDrawColor(_sdlRenderer, 0x00, 0x00, 0x00, 0x00);

            // Register a User event for rendering.
            _renderEventType = SDL.SDL_RegisterEvents(1);
            _renderEvent = new SDL.SDL_Event();
            _renderEvent.type = (SDL.SDL_EventType)_renderEventType;
        }

        private void InitKeymap()
        {
            _keyMap = new Dictionary<SDL.SDL_Keycode, AltoKey>();
           
            _keyMap.Add(SDL.SDL_Keycode.SDLK_a, AltoKey.A);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_b, AltoKey.B);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_c, AltoKey.C);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_d, AltoKey.D);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_e, AltoKey.E);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_f, AltoKey.F);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_g, AltoKey.G);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_h, AltoKey.H);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_i, AltoKey.I);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_j, AltoKey.J);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_k, AltoKey.K);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_l, AltoKey.L);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_m, AltoKey.M);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_n, AltoKey.N);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_o, AltoKey.O);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_p, AltoKey.P);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_q, AltoKey.Q);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_r, AltoKey.R);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_s, AltoKey.S);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_t, AltoKey.T);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_u, AltoKey.U);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_v, AltoKey.V);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_w, AltoKey.W);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_x, AltoKey.X);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_y, AltoKey.Y);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_z, AltoKey.Z);          

            _keyMap.Add(SDL.SDL_Keycode.SDLK_0, AltoKey.D0);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_1, AltoKey.D1);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_2, AltoKey.D2);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_3, AltoKey.D3);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_4, AltoKey.D4);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_5, AltoKey.D5);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_6, AltoKey.D6);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_7, AltoKey.D7);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_8, AltoKey.D8);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_9, AltoKey.D9);

            _keyMap.Add(SDL.SDL_Keycode.SDLK_SPACE, AltoKey.Space);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_PERIOD, AltoKey.Period);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_COMMA, AltoKey.Comma);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_QUOTE, AltoKey.Quote);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_BACKSLASH, AltoKey.BSlash);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_SLASH, AltoKey.FSlash);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_EQUALS, AltoKey.Plus);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_MINUS, AltoKey.Minus);

            _keyMap.Add(SDL.SDL_Keycode.SDLK_ESCAPE, AltoKey.ESC);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_DELETE, AltoKey.DEL);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_LEFT, AltoKey.Arrow);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_LSHIFT, AltoKey.LShift);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_RSHIFT, AltoKey.RShift);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_LCTRL, AltoKey.CTRL);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_RCTRL, AltoKey.CTRL);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_RETURN, AltoKey.Return);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_F1, AltoKey.BlankTop);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_F2, AltoKey.BlankMiddle);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_F3, AltoKey.BlankBottom);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_F4, AltoKey.Lock);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_BACKSPACE, AltoKey.BS);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_TAB, AltoKey.TAB);

            _keyMap.Add(SDL.SDL_Keycode.SDLK_SEMICOLON, AltoKey.Semicolon);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_LEFTBRACKET, AltoKey.LBracket);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_RIGHTBRACKET, AltoKey.RBracket);
            _keyMap.Add(SDL.SDL_Keycode.SDLK_DOWN, AltoKey.LF);
        }

        private static AltoMouseButton GetMouseButton(byte button)
        {
            AltoMouseButton altoButton = AltoMouseButton.None;

            switch (button)
            {
                case 1:
                    altoButton = AltoMouseButton.Left;
                    break;

                case 2:
                    altoButton = AltoMouseButton.Middle;
                    break;

                case 3:
                    altoButton = AltoMouseButton.Right;
                    break;
            }

            return altoButton;
        }

        private AltoSystem _system;

        private IntPtr _sdlWindow = IntPtr.Zero;
        private IntPtr _sdlRenderer = IntPtr.Zero;

        private UInt32 _renderEventType;
        private SDL.SDL_Event _renderEvent;


        //
        // Display data
        //
        private const int _windowWidth = 608;
        private const int _windowHeight = 808;

        // Rendering textures
        private IntPtr _displayTexture = IntPtr.Zero;

        //
        // Buffer for display pixels.  This is 1bpp, directly written by the Alto.
        //
        private byte[] _1bppDisplayBuffer = new byte[808 * 76 + 4];        // + 4 (32-bits) to make cursor display logic simpler.
                                                                           // and 608 pixels wide to make it a multiple of 8 bits.

        //
        // Buffer for rendering pixels.  SDL doesn't support 1bpp pixel formats, so to keep things simple we use
        // an array of ints and a 32bpp format.  What's a few extra bits between friends.
        //
        private int[] _32bppDisplayBuffer = new int[(808 * 608 + 8)];     // + 8 (32-bits) as above.
                                                                          // and 608 pixels wide as above.


        //
        // Keyboard data
        //
        private Dictionary<SDL.SDL_Keycode, AltoKey> _keyMap;

        //
        // Mouse data
        //
        private bool _mouseCaptured;
        private bool _skipNextMouseMove;

    }
}
