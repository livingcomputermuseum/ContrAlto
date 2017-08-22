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

using Contralto.CPU;
using Contralto.Display;
using Contralto.IO;
using Contralto.Properties;
using Contralto.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
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
            _fullScreenDisplay = false;

            _displayBuffer = new Bitmap(608, 808, PixelFormat.Format1bppIndexed);
            DisplayBox.Image = _displayBuffer;

            _lastBuffer = _currentBuffer = _displayData0;
            _frame = 0;

            try
            {
                _frameTimer = new FrameTimer(60.0);
            }
            catch(DllNotFoundException)
            {
                //
                // On Mono platforms, we can't PInvoke to get what we want.
                // We just won't be able to synchronize to 60fps.
                //
                _frameTimer = null;
            }

            ReleaseMouse();

            CreateTridentMenu();

            SystemStatusLabel.Text = _systemStoppedText;
            DiskStatusLabel.Text = String.Empty;

            _diskIdleImage = Resources.DiskNoAccess;
            _diskReadImage = Resources.DiskRead;
            _diskWriteImage = Resources.DiskWrite;
            _diskSeekImage = Resources.DiskSeek;

            this.DoubleBuffered = true;

            _fpsTimer = new System.Timers.Timer();
            _fpsTimer.AutoReset = true;
            _fpsTimer.Interval = 1000;
            _fpsTimer.Elapsed += OnFPSTimerElapsed;
            _fpsTimer.Start();

            _diskAccessTimer = new System.Timers.Timer();
            _diskAccessTimer.AutoReset = true;
            _diskAccessTimer.Interval = 25;
            _diskAccessTimer.Elapsed += OnDiskTimerElapsed;
            _diskAccessTimer.Start();
        }

        public void AttachSystem(AltoSystem system)
        {
            _system = system;
            _system.AttachDisplay(this);

            _controller = new ExecutionController(_system);

            _controller.ErrorCallback += OnExecutionError;

            // Update disk image UI info
            // Diablo disks:
            Drive0ImageName.Text = _system.DiskController.Drives[0].IsLoaded ? Path.GetFileName(_system.DiskController.Drives[0].Pack.PackName) : _noImageLoadedText;
            Drive1ImageName.Text = _system.DiskController.Drives[1].IsLoaded ? Path.GetFileName(_system.DiskController.Drives[1].Pack.PackName) : _noImageLoadedText;

            // Trident disks
            for (int i = 0; i < _tridentImageNames.Count; i++)
            {
                TridentDrive drive = _system.TridentController.Drives[i];
                _tridentImageNames[i].Text = drive.IsLoaded ? Path.GetFileName(drive.Pack.PackName) : _noImageLoadedText;
            }
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
            string path = ShowImageLoadDialog(0, false);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadDiabloDrive(0, path, false);
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
            _system.UnloadDiabloDrive(0);
            Drive0ImageName.Text = _noImageLoadedText;
            Configuration.Drive0Image = String.Empty;
        }

        private void OnSystemDrive0NewClick(object sender, EventArgs e)
        {
            string path = ShowImageNewDialog(0, false);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadDiabloDrive(0, path, true);
                Drive0ImageName.Text = System.IO.Path.GetFileName(path);
                Configuration.Drive0Image = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while creating new disk image: {0}", ex.Message),
                    "Image creation error", MessageBoxButtons.OK);
            }
        }

        private void OnSystemDrive1LoadClick(object sender, EventArgs e)
        {
            string path = ShowImageLoadDialog(1, false);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadDiabloDrive(1, path, false);
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
            _system.UnloadDiabloDrive(1);
            Drive1ImageName.Text = _noImageLoadedText;
            Configuration.Drive1Image = String.Empty;
        }


        private void OnSystemDrive1NewClick(object sender, EventArgs e)
        {
            string path = ShowImageNewDialog(1, false);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadDiabloDrive(1, path, true);
                Drive0ImageName.Text = System.IO.Path.GetFileName(path);
                Configuration.Drive0Image = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while creating new disk image: {0}", ex.Message),
                    "Image creation error", MessageBoxButtons.OK);
            }
        }

        private void OnTridentLoadClick(object sender, EventArgs e)
        {
            int drive = (int)((ToolStripDropDownItem)sender).Tag;
            string path = ShowImageLoadDialog(drive, true);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadTridentDrive(drive, path, false);
                _tridentImageNames[drive].Text = System.IO.Path.GetFileName(path);
                Configuration.TridentImages[drive] = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while loading Trident image: {0}", ex.Message),
                    "Image load error", MessageBoxButtons.OK);
            }
        }

        private void OnTridentUnloadClick(object sender, EventArgs e)
        {
            int drive = (int)((ToolStripDropDownItem)sender).Tag;
            _system.UnloadTridentDrive(drive);
            _tridentImageNames[drive].Text = _noImageLoadedText;
            Configuration.TridentImages[drive] = String.Empty;
        }

        private void OnTridentNewClick(object sender, EventArgs e)
        {
            int drive = (int)((ToolStripDropDownItem)sender).Tag;
            string path = ShowImageNewDialog(drive, true);

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                _system.LoadTridentDrive(drive, path, true);
                _tridentImageNames[drive].Text = System.IO.Path.GetFileName(path);
                Configuration.TridentImages[drive] = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occurred while creating new Trident image: {0}", ex.Message),
                    "Image creation error", MessageBoxButtons.OK);
            }
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

        private void OnFullScreenMenuClick(object sender, EventArgs e)
        {
            BeginInvoke(new DisplayDelegate(ToggleFullScreen));
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

        private void OnFileSaveScreenshotClick(object sender, EventArgs e)
        {
            // Pause execution while the user selects the destination for the screenshot
            bool wasRunning = _controller.IsRunning;

            _controller.StopExecution();

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.DefaultExt = "png";

            fileDialog.Filter = "PNG Images (*.png)|*.png|All Files (*.*)|*.*";            
            fileDialog.Title = String.Format("Select destination for screenshot.");
            fileDialog.CheckPathExists = true;
            fileDialog.FileName = "Screenshot.png";        

            DialogResult res = fileDialog.ShowDialog();

            if (res == DialogResult.OK)
            {
                EncoderParameters p = new EncoderParameters(1);
                p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                try
                {
                    _displayBuffer.Save(fileDialog.FileName, GetEncoderForFormat(ImageFormat.Png), p);
                }
                catch
                {
                    MessageBox.Show("Could not save screenshot.  Check the specified filename and path and try again.");
                }
            }

            if (wasRunning)
            {
                _controller.StartExecution(AlternateBootType.None);
            }
        }

        private void OnFileExitClick(object sender, EventArgs e)
        {
            _controller.StopExecution();
            this.Close();           
        }

        private void OnAltoWindowClosed(object sender, FormClosedEventArgs e)
        {
            //
            // Stop UI timers
            //
            _fpsTimer.Stop();
            _diskAccessTimer.Stop();

            // Halt the system and detach our display              
            _controller.StopExecution();
            _system.DetachDisplay();
            _system.Shutdown();

            //
            // Commit current configuration to disk
            //
            Configuration.WriteConfiguration();            

            DialogResult = DialogResult.OK;            
        }

        private string ShowImageLoadDialog(int drive, bool trident)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.DefaultExt = trident ? "dsk80" : "dsk";
            fileDialog.Filter = trident ? _tridentFilter : _diabloFilter;
            fileDialog.Multiselect = false;
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.Title = String.Format("Select image to load into {0} drive {1}", trident ? "Trident" : "Diablo", drive);

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

        private string ShowImageNewDialog(int drive, bool trident)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.DefaultExt = trident ? "dsk80" : "dsk";
            fileDialog.Filter = trident ? _tridentFilter : _diabloFilter;
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = true;
            fileDialog.OverwritePrompt = true;
            fileDialog.ValidateNames = true;
            fileDialog.Title = String.Format("Select path for new {0} image for drive {1}", trident ? "Trident" : "Diablo", drive);

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
            if (Configuration.ThrottleSpeed && _frameTimer != null)
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
            else
            {
                _lastBuffer = _currentBuffer;                
            }            

            // Asynchronously render this frame.
            BeginInvoke(new DisplayDelegate(RefreshDisplayBox));            
        }

        private void RefreshDisplayBox()
        {
            DisplayBox.Invalidate();

        }

        private void OnPaint(object sender, PaintEventArgs e)
        { 
            // Update the display
            BitmapData data = _displayBuffer.LockBits(_displayRect, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            IntPtr ptr = data.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_lastBuffer, 0, ptr, _lastBuffer.Length - 4);

            _displayBuffer.UnlockBits(data);

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

            if (dx != 0 || dy != 0)
            {
                _system.Mouse.MouseMove(dx, dy);

                // Don't handle the very next Mouse Move event (which will just be the motion we caused in the
                // below line...)
                _skipNextMouseMove = true;
                
                Cursor.Position = DisplayBox.PointToScreen(middle);
            }
        }

        private void HackMouseMove()
        {
            Point middle = new Point(DisplayBox.Width / 2, DisplayBox.Height / 2);
            // Force (invisible) cursor to middle of window            
            Cursor.Position = DisplayBox.PointToScreen(middle);
        }

        private void OnDisplayMouseDown(object sender, MouseEventArgs e)
        {
            if (!_mouseCaptured)
            {
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

        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            //
            // If we've switched to a fullscreen mode, update the
            // Alto's display area.
            //
            if (_fullScreenDisplay)
            {
                DisplayBox.Top = 0;
                DisplayBox.Left = 0;
                DisplayBox.Width = this.Width;
                DisplayBox.Height = this.Height;
                DisplayBox.SizeMode = PictureBoxSizeMode.Zoom;
                DisplayBox.BackColor = Color.Black;
            }            
        }

        private void OnFPSTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string fpsMessage = String.Format("{0} fields/sec", _system.DisplayController.Fields);

            FPSLabel.Text = fpsMessage;

            _system.DisplayController.Fields = 0;
        }

        private void OnDiskTimerElapsed(object sender, ElapsedEventArgs e)
        {
            BeginInvoke(new DisplayDelegate(RefreshDiskStatus));            
        }

        private void RefreshDiskStatus()
        {
            if (_lastActivity != _system.DiskController.LastDiskActivity)
            {
                _lastActivity = _system.DiskController.LastDiskActivity;
                switch (_lastActivity)
                {
                    case DiskActivityType.Idle:
                        DiskStatusLabel.Image = _diskIdleImage;
                        break;

                    case DiskActivityType.Read:
                        DiskStatusLabel.Image = _diskReadImage;
                        break;

                    case DiskActivityType.Write:
                        DiskStatusLabel.Image = _diskWriteImage;
                        break;

                    case DiskActivityType.Seek:
                        DiskStatusLabel.Image = _diskSeekImage;
                        break;
                }
            }
        }

        private void ToggleFullScreen()
        {
            _fullScreenDisplay = !_fullScreenDisplay;

            if (_fullScreenDisplay)
            {
                // Save the original size and location of the Alto's Display so we can
                // restore it when full-screen mode is exited.
                _windowedDisplayBoxLocation = DisplayBox.Location;
                _windowedDisplayBoxSize = DisplayBox.Size;

                // Hide window adornments and make the window full screen on the current
                // display.
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;                
                _mainMenu.Visible = false;
                StatusLine.Visible = false;                

                // Once the window repaints we will center the Alto's display and
                // stretch if applicable.
            }
            else
            {
                // Show everything that was hidden before.
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;                
                _mainMenu.Visible = true;
                StatusLine.Visible = true;

                DisplayBox.SizeMode = PictureBoxSizeMode.Normal;
                DisplayBox.Location = _windowedDisplayBoxLocation;
                DisplayBox.Size = _windowedDisplayBoxSize;
            }
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
            _keyMap.Add(Keys.F4, AltoKey.Lock);
            _keyMap.Add(Keys.Back, AltoKey.BS);
            _keyMap.Add(Keys.Tab, AltoKey.TAB);
            _keyMap.Add(Keys.OemSemicolon, AltoKey.Semicolon);
            _keyMap.Add(Keys.OemOpenBrackets, AltoKey.LBracket);
            _keyMap.Add(Keys.OemCloseBrackets, AltoKey.RBracket);
            _keyMap.Add(Keys.Down, AltoKey.LF);            
        }

        private ImageCodecInfo GetEncoderForFormat(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void CreateTridentMenu()
        {
            //
            // Add eight sub-menus, one per drive.
            //
            _tridentImageNames = new List<ToolStripMenuItem>(8);

            for (int i=0;i<8;i++)
            {
                // Parent menu item
                ToolStripMenuItem tridentMenu = new ToolStripMenuItem(String.Format("Drive {0}", i));

                // Children:
                // - Load
                // - Unload
                // - New
                // - Pack Name (disabled)
                //
                ToolStripMenuItem loadMenu = new ToolStripMenuItem("Load...", null, OnTridentLoadClick);
                loadMenu.Tag = i;

                ToolStripMenuItem unloadMenu = new ToolStripMenuItem("Unload", null, OnTridentUnloadClick);
                unloadMenu.Tag = i;

                ToolStripMenuItem newMenu = new ToolStripMenuItem("New...", null, OnTridentNewClick);
                newMenu.Tag = i;

                ToolStripMenuItem imageMenu = new ToolStripMenuItem(_noImageLoadedText);
                imageMenu.Tag = i;
                imageMenu.Enabled = false;
                _tridentImageNames.Add(imageMenu);

                tridentMenu.DropDownItems.Add(loadMenu);
                tridentMenu.DropDownItems.Add(unloadMenu);
                tridentMenu.DropDownItems.Add(newMenu);
                tridentMenu.DropDownItems.Add(imageMenu);

                TridentToolStripMenuItem.DropDownItems.Add(tridentMenu);
            }
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

        // Mouse capture state
        private bool _mouseCaptured;
        private bool _currentCursorState;
        private bool _skipNextMouseMove;

        // Full-screen state
        private bool _fullScreenDisplay;
        private Point _windowedDisplayBoxLocation;
        private Size _windowedDisplayBoxSize;

        // The Alto system we're running
        private AltoSystem _system;

        // The controller for the system (to allow control by both debugger and main UI)
        private ExecutionController _controller;

        // The debugger, which may or may not be running.
        private Debugger _debugger;

        // Status bar things
        System.Timers.Timer _fpsTimer;
        System.Timers.Timer _diskAccessTimer;

        private DiskActivityType _lastActivity;
        private Image _diskIdleImage;
        private Image _diskReadImage;
        private Image _diskWriteImage;
        private Image _diskSeekImage;

        // Trident menu items for disk names
        private List<ToolStripMenuItem> _tridentImageNames;

        // strings.  TODO: move to resource
        private const string _noImageLoadedText = "<no image loaded>";
        private const string _systemStoppedText = "Alto Stopped.";
        private const string _systemRunningText = "Alto Running.";
        private const string _systemErrorText = "Alto Stopped due to error.  See Debugger.";
        private const string _diabloFilter = "Alto Diablo Disk Images (*.dsk, *.dsk44)|*.dsk;*.dsk44|Diablo 31 Disk Images (*.dsk)|*.dsk|Diablo 44 Disk Images (*.dsk44)|*.dsk44|All Files (*.*)|*.*";
        private const string _tridentFilter = "Alto Trident Disk Images (*.dsk80, *.dsk300)|*.dsk80;*.dsk300|Trident T80 Disk Images (*.dsk80)|*.dsk80|Trident T300 Disk Images (*.dsk300)|*.dsk300|All Files (*.*)|*.*";


    }                 
}
