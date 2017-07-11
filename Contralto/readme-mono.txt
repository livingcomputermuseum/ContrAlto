Readme-mono.txt for Contralto v1.2.1:

1. Introduction and Overview
============================

ContrAlto aspires to be a faithful emulation of the Xerox Alto II series of 
pioneering graphical workstations developed at Xerox PARC in 1973.

This document covers installation and use on Unix and Mac OS platforms under
the Mono runtime.  See readme.txt for instructions for Windows platforms.

1.1 What's Emulated
-------------------

ContrAlto currently emulates the following Alto hardware:
   - Alto I and Alto II XM CPU
   - Microcode RAM (in 1K RAM, 1K RAM/2K ROM, or 3K RAM configurations)
   - 256KW of main memory (in 64KW banks)
   - Two Diablo Model 31 or 44 drives
   - Ethernet (encapsulated in UDP datagrams on the host machine)
   - Standard Keyboard/Mouse/Video
   - Audio DAC (used with the Smalltalk Music System)
   - The Orbit raster hardware, Dover Raster Output Scanner and Dover print
     engine, which provides 384dpi print output (currently PDF only)

1.2 What's Not
--------------

At this time, ContrAlto does not support more exotic hardware such as Trident 
disks, printers or audio using the utility port, or the keyset input device.

The Audio DAC is technically emulated, but output is not connected to an audio
device on non-Windows platforms at this time.


2.0 Requirements
================

ContrAlto is a .NET application and will run under Mono (http://www.mono-project.com/) 
on Unix and MacOS.

Ensure you have the latest Mono environment installed on your system.  ContrAlto was
developed and tested on 5.0.1 and while it will work on earlier versions, it has not
been well tested on them.  You can find download links and instructions at
http://www.mono-project.com/download/.

Additionally, ContrAlto relies on SDL (Simple Directmedia Layer) 2.0 for the Alto's
display, keyboard and mouse.  On OS X, the native SDL library is included with the
ContrAlto archive.  On Linux, you will want to ensure that the SDL 2.0 libraries 
built for your distribution are installed.

A three-button mouse is essential for using most Alto software.  On most mice,
the mousewheel can be clicked to provide the third (middle) button.  Laptops
with trackpads may have configuration options to simulate multiple buttons but
will likely be clumsy to use.


3.0 Getting Started
===================

Installation of ContrAlto on Unix and Mac OS machines is straightforward --
unpack the ContrAlto-Mono.zip archive to a directory on your machine (a
subdirectory of your home directory will work fine).  Ensure you've installed
the prerequisites outlined in Section 2.0 and you should be good to go.


3.1 Starting the Alto
=====================

To launch ContrAlto, run "mono ContrAlto.exe" from a terminal window.  A window 
for the Alto's display labeled "ContrAlto" will appear -- this is where you will 
interact with your virtual Alto.  (Note that on the first run of ContrAlto, it may
take several seconds for this window to appear.)  The terminal window will provide 
you with a command-line console for configuring and controlling the emulated Alto.

On a real Alto, the system is booted by loading a 14" disk pack into the front
of a Diablo 31 drive, waiting for it to spin up for 20 seconds and then
pressing the "Reset" button on the back of the keyboard.

Booting an emulated Alto under ContrAlto is slightly less time-consuming.
To load a disk pack into the virtual Diablo drive, you use the "Load Disk"
command at the console -- this allows selection of the disk image (effectively 
a "virtual disk pack") to be loaded.  Disk images are not included with ContrAlto 
but may be found in various places on the Internet -- see Section 3.1.3 for details.
For example "Load Disk 0 Smalltalk.dsk" will load a disk image named Smalltalk.dsk
into drive 0.

Once the pack has been loaded, you can start the Alto by using the "Start" command
at the console.  The display will turn white and after 5-10 seconds a mouse cursor 
will appear, followed shortly by the banner of the Xerox Alto Executive.  
Congratulations, your Alto is now running!  Click on the display window to start 
interacting with it using the keyboard and mouse (and if you need your mouse back 
for other things, press either "Alt" key on your keyboard.)  See Section 3.1 for 
details on using ContrAlto.


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
not be able to use the mouse for other programs running on your computer.  To release 
ContrAlto's control, press either "Alt" key (the "Option" key on Macintosh)
on your keyboard.  Mouse movements will return to normal, and you will not be 
able to control the Alto's mouse or keyboard until you click on the ContrAlto 
display window again.

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

Disk images can be loaded and unloaded via the "Load Disk" command.  (See Section
5 for details on this and other commands.)

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

The system can be started at any time by using the "Start" command, though
in general having a pack image loaded first is a good idea.  Similarly, the
"Reset" command will reset the Alto.

You can shut down the Alto by closing the ContrAlto window or using the
"Quit" console command.  Either will commit disk changes made to the 
currently loaded disks back to the disk image files before exiting.  
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

ContrAlto can be configured through the use of configuration files.  These
are simple text files with sets of parameters and their values in the form:

  ParameterName = Value

By default, ContrAlto looks for configuration data in a file named
ContrAlto.cfg.  Alternate configuration files can be specified as a 
command-line argument at startup via:
  mono ContrAlto.exe <configuration file>

An example configuration file looks something like:

    # contralto.cfg:
    #
    # This file contains configuration parameters for ContrAlto.
    # All integers are specified in octal.
    #

    # System configuration
    SystemType = TwoKRom
    HostAddress = 42

    # Disk configuration
    Disk0Image = alto.dsk
    Disk1Image = bcpl.dsk

    # Host networking configuration
    HostPacketInterfaceType = UDPEncapsulation
    HostPacketInterfaceName = eth0

    # Emulation Options
    BootAddress = 0
    BootFile = 0
    AlternateBootType = Ethernet

    # Printing options
    EnablePrinting = true
    PrintOutputPath = .
    ReversePageOrder = true


The following parameters are configurable:

SystemType:  Selects the type of Alto system to emulate.  One of:
    - AltoI     : an Alto I, with 64KW memory, 1K ROM, and 1K CRAM
    - OneKRom   : an Alto II XM system with 1K ROM, 1K CRAM
    - TwoKRom   : an Alto II XM system with 2K ROM, 1K CRAM
    - ThreeKRam : an Alto II XM system with 1K ROM, 3K CRAM
    The default is TwoKRom.

HostAddress:  Specifies the Alto's Ethernet address (in octal).  Any value
              between 1 and 376 is allowed.

Drive0Image and Drive1Image:  Specifies a disk image to be loaded into the 
              specified drive.  These parameters are optional.

HostPacketInterfaceType:  Specifies the type of interface to be used on the 
              host for Ethernet emulation.  One of:
    - UDPEncapsulation: Transmits Alto Ethernet packets over UDP broadcasts
    - EthernetEncapsulation: Transmits Alto Ethernet packets over raw Ethernet packets.
            (See Section 4.1 for configuration details)
    - None: No packet encapsulation.

HostPacketInterfaceName:  Specifies the name of the host network interface
              to use for Ethernet emulation.  (e.g. "eth0"")  If no network
              interface is to be used, this parameter can be omitted.

BootAddress: The address to use with a Keyboard Disk Boot (See section 5.0)
BootFile:    The file number to use with a Keyboard Net Boot (again, Section 5.0)
AlternateBootType:  The type of boot to default to (Section 5.0)

EnablePrinting:     Enables or disables printing via the emulated Orbit / Dover interface.

PrintOutputPath:    Specifies the folder that output PDFs are written to. When the  Alto 
                    prints a new document, a new PDF file will be created in this
                    directory containing the printer's output.

ReversePageOrder:   Controls the order in which pages are written to the PDF -- due to 
                    the way the original Dover printer worked, most Alto software printed 
                    documents in reverse order (i.e. the last page printed first) so 
                    that the pages didn't have to be reshuffled when picked up from the
                    tray.  By default, leaving this box checked is probably what you want, 
                    but if your documents come out backwards, uncheck it.


4.1 Ethernet Encapsulation Setup
================================

Encapsulation of Alto (3mbit) Ethernet packets in Ethernet broadcasts is supported
on Linux and OS X using libpcap.  While it is tested and works well, it may require some extra
configuration on your system before it will work properly for you.

- Ensure that the latest libpcap libraries are installed.  These should be present by default
  on OS X; on other platforms check your distribution's documentation for details.

- On many systems, libpcap requires additional privileges in order to capture packets.
  You can either run ContrAlto as root, or setuid ContrAlto to root.  Depending on
  your operating system, there may be other options.  See (for example)
  http://www.tcpdump.org/manpages/pcap.3pcap.html.

- You may need to modify SharpPcap.dll.config to point to the specific libpcap 
  version you have installed on your system.

5.0 Console Interface
=====================

After startup, you will be at the ContrAlto console prompt (a '>' character).

ContrAlto provides a somewhat-context-sensitive input line.  Press TAB at any
point during input to see possible completions for the command you're entering.

The "show commands" command provides a brief synopsis of available commands,
these are described in greater detail in Section 5.1.

All numeric arguments are specified in Octal by default.  A number may be
prefixed with 'b', 'o', 'd', or 'x' to specify binary, octal, decimal or
hexadecimal, respectively.

All numeric outputs are presented in Octal.

At any point while the emulator is running is running the console is active
and commands may be entered.


5.1 Console Commands
--------------------

Quit - Exits ContrAlto.  Any modifications to loaded disk images are saved.

Start - Starts the emulated Alto system.

Stop - Stops the emulated Alto.

Reset - Resets the emulated Alto.

Start With Keyboard Disk Boot - Starts the emulated Alto with the keyboard disk boot address specified
                                either in the configuration file or by the Set Keyboard Disk Boot Address
                                command.

Start With Keyboard Net Boot - Starts the emulated Alto with the keyboard ethernet boot number specified
                               either in the configuration file or by the Set Keyboard Net Boot File
                               command.

Load Disk <drive> <path> - Loads the specified drive (0 or 1) with the requested disk image.

Unload Disk <drive> - Unloads the specified drive (0 or 1).  Changes to disk contents are saved.

Show Disk <drive> - Displays the currently loaded image for the specified drive (0 or 1).

Show System Type - Displays the Alto system type as configured by the configuration file.

Set Ethernet Address - Sets the Alto's host Ethernet address.  Values between 1 and 376 (octal) are
                       allowed.

Show Ethernet Address - Displays the Alto's host Ethernet address.

Show Host Network Interface Name - Displays the host network interface used for Ethernet emulation.

Show Host Network Interface Type - Displays the host network interface type.

Set Keyboard Net Boot File - Sets the boot file used for net booting.  Values between 0 and 177777
                             are allowed.

Set Keyboard Disk Boot Address - Sets the boot address used for disk booting.  Values between 0 and 
                                 177777 are allowed.

Show Commands - Shows debugger commands and their descriptions.


5.2 Alternate ("keyboard") Boots
--------------------------------

The Alto allowed the specification of alternate boot addresses by holding down
a set of keys on the keyboard while hitting the "reset" switch on the back of
the keyboard.  Since this would be difficult to pull off by hand on the emulator
due to the UI involved, ContrAlto provides a set of console commands to select the
alternate address to boot.  When the "Start With Keyboard Net|Disk Boot" command is
used, the system will be started (or restarted) with these keys held down on
your behalf.

Ethernet booting will only work if another host on the network is providing boot services.

The "Set Keyboard Disk Boot Address" command accepts a 16-bit octal value (from 0 to 177777)
specifying the address to be booted from disk.

The "Set Keyboard Net Boot Address" command accepts a 16-bit octal value (from 0 to 177777)
specifying the file to be net booted.
         

6.0 Known Issues
================

At the moment, the following issues are known and being worked on.  If you find
an issue not listed here, see section 7.0 to report a new bug.

- Audio is not available on Unix / OS X.


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

Ethernet encapsulation is provided courtesy of SharpPcap, a WinPcap/LibPcap wrapper.
See: https://github.com/chmorgan/sharppcap.

Audio output and capture on Windows is provided using the NAudio libraries, see:
https://github.com/naudio/NAudio.

On Unix and OS X, display and keyboard/mouse input is provided through SDL, see:
https://www.libsdl.org/ and is accessed using the SDL2# wrapper, see:
https://github.com/flibitijibibo/SDL2-CS.

PDF generation is provided by the iTextSharp library, see: https://github.com/itext.

10.0 Change History
===================

V1.2.1
------
- Completed implementation of Orbit, Dover ROS and Dover print engine.
- Bugfixes to memory state machine around overlapped double-word reads/writes.
  Smalltalk-80 now runs, as does Spruce.

V1.2
----
- First release supporting Unix / OS X
- Audio DAC for use with Smalltalk Music system implemented
- Initial implementation of Orbit rasterization device; Dover ROS is implemented
  but not working properly.
- Added ability to load a configuration file at startup
- Switched to cross-platform SharpPcap library for Ethernet encapsulation.

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
