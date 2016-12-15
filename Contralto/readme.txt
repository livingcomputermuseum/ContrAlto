Readme.txt for Contralto v1.1:

1. Introduction and Overview
============================

ContrAlto aspires to be a faithful emulation of the Xerox Alto II series of 
pioneering graphical workstations developed at Xerox PARC in 1973. 


1.1 What's Emulated
-------------------

ContrAlto currently emulates the following Alto hardware:
   - Alto I and Alto II XM CPU
   - Microcode RAM (in 1K RAM, 1K RAM/2K ROM, or 3K RAM configurations)
   - 256KW of main memory (in 64KW banks)
   - Two Diablo Model 31 or 44 drives
   - Ethernet (encapsulated in either UDP datagrams or raw Ethernet frames
               on the host machine)
   - Standard Keyboard/Mouse/Video

1.2 What's Not
--------------

At this time, ContrAlto does not support more exotic hardware such as Trident 
disks, printers or audio using the utility port, the Orbit printer interface, or the 
keyset input device.


2.0 Requirements
================

ContrAlto will run on any Windows PC running Windows Vista or later, with version
4.5.3 or later of the .NET Framework installed.  .NET should be present by default
on Windows Vista and later; if it is not installed on your computer it can be
obtained at https://www.microsoft.com/net.

As ContrAlto is a .NET application and has no Windows-specific dependencies,
it will also run under Mono (http://www.mono-project.com/) on Unix and MacOS, 
however this usage has many rough edges at the moment -- please file bugs if 
you find them (see Section 7 for details).

A three-button mouse is essential for using most Alto software.  On most mice,
the mousewheel can be clicked to provide the third (middle) button.  Laptops
with trackpads may have configuration options to simulate three buttons but
will likely be clumsy to use.


3.0 Getting Started
===================

Installation of ContrAlto is simple:  Double-click the installer file, named 
"ContraltoSetup.msi" and follow the on-screen instructions.  The installer
will install all of the necessary files and create two icons on your Start menu,
one for ContrAlto itself, and one for its documentation (the file you're reading
now!)


3.1 Starting the Alto
=====================

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
be loaded.  Disk images are not included with ContrAlto but may be found in
various places on the Internet -- see Section 3.1.3 for details.

Once the pack has been loaded, you can start the Alto by clicking on the 
"System->Start" menu (or hitting Ctrl+Alt+S).  The display will turn white and
after 5-10 seconds a mouse cursor will appear, followed shortly by the banner
of the Xerox Alto Executive.  Congratulations, your Alto is now running!  Click
on the display window to start interacting with it using the keyboard and mouse
(and if you need your mouse back for other things, press either "Alt" key on
your keyboard.)  See Section 3.1 for details on using ContrAlto.


3.1 Using the Alto
==================

3.1.1 Mouse
-----------

ContrAlto uses your computer's mouse to simulate the one the Alto uses.  
In order to accurately simulate the mouse, ContrAlto must "capture" the real 
mouse, which effectively makes your system's mouse exclusive to the ContrAlto 
window.  (If you've ever used virtualization software like VMWare, VirtualBox, 
Virtual PC, or Parallels, you may be familiar with this behavior.)  

Clicking on the ContrAlto display window will cause ContrAlto to capture the
mouse.  Once the mouse has been captured, any mouse movements will be reflected
by the Alto's mouse cursor.  While ContrAlto has control of the mouse, you will 
not be able to use the mouse for other programs running on your PC.  To release 
ContrAlto's control, press either "Alt" key on your keyboard.  Mouse movements
will return to normal, and you will not be able to control the Alto's mouse
until you click on the ContrAlto display window again.

The Alto mouse is a three-button mouse.  Alto mouse buttons are mapped as you
would expect.  If you have a real three-button mouse then this is completely
straightforward.  If you have a two button mouse with a "mousewheel" then
a mousewheel click maps to a click of the Alto's middle mouse button.

If you have a trackpad or other pointing device, using the middle mouse button 
may be more complicated.  See what configuration options your operating system 
and/or drivers provide you for mapping mouse buttons.


3.1.2 Keyboard
--------------

ContrAlto emulates the 61-key Alto II keyboard.  The vast majority of keys
(the alphanumerics and punctuation) work as you would expect them to, but the
Alto has a few special keys, which are described below:

Alto Key       PC Key
--------       ----------
LF             Down Arrow
BS             Backspace
Blank-Top      F1
Blank-Middle   F2
Blank-Bottom   F3
<- (arrow)     Left Arrow
DEL            Del
LOCK           F4


3.1.3 Disk Packs
----------------

A real Alto uses large 14" disk packs for disk storage, each containing
approximately 2.5 megabytes (for Diablo 31) or 5 megabytes (for Diablo 44) of 
data.  ContrAlto uses files, referred to as "disk images" or just "images" 
that contain a bit-for-bit copy of these original packs.  These are a lot 
easier to use with a modern PC.

Disk images can be loaded and unloaded via the "System->Drive 0" and 
System->Drive 1" menus.  A file dialog will be presented showing possible disk
images in the current directory.

If you modify the contents of a loaded disk (for example creating new files or
deleting existing ones) the changes will be written back out to the disk image
when a new image is loaded or when ContrAlto exits.  For this reason it may be
a good idea to make backups of packs from time to time (just like on the real
machine.)

ContrAlto does not come with any disk images, however an assortment of Alto 
programs can be found on Bitsavers.org, at 
http://www.bitsavers.org/bits/Xerox/Alto/disk_images/.  Images include:

AllGames.dsk   -  A collection of games and toys for the Alto
Bcpl.dsk       -  A set of BCPL development tools
Diags.dsk      -  Diagnostic tools
NonProg.dsk    -  The "Non-Programmer's Disk," containing Bravo
Xmsmall.dsk    -  Smalltalk-76


3.1.4 Startup, Reset and Shutdown
---------------------------------

The system can be started at any time by using the "System->Start" menu, though
in general having a pack image loaded first is a good idea.  Similarly, the
"Start->Reset" menu will reset the Alto.

You can shut down the Alto by closing the ContrAlto window; this will commit
disk changes made to the currently loaded disks back to the disk image files.
However, you will want to be sure the software running on the Alto is ready
to be shutdown first, or else you may lose work or corrupt your disk.


3.2 Additional Reading Materials
----------------------------------

The Bitsavers Alto archive at http://http://bitsavers.org/pdf/xerox/alto is an 
excellent repository of original Alto documentation, here are a few documents to 
get you started:

- The "Alto User's Handbook" is indispensable and contains an overview of the 
  Alto Executive (the OS "shell"), Bravo (great-granddaddy of Microsoft Word) 
  and other utilities.  
  http://bitsavers.org/pdf/xerox/alto/Alto_Users_Handbook_Sep79.pdf
  
- "Alto Subsystems" documents many of the common Alto programs and tools
  ("subsystems" in Alto parlance) in detail.  
  http://bitsavers.org/pdf/xerox/alto/AltoSubsystems_Oct79.pdf

- "Alto Operating System Reference Manual" is useful if you are going to do
  any programming for the Alto.
  http://bitsavers.org/pdf/xerox/alto/AltoSWRef.part1.pdf
  http://bitsavers.org/pdf/xerox/alto/AltoSWRef.part2.pdf

- "BCPL Reference Manual" is definitely required if you are going to do any
  programming on the Alto (in BCPL, anyway...)
  http://bitsavers.org/pdf/xerox/alto/bcpl/AltoBCPLdoc.pdf

- "Bravo Course Outline" is a tutorial that will show you how to use the Bravo
  editor.
  http://bitsavers.org/pdf/xerox/alto/BravoCourse.pdf

- The "Alto Hardware Manual" is fun to read through if you're planning on
  writing an Alto emulator of your own.  If you're into that sort of thing.
  http://bitsavers.org/pdf/xerox/alto/AltoHWRef.part1.pdf
  http://bitsavers.org/pdf/xerox/alto/AltoHWRef.part2.pdf

- "A Field Guide to Alto-Land" is a casual perspective on Alto use (and
  the culture that grew around it) at Xerox PARC.
  http://xeroxalto.computerhistory.org/_cd8_/altodocs/.fieldguide.press!2.pdf
 

4.0 Configuration
=================

ContrAlto provides a number of configuration options via the 
"System->System Configuration..." menu.  Selecting this menu item will invoke
a small configuration dialog with three tabs, which are described in the
following sections.


4.1 CPU
-------

This tab allows selection of the CPU configuration.  Normally, this setting 
should not need to be changed from the default 
(Alto II, 2K Control ROM, 1K Control RAM).  If you need to run software that
demands a specific configuration (which is very rarely the case) then change
the configuration here.  The system will need to be reset for the change to
take effect.


4.2 Ethernet
------------

The Ethernet tab provides configuration options for ContrAlto's host Ethernet
encapsulation.  ContrAlto can encapsulate the Alto's 3mbit ("experimental") 
Ethernet packets in either UDP datagrams or raw Ethernet packets on a network
interface on the "host" computer (the computer running ContrAlto).

Raw packet encapsulation requires WinPCAP and the Microsoft Visual C++ 2010 
redistributable to be installed; these can be acquired from:

http://www.winpcap.org/
and
http://www.microsoft.com/en-us/download/details.aspx?id=5555

ContrAlto uses binaries from the Pcap.NET project to expose WinPCAP
functionality to the emulator:

https://github.com/PcapDotNet/Pcap.Net

Pcap.NET is released under the BSD license.

4.2.1 Host Address
------------------

The Alto's network address can be specified via the "Alto Address" box at the
top of the tab.  This is an octal value between 1 and 376.  (The addresses 0
and 377 are reserved for broadcast and Breath Of Life packets, respectively.)

The default address is "42" and need only be changed if you will be
communicating with other Alto hosts on the network.  Duplicate network addresses
will cause odd problems in communication, so make sure all hosts have unique
addresses!


4.2.2 UDP Encapsulation
-----------------------

UDP Encapsulation is selected via the "UDP" radio button.  This causes Alto 
Ethernet packets to be encapsulated in broadcast UDP datagrams.  These 
broadcasts are sent to the IPV4 network associated with the network adapter
selected in the "Host Interface" network list box.


4.2.3 Raw Ethernet Encapsulation
--------------------------------

Raw Ethernet Encapsulation is selected via the "Raw Ethernet" radio button.
This causes Alto Ethernet packets to be encapsulated in ethernet packets on the
selected network interface.


4.3 Display
-----------

The Display tab provides options governing the way ContrAlto displays the
simulated Alto display.

The "Throttle Framerate" checkbox will force ContrAlto to run at an even 60
fields/second (matching the speed of the original Alto).  Use this if things 
are running too fast (for example, games that require reflexes.)  Uncheck this
if you want things to run as fast as possible (for example, compiling code or
running Smalltalk.)

The "Interlaced Display" checkbox attempts to simulate the Alto's original
interlaced display.  Depending on your monitor and the speed of your computer
this may or may not work well.


4.4 Alternate ("keyboard") Boots
--------------------------------

The Alto allowed the specification of alternate boot addresses by holding down
a set of keys on the keyboard while hitting the "reset" switch on the back of
the keyboard.  Since this would be difficult to pull off by hand on the emulator
due to the UI involved, ContrAlto provides a configuration dialog to select the
alternate address to boot.  When the "Start with Alternate Boot" menu is
chosen, the system will be started (or restarted) with these keys held down on
your behalf.

The "Alternate Boot Options" dialog is invoked by the "System->Alternate Boot
Options" menu and provides configuration for alternate boots.

The boot type (disk or ethernet) can be selected via the "Alternate Boot Type"
radio buttons.  Ethernet booting will only work if another host on the network
is providing boot services.

The "Disk Boot Address" text box accepts a 16-bit octal value (from 0 to 177777)
specifying the address to be booted.

The "Ethernet Boot File" option provides a list box containing a number of
standard boot files, or a 16-bit octal value (from 0 to 177777) can be manually
supplied.


5.0 Debugger
============

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
         
Run T:   Runs the CPU until the next TASK switch occurs, the next instruction 
         executed will be the instruction after the TASK SF that caused the 
         switch.

Nova Step: Runs the CPU until the current Nova instruction is completed.  This
         will only work properly if the standard Nova microcode is running in 
         the Emulator task.
         
Stop:    Stops the CPU.

Reset:   Resets the Alto system.

5.2 Microcode Source Pane
-------------------------

The pane in the upper left of the debugger window shows the microcode listings
for ROM0, ROM1, and RAM0-RAM2.  The listings for ROM0 and ROM1 are derived from the
original source code listings.  The listing for the RAM banks is automatically 
disassembled from the contents of control RAM (and is generally more annoying
to read.)

ROM0 contains the listing for the main microcode ROMs -- this 1K of ROM contains
code for all of the microcode tasks (Emulator, Disk Sector, Ethernet, Memory
Refresh, Display Word, Cursor, Display Horizontal, Display Vertical, Parity, and
Disk Word).  The source code for each task is highlighted in a different color
to make task-specific code easy to differentiate.

ROM1 contains the listing for the Mesa 5.0 microcode ROMs.


5.3 Memory Pane
---------------

The pane near the top-middle (labeled "System Memory") shows a view into the main
memory of the Alto, providing address/data and a machine-generated disassembly 
of Alto (Nova) instructions.


5.4 Breakpoints
---------------

Breakpoints can be set on either microcode or Nova code by checking the box
in the "B" column next to the instruction.  Unchecking the box will remove the
breakpoint.

Nova code breakpoints will only work if the standard Nova microcode is running
in the Emulator task.


5.5 Everything Else
-------------------

The other panes in the debugger are:

Tasks:   Shows the current microcode task status.  The "T" column indicates the
         task name, "S" indicates the status ("W"akeup and "R"unning), and the
         "uPC" column indicates the micro-PC for the corresponding task.  There
         are 16 possible tasks, not all are used on most Altos.
         
CPU Registers:
         Shows the CPU L, M and T registers as well as ALU and memory registers.

General Registers:
         Shows the contents of the 32 R and 32 S registers (in octal).  
		 (The extra 7 sets of R and S registers on 3K CRAM machines are not yet
		 displayed.)
         
Reserved Memory:
         Shows the contents of most "well known" memory locations.  See the
         Alto HW Reference manual (link in Sectoin 3.1.2) for what these mean.

         
6.0 Known Issues
================

At the moment, the following issues are known and being worked on.  If you find
an issue not listed here, see section 7.0 to report a new bug.

- Smalltalk-80 does not run.


7.0 Reporting Bugs
==================

If you believe you have found a new issue (or have a feature request) please
send an e-mail to joshd@livingcomputers.org with a subject line starting
with "ContrAlto Bug".

When you send a report, please be as specific and detailed as possible:
- What issue are you seeing?
- What Alto software are you running?
- What operating system are you running ContrAlto on?
- What are the exact steps needed to reproduce the issue?

The more detailed the bug report, the more possible it is for me to track down
the cause.


8.0 Source Code
===============

The complete source code is available under the GPLv3 license on GitHub at:

https://github.com/livingcomputermuseum/ContrAlto

Contributions are welcome!


9.0 Thanks and Acknowledgements
===============================

ContrAlto would not have been possible without the amazing preservation work of 
the Computer History Museum.


10.0 Change History
===================

V1.1
----
- A few minor performance tweaks, adding to a 10-15% speed increase.
- Switched back to targeting .NET 4.5.3 rather than 4.6; this works better under Mono
  and avoids odd issues on Windows machines running pre-4.6 frameworks.
- Microcode disassembly improved slightly, annotated microcode source updated.
- Nova disassembler now handles BRI, DIR, EIR, DIRS instructions rather than treating
  them all as TRAPs.
- Fixed serious bugs in memory state machine, BravoX now runs.
- Fixed minor bug in Constant ROM selection.
- Raw Ethernet packets sent as broadcasts (matching IFS encapsulation behavior)

V1.0
----
Initial release.
