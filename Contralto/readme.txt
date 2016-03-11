Readme.txt for Contralto v0.1:

1. Introduction and Overview
----------------------------

ContrAlto purports to be a faithful emulation of the Xerox Alto series of 
pioneering graphical workstations developed at Xerox PARC in 1973. 

1.1 What's Emulated
-------------------

ContrAlto currently emulates the following Alto hardware:
   - Alto IIxm CPU
   - Microcode RAM (in 1K RAM, 1K RAM/2K ROM, or 3K RAM configurations)
   - 256KW of main memory (in 64KW banks)
   - Diablo Model 31 drives
   - Ethernet (encapsulated in either UDP datagrams or raw Ethernet frames
               on the host machine)
   - Standard Keyboard/Mouse/Video

At this time, ContrAlto does not support more exotic hardware such as Diablo 44
and Trident disks, the Orbit printer interface, or the keyset input device.

2.0 Requirements
----------------

ContrAlto will run on any Windows PC running Windows XP or later, with version
2.0 or later of the .NET Framework installed.  .NET should be present by default
on Windows Vista and later; if it is not installed on your computer it can be
obtained at https://www.microsoft.com/net.

As ContrAlto is a .NET application and has no Windows-specific dependencies,
it will also run under Mono (http://www.mono-project.com/) on Unix and MacOS, 
however this usage is not as well tested -- please file bugs if you find them 
(see Section 7 for details).


3.0 Getting Started
-------------------

To launch ContrAlto, simply click on the shortcut created by the installer
on your Start Menu (or Start Screen, depending on your Windows version.)  Two
windows will appear: a console window for diagnostic output and the main display
window, labeled "ContrAlto."  This latter window is where you will interact
with your virtual Alto.

On a real Alto, the system is booted by loading a 14" disk pack into the front
of a Diablo 31 drive, waiting for it to spin up for 20 seconds and then
pressing the "Reset" button on the back of the keyboard.

Booting an emulated Alto under ContrAlto is slightly less time-consuming.
To load a disk pack into the virtual Diablo drive, click on the "System"
menu and go to "Drive 0 -> Load...".  You will be presented with a file dialog
allowing selection of the disk image (effectively a "virtual disk pack") to
be loaded.  Several images are provided with ContrAlto, select one and click
the "Open" button.

Once the pack has been loaded, you can start the Alto by clicking on the 
"System->Start" menu (or hitting Ctrl+Alt+S).  The display will turn white and
after 5-10 seconds a mouse cursor will appear, followed shortly by the banner
of the Xerox Alto Executive.  Congratulations, your Alto is now running!  Click
on the display window to start interacting with it using the keyboard and mouse
(and if you need your mouse back for other things, press either "Alt" key on
your keyboard.)

3.1 Using the Alto
------------------

3.1.1 The Basics
----------------

3.1.2 Reading Materials
-----------------------



4.0 Configuration
-----------------

4.1 CPU
-------


4.2 Ethernet
------------


4.3 Display
-----------


5.0 Debugger
------------

ContrAlto contains a fairly capable debugger window that can be invoked via
the "System->Show Debugger" menu (or Ctrl+Alt+D) at any time.  When the debugger
is invoked, it takes over control of the system from the main display window.
The system can be micro-stepped or single-stepped and breakpoints can be set on
microcode addresses or Nova instruction addresses.

Usage of the debugger is mostly straightforward but it is intended for "expert"
users only and still has many rough edges.

5.1 The Controls
----------------
At the very bottom of the debugger window is a row of buttons.  These are (from
left to right):

Step:    Runs the Alto CPU for one clock cycle.  Normally this coincides with
         a single microinstruction, but not always (for example, memory accesses
         may require multiple cycles.)  The next  microinstruction to be 
         executed will be highlighted in the "Microcode Source" pane.

Auto:    Automatically single-steps the CPU at a relatively slow rate, while
         refreshing the debugger UI after every step.  Not particularly useful
         in most circumstances (but it looks neat.)
    
Run:     Starts the CPU running normally.  Execution will continue until a 
         breakpoint is hit one of the other control buttons are pressed.
         
Run T:   Runs the CPU until the next TASK switch occurs

Nova Step: Runs the CPU until the current Nova instruction is completed.  This
         will only work properly if the standard Nova microcode is running (if
         you are executing a Mesa program, this will not work correctly.)
         
Stop:    Stops the CPU

Reset:   Resets the Alto system.

5.2 Microcode Source Pane
-------------------------

The pane in the upper left of the debugger window shows the microcode listings
for ROM0, ROM1, and RAM1.  The listings for ROM0 and ROM1 are derived from the
original source code listings.  The listing for RAM1 is automatically 
disassembled from the contents of control RAM (and is generally more annoying
to read.)

ROM0 contains the listing for the main microcode ROMs -- this 1K of ROM contains
code for all of the microcode tasks (Emulator, Disk Sector, Ethernet, Memory
Refresh, Display Word, Cursor, Display Horizontal, Display Vertical, Parity, and
Disk Word).  The source code for each task is highlighted in a different color
to make it easy to differentiate.

ROM1 contains the listing for the Mesa microcode ROMs.

5.3 Memory Pane
---------------

The pane near the lower-left (labeled "Memory") shows a view into the main
memory of the Alto, providing address/data and an automated disassembly of Alto
(Nova) instructions.

5.4 Breakpoints
---------------

Breakpoints can be set on either microcode or Nova code by checking the box
in the "B" column next to the instruction.  Unchecking the box will remove the
breakpoint.

Nova code breakpoints will only work if the standard Nova microcode is running.

5.5 Everything Else
-------------------

The other panes in the debugger are:

Disk:    Shows registers and other status of the disk hardware

Tasks:   Shows the current microcode task status.  The "T" column indicates the
         task name, "S" indicates the status ("W"akeup and "R"unning), and the
         "uPC" column indicates the micro-PC for the corresponding task.  There
         are 16 possible tasks, not all are used on most Altos.
         
CPU Registers:
         Shows the CPU L, M and T registers as well as ALU and memory registers.

General Registers:
         Shows the contents of the 32 R and 32 S registers (in octal).
         
Reserved Memory:
         Shows the contents of most "well known" memory locations.  See the
         Alto HW Reference manual (link in Sectoin 3.1.2) for what these mean.

         
6.0 Known Issues
----------------


7.0 Reporting Bugs
------------------

