using Contralto.CPU;
using Contralto.Display;
using Contralto.IO;
using Contralto.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Contralto
{
    public partial class AltoWindow : Form, IAltoDisplay
    {
        public AltoWindow()
        {
            InitializeComponent();
            InitKeymap();

            _mouseCaptured = false;
            _currentCursorState = true;

            _displayBuffer = new Bitmap(608, 808, PixelFormat.Format1bppIndexed);
            DisplayBox.Image = _displayBuffer;

            _lastBuffer = _currentBuffer = _displayData0;
            _frame = 0;

            _frameTimer = new FrameTimer(60.0);

            ReleaseMouse();

            SystemStatusLabel.Text = _systemStoppedText;
            DiskStatusLabel.Text = String.Empty;

            this.DoubleBuffered = true;            
        }

        public void AttachSystem(AltoSystem system)
        {
            _system = system;
            _system.AttachDisplay(this);

            _controller = new ExecutionController(_system);

            _controller.ErrorCallback += OnExecutionError;

            // Update disk image UI info
            Drive0ImageName.Text = _system.DiskController.Drives[0].IsLoaded ? Path.GetFileName(_system.DiskController.Drives[0].Pack.PackName) : _noImageLoadedText;
            Drive1ImageName.Text = _system.DiskController.Drives[1].IsLoaded ? Path.GetFileName(_system.DiskController.Drives[1].Pack.PackName) : _noImageLoadedText;
        }

        /// <summary>
        /// Handler for "System->Start" menu.  Start the Alto system running.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSystemStartMenuClick(object sender, EventArgs e)
        {
            StartSystem(AlternateBootType.None);
        }        

        private void OnStartWithAlternateBootClicked(object sender, EventArgs e)
        {
            if (_controller.IsRunning)
            {
                _controller.Reset(Configuration.AlternateBootType);
            }
            else
            {
                StartSystem(Configuration.AlternateBootType);                
            }

        }

        private void OnSystemResetMenuClick(object sender, EventArgs e)
        {
            _controller.Reset(AlternateBootType.None);
        }

        private void OnSystemDrive0LoadClick(object sender, EventArgs e)
        {
            string path = ShowImageLoadDialog(0);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                // Commit loaded pack back to disk
                CommitDiskPack(0);
                _system.LoadDrive(0, path);
                Drive0ImageName.Text = System.IO.Path.GetFileName(path);
                Configuration.Drive0Image = path;
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while loading image: {0}", ex.Message),
                    "Image load error", MessageBoxButtons.OK);
            }
        }

        private void OnSystemDrive0UnloadClick(object sender, EventArgs e)
        {
            CommitDiskPack(0);
            _system.UnloadDrive(0);
            Drive0ImageName.Text = _noImageLoadedText;
            Configuration.Drive0Image = String.Empty;
        }

        private void OnSystemDrive1LoadClick(object sender, EventArgs e)
        {
            string path = ShowImageLoadDialog(1);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                // Commit loaded pack back to disk                
                CommitDiskPack(1);
                _system.LoadDrive(1, path);
                Drive1ImageName.Text = System.IO.Path.GetFileName(path);
                Configuration.Drive1Image = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while loading image: {0}", ex.Message),
                    "Image load error", MessageBoxButtons.OK);
            }
        }

        private void OnSystemDrive1UnloadClick(object sender, EventArgs e)
        {
            CommitDiskPack(1);
            _system.UnloadDrive(1);
            Drive1ImageName.Text = _noImageLoadedText;
            Configuration.Drive1Image = String.Empty;
        }

        private void OnAlternateBootOptionsClicked(object sender, EventArgs e)
        {
            AlternateBootOptions bootWindow = new AlternateBootOptions();
            bootWindow.ShowDialog();    
        }

        private void OnSystemOptionsClick(object sender, EventArgs e)
        {
            SystemOptions optionsWindow = new SystemOptions();
            optionsWindow.ShowDialog();            
        }

        private void OnHelpAboutClick(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        private void OnDebuggerShowClick(object sender, EventArgs e)
        {
            if (_debugger == null)
            {
                _debugger = new Debugger(_system, _controller);
                _debugger.LoadSourceCode(MicrocodeBank.ROM0, "Disassembly\\altoIIcode3.mu");
                _debugger.LoadSourceCode(MicrocodeBank.ROM1, "Disassembly\\MesaROM.mu");
                _debugger.FormClosed += OnDebuggerClosed;
                _debugger.Show();

                // Disable the Start/Reset menu items
                // (debugger will control those now)                
                SystemStartMenuItem.Enabled = false;
                SystemResetMenuItem.Enabled = false;
            }
            else
            {
                // Debugger is already opened, just bring it to the front
                _debugger.BringToFront();
            }
        }

        private void OnDebuggerClosed(object sender, FormClosedEventArgs e)
        {
            // Re-enable start/reset menu items depending on current execution state.
            SystemStartMenuItem.Enabled = !_controller.IsRunning;
            SystemResetMenuItem.Enabled = _controller.IsRunning;
            _debugger = null;
        }

        private void OnFileExitClick(object sender, EventArgs e)
        {
            _controller.StopExecution();
            this.Close();           
        }

        private void OnAltoWindowClosed(object sender, FormClosedEventArgs e)
        {
            // Halt the system and detach our display            
            _controller.StopExecution();
            _system.DetachDisplay();

            // Commit loaded packs back to disk
            CommitDiskPack(0);
            CommitDiskPack(1);

            // Commit current configuration to disk
            Configuration.WriteConfiguration();

            this.Dispose();
            Application.Exit();
        }

        private string ShowImageLoadDialog(int drive)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.DefaultExt = "dsk";
            fileDialog.Filter = "Raw Alto Disk Images (*.dsk)|*.dsk|All Files (*.*)|*.*";
            fileDialog.Multiselect = false;
            fileDialog.Title = String.Format("Select image to load into drive {0}", drive);

            DialogResult res = fileDialog.ShowDialog();

            if (res == DialogResult.OK)
            {
                return fileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Error handling
        /// </summary>
        /// <param name="e"></param>
        private void OnExecutionError(Exception e)
        {
            // TODO: invoke the debugger when an error is hit
            //OnDebuggerShowClick(null, null);
            SystemStatusLabel.Text = _systemErrorText;

            Console.WriteLine("Execution error: {0} - {1}", e.Message, e.StackTrace);

            System.Diagnostics.Debugger.Break();
        }

        //
        // Disk handling
        //
        private void CommitDiskPack(int driveId)
        {
            DiabloDrive drive = _system.DiskController.Drives[driveId];
            if (drive.IsLoaded)
            {
                using (FileStream fs = new FileStream(drive.Pack.PackName, FileMode.Create, FileAccess.Write))
                {
                    try
                    {
                        drive.Pack.Save(fs);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(String.Format("Unable to save disk {0}'s contents.  Error {0}.  Any changes have been lost.", e.Message), "Disk save error");
                    }
                }                
            }
        }

        private void StartSystem(AlternateBootType bootType)
        {
            // Disable "Start" menu item
            SystemStartMenuItem.Enabled = false;

            // Enable "Reset" menu item
            SystemResetMenuItem.Enabled = true;

            _controller.StartExecution(bootType);

            SystemStatusLabel.Text = _systemRunningText;
        }

        //
        // Display handling
        //

        public void Render()
        {
            _frame++;

            // Wait for the next frame
            if (Configuration.ThrottleSpeed)
            {
                _frameTimer.WaitForFrame();
            }

            if (Configuration.InterlaceDisplay)
            {
                // Flip the back-buffer
                if ((_frame % 2) == 0)
                {
                    _currentBuffer = _displayData0;
                    _lastBuffer = _displayData1;
                }
                else
                {
                    _currentBuffer = _displayData1;
                    _lastBuffer = _displayData0;
                }
            }

            // Asynchronously render this frame.
            BeginInvoke(new DisplayDelegate(RefreshDisplayBox));            
        }

        private void RefreshDisplayBox()
        {
            // Update the display
            BitmapData data = _displayBuffer.LockBits(_displayRect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            IntPtr ptr = data.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_lastBuffer, 0, ptr, _lastBuffer.Length - 4);

            _displayBuffer.UnlockBits(data);
            DisplayBox.Refresh();

            // Clear the buffer if we're displaying in fakey-"interlaced" mode.
            if (Configuration.InterlaceDisplay)
            {
                Array.Clear(_lastBuffer, 0, _lastBuffer.Length);
            }
        }

        /// <summary>
        /// Invoked by the DisplayController to put a word on the emulated screen.
        /// </summary>
        /// <param name="scanline"></param>
        /// <param name="wordOffset"></param>
        /// <param name="word"></param>
        public void DrawDisplayWord(int scanline, int wordOffset, ushort word, bool lowRes)
        {
            // TODO: move magic numbers to constants

            if (lowRes)
            {
                // Low resolution; double up pixels.
                int address = scanline * 76 + wordOffset * 4;

                if (address > _currentBuffer.Length)
                {
                    throw new InvalidOperationException("Display word address is out of bounds.");
                }

                UInt32 stretched = StretchWord(word);

                _currentBuffer[address] = (byte)(stretched >> 24);
                _currentBuffer[address + 1] = (byte)(stretched >> 16);
                _currentBuffer[address + 2] = (byte)(stretched >> 8);
                _currentBuffer[address + 3] = (byte)(stretched);
            }
            else
            {
                int address = scanline * 76 + wordOffset * 2;

                if (address > _currentBuffer.Length)
                {
                    throw new InvalidOperationException("Display word address is out of bounds.");
                }

                _currentBuffer[address] = (byte)(word >> 8);
                _currentBuffer[address + 1] = (byte)(word);
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
            UInt32 displayWord = (UInt32)((_currentBuffer[address] << 24) |
                                        (_currentBuffer[address + 1] << 16) |
                                        (_currentBuffer[address + 2] << 8) |
                                        _currentBuffer[address + 3]);

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

            _currentBuffer[address] = (byte)(displayWord >> 24);
            _currentBuffer[address + 1] = (byte)(displayWord >> 16);
            _currentBuffer[address + 2] = (byte)(displayWord >> 8);
            _currentBuffer[address + 3] = (byte)(displayWord);
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

        //
        // Keyboard and Mouse handling
        //

        /// <summary>
        /// Handle modifier keys here mostly because Windows Forms doesn't 
        /// normally distinguish between left and right shift; the Alto keyboard does.
        /// CTRL is also handled here just because.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override bool ProcessKeyEventArgs(ref Message m)                 
        {
            // Grab the scancode from the message
            int scanCode = (int)((m.LParam.ToInt64() >> 16) & 0x1ff);
            bool down = false;

            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;

            if (m.Msg == WM_KEYDOWN)
            {
                down = true;
            }
            else if (m.Msg == WM_KEYUP)
            {
                down = false;
            }
            else
            {
                // Something else?
                return base.ProcessKeyEventArgs(ref m);
            }

            AltoKey modifierKey = AltoKey.None;

            switch(scanCode)
            {
                case 0x2a: // LShift
                    modifierKey = AltoKey.LShift;
                    break;                                       

                case 0x36:
                    modifierKey = AltoKey.RShift;
                    break;

                case 0x1d:
                case 0x11d:
                    modifierKey = AltoKey.CTRL;
                    break;
            }       
            
            if (modifierKey != AltoKey.None)
            {
                if (down)
                {
                    _system.Keyboard.KeyDown(modifierKey);
                }
                else
                {
                    _system.Keyboard.KeyUp(modifierKey);
                }

                return true; // handled
            }
            

            return base.ProcessKeyEventArgs(ref m);
        }
        // Hacky initial implementation of keyboard input.
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_mouseCaptured)
            {
                return;
            }            

            // Handle non-modifier keys here
            if (_keyMap.ContainsKey(e.KeyCode))
            {
                _system.Keyboard.KeyDown(_keyMap[e.KeyCode]);
            }
           
            if (e.Alt)
            {
                ReleaseMouse();
                e.SuppressKeyPress = true;
            }
        }        

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_mouseCaptured)
            {
                return;
            }

            // Handle non-modifier keys here
            if (_keyMap.ContainsKey(e.KeyCode))
            {
                _system.Keyboard.KeyUp(_keyMap[e.KeyCode]);
            }            
            
            if (e.Alt)
            {
                ReleaseMouse();
                e.SuppressKeyPress = true;
            }
        }

        private void OnDisplayMouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseCaptured)
            {
                // We do nothing with mouse input unless we have capture.                
                return;
            }

            if (_skipNextMouseMove)
            {
                _skipNextMouseMove = false;
                return;
            }


            // We implement a crude "mouse capture" behavior by forcing the mouse cursor to the
            // center of the display window and taking the delta of the last movement and using it
            // to move the Alto's mouse.
            // In this way the Windows cursor never leaves our window (important in order to prevent
            // other programs from getting clicked on while Missile Command is being played) and we
            // still get motion events.
            Point middle = new Point(DisplayBox.Width / 2, DisplayBox.Height / 2 );

            int dx = e.X - middle.X;
            int dy = e.Y - middle.Y;            

            _system.Mouse.MouseMove(dx, dy);

            // Force (invisible) cursor to middle of window            
            Cursor.Position = DisplayBox.PointToScreen(middle);

            // Don't handle the very next Mouse Move event (which will just be the motion we caused in the
            // above line...)
            _skipNextMouseMove = true;

        }

        private void OnDisplayMouseDown(object sender, MouseEventArgs e)
        {
            AltoMouseButton button = AltoMouseButton.None;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    button = AltoMouseButton.Left;
                    break;

                case MouseButtons.Right:
                    button = AltoMouseButton.Right;
                    break;

                case MouseButtons.Middle:
                    button = AltoMouseButton.Middle;
                    break;
            }

            _system.Mouse.MouseDown(button);

        }

        private void OnDisplayMouseUp(object sender, MouseEventArgs e)
        {
            if (!_mouseCaptured)
            {
                // On mouse-up, capture the mouse if the system is running.
                if (_controller.IsRunning)
                {
                    CaptureMouse();
                }
                return;
            }

            AltoMouseButton button = AltoMouseButton.None;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    button = AltoMouseButton.Left;
                    break;

                case MouseButtons.Right:
                    button = AltoMouseButton.Right;
                    break;

                case MouseButtons.Middle:
                    button = AltoMouseButton.Middle;
                    break;
            }

            _system.Mouse.MouseUp(button);
        }

        private void CaptureMouse()
        {
            // In mouse-capture mode we do the following:
            // - Set the mouse pointer to nothing (so we just see the Alto mouse pointer)
            // - Keep the mouse within our window bounds (so it doesn't escape our window, hence "capture").
            // - Put some text in the Status area telling people how to leave...
            _mouseCaptured = true;
            ShowCursor(false);            

            CaptureStatusLabel.Text = "Alto Mouse/Keyboard captured.  Press Alt to release.";
        }

        private void ReleaseMouse()
        {
            _mouseCaptured = false;
            ShowCursor(true);            

            CaptureStatusLabel.Text = "Click on display to capture Alto Mouse/Keyboard.";
        }

        /// <summary>
        /// Work around Windows Forms's bizarre refcounted cursor behavior...
        /// </summary>
        /// <param name="show"></param>
        private void ShowCursor(bool show)
        {
            if (show == _currentCursorState)
            {
                return;
            }

            if (show)
            {
                Cursor.Show();
            }
            else
            {
                Cursor.Hide();
            }

            _currentCursorState = show;
        }


        private void OnWindowLeave(object sender, EventArgs e)
        {
            // We are no longer the focus, make sure to release the mouse.
            ReleaseMouse();
        }

        private void OnWindowDeactivate(object sender, EventArgs e)
        {
            // We are no longer the focus, make sure to release the mouse.
            ReleaseMouse();
        }

        private void InitKeymap()
        {
            _keyMap = new Dictionary<Keys, AltoKey>();
            
            _keyMap.Add(Keys.A, AltoKey.A);
            _keyMap.Add(Keys.B, AltoKey.B);
            _keyMap.Add(Keys.C, AltoKey.C);
            _keyMap.Add(Keys.D, AltoKey.D);
            _keyMap.Add(Keys.E, AltoKey.E);
            _keyMap.Add(Keys.F, AltoKey.F);
            _keyMap.Add(Keys.G, AltoKey.G);
            _keyMap.Add(Keys.H, AltoKey.H);
            _keyMap.Add(Keys.I, AltoKey.I);
            _keyMap.Add(Keys.J, AltoKey.J);
            _keyMap.Add(Keys.K, AltoKey.K);
            _keyMap.Add(Keys.L, AltoKey.L);
            _keyMap.Add(Keys.M, AltoKey.M);
            _keyMap.Add(Keys.N, AltoKey.N);
            _keyMap.Add(Keys.O, AltoKey.O);
            _keyMap.Add(Keys.P, AltoKey.P);
            _keyMap.Add(Keys.Q, AltoKey.Q);
            _keyMap.Add(Keys.R, AltoKey.R);
            _keyMap.Add(Keys.S, AltoKey.S);
            _keyMap.Add(Keys.T, AltoKey.T);
            _keyMap.Add(Keys.U, AltoKey.U);
            _keyMap.Add(Keys.V, AltoKey.V);
            _keyMap.Add(Keys.W, AltoKey.W);
            _keyMap.Add(Keys.X, AltoKey.X);
            _keyMap.Add(Keys.Y, AltoKey.Y);
            _keyMap.Add(Keys.Z, AltoKey.Z);
            _keyMap.Add(Keys.D0, AltoKey.D0);
            _keyMap.Add(Keys.D1, AltoKey.D1);
            _keyMap.Add(Keys.D2, AltoKey.D2);
            _keyMap.Add(Keys.D3, AltoKey.D3);
            _keyMap.Add(Keys.D4, AltoKey.D4);
            _keyMap.Add(Keys.D5, AltoKey.D5);
            _keyMap.Add(Keys.D6, AltoKey.D6);
            _keyMap.Add(Keys.D7, AltoKey.D7);
            _keyMap.Add(Keys.D8, AltoKey.D8);
            _keyMap.Add(Keys.D9, AltoKey.D9);
            _keyMap.Add(Keys.Space, AltoKey.Space);
            _keyMap.Add(Keys.OemPeriod, AltoKey.Period);
            _keyMap.Add(Keys.Oemcomma, AltoKey.Comma);
            _keyMap.Add(Keys.OemQuotes, AltoKey.Quote);
            _keyMap.Add(Keys.Oem5, AltoKey.BSlash);
            _keyMap.Add(Keys.OemBackslash, AltoKey.BSlash);
            _keyMap.Add(Keys.OemQuestion, AltoKey.FSlash);
            _keyMap.Add(Keys.Oemplus, AltoKey.Plus);
            _keyMap.Add(Keys.OemMinus, AltoKey.Minus);
            _keyMap.Add(Keys.Escape, AltoKey.ESC);
            _keyMap.Add(Keys.Delete, AltoKey.DEL);
            _keyMap.Add(Keys.Left, AltoKey.Arrow);
            _keyMap.Add(Keys.LShiftKey, AltoKey.LShift);
            _keyMap.Add(Keys.RShiftKey, AltoKey.RShift);
            _keyMap.Add(Keys.ControlKey, AltoKey.CTRL);
            _keyMap.Add(Keys.Return, AltoKey.Return);
            _keyMap.Add(Keys.F1, AltoKey.BlankTop);
            _keyMap.Add(Keys.F2, AltoKey.BlankMiddle);
            _keyMap.Add(Keys.F3, AltoKey.BlankBottom);
            _keyMap.Add(Keys.Back, AltoKey.BS);
            _keyMap.Add(Keys.Tab, AltoKey.TAB);
            _keyMap.Add(Keys.OemSemicolon, AltoKey.Semicolon);
            _keyMap.Add(Keys.OemOpenBrackets, AltoKey.LBracket);
            _keyMap.Add(Keys.OemCloseBrackets, AltoKey.RBracket);
            _keyMap.Add(Keys.Down, AltoKey.LF);
        }
        
        
        // Display related data.
        // Note: display is actually 606 pixels wide, but that's not an even multiple of 8, so we round up.
        // Two backbuffers and references to the current / last buffer for rendering
        private byte[] _displayData0 = new byte[808 * 76 + 4];       // + 4 to make cursor display logic simpler.
        private byte[] _displayData1 = new byte[808 * 76 + 4];       // + 4 to make cursor display logic simpler.
        private byte[] _currentBuffer;
        private byte[] _lastBuffer;
        private int _frame;
        private Bitmap _displayBuffer;
        private Rectangle _displayRect = new Rectangle(0, 0, 608, 808);
        private delegate void DisplayDelegate();

        // Speed throttling
        FrameTimer _frameTimer;

        // Input related data

        // Keyboard mapping from windows vkeys to Alto keys
        private Dictionary<Keys, AltoKey> _keyMap;

        private bool _mouseCaptured;
        private bool _currentCursorState;
        private bool _skipNextMouseMove;

        // The Alto system we're running
        private AltoSystem _system;

        // The controller for the system (to allow control by both debugger and main UI)
        private ExecutionController _controller;

        // The debugger, which may or may not be running.
        private Debugger _debugger;

        // strings.  TODO: move to resource
        private const string _noImageLoadedText = "<no image loaded>";
        private const string _systemStoppedText = "Alto Stopped.";
        private const string _systemRunningText = "Alto Running.";
        private const string _systemErrorText = "Alto Stopped due to error.  See Debugger.";

    }
}
