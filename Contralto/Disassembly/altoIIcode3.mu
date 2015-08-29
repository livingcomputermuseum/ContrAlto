;	A L T O I I C O D E 3 . M U
; Copyright Xerox Corporation 1979

;***Derived from ALTOIICODE2.MU, as last modified by
;***Tobol, August 5, 1976 12:13 PM -- fix DIOG2 bug
;***modified by Ingalls, September 6, 1977
; BitBLT fixed (LREG bug) and extended for new memory
;***modified by Boggs and Taft September 15, 1977  10:10 PM
; Modified MRT to refresh 16K chips and added XMSTA and XMLDA.
; Fixed two bugs in DEXCH and a bug in the interval timer.
; Moved symbol and constant definitions into AltoConsts23.mu.
; MRT split and moved into two 'get' files.
;***modified by Boggs and Taft November 21, 1977  5:10 PM
; Fixed a bug in the Ethernet input main loop.
;***modified by Boggs November 28, 1977  3:53 PM
; Mess with the information returned by VERS
;***modified by Dersch, August 26, 2015 4:04 PM
; Annotated with PROM addresses and Tasks for use in Contralto

;Get the symbol and constant definitions
#AltoConsts23.mu;

;LABEL PREDEFINITIONS

;The reset locations of the tasks:

!17,20,NOVEM,,,,KSEC,,,EREST,MRT,DWT,CURT,DHT,DVT,PART,KWDX,;

;Locations which may need to be accessible from the Ram, or Ram
;  locations which are accessed from the Rom (TRAP1):
!37,20,START,RAMRET,RAMCYCX,,,,,,,,,,,,,TRAP1;

;Macro-op dispatch table:
!37,20,DOINS,DOIND,EMCYCLE,NOPAR,JSRII,U5,U6,U7,,,,,,,RAMTRAP,TRAP;

;Parameterless macro-op sub-table:
!37,40,DIR,EIR,BRI,RCLK,SIO,BLT,BLKS,SIT,JMPR,RDRM,WTRM,DIRS,VERS,DREAD,DWRITE,DEXCH,MUL,DIV,DIOG1,DIOG2,BITBLT,XMLDA,XMSTA,,,,,,,,,;

;Cycle dispatch table:
!37,20,L0,L1,L2,L3,L4,L5,L6,L7,L8,R7,R6,R5,R4,R3X,R2X,R1X;

;some global R-Registers
$NWW		$R4;		State of interrupt system
$R37		$R37;		Used by MRT, interval timer and EIA
$MTEMP		$R25;		Public temporary R-Register


;The Display Controller

; its R-Registers:
$CBA		$R22;
$AECL		$R23;
$SLC		$R24;
$HTAB		$R26;
$YPOS		$R27;
$DWA		$R30;
$CURX		$R20;
$CURDATA	$R21;

; its task specific functions:
$EVENFIELD	$L024010,000000,000000; F2 = 10 DHT DVT
$SETMODE	$L024011,000000,000000; F2 = 11 DHT
$DDR		$L026010,000000,124100; F2 = 10 DWT

!1,2,DVT1,DVT11;
!1,2,MOREB,NOMORE;
!1,2,NORMX,HALFX;
!1,2,NODD,NEVEN;
!1,2,DHT0,DHT1;
!1,2,NORMODE,HALFMODE;
!1,2,DWTZ,DWTY;
!1,2,DOTAB,NOTAB;
!1,2,XNOMORE,DOMORE;

;Display Vertical Task

DV00014> DVT:	MAR<- L<- DASTART+1;
DV00001>	CBA<- L, L<- 0;
DV00005>	CURDATA<- L;
DV00006>	SLC<- L;
DV00017>	T<- MD;			CAUSE A VERTICAL FIELD INTERRUPT
DV00023>	L<- NWW OR T;
DV00036>	MAR<- CURLOC;		SET UP THE CURSOR
DV00046>	NWW<- L, T<- 0-1;
DV00047>	L<- MD XOR T;		HARDWARE EXPECTS X COMPLEMENTED
DV00050>	T<- MD, EVENFIELD;
DV00051>	CURX<- L, :DVT1;

DV00002> DVT1:	L<- BIAS-T-1, TASK, :DVT2;	BIAS THE Y COORDINATE 
DV00003> DVT11:	L<- BIAS-T, TASK;

DV00052> DVT2:	YPOS<- L, :DVT;

;Display Horizontal Task.
;11 cycles if no block change, 17 if new control block.

DH00013> DHT:	MAR<- CBA-1;
DH00053>	L<- SLC -1, BUS=0;
DH00054>	SLC<- L, :DHT0;

DH00032> DHT0:	T<- 37400;		MORE TO DO IN THIS BLOCK
DH00055>	SINK<- MD;
DH00056>	L<- T<- MD AND T, SETMODE;
DH00057>	HTAB<- L LCY 8, :NORMODE;

DH00034> NORMODE:L<- T<- 377 . T;
DH00070>	AECL<- L, :REST;	

DH00035> HALFMODE: L<- T<-  377 . T;
DH00071>	AECL<- L, :REST, T<- 0;

DH00072> REST:	L<- DWA + T,TASK;	INCREMENT DWA BY 0 OR NWRDS
DH00073> NDNX:	DWA<- L, :DHT;

DH00033> DHT1:	L<- T<- MD+1, BUS=0;
DH00074>	CBA<- L, MAR<- T, :MOREB;

DH00025> NOMORE:	BLOCK, :DNX;
DH00024> MOREB:	T<- 37400;
DH00075>	L<- T<- MD AND T, SETMODE;
DH00127>	MAR<- CBA+1, :NORMX, EVENFIELD;

DH00026> NORMX:	HTAB<- L LCY 8, :NODD;
DH00027> HALFX:	HTAB_ L LCY 8, :NEVEN;

DH00030> NODD:	L<-T<- 377 . T;
DH00130>	AECL<- L, :XREST;	ODD FIELD, FULL RESOLUTION

DH00031> NEVEN:	L_ 377 AND T;		EVEN FIELD OR HALF RESOLUTION
DH00131>	AECL<-L, T<-0;

DH00132> XREST:	L<- MD+T;
DH00133>	T_MD-1;
DH00134> DNX:	DWA<-L, L<-T, TASK;
DH00135>	SLC<-L, :DHT;

;Display Word Task

DW00011> DWT:	T<- DWA;
DW00136>	T<- -3+T+1;
DW00137>	L<- AECL+T,BUS=0,TASK;	AECL CONTAINS NWRDS AT THIS TIME
DW00140>	AECL<-L, :DWTZ;

DW00041> DWTY:	BLOCK;
DW00141>	TASK, :DWTF;

DW00040> DWTZ:	L<-HTAB-1, BUS=0,TASK;
DW00142>	HTAB<-L, :DOTAB;

DW00042> DOTAB:	DDR<-0, :DWTZ;
DW00043> NOTAB:	MAR_T_DWA;
DW00143>	L<-AECL-T-1;
DW00144>	ALUCY, L<-2+T;
DW00145>	DWA<-L, :XNOMORE;

DW00045> DOMORE:	DDR<-MD, TASK;
DW00146>	DDR_MD, :NOTAB;

DW00144> XNOMORE:DDR<- MD, BLOCK;
DW00147>	DDR<- MD, TASK;

DW00150> DWTF:	:DWT;

;Alto Ethernet Microcode, Version III, Boggs and Metcalfe

;4-way branches using NEXT6 and NEXT7
!17,20,EIFB00,EODOK,EOEOK,ENOCMD,EIFB01,EODPST,EOEPST,EOREST,EIFB10,EODCOL,EOECOL,EIREST,EIFB11,EODUGH,EOEUGH,ERBRES;

;2-way branches using NEXT7
;EOCDW1, EOCDWX, and EIGO are all related.  Be careful!
!7,10,,EIFOK,,EOCDW1,,EIFBAD,EOCDWX,EIGO;

;Miscellaneous address constraints
!7,10,,EOCDW0,EODATA,EIDFUL,EIDZ4,EOCDRS,EIDATA,EPOST;
!7,10,,EIDOK,,,EIDMOR,EIDPST;
!1,1,EIFB1;
!1,1,EIFRST;

;2-way branches using NEXT9
!1,2,EOINPR,EOINPN;
!1,2,EODMOR,EODEND;
!1,2,EOLDOK,EOLDBD;
!1,2,EIFCHK,EIFPRM;
!1,2,EOCDWT,EOCDGO;
!1,2,ECNTOK,ECNTZR;
!1,2,EIFIGN,EISET;
!1,2,EIFNBC,EIFBC;

;R Memory Locations

$ECNTR	$R12;	Remaining words in buffer
$EPNTR	$R13;	points BEFORE next word in buffer

;Ethernet microcode Status codes

$ESIDON	$377;	Input Done
$ESODON	$777;	Output Done
$ESIFUL	$1377;	Input Buffer full - words lost from tail of packet
$ESLOAD	$1777;	Load location overflowed
$ESCZER	$2377;	Zero word count for input or output command
$ESABRT	$2777;	Abort - usually caused by reset command
$ESNEVR	$3377;	Never Happen - Very bad if it does

;Main memory locations in page 1 reserved for Ethernet

$EPLOC	$600;	Post location
$EBLOC	$601;	Interrupt bit mask

$EELOC	$602;	Ending count location
$ELLOC	$603;	Load location

$EICLOC	$604;	Input buffer Count
$EIPLOC	$605;	Input buffer Pointer

$EOCLOC	$606;	Output buffer Count
$EOPLOC	$607;	Output buffer Pointer

$EHLOC	$610;	Host Address

;Function Definitions

$EIDFCT	$L000000,014004,000100;	BS = 4,	 Input data
$EILFCT	$L016013,070013,000100;	F1 = 13, Input Look
$EPFCT	$L016014,070014,000100;	F1 = 14, Post
$EWFCT	$L016015,000000,000000;	F1 = 15, Wake-Up

$EODFCT	$L026010,000000,124000;	F2 = 10, Output data
$EOSFCT	$L024011,000000,000000;	F2 = 11, Start output
$ERBFCT	$L024012,000000,000000;	F2 = 12, Rest branch
$EEFCT	$L024013,000000,000000;	F2 = 13, End of output
$EBFCT	$L024014,000000,000000;	F2 = 14, Branch
$ECBFCT	$L024015,000000,000000;	F2 = 15, Countdown branch
$EISFCT	$L024016,000000,000000;	F2 = 16, Start input

; - Whenever a label has a pending branch, the list of possible
;   destination addresses is shown in brackets in the comment field.
; - Special functions are explained in a comment near their first use.
; - To avoid naming conflicts, all labels and special functions
;   have "E" as the first letter.

;Top of Ethernet Task loop

;Ether Rest Branch Function - ERBFCT
;merge ICMD and OCMD Flip Flops into NEXT6 and NEXT7
;ICMD and OCMD are set from AC0 [14:15] by the SIO instruction
;	00  neither 
;	01  OCMD - Start output
;	10  ICMD - Start input
;	11  Both - Reset interface

;in preparation for a hack at EIREST, zero EPNTR

EN00007> EREST:	L<- 0,ERBFCT;		What's happening ?
EN00152>	EPNTR<- L,:ENOCMD;	[ENOCMD,EOREST,EIREST,ERBRES]

EN00203> ENOCMD:	L<- ESNEVR,:EPOST;	Shouldn't happen
ERBRES:	L_ ESABRT,:EPOST;	Reset Command

;Post status and halt.  Microcode status in L.
;Put microstatus,,hardstatus in EPLOC, merge c(EBLOC) into NWW.
;Note that we write EPLOC and read EBLOC in one operation

;Ether Post Function - EPFCT.  Gate the hardware status
;(LOW TRUE) to Bus [10:15], reset interface.

EN00237> EPOST:	MAR<- EELOC;
EN00220>	EPNTR<- L,TASK;		Save microcode status in EPNTR
EN00222>	MD<- ECNTR;		Save ending count

EN00224>	MAR<- EPLOC;		double word reference
EN00230>	T<- NWW;
EN00240>	MD<- EPNTR,EPFCT;	BUS AND EPNTR with Status
EN00260>	L<- MD OR T,TASK;	NWW OR c(EBLOC)
EN00261>	NWW<- L,:EREST;		Done.  Wait for next command

;This is a subroutine called from both input and output (EOCDGO
;and EISET).  The return address is determined by testing ECBFCT,
;which will branch if the buffer has any words in it, which can
;only happen during input.

ESETUP:	NOP;
	L_ MD,BUS=0;		check for zero length
	T_ MD-1,:ECNTOK;	[ECNTOK,ECNTZR] start-1

ECNTZR:	L_ ESCZER,:EPOST;	Zero word count.  Abort

;Ether Countdown Branch Function - ECBFCT.
;NEXT7 = Interface buffer not empty.

ECNTOK:	ECNTR_ L,L_ T,ECBFCT,TASK;
	EPNTR_ L,:EODATA;	[EODATA,EIDATA]

;Ethernet Input

;It turns out that starting the receiver for the first time and
;restarting it after ignoring a packet do the same things.

EIREST:	:EIFIGN;		Hack

;Address filtering code.

;When the first word of a packet is available in the interface
;buffer, a wakeup request is generated.  The microcode then
;decides whether to accept the packet.  Decision must be reached
;before the buffer overflows, within about 14*5.44 usec.
;if EHLOC is zero, machine is 'promiscuous' - accept all packets
;if destination byte is zero, it is a 'broadcast' packet, accept.
;if destination byte equals EHLOC, packet is for us, accept.

;EIFRST is really a subroutine that can be called from EIREST
;or from EIGO, output countdown wait.  If a packet is ignored
;and EPNTR is zero, EIFRST loops back and waits for more
;packets, else it returns to the countdown code.

;Ether Branch Function - EBFCT
;NEXT7 = IDL % OCMD % ICMD % OUTGONE % INGONE (also known as POST)
;NEXT6 = COLLision - Can't happen during input

EIFRST:	MAR_ EHLOC;		Get Ethernet address
	T_ 377,EBFCT;		What's happening?
	L_ MD AND T,BUS=0,:EIFOK;[EIFOK,EIFBAD] promiscuous?

EIFOK:	MTEMP_ LLCY8,:EIFCHK;	[EIFCHK,EIFPRM] Data wakeup

EIFBAD:	ERBFCT,TASK,:EIFB1;	[EIFB1] POST wakeup; xCMD FF set?
EIFB1:	:EIFB00;		[EIFB00,EIFB01,EIFB10,EIFB11]

EIFB00:	:EIFIGN;		IDL or INGONE, restart rcvr
EIFB01:	L_ ESABRT,:EPOST;	OCMD, abort
EIFB10:	L_ ESABRT,:EPOST;	ICMD, abort
EIFB11:	L_ ESABRT,:EPOST;	ICMD and OCMD, abort

EIFPRM:	TASK,:EIFBC;		Promiscuous. Accept

;Ether Look Function - EILFCT.  Gate the first word of the 
;data buffer to the bus, but do not increment the read pointer.

EIFCHK:	L_ T_ 177400,EILFCT;	Mask off src addr byte (BUS AND)
	L_ MTEMP-T,SH=0;	Broadcast?
	SH=0,TASK,:EIFNBC;	[EIFNBC,EIFBC] Our Address?

EIFNBC:	:EIFIGN;		[EIFIGN,EISET]

EIFBC:	:EISET;			[EISET] Enter input main loop

;Ether Input Start Function - EISFCT.  Start receiver.  Interface
;will generate a data wakeup when the first word of the next
;packet arrives, ignoring any packet currently passing.

EIFIGN:	SINK_ EPNTR,BUS=0,EPFCT;Reset; Called from output?
	EISFCT,TASK,:EOCDWX;	[EOCDWX,EIGO] Restart rcvr

EOCDWX:	EWFCT,:EOCDWT;		Return to countdown wait loop

EISET:	MAR_ EICLOC,:ESETUP;	Double word reference

;Input Main Loop

;Ether Input Data Function - EIDFCT.  Gate a word of data to
;the bus from the interface data buffer, increment the read ptr.
;		* * * * * W A R N I N G * * * * *
;The delay from decoding EIDFCT to gating data to the bus is
;marginal.  Some logic in the interface detects the situation
;(which only happens occasionally) and stops SysClk for one cycle.
;Since memory data must be available during cycle 4, and SysClk
;may stop for one cycle, this means that the MD_ EIDFCT must
;happen in cycle 3.  There is a bug in this logic which occasionally
;stops the clock in the instruction following the EIDFCT, so
;the EIDFCT instruction should not be the last one of the task,
;or it may screw up someone else (such as RDRAM).

;EIDOK, EIDMOR, and EIDPST must have address bits in the pattern:
;xxx1   xxx4        xxx5
;ECBFCT is used to force an unconditional branch on NEXT7

EIDATA:	T_ ECNTR-1, BUS=0;
	MAR_ L_ EPNTR+1, EBFCT;	[EIDMOR,EIDPST] What's happening
EIDMOR:	EPNTR_ L, L_ T, ECBFCT;	[EIDOK,EIDPST] Guaranteed to branch
EIDOK:	MD_ EIDFCT, TASK;	[EIDZ4] Read a word from the interface
EIDZ4:	ECNTR_ L, :EIDATA;

; We get to EIDPST for one of two reasons:
; (1) The buffer is full.  In this case, an EBFCT (NEXT[7]) is pending.
;     We want to post "full" if this is a normal data wakeup (no branch)
;     but just "input done" if hardware input terminated (branch).
; (2) Hardware input terminated while the buffer was not full.
;     In this case, an unconditional branch on NEXT[7] is pending, so
;     we always terminate with "input done".
EIDPST:	L_ ESIDON, :EIDFUL;	[EIDFUL,EPOST] Presumed to be INGONE
EIDFUL:	L_ ESIFUL, :EPOST;	Input buffer overrun

;Ethernet output

;It is possible to get here due to a collision.  If a collision
;happened, the interface was reset (EPFCT) to shut off the
;transmitter.  EOSFCT is issued to guarantee more wakeups while
;generating the countdown.  When this is done, the interface is
;again reset, without really doing an output.

EN00207> EOREST:	MAR<- ELLOC;		Get load
EN00274>	L<- R37;			Use clock as random # gen
EN00275>	EPNTR<- LLSH1;		Use bits [2:9]
EN00276>	L<- MD,EOSFCT;		L<- current load
EN00277>	SH<0,ECNTR<- L;		Overflowed?
EN00300>	MTEMP<- LLSH1,:EOLDOK;	[EOLDOK,EOLDBD]

EOLDBD:	L_ ESLOAD,:EPOST;	Load overlow

EOLDOK:	L_ MTEMP+1;		Write updated load
	MAR_ ELLOC;
	MTEMP_ L,TASK;
	MD_ MTEMP,:EORST1;	New load = (old lshift 1) + 1

EORST1:	L_ EPNTR;		Continue making random #
	EPNTR_ LRSH1;
	T_ 377;
	L_ EPNTR AND T,TASK;
	EPNTR_ L,:EORST2;

;At this point, EPNTR has 0,,random number, ENCTR has old load.

EORST2:	MAR_ EICLOC;		Has an input buffer been set up?
	T_ ECNTR;
	L_ EPNTR AND T;		L_ Random & Load
	SINK_ MD,BUS=0;
	ECNTR_ L,SH=0,EPFCT,:EOINPR;[EOINPR,EOINPN] 

EOINPR:	EISFCT,:EOCDWT;		[EOCDWT,EOCDGO] Enable in under out

EOINPN:	:EOCDWT;		[EOCDWT,EOCDGO] No input.

;Countdown wait loop.  MRT will generate a wakeup every
;37 usec which will decrement ECNTR.  When it is zero, start
;the transmitter.

;Ether Wake Function - EWFCT.  Sets a flip flop which will cause
;a wakeup to this task the next time MRT wakes up (every 37 usec).
;Wakeup is cleared when Ether task next runs.  EWFCT must be
;issued in the instruction AFTER a task.

EOCDWT:	L_ 177400,EBFCT;	What's happening?
	EPNTR_ L,ECBFCT,:EOCDW0;[EOCDW0,EOCDRS] Packet coming in?
EOCDW0:	L_ ECNTR-1,BUS=0,TASK,:EOCDW1; [EOCDW1,EIGO]
EOCDW1:	ECNTR_ L,EWFCT,:EOCDWT;	[EOCDWT,EOCDGO]

EOCDRS:	L_ ESABRT,:EPOST;	[EPOST] POST event

EIGO:	:EIFRST;		[EIFRST] Input under output

;Output main loop setup

EOCDGO:	MAR_ EOCLOC;		Double word reference
	EPFCT;			Reset interface
	EOSFCT,:ESETUP;		Start Transmitter

;Ether Output Start Function - EOSFCT.  The interface will generate
;a burst of data requests until the interface buffer is full or the
;memory buffer is empty, wait for silence on the Ether, and begin
;transmitting.  Thereafter it will request a word every 5.44 us.

;Ether Output Data Function - EODFCT.  Copy the bus into the
;interface data buffer, increment the write pointer, clears wakeup
;request if the buffer is now nearly full (one slot available).

;Output main loop

EODATA:	L_ MAR_ EPNTR+1,EBFCT;	What's happening?
	T_ ECNTR-1,BUS=0,:EODOK; [EODOK,EODPST,EODCOL,EODUGH]
EODOK:	EPNTR_ L,L_ T,:EODMOR;	[EODMOR,EODEND]
EODMOR:	ECNTR_ L,TASK;
	EODFCT_ MD,:EODATA;	Output word to transmitter

EODPST:	L_ ESABRT,:EPOST;	[EPOST] POST event

EODCOL:	EPFCT,:EOREST;		[EOREST] Collision

EODUGH:	L_ ESABRT,:EPOST;	[EPOST] POST + Collision

;Ether EOT Function - EEFCT.  Stop generating output data wakeups,
;the interface has all of the packet.  When the data buffer runs
;dry, the interface will append the CRC and then generate an
;OUTGONE post wakeup.

EODEND:	EEFCT;			Disable data wakeups
	TASK;			Wait for EEFCT to take
	:EOEOT;			Wait for Outgone

;Output completion.  We are waiting for the interface buffer to
;empty, and the interface to generate an OUTGONE Post wakeup.

EOEOT:	EBFCT;			What's happening?
	:EOEOK;			[EOEOK,EOEPST,EOECOL,EOEUGH]

EOEOK:	L_ ESNEVR,:EPOST;	Runaway Transmitter. Never Never.

EOEPST:	L_ ESODON,:EPOST;	POST event.  Output done

EOECOL:	EPFCT,:EOREST;		Collision

EOEUGH:	L_ ESABRT,:EPOST;	POST + Collision


;Memory Refresh Task,
;Mouse Handler,
;EIA Handler,
;Interval Timer,
;Calender Clock, and
;part of the cursor.

!17,20,TX0,TX6,TX3,TX2,TX8,TX5,TX1,TX7,TX4,,,,,,,;
!1,2,DOTIMER,NOTIMER;
!1,2,NOTIMERINT,TIMERINT;
!1,2,DOCUR,NOCUR;
!1,2,SHOWC,WAITC;
!1,2,SPCHK,NOSPCHK;

!1,2,NOCLK,CLOCK;
!1,1,MRTLAST;
!1,2,CNOTLAST,CLAST;

$CLOCKTEMP	$R11;
$REFIIMSK	$7777;

;		* * * A T T E N T I O N * * *
;There are two versions of the Memory refresh code:
;	AltoIIMRT4K.mu 		for refreshing 4K chips
;	AltoIIMRT16K.mu		for refreshing 16K chips
;You must name one or the other 'AltoIIMRT.mu'.
;I suggest the following convention for naming the resulting .MB file:
;	AltoIICode3.MB for the 4K version
;	AltoIICode3XM.MB for the 16K version

#AltoIIMRT.mu;

CLOCK:	MAR_ CLOCKLOC;		R37 OVERFLOWED.
	NOP;
	L_ MD+1;		INCREMENT CLOCK IM MEMORY
	MAR_ CLOCKLOC;
	MTEMP_ L, TASK;
	MD_ MTEMP, :NOCLK;

DOCUR:	L_ T_ YPOS;		CHECK FOR VISIBLE CURSOR ON THIS SCAN
	SH<0, L_ 20-T-1;	 ***x13 change: the constant 20 was 17
	SH<0, L_ 2+T, :SHOWC;	[SHOWC,WAITC]

WAITC:	YPOS_ L, L_ 0, TASK, :MRTLAST;	SQUASHES PENDING BRANCH
SHOWC:	MAR_ CLOCKLOC+T+1, :CNOTLAST;

CNOTLAST: T_ CURX, :CURF;
CLAST:	T_ 0;
CURF:	YPOS_ L, L_ T;
	CURX_ L;
	L_ MD, TASK;
	CURDATA_ L, :MRT;

;AFTER THIS DISPATCH, T WILL CONTAIN XCHANGE, L WILL CONTAIN YCHANGE-1

TX1:	L_ T_ ONE +T, :M00;		Y=0, X=1
TX2:	L_ T_ ALLONES, :M00;		Y=0, X=-1
TX3:	L_ T_ 0, :M00;			Y=1, X=0
TX4:	L_ T_ ONE AND T, :M00;		Y=1, X=1
TX5:	L_ T_ ALLONES XOR T, :M00;	Y=1, X=-1
TX6:	T_ 0, :M00;			Y=-1, X=0
TX7:	T_ ONE, :M00;			Y=-1, X=1
TX8:	T_ ALLONES, :M00;		Y=-1, X=-1

M00:	MAR_ MOUSELOC;			START THE FETCH OF THE COORDINATES
	MTEMP_ L;			YCHANGE -1
	L_ MD+ T;			X+ XCHANGE
	T_ MD;				Y
	T_ MTEMP+ T+1;			Y+ (YCHANGE-1) + 1
	MTEMP_ L, L_ T;
	MAR_ MOUSELOC;			NOW RESTORE THE UPDATED COORDINATES
	CLOCKTEMP_ L;
	MD_ MTEMP, TASK;
	MD_ CLOCKTEMP, :MRTA;


;CURSOR TASK

;Cursor task specific functions
$XPREG		$L026010,000000,124000; F2 = 10
$CSR		$L026011,000000,124000; F2 = 11

CU00012> CURT:	XPREG<- CURX, TASK;
CU00437>	CSR<- CURDATA, :CURT;


;PREDEFINITION FOR PARITY TASK.
;THE CODE IS AT THE END OF THE FILE
!17,20,PR0,,PR2,PR3,PR4,PR5,PR6,PR7,PR8,,,,,,,;

;NOVA EMULATOR

$SAD	$R5;
$PC	$R6;		USED BY MEMORY INIT


!7,10,Q0,Q1,Q2,Q3,Q4,Q5,Q6,Q7;
!1,2,FINSTO,INCPC;
!1,2,EReRead,FINJMP;		***X21 addition.
!1,2,EReadDone,EContRead;	***X21 addition.
!1,2,EtherBoot,DiskBoot;	***X21 addition.

EM00000> NOVEM:	IR<-L<-MAR<-0, :INXB,SAD<- L;  LOAD SAD TO ZERO THE BUS. STORE PC AT 0
EM00460> Q0:	L<- ONE, :INXA;	EXECUTED TWICE
EM00461> Q1:	L<- TOTUWC, :INXA;
EM00462> Q2:	L<-402, :INXA;	FIRST READ HEADER INTO 402, THEN
EM00463> Q3:	L<- 402, :INXA;	STORE LABEL AT 402
EM00464> Q4:	L<- ONE, :INXA;	STORE DATA PAGE STARTING AT 1
EM00465> Q5:	L<-377+1, :INXE;	Store Ethernet Input Buffer Length ***X21.
EM00466> Q6:	L<-ONE, :INXE;	Store Ethernet Input Buffer Pointer ***X21.
EM00467> Q7:	MAR<- DASTART;		CLEAR THE DISPLAY POINTER
EM00441>	L<- 0;
EM00451>	R37<- L;
EM00472>	MD<- 0;
EM00473>	MAR<- 177034;		FETCH KEYBOARD
EM00474>	L<- 100000;
EM00475>	NWW<- L, T<- 0-1;
EM00476>	L<- MD XOR T, BUSODD;	*** X21 change.
EM00477>	MAR<- BDAD, :EtherBoot;	[EtherBoot, DiskBoot]  *** X21 change.
				; BOOT DISK ADDRESS GOES IN LOCATION 12
EM00471> DiskBoot: SAD<- L, L<- 0+1;
EM00500>	MD<- SAD;
EM00501>	MAR<- KBLKADR, :FINSTO;


; Ethernet boot section added in X21.
$NegBreathM1	$177175;
$EthNovaGo	$3;	First data location of incoming packet

EM00470> EtherBoot: L<-EthNovaGo, :EReRead; [EReRead, FINJMP]

EM00454> EReRead:MAR_ EHLOC;	Set the host address to 377 for breath packets
EM00502>	TASK;
EM00503>	MD_ 377;

EM00504>	MAR_ EPLOC;	Zero the status word and start 'er up
EM00505>	SINK_ 2, STARTF;
EM00506>	MD _ 0;

EM00457> EContRead: MAR_ EPLOC;	See if status is still 0
EM00507>	T_ 377;		Status for correct read
EM00510>	L_ MD XOR T, TASK, BUS=0;
EM00511>	SAD_ L, :EReadDone; [EReadDone, EContRead]

EM00456> EReadDone: MAR_ 2;	Check the packet type
EM00512>	T_ NegBreathM1;	-(Breath-of-life)-1
EM00513>	T_MD+T+1;
EM00514>	L_SAD OR T;
EM00515>	SH=0, :EtherBoot;


; SUBROUTINE USED BY INITIALIZATION TO SET UP BLOCKS OF MEMORY
$EIOffset	$576;

EM00516> INXA:	T<-ONE, :INXCom;	***X21 change.
EM00517> INXE:	T<-EIOffset, :INXCom;		***X21 addition.

EM00520> INXCom: MAR<-T<-IR<- SAD+T;	*** X21 addition.
EM00521>	PC<- L, L<- 0+T+1;	*** X21 change.
EM00522> INXB:	SAD<- L;
EM00523>	SINK<- DISP, BUS,TASK;
EM00524>	SAD<- L, :Q0;


;REGISTERS USED BY NOVA EMULATOR 
$AC0	$R3;	AC'S ARE BACKWARDS BECAUSE THE HARDWARE SUPPLIES THE
$AC1	$R2;	COMPLEMENT ADDRESS WHEN ADDRESSING FROM IR
$AC2	$R1;
$AC3	$R0;
$XREG	$R7;


;PREDEFINITIONS FOR NOVA

!17,20,GETAD,G1,G2,G3,G4,G5,G6,G7,G10,G11,G12,G13,G14,G15,G16,G17;
!17,20,XCTAB,XJSR,XISZ,XDSZ,XLDA,XSTA,CONVERT,,,,,,,,,;
!3,4,SHIFT,SH1,SH2,SH3;
!1,2,MAYBE,NOINT;
!1,2,DOINT,DIS0;
!1,2,SOMEACTIVE,NOACTIVE;
!1,2,IEXIT,NIEXIT;
!17,1,ODDCX;
!1,2,EIR0,EIR1;
!7,1,INTCODE;
!1,2,INTSOFF,INTSON;	***X21 addition for DIRS
!7,10,EMCYCRET,RAMCYCRET,CYX2,CYX3,CYX4,CONVCYCRET,,;
!7,2,MOREBLT,FINBLT;
!1,2,DOIT,DISABLED;

; ALL INSTRUCTIONS RETURN TO START WHEN DONE

EM00020> START:	T<- MAR_PC+SKIP;
EM00525> START1:	L<- NWW, BUS=0;	BUS# 0 MEANS DISABLED OR SOMETHING TO DO
EM00576>	:MAYBE, SH<0, L<- 0+T+1;  	SH<0 MEANS DISABLED
EM00526> MAYBE:	PC<- L, L<- T, :DOINT;
EM00527> NOINT:	PC<- L, :DIS0;

EM00534> DOINT:	MAR<- WWLOC, :INTCODE;	TRY TO CAUSE AN INTERRUPT

;DISPATCH ON FUNCTION FIELD IF ARITHMETIC INSTRUCTION,
;OTHERWISE ON INDIRECT BIT AND INDEX FIELD

EM00535> DIS0:	L<- T<- IR<- MD;	SKIP CLEARED HERE

;DISPATCH ON SHIFT FIELD IF ARITHMETIC INSTRUCTION,
;OTHERWISE ON THE INDIRECT BIT OR IR[3-7]

EM00612> DIS1:	T<- ACSOURCE, :GETAD;

;GETAD MUST BE 0 MOD 20
EM00540> GETAD: T<- 0, :DOINS;			PAGE 0
EM00541> G1:	T<- PC -1, :DOINS;		RELATIVE
EM00542> G2:	T<- AC2, :DOINS;			AC2 RELATIVE
EM00543> G3:	T<- AC3, :DOINS;			AC3 RELATIVE
EM00544> G4:	T<- 0, :DOINS;			PAGE 0 INDIRECT
EM00545> G5:	T<- PC -1, :DOINS;		RELATIVE INDIRECT
EM00546> G6:	T<- AC2, :DOINS;			AC2 RELATIVE INDIRECT
EM00547> G7:	T<- AC3, :DOINS;			AC3 RELATIVE INDIRECT
EM00550> G10:	L<- 0-T-1, TASK, :SHIFT;		COMPLEMENT
EM00551> G11:	L<- 0-T, TASK, :SHIFT;		NEGATE
EM00552> G12:	L<- 0+T, TASK, :SHIFT;		MOVE
EM00553> G13:	L<- 0+T+1, TASK, :SHIFT;		INCREMENT
EM00554> G14:	L<- ACDEST-T-1, TASK, :SHIFT;	ADD COMPLEMENT
EM00555> G15:	L<- ACDEST-T, TASK, :SHIFT;	SUBTRACT
EM00556> G16:	L<- ACDEST+T, TASK, :SHIFT;	ADD
EM00557> G17:	L<- ACDEST AND T, TASK, :SHIFT;

EM00530> SHIFT:	DNS<- L LCY 8, :START; 	SWAP BYTES
EM00531> SH1:	DNS<- L RSH 1, :START;	RIGHT 1
EM00532> SH2:	DNS<- L LSH 1, :START;	LEFT 1
EM00533> SH3:	DNS<- L, :START;		NO SHIFT

EM00060> DOINS:	L<- DISP + T, TASK, :SAVAD, IDISP;	DIRECT INSTRUCTIONS
EM00061> DOIND:	L<- MAR<- DISP+T;				INDIRECT INSTRUCTIONS
EM00613>	XREG<- L;
EM00614>	L<- MD, TASK, IDISP, :SAVAD;

EM00102> BRI:	L<- MAR<- PCLOC	;INTERRUPT RETURN BRANCH
EM00615> BRI0:	T<- 77777;
EM00616>	L<- NWW AND T, SH < 0;
EM00617>	NWW<- L, :EIR0;	BOTH EIR AND BRI MUST CHECK FOR INTERRUPT
;			REQUESTS WHICH MAY HAVE COME IN WHILE
;			INTERRUPTS WERE OFF

EM00572> EIR0:	L<- MD, :DOINT;
EM00573> EIR1:	L<- PC, :DOINT;

;***X21 addition
; DIRS - 61013 - Disable Interrupts and Skip if they were On
EM00113> DIRS:	T<-100000;
EM00620>	L<-NWW AND T;
EM00621>	L<-PC+1, SH=0;

; DIR - 61000 - Disable Interrupts
EM00100> DIR:	T<- 100000, :INTSOFF;
EM00574> INTSOFF: L<- NWW OR T, TASK, :INTZ;

EM00575> INTSON: PC<-L, :INTSOFF;

;EIR - 61001 - Enable Interrupts
EM00101> EIR:	L<- 100000, :BRI0;

;SIT - 61007 - Start Interval Timer
EM00107> SIT:	T<- AC0;
EM00622>	L<- R37 OR T, TASK;
EM00623>	R37<- L, :START;


EM00624> FINJSR:	L<- PC;
EM00625>	AC3<- L, L_ T, TASK;
EM00455> FINJMP:	PC<- L, :START;
EM00626> SAVAD:	SAD<- L, :XCTAB;

;JSRII - 64400 - JSR double indirect, PC relative.  Must have X=1 in opcode
;JSRIS - 65000 - JSR double indirect, AC2 relative.  Must have X=2 in opcode
EM00064> JSRII:	MAR<- DISP+T;	FIRST LEVEL
EM00627>	IR<- JSRCX;	<JSR 0>
EM00630>	T_ MD, :DOIND;	THE IR_ INSTRUCTION WILL NOT BRANCH	


;TRAP ON UNIMPLEMENTED OPCODES.  SAVES  PC AT
;TRAPPC, AND DOES A JMP@ TRAPVEC ! OPCODE.
EM00077> TRAP:	XREG<- L LCY 8;	THE INSTRUCTION
EM00037> TRAP1:	MAR<- TRAPPC;***X13 CHANGE: TAG 'TRAP1' ADDED
EM00631>	IR<- T<- 37;
EM00632>	MD<- PC;
EM00633>	T<- XREG.T;
EM00634>	T<- TRAPCON+T+1, :DOIND;	T NOW CONTAINS 471+OPCODE
;				THIS WILL DO JMP@ 530+OPCODE

;***X21 CHANGE: ADDED TAG RAMTRAP
EM00076> RAMTRAP: SWMODE, :TRAP;

; Parameterless operations come here for dispatch.

!1,2,NPNOTRAP,NPTRAP;

EM00063> NOPAR:	XREG<-L LCY 8;	***X21 change. Checks < 27.
EM00635>	T<-27;		***IIX3. Greatest defined op is 26.
EM00640>	L<-DISP-T;
EM00641>	ALUCY;
EM00642>	SINK<-DISP, SINK<-X37, BUS, TASK, :NPNOTRAP;

EM00636> NPNOTRAP: :DIR;

EM00637> NPTRAP: :TRAP1;

;***X21 addition for debugging w/ expanded DISP Prom
EM00065> U5:	:RAMTRAP;
EM00066> U6:	:RAMTRAP;
EM00067> U7:	:RAMTRAP;

;MAIN INSTRUCTION TABLE.  GET HERE:
;		(1) AFTER AN INDIRECTION
;		(2) ON DIRECT INSTRUCTIONS 

EM00560> XCTAB:	L<- SAD, TASK, :FINJMP;	JMP
EM00561> XJSR:	T<- SAD, :FINJSR;	JSR
EM00562> XISZ:	MAR<- SAD, :ISZ1;	ISZ
EM00563> XDSZ:	MAR<- SAD, :DSZ1;	DSZ
EM00564> XLDA:	MAR<- SAD, :FINLOAD;	LDA 0-3
EM00565> XSTA:	MAR<- SAD;		/*NORMAL
EM00643> XSTA1:	L<- ACDEST, :FINSTO;	/*NORMAL

;	BOUNDS-CHECKING VERSION OF STORE
;	SUBST ";**<CR>" TO "<CR>;**" TO ENABLE THIS CODE:
;**	!1,2,XSTA1,XSTA2;
;**	!1,2,DOSTA,TRAPSTA;
;**XSTA:	MAR_ 10;	LOCS 10,11 CONTAINS HI,LO BOUNDS
;**	T_ SAD
;**	L_ MD-T;	HIGHBOUND-ADDR
;**	T_ MD, ALUCY;
;**	L_ SAD-T, :XSTA1;	ADDR-LOWBOUND
;**XSTA1:	TASK, :XSTA3;
;**XSTA2:	ALUCY, TASK;
;**XSTA3:	L_ 177, :DOSTA;
;**TRAPSTA:	XREG_ L, :TRAP1;	CAUSE A SWAT
;**DOSTA:	MAR_ SAD;	DO THE STORE NORMALLY
;**	L_ ACDEST, :FINSTO;
;**

EM00644> DSZ1:	T<- ALLONES, :FINISZ;
EM00645> ISZ1:	T<- ONE, :FINISZ;

EM00452> FINSTO:	SAD<- L,TASK;
EM00646> FINST1:	MD<-SAD, :START;

EM00647> FINLOAD: NOP;
EM00650> LOADX:	L<- MD, TASK;
EM00651> LOADD:	ACDEST<- L, :START;

EM00652> FINISZ:	L<- MD+T;
EM00653>	MAR<- SAD, SH=0;
EM00654>	SAD<- L, :FINSTO;

EM00453> INCPC:	MD<- SAD;
EM00655>	L<- PC+1, TASK;
EM00656>	PC<- L, :START;

;DIVIDE.  THIS DIVIDE IS IDENTICAL TO THE NOVA DIVIDE EXCEPT THAT
;IF THE DIVIDE CANNOT BE DONE, THE INSTRUCTION FAILS TO SKIP, OTHERWISE
;IT DOES.  CARRY IS UNDISTURBED.

!1,2,DODIV,NODIV;
!1,2,DIVL,ENDDIV;
!1,2,NOOVF,OVF;
!1,2,DX0,DX1;
!1,2,NOSUB,DOSUB;

EM00121> DIV:	T<- AC2;
EM00657> DIVX:	L<- AC0 - T;	DO THE DIVIDE ONLY IF AC2>AC0
EM00672>	ALUCY, TASK, SAD<- L, L<- 0+1;
EM00673>	:DODIV, SAD<- L LSH 1;		SAD<- 2.  COUNT THE LOOP BY SHIFTING

EM00661> NODIV:	:FINBLT;		***X21 change.
EM00660> DODIV:	L<- AC0, :DIV1;

EM00662> DIVL:	L<- AC0;
EM00674> DIV1:	SH<0, T<- AC1;	WILL THE LEFT SHIFT OF THE DIVIDEND OVERFLOW?
EM00675>	:NOOVF, AC0<- L MLSH 1, L<- T<- 0+T;	L<- AC1, T<- 0

EM00665> OVF:	AC1<- L LSH 1, L<- 0+INCT, :NOV1;		L_ 1. SHIFT OVERFLOWED
EM00664> NOOVF:	AC1<- L LSH 1 , L<- T;			L_ 0. SHIFT OK

EM00676> NOV1:	T<- AC2, SH=0;
EM00677>	L<- AC0-T, :DX0;

EM00667> DX1:	ALUCY;		DO THE TEST ONLY IF THE SHIFT DIDN'T OVERFLOW.  IF 
;			IT DID, L IS STILL CORRECT, BUT THE TEST WOULD GO
;			THE WRONG WAY.
EM00700>	:NOSUB, T<- AC1;

EM00666> DX0:	:DOSUB, T<- AC1;

EM00671> DOSUB:	AC0<- L, L<- 0+INCT;	DO THE SUBTRACT
EM00701>	AC1<- L;			AND PUT A 1 IN THE QUOTIENT

EM00670> NOSUB:	L<- SAD, BUS=0, TASK;
EM00702>	SAD<- L LSH 1, :DIVL;

EM00663> ENDDIV:	L<- PC+1, TASK, :DOIT; ***X21 change. Skip if divide was done.


;MULTIPLY.  THIS IS AN EXACT EMULATION OF NOVA HARDWARE MULTIPLY.
;AC2 IS THE MULTIPLIER, AC1 IS THE MULTIPLICAND.
;THE PRODUCT IS IN AC0 (HIGH PART), AND AC1 (LOW PART).
;PRECISELY: AC0,AC1 <- AC1*AC2  + AC0

!1,2,DOMUL,NOMUL;
!1,2,MPYL,MPYA;
!1,2,NOADDIER,ADDIER;
!1,2,NOSPILL,SPILL;
!1,2,NOADDX,ADDX;
!1,2,NOSPILLX,SPILLX;


EM00120> MUL:	L<- AC2-1, BUS=0;
EM00703> MPYX:	XREG<-L,L<- 0, :DOMUL;	GET HERE WITH AC2-1 IN L. DON'T MUL IF AC2=0
EM00704> DOMUL:	TASK, L<- -10+1;
EM00720>	SAD<- L;		COUNT THE LOOP IN SAD

EM00706> MPYL:	L<- AC1, BUSODD;
EM00721>	T<- AC0, :NOADDIER;

EM00710> NOADDIER: AC1<- L MRSH 1, L<- T, T<- 0, :NOSPILL;
EM00711> ADDIER:	L<- T<- XREG+INCT;
EM00722>	L<- AC1, ALUCY, :NOADDIER;

EM00713> SPILL:	T<- ONE;
EM00712> NOSPILL: AC0<- L MRSH 1;
EM00723> 	L<- AC1, BUSODD;
EM00724>	T<- AC0, :NOADDX;

EM00714> NOADDX:	AC1<- L MRSH 1, L<- T, T<- 0, :NOSPILLX;
EM00715> ADDX:	L<- T<- XREG+ INCT;
EM00725>	L<- AC1,ALUCY, :NOADDX;

EM00717> SPILLX:	T<- ONE;
EM00716> NOSPILLX: AC0<- L MRSH 1;
EM00726>	L<- SAD+1, BUS=0, TASK;
EM00727>	SAD<- L, :MPYL;

EM00705> NOMUL:	T<- AC0;
EM00730>	AC0<- L, L<- T, TASK;	CLEAR AC0
EM00731>	AC1<- L;			AND REPLACE AC1 WITH AC0
EM00707> MPYA:	:FINBLT;		***X21 change.

;CYCLE AC0 LEFT BY DISP MOD 20B, UNLESS DISP=0, IN WHICH
;CASE CYCLE BY AC1 MOD 20B
;LEAVES AC1=CYCLE COUNT-1 MOD 20B

$CYRET		$R5;	Shares space with SAD.
$CYCOUT		$R7;	Shares space with XREG.

!1,2,EMCYCX,ACCYCLE;
!1,1,Y1;
!1,1,Y2;
!1,1,Y3;
!1,1,Z1;
!1,1,Z2;
!1,1,Z3;

EM00062> EMCYCLE: L<- DISP, SINK<- X17, BUS=0;	CONSTANT WITH BS=7
EM00734> CYCP:	T<- AC0, :EMCYCX;

EM00733> ACCYCLE: T<- AC1;
EM00736>	L<- 17 AND T, :CYCP;

EM00732> EMCYCX: CYCOUT<-L, L<-0, :RETCYCX;

RAMCYCX: CYCOUT_L, L_0+1;

EM00740> RETCYCX: CYRET<-L, L<-0+T;
EM00742>	SINK<-CYCOUT, BUS;
EM00744>	TASK, :L0;

;TABLE FOR CYCLE
EM00174> R4:	CYCOUT<- L MRSH 1;
EM00741> Y3:	L<- T<- CYCOUT, TASK;
EM00175> R3X:	CYCOUT<- L MRSH 1;
EM00737> Y2:	L<- T<- CYCOUT, TASK;
EM00176> R2X:	CYCOUT<- L MRSH 1;
EM00735> Y1:	L<- T<- CYCOUT, TASK;
EM00177> R1X:	CYCOUT<- L MRSH 1, :ENDCYCLE;

EM00164> L4:	CYCOUT<- L MLSH 1;
EM00747> Z3:	L<- T<- CYCOUT, TASK;
EM00163> L3:	CYCOUT<- L MLSH 1;
EM00745> Z2:	L<- T<- CYCOUT, TASK;
EM00162> L2:	CYCOUT<- L MLSH 1;
EM00743> Z1:	L<- T<- CYCOUT, TASK;
EM00161> L1:	CYCOUT<- L MLSH 1, :ENDCYCLE;
EM00160> L0:	CYCOUT<- L, :ENDCYCLE;

EM00164> L8:	CYCOUT<- L LCY 8, :ENDCYCLE;
EM00165> L7:	CYCOUT<- L LCY 8, :Y1;
EM00166> L6:	CYCOUT<- L LCY 8, :Y2;
EM00165> L5:	CYCOUT<- L LCY 8, :Y3;

EM00171> R7:	CYCOUT<- L LCY 8, :Z1;
EM00172> R6:	CYCOUT<- L LCY 8, :Z2;
EM00173> R5:	CYCOUT<- L LCY 8, :Z3;

EM00746> ENDCYCLE: SINK<- CYRET, BUS, TASK;
EM00750>	:EMCYCRET;

EM00600> EMCYCRET: L_<-YCOUT, TASK, :LOADD;

EM00601> RAMCYCRET: T<-PC, BUS, SWMODE, :TORAM;

; Scan convert instruction for characters. Takes DWAX (Destination
; word address)-NWRDS in AC0, and a pointer to a .AL-format font
; in AC3. AC2+displacement contains a pointer to a two-word block
; containing NWRDS and DBA (Destination Bit Address).

$XH		$R10;
$DWAX		$R35;
$MASK		$R36;

!1,2,HDLOOP,HDEXIT;
!1,2,MERGE,STORE;
!1,2,NFIN,FIN;
!17,2,DOBOTH,MOVELOOP;

CONVERT: MAR_XREG+1;	Got here via indirect mechanism which
;			left first arg in SAD, its address in XREG. 
	T_17;
	L_MD AND T;

	T_MAR_AC3;
	AC1_L;		AC1_DBA&#17
	L_MD+T, TASK;
	AC3_L;		AC3_Character descriptor block address(Char)

	MAR_AC3+1;
	T_177400;
	IR_L_MD AND T;		IR_XH
	XH_L LCY 8, :ODDCX;	XH register temporarily contains HD
ODDCX:	L_AC0, :HDENTER;

HDLOOP: T_SAD;			(really NWRDS)
	L_DWAX+T;

HDENTER: DWAX_L;		DWAX _ AC0+HD*NWRDS
	L_XH-1, BUS=0, TASK;
	XH_L, :HDLOOP;

HDEXIT:	T_MASKTAB;
	MAR_T_AC1+T;		Fetch the mask.
	L_DISP;
	XH_L;			XH register now contains XH
	L_MD;
	MASK_L, L_0+T+1, TASK;
	AC1_L;			***X21. AC1 _ (DBA&#17)+1

	L_5;			***X21. Calling conventions changed.
	IR_SAD, TASK;
	CYRET_L, :MOVELOOP;	CYRET_CALL5

MOVELOOP: L_T_XH-1, BUS=0;
	MAR_AC3-T-1, :NFIN;	Fetch next source word
NFIN:	XH_L;
	T_DISP;			(really NWRDS)
	L_DWAX+T;		Update destination address
	T_MD;
	SINK_AC1, BUS;
	DWAX_L, L_T, TASK, :L0;	Call Cycle subroutine

CONVCYCRET: MAR_DWAX;
	T_MASK, BUS=0;
	T_CYCOUT.T, :MERGE;	Data for first word. If MASK=0
				; then store the word rather than
				; merging, and do not disturb the
				; second word.
MERGE:	L_XREG AND NOT T;	Data for second word.
	T_MD OR T;		First word now merged,
	XREG_L, L_T;
	MTEMP_L;
	MAR_DWAX;			restore it.
	SINK_XREG, BUS=0, TASK;
	MD_MTEMP, :DOBOTH;	XREG=0 means only one word
				; is involved.

DOBOTH: MAR_DWAX+1;
	T_XREG;
	L_MD OR T;
	MAR_DWAX+1;
	XREG_L, TASK;		***X21. TASK added.
STORE:	MD_XREG, :MOVELOOP;

FIN:	L_AC1-1;		***X21. Return AC1 to DBA&#17.
	AC1_L;			*** ... bletch ...
	IR_SH3CONST;
	L_MD, TASK, :SH1;

;RCLK - 61003 - Read the Real Time Clock into AC0,AC1
EM00103> RCLK:	MAR<- CLOCKLOC;
EM01035>	L<- R37;
EM01036>	AC1<- L, :LOADX;

;SIO - 61004 - Put AC0 on the bus, issue STARTF to get device attention,
;Read Host address from Ethernet interface into AC0.
EM0104> SIO:	L<- AC0, STARTF;
EM1037>	T<- 77777;		***X21 sets AC0[0] to 0
EM1040>	L<- RSNF AND T;
EM1041> LTOAC0:	AC0<- L, TASK, :TOSTART;

;EngNumber is a constant returned by VERS that contains a discription
;of the Alto and it's Microcode. The composition of EngNumber is:
;	bits 0-3	Alto engineering number
;	bits 4-7	Alto build
;	bits 8-15	Version number of Microcode
;Use of the Alto Build number has been abandoned.
;the engineering number (EngNumber) is in the MRT files because it
; it different for Altos with and without Extended memory.
EM00114> VERS:	T<- EngNumber;		***V3 change
EM01042>	L<- 3+T, :LTOAC0;	***V3 change

;XMLDA - Extended Memory Load Accumulator.
;	AC0 _ @AC1 in the alternate bank
XMLDA:	XMAR_ AC1, :FINLOAD;	***V3 change

;XMSTA - Extended Memory Store Accumulator
;	@AC1 _ AC0 in the alternate bank
XMSTA:	XMAR_ AC1, :XSTA1;	***V3 change

;BLT - 61005 - Block Transfer
;BLKS - 61006 - Block Store
; Accepts in
;	AC0/ BLT: Address of first word of source block-1
;	     BLKS: Data to be stored
;	AC1/ Address of last word of destination block 
;	AC3/ NEGATIVE word count
; Leaves
;	AC0/ BLT: Address of last word of source block+1
;	     BLKS: Unchanged
;	AC1/ Unchanged
;	AC2/ Unchanged
;	AC3/ 0
; These instructions are interruptable.  If an interrupt occurs,
; the PC is decremented by one, and the ACs contain the intermediate
; so the instruction can be restarted when the interrupt is dismissed.

!1,2,PERHAPS, NO;

EM00105> BLT:	L<- MAR<- AC0+1;
EM01043>	AC0<- L;
EM01046>	L<- MD, :BLKSA;

EM00106> BLKS:	L<- AC0;
EM01047> BLKSA:	T<- AC3+1, BUS=0;
EM01050>	MAR<- AC1+T, :MOREBLT;

EM00606> MOREBLT: XREG<- L, L<- T;
EM01051>	AC3<- L, TASK;
EM01052>	MD<- XREG;		STORE
EM01053>	L<- NWW, BUS=0;		CHECK FOR INTERRUPT
EM01054>	SH<0, :PERHAPS, L<- PC-1;	Prepare to back up PC.

EM01045> NO:	SINK<- DISP, SINK<- M7, BUS, :DISABLED;

EM01044> PERHAPS: SINK<- DISP, SINK<- M7, BUS, :DOIT;

EM00610> DOIT:	PC<-L, :FINBLT;	***X21. Reset PC, terminate instruction.

EM00611> DISABLED: :DIR;	GOES TO BLT OR BLKS

EM00607> FINBLT:	T<-777;	***X21. PC in [177000-177777] means Ram return
EM01055>	L<-PC+T+1;
EM01056>	L<-PC AND T, TASK, ALUCY;
EM01057> TOSTART: XREG<-L, :START;

RAMRET: T_XREG, BUS, SWMODE;
EM01060> TORAM:	:NOVEM;

;PARAMETERLESS INSTRUCTIONS FOR DIDDLING THE WCS.

;JMPRAM - 61010 - JUMP TO THE RAM ADDRESS SPECIFIED BY AC1
EM00110> JMPR:	T<-AC1, BUS, SWMODE, :TORAM;


;RDRAM - 61011 - READ THE RAM WORD ADDRESSED BY AC1 INTO AC0
EM00111> RDRM:	T<- AC1, RDRAM;
EM01061>	L_ ALLONES, TASK, :LOADD;


;WRTRAM - 61012 - WRITE AC0,AC3 INTO THE RAM LOCATION ADDRESSED BY AC1
EM00112> WTRM:	T<- AC1;
EM01062>	L<- AC0, WRTRAM;
EM01063>	L<- AC3, :FINBLT;

;DOUBLE WORD INSTRUCTIONS

;DREAD - 61015
;	AC0<- rv(AC3); AC1<- rv(AC3 xor 1)

EM00115> DREAD:	MAR<- AC3;		START MEMORY CYCLE
EM01064>	NOP;			DELAY
EM01065> DREAD1:	L<- MD;			FIRST READ
EM01066>	T<-MD;			SECOND READ
EM01067>	AC0<- L, L<-T, TASK;	STORE MSW
EM01070>	AC1<- L, :START;		STORE LSW


;DWRITE - 61016
;	rv(AC3)<- AC0; rv(AC3 xor 1)<- AC1

EM00116> DWRITE:	MAR<- AC3;		START MEMORY CYCLE
EM01071>	NOP;			DELAY
EM01072>	MD<- AC0, TASK;		FIRST WRITE
EM01073>	MD<- AC1, :START;	SECOND WRITE


;DEXCH - 61017
;	t<- rv(AC3); rv(AC3)<- AC0; AC0<- t
;	t<- rv(AC3 xor 1); rv(AC3 xor 1)<- AC1; AC1<- t

EM00117> DEXCH:	MAR<- AC3;		START MEMORY CYCLE
EM01074>	NOP;			DELAY
EM01075>	MD<- AC0;		FIRST WRITE
EM01076>	MD<- AC1,:DREAD1;	SECOND WRITE, GO TO READ


;DIOGNOSE INSTRUCTIONS

;DIOG1 - 61022
;	Hamming Code_ AC2
;	rv(AC3)_ AC0; rv(AC3 xor 1)_ AC1

EM00122> DIOG1:	MAR<- ERRCTRL;		START WRITE TO ERROR CONTROL
EM01077>	NOP;			DELAY
EM01100>	MD<- AC2,:DWRITE;	WRITE HAMMING CODE, GO TO DWRITE

;DIOG2 - 61023
;	rv(AC3)_ AC0
;	rv(AC3)_ AC0 xor AC1

EM00123> DIOG2:	MAR<- AC3;		START MEMORY CYCLE
EM01101>	T<- AC0;			SETUP FOR XOR
EM01102>	L<- AC1 XORT;		DO XOR
EM01103>	MD<- AC0;		FIRST WRITE
EM01104>	MAR<- AC3;		START MEMORY CYCLE
EM01105>	AC0<- L, TASK;		STORE XOR WORD
EM01106>	MD<- AC0, :START;	SECOND WRITE

;INTERRUPT SYSTEM.  TIMING IS 0 CYCLES IF DISABLED, 18 CYCLES
;IF THE INTERRUPTING CHANEL IS INACTIVE, AND 36+6N CYCLES TO CAUSE
;AN INTERRUPT ON CHANNEL N

EM00567> INTCODE:PC<- L, IR<- 0;	
EM01107>	T<- NWW;
EM01110>	T<- MD OR T;
EM01111>	L<- MD AND T;
EM01112>	SAD<- L, L<- T, SH=0;		SAD HAD POTENTIAL INTERRUPTS
EM01113>	NWW<- L, L<- 0+1, :SOMEACTIVE;	NWW HAS NEW WW

EM00537> NOACTIVE: MAR<- WWLOC;		RESTORE WW TO CORE
EM01114>	L<- SAD;			AND REPLACE IT WITH SAD IN NWW
EM01115>	MD<- NWW, TASK;
EM01116> INTZ:	NWW<- L, :START;

EM00536> SOMEACTIVE: MAR<- PCLOC;	STORE PC AND SET UP TO FIND HIGHEST PRIORITY REQUEST
EM01117>	XREG<- L, L<- 0;
EM01120>	MD<- PC, TASK;

EM01121> ILPA:	PC<- L;
EM01122> ILP:	T<- SAD;
EM01123> 	L<- T<- XREG AND T;
EM01124>	SH=0, L<- T, T<- PC;
EM01125>	:IEXIT, XREG<- L LSH 1;

EM00571> NIEXIT:	L_ 0+T+1, TASK, :ILPA;
EM00570> IEXIT:	MAR<- PCLOC+T+1;		FETCH NEW PC. T HAS CHANNEL #, L HAS MASK

EM01126>	XREG<- L;
EM01127>	T<- XREG;
EM01130>	L<- NWW XOR T;	TURN OFF BIT IN WW FOR INTERRUPT ABOUT TO HAPPEN
EM01131>	T<- MD;
EM01132>	NWW<- L, L<- T;
EM01133>	PC<- L, L<- T<- 0+1, TASK;
EM01134>	SAD<- L MRSH 1, :NOACTIVE;	SAD_ 1B5 TO DISABLE INTERRUPTS

;
;	************************
;	* BIT-BLT - 61024 *
;	************************
;	Modified September 1977 to support Alternate memory banks
;	Last modified Sept 6, 1977 by Dan Ingalls
;
;	/* NOVA REGS
;	AC2 -> BLT DESCRIPTOR TABLE, AND IS PRESERVED
;	AC1 CARRIES LINE COUNT FOR RESUMING AFTER AN
;		INTERRUPT. MUST BE 0 AT INITIAL CALL
;	AC0 AND AC3 ARE SMASHED TO SAVE S-REGS
;
;	/* ALTO REGISTER USAGE
;DISP CARRIES:	TOPLD(100), SOURCEBANK(40), DESTBANK(20),
;		SOURCE(14), OP(3)
$MASK1		$R0;
$YMUL		$R2;	HAS TO BE AN R-REG FOR SHIFTS
$RETN		$R2;
$SKEW		$R3;
$TEMP		$R5;
$WIDTH		$R7;
$PLIER		$R7;	HAS TO BE AN R-REG FOR SHIFTS
$DESTY		$R10;
$WORD2		$R10;
$STARTBITSM1	$R35;
$SWA		$R36;
$DESTX		$R36;
$LREG		$R40;	HAS TO BE R40 (COPY OF L-REG)
$NLINES		$R41;
$RAST1		$R42;
$SRCX		$R43;
$SKMSK		$R43;
$SRCY		$R44;
$RAST2		$R44;
$CONST		$R45;
$TWICE		$R45;
$HCNT		$R46;
$VINC		$R46;
$HINC		$R47;
$NWORDS		$R50;
$MASK2		$R51;	WAS $R46;
;
$LASTMASKP1	$500;	MASKTABLE+021
$170000		$170000;
$CALL3		$3;	SUBROUTINE CALL INDICES
$CALL4		$4;
$DWAOFF		$2;	BLT TABLE OFFSETS
$DXOFF		$4;
$DWOFF		$6;
$DHOFF		$7;
$SWAOFF		$10;
$SXOFF		$12;
$GRAYOFF	$14;	GRAY IN WORDS 14-17
$LASTMASK	$477;	MASKTABLE+020	**NOT IN EARLIER PROMS!


;	BITBLT SETUP - CALCULATE RAM STATE FROM AC2'S TABLE
;----------------------------------------------------------
;
;	/* FETCH COORDINATES FROM TABLE
	!1,2,FDDX,BLITX;
	!1,2,FDBL,BBNORAM;
	!17,20,FDBX,,,,FDX,,FDW,,,,FSX,,,,,;	FDBL RETURNS (BASED ON OFFSET)
;	        (0)     4    6      12
EM00124> BITBLT:	L<- 0;
EM01135>	SINK<-LREG, BUSODD;	SINK<- -1 IFF NO RAM
EM01142>	L<- T<- DWOFF, :FDBL;
EM01141> BBNORAM: TASK, :NPTRAP;		TRAP IF NO RAM
;
EM01166> FDW:	T<- MD;			PICK UP WIDTH, HEIGHT
EM01143>	WIDTH<- L, L<- T, TASK, :NZWID;
EM01144> NZWID:	NLINES<- L;
EM01145>	T<- AC1;
EM01146>	L<- NLINES-T;
EM01147>	NLINES<- L, SH<0, TASK;
EM01150>	:FDDX;
;
EM01136> FDDX:	L<- T<- DXOFF, :FDBL;	PICK UP DEST X AND Y
EM01164> FDX:	T<- MD;
EM01151>	DESTX<- L, L<- T, TASK;
EM01152>	DESTY<- L;
;
EM01153>	L<- T<- SXOFF, :FDBL;	PICK UP SOURCE X AND Y
EM01172> FSX:	T<- MD;
EM01154>	SRCX<- L, L<- T, TASK;
EM01155>	SRCY<- L, :CSHI;
;
;	/* FETCH DOUBLEWORD FROM TABLE (L<- T<- OFFSET, :FDBL)
EM01140> FDBL:	MAR<- AC2+T;
EM01156>	SINK<- LREG, BUS;
EM01160> FDBX:	L<- MD, :FDBX;
;
;	/* CALCULATE SKEW AND HINC
	!1,2,LTOR,RTOL;
EM01157> CSHI:	T<- DESTX;
EM01161>	L<- SRCX-T-1;
EM01165>	T<- LREG+1, SH<0;	TEST HORIZONTAL DIRECTION
EM01167>	L<- 17.T, :LTOR;	SKEW <- (SRCX - DESTX) MOD 16
EM01163> RTOL:	SKEW<- L, L<- 0-1, :AH, TASK;	HINC <- -1
EM00162> LTOR:	SKEW<- L, L<- 0+1, :AH, TASK;	HINC <- +1
EM01170> AH:	HINC<- L;
;
;	CALCULATE MASK1 AND MASK2
	!1,2,IFRTOL,LNWORDS;
	!1,2,POSWID,NEGWID;
EM01171> CMASKS:	T<- DESTX;
EM01173>	T<- 17.T;
EM01200>	MAR<- LASTMASKP1-T-1;
EM01201>	L<- 17-T;		STARTBITS <- 16 - (DESTX.17)
EM01202>	STARTBITSM1<- L;
EM01203>	L<- MD, TASK;
EM01204>	MASK1<- L;		MASK1 <- @(MASKLOC+STARTBITS)
EM01205>	L<- WIDTH-1;
EM01206>	T<- LREG-1, SH<0;
EM01207>	T<- DESTX+T+1, :POSWID;
EM01176> POSWID:	T<- 17.T;
EM01210>	MAR<- LASTMASK-T-1;
EM01211>	T<- ALLONES;		MASK2 <- NOT
EM01212>	L<- HINC-1;
EM01213>	L<- MD XOR T, SH=0, TASK;	@(MASKLOC+(15-((DESTX+WIDTH-1).17)))
EM01214>	MASK2<- L, :IFRTOL;
;	/* IF RIGHT TO LEFT, ADD WIDTH TO X'S AND EXCH MASK1, MASK2
EM01174> IFRTOL:	T<- WIDTH-1;	WIDTH-1
EM01215>	L<- SRCX+T;
EM01216>	SRCX<- L;		SRCX <- SCRX + (WIDTH-1)
EM01217>	L<- DESTX+T;
EM01220>	DESTX<- L;	DESTX <- DESTX + (WIDTH-1)
EM01221>	T<- DESTX;
EM01222>	L<- 17.T, TASK;
EM01223>	STARTBITSM1<- L;	STARTBITS <- (DESTX.17) + 1
EM01224>	T<- MASK1;
EM01225>	L<- MASK2;
EM01226>	MASK1<- L, L<- T,TASK;	EXCHANGE MASK1 AND MASK2
EM01227>	MASK2<-L;
;
;	/* CALCULATE NWORDS
	!1,2,LNW1,THIN;
EM01175> LNWORDS:T<- STARTBITSM1+1;
EM01232>	L<- WIDTH-T-1;
EM01233>	T<- 177760, SH<0;
EM01234>	T<- LREG.T, :LNW1;
EM01230> LNW1:	L<- CALL4;		NWORDS <- (WIDTH-STARTBITS)/16
EM01235>	CYRET<- L, L<- T, :R4, TASK; CYRET<-CALL4
;	**WIDTH REG NOW FREE**
EM00604> CYX4:	L<- CYCOUT, :LNW2;
EM01231> THIN:	T<- MASK1;	SPECIAL CASE OF THIN SLICE
EM01236>	L<-MASK2.T;
EM01237>	MASK1<- L, L<- 0-1;	MASK1 <- MASK1.MASK2, NWORDS <- -1
EM01240> LNW2:	NWORDS<- L;	LOAD NWORDS
;	**STARTBITSM1 REG NOW FREE**
;
;	/* DETERMINE VERTICAL DIRECTION
	!1,2,BTOT,TTOB;
	T_ SRCY;
EM01244>	L<- DESTY-T;
EM01245>	T<- NLINES-1, SH<0;
EM01246>	L<- 0, :BTOT;	VINC _ 0 IFF TOP-TO-BOTTOM
EM01242> BTOT:	L<- ALLONES;	ELSE -1
EM01247> BTOT1:	VINC<- L;
EM01250>	L<- SRCY+T;		GOING BOTTOM TO TOP
EM01251>	SRCY<- L;			ADD NLINES TO STARTING Y'S
EM01252>	L<- DESTY+T;
EM01253>	DESTY<- L, L<- 0+1, TASK;
EM01254>	TWICE<-L, :CWA;
;
EM01243> TTOB:	T<- AC1, :BTOT1;		TOP TO BOT, ADD NDONE TO STARTING Y'S
;	**AC1 REG NOW FREE**;
;
;	/* CALCULATE WORD ADDRESSES - DO ONCE FOR SWA, THEN FOR DWAX
EM01255> CWA:	L<- SRCY;	Y HAS TO GO INTO AN R-REG FOR SHIFTING
EM01256>	YMUL<- L;
EM01257>	T<- SWAOFF;		FIRST TIME IS FOR SWA, SRCX
EM01260>	L<- SRCX;
;	**SRCX, SRCY REG NOW FREE**
EM01261> DOSWA:	MAR<- AC2+T;		FETCH BITMAP ADDR AND RASTER
EM01262>	XREG<- L;
EM01263>	L<-CALL3;
EM01264>	CYRET<- L;		CYRET<-CALL3
EM01265>	L<- MD;
EM01266>	T<- MD;
EM01267>	DWAX<- L, L<-T, TASK;
EM01270>	RAST2<- L;
EM01271>	T<- 177760;
EM01272>	L<- T_ XREG.T, :R4, TASK;	SWA <- SWA + SRCX/16
EM00603> CYX3:	T<- CYCOUT;
EM01273>	L<- DWAX+T;
EM01274>	DWAX<- L;
;
	!1,2,NOADD,DOADD;
	!1,2,MULLP,CDELT;	SWA <- SWA + SRCY*RAST1
EM01275>	L<- RAST2;
EM01302>	SINK<- YMUL, BUS=0, TASK;	NO MULT IF STARTING Y=0
EM01303>	PLIER<- L, :MULLP;
EM01300> MULLP:	L<- PLIER, BUSODD;		MULTIPLY RASTER BY Y
EM01304>	PLIER<- L RSH 1, :NOADD;
EM01276> NOADD:	L<- YMUL, SH=0, TASK;	TEST NO MORE MULTIPLIER BITS
EM01305> SHIFTB:	YMUL<- L LSH 1, :MULLP;
EM01277> DOADD:	T<- YMUL;
EM01306>	L<- DWAX+T;
EM01307>	DWAX<- L, L<-T, :SHIFTB, TASK;
;	**PLIER, YMUL REG NOW FREE**
;
	!1,2,HNEG,HPOS;
	!1,2,VPOS,VNEG;
	!1,1,CD1;	CALCULATE DELTAS = +-(NWORDS+2)[HINC] +-RASTER[VINC]
EM01301> CDELT:	L<- T<- HINC-1;	(NOTE T<- -2 OR 0)
EM01314>	L<- T<- NWORDS-T, SH=0;	(L<-NWORDS+2 OR T<-NWORDS)
EM01315> CD1:	SINK<- VINC, BUSODD, :HNEG;
EM01310> HNEG:	T<- RAST2, :VPOS;
EM01311> HPOS:	L<- -2-T, :CD1;	(MAKES L<- -(NWORDS+2))
EM01312> VPOS:	L<- LREG+T, :GDELT, TASK;	BY NOW, LREG = +-(NWORDS+2)
EM01313> VNEG:	L<- LREG-T, :GDELT, TASK;	AND T = RASTER
EM01316> GDELT:	RAST2<- L;
;
;	/* END WORD ADDR LOOP
	!1,2,ONEMORE,CTOPL;
EM01317>	L<- TWICE-1;
EM01322>	TWICE<- L, SH<0;
EM01323>	L<- RAST2, :ONEMORE;	USE RAST2 2ND TIME THRU
EM01320> ONEMORE:	RAST1<- L;
EM01324>	L<- DESTY, TASK;	USE DESTY 2ND TIME THRU
EM01325>	YMUL<- L;
EM01326>	L<- DWAX;		USE DWAX 2ND TIME THRU
EM01327>	T<- DESTX;	CAREFUL - DESTX=SWA!!
EM01330>	SWA<- L, L<- T;	USE DESTX 2ND TIME THRU
EM01331>	T<- DWAOFF, :DOSWA;	AND DO IT AGAIN FOR DWAX, DESTX
;	**TWICE, VINC REGS NOW FREE**
;
;	/* CALCULATE TOPLD
	!1,2,CTOP1,CSKEW;
	!1,2,HM1,H1;
	!1,2,NOTOPL,TOPL;
EM01321> CTOPL:	L<- SKEW, BUS=0, TASK;	IF SKEW=0 THEN 0, ELSE
EM01340> CTX:	IR<- 0, :CTOP1;
EM01332> CTOP1:	T<- SRCX;	(SKEW GR SRCX.17) XOR (HINC EQ 0)
EM01341>	L<- HINC-1;
EM01342>	T<- 17.T, SH=0;	TEST HINC
EM01343>	L<- SKEW-T-1, :HM1;
EM01335> H1:	T<- HINC, SH<0;
EM01344>	L<- SWA+T, :NOTOPL;
EM01334> HM1:	T<- LREG;		IF HINC=-1, THEN FLIP
EM01345>	L<- 0-T-1, :H1;	THE POLARITY OF THE TEST
EM01336> NOTOPL:	SINK<- HINC, BUSODD, TASK, :CTX;	HINC FORCES BUSODD
EM01337> TOPL:	SWA<- L, TASK;		(DISP <- 100 FOR TOPLD)
EM01346>	IR<- 100, :CSKEW;
;	**HINC REG NOW FREE**
;
;	/* CALCULATE SKEW MASK
	!1,2,THINC,BCOM1;
	!1,2,COMSK,NOCOM;
EM01333> CSKEW:	T<- SKEW, BUS=0;	IF SKEW=0, THEN COMP
EM01347>	MAR<- LASTMASKP1-T-1, :THINC;
EM01350> THINC:	L<-HINC-1;
EM01354>	SH=0;			IF HINC=-1, THEN COMP
EM01351> BCOM1:	T<- ALLONES, :COMSK;
EM01352> COMSK:	L<- MD XOR T, :GFN;
EM01353> NOCOM:	L<- MD, :GFN;
;
;	/* GET FUNCTION
EM01355> GFN:	MAR<- AC2;
EM01356>	SKMSK<- L;

EM01357>	T- MD;
EM01360>	L<- DISP+T, TASK;
EM01361>	IR<- LREG, :BENTR;		DISP _<-DISP .OR. FUNCTION

;	BITBLT WORK - VERT AND HORIZ LOOPS WITH 4 SOURCES, 4 FUNCTIONS
;-----------------------------------------------------------------------
;
;	/* VERTICAL LOOP: UPDATE SWA, DWAX
	!1,2,DO0,VLOOP;
VLOOP:	T_ SWA;
	L_ RAST1+T;	INC SWA BY DELTA
	SWA_ L;
	T_ DWAX;
	L_ RAST2+T, TASK;	INC DWAX BY DELTA
	DWAX_ L;
;
;	/* TEST FOR DONE, OR NEED GRAY
	!1,2,MOREV,DONEV;
	!1,2,BMAYBE,BNOINT;
	!1,2,BDOINT,BDIS0;
	!1,2,DOGRAY,NOGRAY;
EM01371> BENTR:	L<- T<- NLINES-1;		DECR NLINES AND CHECK IF DONE
EM01402>	NLINES<- L, SH<0;
EM01403>	L<- NWW, BUS=0, :MOREV;	CHECK FOR INTERRUPTS
EM01372> MOREV:	L<- 3.T, :BMAYBE, SH<0;	CHECK DISABLED   ***V3 change
EM01375> BNOINT:	SINK<- DISP, SINK<- lgm10, BUS=0, :BDIS0, TASK;
EM01374> BMAYBE:	SINK<- DISP, SINK<- lgm10, BUS=0, :BDOINT, TASK;	TEST IF NEED GRAY(FUNC=8,12)
EM01377> BDIS0:	CONST<- L, :DOGRAY;   ***V3 change
;
;	/* INTERRUPT SUSPENSION (POSSIBLY)
	!1,1,DOI1;	MAY GET AN OR-1
EM01376> BDOINT:	:DOI1;	TASK HERE
EM01405> DOI1:	T<- AC2;
EM01404>	MAR<- DHOFF+T;		NLINES DONE = HT-NLINES-1
EM01406>	T<- NLINES;
EM01407>	L<- PC-1;		BACK UP THE PC, SO WE GET RESTARTED
EM01410>	PC<- L;
EM01411>	L<- MD-T-1, :BLITX, TASK;	...WITH NO LINES DONE IN AC1
;
;	/* LOAD GRAY FOR THIS LINE (IF FUNCTION NEEDS IT)
	!1,2,PRELD,NOPLD;
EM01400> DOGRAY:	T<- CONST-1;
EM01414>	T<- GRAYOFF+T+1;
EM01415>	MAR<- AC2+T;
EM01416>	NOP;	UGH
EM01417>	L<- MD;
EM01401> NOGRAY:	SINK<- DISP, SINK<- lgm100, BUS=0, TASK;	TEST TOPLD
EM01420>	CONST<- L, :PRELD;
;
;	/* NORMAL COMPLETION
EM01177> NEGWID:	L<- 0, :BLITX, TASK;
EM01373> DONEV:	L<- 0, :BLITX, TASK;	MAY BE AN OR-1 HERE!
EM01137> BLITX:	AC1<- L, :FINBLT;
;
;	/* PRELOAD OF FIRST SOURCE WORD (DEPENDING ON ALIGNMENT)
	!1,2,AB1,NB1;
EM01412> PRELD:	SINK<- DISP, SINK<- lgm40, BUS=0;	WHICH BANK
EM01421>	T<- HINC, :AB1;
NB1:	MAR_ SWA-T, :XB1;	(NORMAL BANK)
EM01422> AB1:	XMAR<- SWA-T, :XB1;	(ALTERNATE BANK)
EM01424> XB1:	NOP;
EM01425>	L<- MD, TASK;
EM01426>	WORD2<- L, :NOPLD;
;
;
;	/* HORIZONTAL LOOP - 3 CALLS FOR 1ST, MIDDLE AND LAST WORDS
	!1,2,FDISPA,LASTH;
	%17,17,14,DON0,,DON2,DON3;		CALLERS OF HORIZ LOOP
;	NOTE THIS IGNORES 14-BITS, SO lgm14 WORKS LIKE L_0 FOR RETN
	!14,1,LH1;	IGNORE RESULTING BUS
EM01413> NOPLD:	L_ 3, :FDISP;		CALL #3 IS FIRST WORD
DON3:	L_ NWORDS;
	HCNT_ L, SH<0;		HCNT COUNTS WHOLE WORDS
DON0:	L_ HCNT-1, :DO0;	IF NEG, THEN NO MIDDLE OR LAST
DO0:	HCNT_ L, SH<0;		CALL #0 (OR-14!) IS MIDDLE WORDS
;	UGLY HACK SQUEEZES 2 INSTRS OUT OF INNER LOOP:
	L_ DISP, SINK_ lgm14, BUS, TASK, :FDISPA;	(WORKS LIKE L_0)
LASTH:	:LH1;	TASK AND BUS PENDING
LH1:	L_ 2, :FDISP;		CALL #2 IS LAST WORD
DON2:	:VLOOP;
;
;
;	/* HERE ARE THE SOURCE FUNCTIONS
	!17,20,,,,F0,,,,F1,,,,F2,,,,F3;	IGNORE OP BITS IN FUNCTION CODE
	!17,20,,,,F0A,,,,F1A,,,,F2A,,,, ;	SAME FOR WINDOW RETURNS
	!3,4,OP0,OP1,OP2,OP3;
	!1,2,AB2,NB2;
EM01433> FDISP:	SINK_ DISP, SINK_lgm14, BUS, TASK;
FDISPA:	RETN_ L, :F0;
F0:	SINK_ DISP, SINK_ lgm40, BUS=0, :WIND;	FUNC 0 - WINDOW
F1:	SINK_ DISP, SINK_ lgm40, BUS=0, :WIND;	FUNC 1 - NOT WINDOW
F1A:	T_ CYCOUT;
	L_ ALLONES XOR T, TASK, :F3A;
F2:	SINK_ DISP, SINK_ lgm40, BUS=0, :WIND;	FUNC 2 - WINDOW .AND. GRAY
F2A:	T_ CYCOUT;
	L_ ALLONES XOR T;
	SINK_ DISP, SINK_ lgm20, BUS=0;	WHICH BANK
	TEMP_ L, :AB2;		TEMP _ NOT WINDOW
NB2:	MAR_ DWAX, :XB2;	(NORMAL BANK)
AB2:	XMAR_ DWAX, :XB2;	(ALTERNATE BANK)
XB2:	L_ CONST AND T;		WINDOW .AND. GRAY
	T_ TEMP;
	T_ MD .T;		DEST.AND.NOT WINDOW
	L_ LREG OR T, TASK, :F3A;		(TRANSPARENT)
F3:	L_ CONST, TASK, :F3A;	FUNC 3 - CONSTANT (COLOR)
;
;
;	/* AFTER GETTING SOURCE, START MEMORY AND DISPATCH ON OP
	!1,2,AB3,NB3;
F3A:	CYCOUT_ L;	(TASK HERE)
F0A:	SINK_ DISP, SINK_ lgm20, BUS=0;	WHICH BANK
	SINK_ DISP, SINK_ lgm3, BUS, :AB3;	DISPATCH ON OP
NB3:	T_ MAR_ DWAX, :OP0;	(NORMAL BANK)
AB3:	T_ XMAR_ DWAX, :OP0;	(ALTERNATE BANK)
;
;
;	/* HERE ARE THE OPERATIONS - ENTER WITH SOURCE IN CYCOUT
	%16,17,15,STFULL,STMSK;	MASKED OR FULL STORE (LOOK AT 2-BIT)
;				OP 0 - SOURCE
OP0:	SINK_ RETN, BUS;	TEST IF UNMASKED
OP0A:	L_ HINC+T, :STFULL;	ELSE :STMSK
OP1:	T_ CYCOUT;		OP 1 - SOURCE .OR. DEST
	L_ MD OR T, :OPN;
OP2:	T_ CYCOUT;		OP 2 - SOURCE .XOR. DEST
	L_ MD XOR T, :OPN;
OP3:	T_ CYCOUT;		OP 3 - (NOT SOURCE) .AND. DEST
	L_ 0-T-1;
	T_ LREG;
	L_ MD AND T, :OPN;
OPN:	SINK_ DISP, SINK_ lgm20, BUS=0, TASK;	WHICH BANK
	CYCOUT_ L, :AB3;
;
;
;	/* STORE MASKED INTO DESTINATION
	!1,2,STM2,STM1;
	!1,2,AB4,NB4;
STMSK:	L_ MD;
	SINK_ RETN, BUSODD, TASK;	DETERMINE MASK FROM CALL INDEX
	TEMP_ L, :STM2;		STACHE DEST WORD IN TEMP
STM1:	T_MASK1, :STM3;
STM2:	T_MASK2, :STM3;
STM3:	L_ CYCOUT AND T;  ***X24. Removed TASK clause.
	CYCOUT_ L, L_ 0-T-1;	AND INTO SOURCE
	T_ LREG;		T_ MASK COMPLEMENTED
	T_ TEMP .T;		AND INTO DEST
	L_ CYCOUT OR T;		OR TOGETHER THEN GO STORE
	SINK_ DISP, SINK_ lgm20, BUS=0, TASK;	WHICH BANK
	CYCOUT_ L, :AB4;
NB4:	T_ MAR_ DWAX, :OP0A;	(NORMAL BANK)
AB4:	T_ XMAR_ DWAX, :OP0A;	(ALTERNATE BANK)
;
;	/* STORE UNMASKED FROM CYCOUT (L=NEXT DWAX)
STFULL:	MD_ CYCOUT;
STFUL1:	SINK_ RETN, BUS, TASK;
	DWAX_ L, :DON0;
;
;
;	/* WINDOW SOURCE FUNCTION
;	TASKS UPON RETURN, RESULT IN CYCOUT
	!1,2,DOCY,NOCY;
	!17,1,WIA;
	!1,2,NZSK,ZESK;
	!1,2,AB5,NB5;
WIND:	L_ T_ SKMSK, :AB5;	ENTER HERE (8 INST TO TASK)
NB5:	MAR_ SWA, :XB5;		(NORMAL BANK)
AB5:	XMAR_ SWA, :XB5;	(ALTERNATE BANK)
XB5:	L_ WORD2.T, SH=0;
	CYCOUT_ L, L_ 0-T-1, :NZSK;	CYCOUT_ OLD WORD .AND. MSK
ZESK:	L_ MD, TASK;	ZERO SKEW BYPASSES LOTS
	CYCOUT_ L, :NOCY;
NZSK:	T_ MD;
	L_ LREG.T;
	TEMP_ L, L_T, TASK;	TEMP_ NEW WORD .AND. NOTMSK
	WORD2_ L;
	T_ TEMP;
	L_ T_ CYCOUT OR T;		OR THEM TOGETHER
	CYCOUT_ L, L_ 0+1, SH=0;	DONT CYCLE A ZERO ***X21.
	SINK_ SKEW, BUS, :DOCY;
DOCY:	CYRET_ L LSH 1, L_ T, :L0;	CYCLE BY SKEW ***X21.
NOCY:	T_ SWA, :WIA;	(MAY HAVE OR-17 FROM BUS)
CYX2:	T_ SWA;
WIA:	L_ HINC+T;
	SINK_ DISP, SINK_ lgm14, BUS, TASK;	DISPATCH TO CALLER 
	SWA_ L, :F0A;

;	THE DISK CONTROLLER

;	ITS REGISTERS:
$DCBR		$R34;
$KNMAR		$R33;
$CKSUMR		$R32;
$KWDCT		$R31;
$KNMARW		$R33;
$CKSUMRW	$R32;
$KWDCTW		$R31;

;	ITS TASK SPECIFIC FUNCTIONS AND BUS SOURCES:
$KSTAT		$L020012,014003,124100;	DF1 = 12 (LHS) BS = 3 (RHS)
$RWC		$L024011,000000,000000;	NDF2 = 11
$RECNO		$L024012,000000,000000;	NDF2 = 12
$INIT		$L024010,000000,000000;	NDF2 = 10
$CLRSTAT	$L016014,000000,000000;	NDF1 = 14
$KCOMM		$L020015,000000,124000;	DF1 = 15 (LHS only) Requires bus def
$SWRNRDY	$L024014,000000,000000;	NDF2 = 14
$KADR		$L020016,000000,124000;	DF1 = 16 (LHS only) Requires bus def
$KDATA		$L020017,014004,124100;	DF1 = 17 (LHS)  BS = 4 (RHS)
$STROBE		$L016011,000000,000000;	NDF1 = 11
$NFER		$L024015,000000,000000;	NDF2 = 15
$STROBON	$L024016,000000,000000;	NDF2 = 16
$XFRDAT		$L024013,000000,000000;	NDF2 = 13
$INCRECNO	$L016013,000000,000000;	NDF1 = 13

;	THE DISK CONTROLLER COMES IN TWO PARTS. THE SECTOR
;	TASK HANDLES DEVICE CONTROL AND COMMAND UNDERSTANDING
;	AND STATUS REPORTING AND THE LIKE. THE WORD TASK ONLY
;	RUNS AFTER BEING ENABLED BY THE SECTOR TASK AND
;	ACTUALLY MOVES DATA WORDS TO AND FRO. 

;   THE SECTOR TASK

;	LABEL PREDEFINITIONS:
!1,2,COMM,NOCOMM;
!1,2,COMM2,IDLE1;
!1,2,BADCOMM,COMM3;
!1,2,COMM4,ILLSEC;
!1,2,COMM5,WHYNRDY;
!1,2,STROB,CKSECT;
!1,2,STALL,CKSECT1;
!1,2,KSFINI,CKSECT2;
!1,2,IDLE2,TRANSFER;
!1,2,STALL2,GASP;
!1,2,INVERT,NOINVERT;

SE00004> KSEC:	MAR<- KBLKADR2;
SE01574> KPOQ:	CLRSTAT;	RESET THE STORED DISK ADDRESS
SE01575>	MD<-L<-ALLONES+1, :GCOM2;	ALSO CLEAR DCB POINTER

GETCOM:	MAR_KBLKADR;	GET FIRST DCB POINTER
GCOM1:	NOP;
	L_MD;
SE01601> GCOM2:	DCBR<-L,TASK;
SE01602>	KCOMM<-TOWTT;	IDLE ALL DATA TRANSFERS

SE01603>	MAR<-KBLKADR3;	GENERATE A SECTOR INTERRUPT
SE01604>	T<-NWW;
SE01605>	L<-MD OR T;

SE01606>	MAR<-KBLKADR+1;	STORE THE STATUS
SE01607>	NWW<-L, TASK;
SE01610>	MD<-KSTAT;

SE01611>	MAR<-KBLKADR;	WRITE THE CURRENT DCB POINTER
SE01612>	KSTAT<-5;	INITIAL STATUS IS INCOMPLETE
SE01613>	L<-DCBR,TASK,BUS=0;
SE01614>	MD<-DCBR, :COMM;

;	BUS=0 MAPS COMM TO NOCOMM

SE01546> COMM:	T<-2;	GET THE DISK COMMAND
SE01615>	MAR<-DCBR+T;
SE01616>	T<-TOTUWC;
SE01617>	L<-MD XOR T, TASK, STROBON;
SE01620>	KWDCT<-L, :COMM2;

;	STROBON MAPS COMM2 TO IDLE1

SE01550> COMM2:	T<-10;	READ NEW DISK ADDRESS
SE01621>	MAR<-DCBR+T+1;
SE01622>	T<-KWDCT;
SE01623>	L<-ONE AND T;
SE01624>	L<- -400 AND T, SH=0;
SE01625>	T<-MD, SH=0, :INVERT;

;	SH=0 MAPS INVERT TO NOINVERT

SE01572> INVERT:	L<-2 XOR T, TASK, :BADCOMM;
SE01573> NOINVERT: L<-T, TASK, :BADCOMM;

;	SH=0 MAPS BADCOMM TO COMM3

COMM3:	KNMAR_L;

SE01626>	MAR<-KBLKADR2;	WRITE THE NEW DISK ADDRESS
SE01627>	T<-SECT2CM;	CHECK FOR SECTOR > 13
SE01630>	L<-T<-KDATA<-KNMAR+T;	NEW DISK ADDRESS TO HARDWARE
SE01631>	KADR<-KWDCT,ALUCY;	DISK COMMAND TO HARDWARE
SE01632>	L<-MD XOR T,TASK, :COMM4;	COMPARE OLD AND NEW DISK ADDRESSES

;	ALUCY MAPS COMM4 TO ILLSEC

SE01554> COMM4:	CKSUMR<-L;

SE01633>	MAR<-KBLKADR2;	WRITE THE NEW DISK ADDRESS
SE01634>	T<-CADM,SWRNRDY;	SEE IF DISK IS READY
SE01635>	L<-CKSUMR AND T, :COMM5;

;	SWRNRDY MAPS COMM5 TO WHYNRDY

COMM5:	MD_KNMAR;	COMPLETE THE WRITE
	SH=0,TASK;
	:STROB;

;	SH=0 MAPS STROB TO CKSECT

CKSECT:	T_KNMAR,NFER;
	L_KSTAT XOR T, :STALL;

;	NFER MAPS STALL TO CKSECT1

CKSECT1: CKSUMR_L,XFRDAT;
	T_CKSUMR, :KSFINI;

;	XFRDAT MAPS KSFINI TO CKSECT2

CKSECT2: L_SECTMSK AND T;
KSLAST:	BLOCK,SH=0;
GASP:	TASK, :IDLE2;

;	SH=0 MAPS IDLE2 TO TRANSFER

TRANSFER: KCOMM_TOTUWC;	TURN ON THE TRANSFER

!1,2,ERRFND,NOERRFND;
!1,2,EF1,NEF1;

DMPSTAT: T_COMERR1;	SEE IF STATUS REPRESENTS ERROR
	L_KSTAT AND T;
	MAR_DCBR+1;	WRITE FINAL STATUS
	KWDCT_L,TASK,SH=0;
	MD_KSTAT,:ERRFND;

;	SH=0 MAPS ERRFND TO NOERRFND

NOERRFND: T_6;	PICK UP NO-ERROR INTERRUPT WORD

INTCOM:	MAR_DCBR+T;
	T_NWW;
	L_MD OR T;
	SINK_KWDCT,BUS=0,TASK;
	NWW_L,:EF1;

;	BUS=0 MAPS EF1 TO NEF1

NEF1:	MAR_DCBR,:GCOM1;	FETCH ADDRESS OF NEXT CONTROL BLOCK

ERRFND:	T_7,:INTCOM;	PICK UP ERROR INTERRUPT WORD

SE01646> EF1:	:KSEC;

NOCOMM:	L_ALLONES,CLRSTAT,:KSLAST;

IDLE1:	L_ALLONES,:KSLAST;

IDLE2:	KSTAT_LOW14, :GETCOM;	NO ACTIVITY THIS SECTOR

SE01552> BADCOMM: KSTAT<-7;	ILLEGAL COMMAND ONLY NOTED IN KBLK STAT
SE01661>	BLOCK;
SE01662>	TASK,:EF1;

WHYNRDY: NFER;
STALL:	BLOCK, :STALL2;

;	NFER MAPS STALL2 TO GASP

STALL2:	TASK;
	:DMPSTAT;

ILLSEC:	KSTAT_7, :STALL;	ILLEGAL SECTOR SPECIFIED

STROB:	CLRSTAT;
	L_ALLONES,STROBE,:CKSECT1;

KSFINI:	KSTAT_4, :STALL;	COMMAND FINISHED CORRECTLY


;DISK WORD TASK
;WORD TASK PREDEFINITIONS
!37,37,,,,RP0,INPREF1,CKP0,WP0,,PXFLP1,RDCK0,WRT0,REC1,,REC2,REC3,,,REC0RC,REC0W,R0,,CK0,W0,,R2,,W2,,REC0,,KWD;
!1,2,RW1,RW2;
!1,2,CK1,CK2;
!1,2,CK3,CK4;
!1,2,CKERR,CK5;
!1,2,PXFLP,PXF2;
!1,2,PREFDONE,INPREF;
!1,2,,CK6;
!1,2,CKSMERR,PXFLP0;

KWD:	BLOCK,:REC0;

;	SH<0 MAPS REC0 TO REC0
;	ANYTHING=INIT MAPS REC0 TO KWD

REC0:	L_2, TASK;	LENGTH OF RECORD 0 (ALLOW RELEASE IF BLOCKED) 
	KNMARW_L;

	T_KNMARW, BLOCK, RWC;	 GET ADDR OF MEMORY BLOCK TO TRANSFER
	MAR_DCBR+T+1, :REC0RC;

;	WRITE MAPS REC0RC TO REC0W
;	INIT MAPS REC0RC TO KWD

REC0RC:	T_MFRRDL,BLOCK, :REC12A;	FIRST RECORD READ DELAY
REC0W:	T_MFR0BL,BLOCK, :REC12A;	FIRST RECORD 0'S BLOCK LENGTH

REC1:	L_10, INCRECNO;	 LENGTH OF RECORD 1 
	T_4, :REC12;
REC2:	L_PAGE1, INCRECNO;	 LENGTH OF RECORD 2 
	T_5, :REC12;
REC12:	MAR_DCBR+T, RWC;	 MEM BLK ADDR FOR RECORD
	KNMARW_L, :RDCK0;

;	RWC=WRITE MAPS RDCK0 INTO WRT0
;	RWC=INIT MAPS RDCK0 INTO KWD

RDCK0:	T_MIRRDL, :REC12A;
WRT0:	T_MIR0BL, :REC12A;

REC12A:	L_MD;
	KWDCTW_L, L_T;
COM1:	KCOMM_ STUWC, :INPREF0;

INPREF:	L_CKSUMRW+1, INIT, BLOCK;
INPREF0: CKSUMRW_L, SH<0, TASK, :INPREF1;

;	INIT MAPS INPREF1 TO KWD

INPREF1: KDATA_0, :PREFDONE;

;	SH<0 MAPS PREFDONE TO INPREF

PREFDONE: T_KNMARW;	COMPUTE TOP OF BLOCK TO TRANSFER
KW00016> KWDX:	L<-KWDCTW+T,RWC;		(ALSO USED FOR RESET)
KW01742>	KNMARW<-L,BLOCK,:RP0;

;	RWC=CHECK MAPS RP0 TO CKP0
;	RWC=WRITE MAPS RP0 AND CKP0 TO WP0
;	RWC=INIT MAPS RP0, CKP0, AND WP0 TO KWD

KW01704> RP0:	KCOMM<-STRCWFS,:WP1;

KW01706> CKP0:	L<-KWDCTW-1;	ADJUST FINISHING CONDITION BY 1 FOR CHECKING ONLY
KW01743>	KWDCTW<-L,:RP0;

WP0:	KDATA_ONE;	WRITE THE SYNC PATTERN
KW01744> WP1:	L_KBLKADR,TASK,:RW1;	INITIALIZE THE CHECKSUM AND ENTER XFER LOOP


KW01745> XFLP:	T<-L<-KNMARW-1;	BEGINNING OF MAIN XFER LOOP
KW01746>	KNMARW<-L;
KW01747>	MAR<-KNMARW,RWC;
KW01750>	L<-KWDCTW-T,:R0;

;	RWC=CHECK MAPS R0 TO CK0
;	RWC=WRITE MAPS R0 AND CK0 TO W0
;	RWC=INIT MAPS R0, CK0, AND W0 TO KWD

KW01724> R0:	T<-CKSUMRW,SH=0,BLOCK;
KW01751>	MD<-L<-KDATA XOR T,TASK,:RW1;

;	SH=0 MAPS RW1 TO RW2

KW01666> RW1:	CKSUMRW<-L,:XFLP;

W0:	T_CKSUMRW,BLOCK;
	KDATA_L_MD XOR T,SH=0;
	TASK,:RW1;

;	AS ALREADY NOTED, SH=0 MAPS RW1 TO RW2

CK0:	T_KDATA,BLOCK,SH=0;
	L_MD XOR T,BUS=0,:CK1;

;	SH=0 MAPS CK1 TO CK2

CK1:	L_CKSUMRW XOR T,SH=0,:CK3;

;	BUS=0 MAPS CK3 TO CK4

CK3:	TASK,:CKERR;

;	SH=0 MAPS CKERR TO CK5

CK5:	CKSUMRW_L,:XFLP;

CK4:	MAR_KNMARW, :CK6;

;	SH=0 MAPS CK6 TO CK6

CK6:	CKSUMRW_L,L_0+T;
	MTEMP_L,TASK;
	MD_MTEMP,:XFLP;

CK2:	L_CKSUMRW-T,:R2;

;	BUS=0 MAPS R2 TO R2

RW2:	CKSUMRW_L;

	T_KDATA_CKSUMRW,RWC;	THIS CODE HANDLES THE FINAL CHECKSUM
	L_KDATA-T,BLOCK,:R2;

;	RWC=CHECK NEVER GETS HERE
;	RWC=WRITE MAPS R2 TO W2
;	RWC=INIT MAPS R2 AND W2 TO KWD

R2:	L_MRPAL, SH=0;	SET READ POSTAMBLE LENGTH, CHECK CKSUM
	KCOMM_TOTUWC, :CKSMERR;

;	SH=0 MAPS CKSMERR TO PXFLP0

W2:	L_MWPAL, TASK;	SET WRITE POSTAMBLE LENGTH
	CKSUMRW_L, :PXFLP;

CKSMERR: KSTAT_0,:PXFLP0;	0 MEANS CHECKSUM ERROR .. CONTINUE

PXFLP:	L_CKSUMRW+1, INIT, BLOCK;
PXFLP0:	CKSUMRW_L, TASK, SH=0, :PXFLP1;

;	INIT MAPS PXFLP1 TO KWD

PXFLP1:	KDATA_0,:PXFLP;

;	SH=0 MAPS PXFLP TO PXF2

PXF2:	RECNO, BLOCK;	DISPATCH BASED ON RECORD NUMBER
	:REC1;

;	RECNO=2 MAPS REC1 INTO REC2
;	RECNO=3 MAPS REC1 INTO REC3
;	RECNO=INIT MAPS REC1 INTO KWD

REC3:	KSTAT_4,:PXFLP;	4 MEANS SUCCESS!!!

CKERR:	KCOMM_TOTUWC;	TURN OFF DATA TRANSFER
	L_KSTAT_6, :PXFLP1;	SHOW CHECK ERROR AND LOOP

;The Parity Error Task
;Its label predefinition is way earlier
;It dumps the following interesting registers:
;614/ DCBR	Disk control block
;615/ KNMAR	Disk memory address
;616/ DWA	Display memory address
;617/ CBA	Display control block
;620/ PC	Emulator program counter
;621/ SAD	Emulator temporary register for indirection

PA00015> PART:	T<- 10;
PA01765>	L<- ALLONES;		TURN OFF MEMORY INTERRUPTS
PA01766>	MAR<- ERRCTRL, :PX1;
PA00450> PR8:	L<- SAD, :PX;
PA00447> PR7:	L<- PC, :PX;
PA00446> PR6:	L<- CBA, :PX;
PA00445> PR5:	L<- DWA, :PX;
PA00444> PR4:	L<- KNMAR, :PX;
PA00443> PR3:	L<- DCBR, :PX;
PA00442> PR2:	L<- NWW OR T, TASK;	T CONTAINS 1 AT THIS POINT
PA00440> PR0:	NWW<- L, :PART;

PA01767> PX:	MAR<- 612+T;
PA01770> PX1:	MTEMP<- L, L<- T;
PA01771>	MD<- MTEMP;
PA01772>	CURDATA<- L;		THIS CLOBBERS THE CURSOR FOR ONE 
PA01773>	T<- CURDATA-1, BUS;	FRAME WHEN AN ERROR OCCURS
PA01774>	:PR0;

AltoIIMRT4K.mu:
;
; last modified December 1, 1977  1:14 AM
;
; This is the part of the Memory Refresh Task which
; is specific to Alto IIs WITHOUT Extended memory.
;
; Copyright Xerox Corporation 1979
$EngNumber	$20000;		ALTO 2 WITHOUT EXTENDED MEMORY

MRT:	SINK_ MOUSE, BUS;	MOUSE DATA IS ANDED WITH 17B
MRTA:	L_ T_ -2, :TX0;		DISPATCH ON MOUSE CHANGE
TX0:	L_ T_ R37 AND NOT T;	UPDATE REFRESH ADDRESS
	T_ 3+T+1, SH=0;
	L_ REFIIMSK ANDT, :DOTIMER;
NOTIMER:R37_ L; 		STORE UPDATED REFRESH ADDRESS
TIMERTN:L_ REFZERO AND T;
	SH=0;			TEST FOR CLOCK TICK
	:NOCLK;
NOCLK:	MAR_ R37;		FIRST FEFRESH CYCLE
	L_ CURX;
	T_ 2, SH=0;
	MAR_ R37 XORT, :DOCUR;  SECOND REFRESH CYCLE
NOCUR:	CURDATA_ L, TASK;
MRTLAST:CURDATA_ L, :MRT;

DOTIMER:R37_ L;			SAVE REFRESH ADDRESS
	MAR_EIALOC;		INTERVAL TIMER/EIA INTERFACE
	L_2 AND T;
	SH=0, L_T_REFZERO.T;	***V3 CHANGE (USED TO BE BIAS)
	CURDATA_L, :SPCHK;	CURDATA_CURRENT TIME WITHOUT CONTROL BITS

SPCHK:	SINK_MD, BUS=0, TASK;	CHECK FOR EIA LINE SPACING
SPIA:	:NOTIMERINT, CLOCKTEMP_L;

NOSPCHK:L_MD;			CHECK FOR TIME=NOW
	MAR_TRAPDISP-1;		CONTAINS TIME AT WHICH INTERRUPT SHOULD HAPPEN
	MTEMP_L;		IF INTERRUPT IS CAUSED,
	L_ MD-T;		LINE STATE WILL BE STORED
	SH=0, TASK, L_MTEMP, :SPIA;

TIMERINT:MAR_ ITQUAN;		STORE THE THING IN CLOCKTEMP AT ITQUAN
	L_ CURDATA;
	R37_ L;
	T_NWW;			AND CAUSE AN INTERRUPT ON THE CHANNELS 
	MD_CLOCKTEMP;		SPECIFIED BY ITQUAN+1
	L_MD OR T, TASK;
	NWW_L;

NOTIMERINT: T_R37, :TIMERTN;

;The rest of MRT, starting at the label CLOCK is unchanged

AltoIIMRT16K.mu:
;
; last modified December 1, 1977  1:13 AM
;
; This is the part of the Memory Refresh Task which
; is specific to Alto IIs with Extended memory.
;
; Copyright Xerox Corporation 1979
$EngNumber	$30000;		ALTO II WITH EXTENDED MEMORY
;
; This version assumes MRTACT is cleared by BLOCK, not MAR_ R37
; R37 [4-13] are the low bits of the TOD clock
; R37 [8-14] are the refresh address bits
; Each time MRT runs, four refresh addresses are generated, though
; R37 is incremented only once.  Sprinkled throughout the execution
; of this code are the following operations having to do with refresh:
;	MAR_ R37
;	R37_ R37 +4		NOTE THAT R37 [14] DOES NOT CHANGE
;	MAR_ R37 XOR 2		TOGGLES BIT 14
;	MAR_ R37 XOR 200	TOGGLES BIT 8
;	MAR_ R37 XOR 202	TOGGLES BITS 8 AND 14

MRT:	MAR_ R37;		**FIRST REFRESH CYCLE**
	SINK_ MOUSE, BUS;	MOUSE DATA IS ANDED WITH 17B
MRTA:	L_ T_ -2, :TX0;		DISPATCH ON MOUSE CHANGE
TX0:	L_ R37 AND NOT T, T_ R37;INCREMENT CLOCK
	T_ 3+T+1, SH=0;		IE. T_ T +4.  IS INTV TIMER ON?
	L_ REFIIMSK AND T, :DOTIMER; [DOTIMER,NOTIMER] ZERO HIGH 4 BITS
NOTIMER: R37_ L; 		STORE UPDATED CLOCK
NOTIMERINT: T_ 2;		NO STATE AT THIS POINT IN PUBLIC REGS
	MAR_ R37 XOR T,T_ R37;	**SECOND REFRESH CYCLE**
	L_ REFZERO AND T;	ONLY THE CLOKCK BITS, PLEASE
	SH=0, TASK;		TEST FOR CLOCK OVERFLOW
	:NOCLK;			[NOCLK,CLOCK]
NOCLK:	T _ 200;
	MAR_ R37 XOR T;		**THIRD FEFRESH CYCLE**
	L_ CURX, BLOCK;		CLEARS WAKEUP REQUEST FF
	T_ 2 OR T, SH=0;	NEED TO CHECK CURSOR?
	MAR_ R37 XOR T, :DOCUR;	**FOURTH REFRESH CYCLE**
NOCUR:	CURDATA_ L, TASK;
MRTLAST:CURDATA_ L, :MRT;	END OF MAIN LOOP

DOTIMER:R37_ L;			STORE UPDATED CLOCK
	MAR_ EIALOC;		INTERVAL TIMER/EIA INTERFACE
	L_ 2 AND T;
	SH=0, L_ T_ REFZERO.T;	***V3 CHANGE (USED TO BE BIAS)
	CURDATA_L, :SPCHK;	CURDATA_ CURRENT TIME WITHOUT CONTROL BITS

SPCHK:	SINK_ MD, BUS=0, TASK;	CHECK FOR EIA LINE SPACING
SPIA:	:NOTIMERINT, CLOCKTEMP_ L;

NOSPCHK:L_MD;			CHECK FOR TIME = NOW
	MAR_TRAPDISP-1;		CONTAINS TIME AT WHICH INTERRUPT SHOULD HAPPEN
	MTEMP_L;		IF INTERRUPT IS CAUSED,
	L_ MD-T;		LINE STATE WILL BE STORED
	SH=0, TASK, L_MTEMP, :SPIA;

TIMERINT:MAR_ ITQUAN;		STORE THE THING IN CLOCKTEMP AT ITQUAN
	L_ CURDATA;
	R37_ L;
	T_NWW;			AND CAUSE AN INTERRUPT ON THE CHANNELS 
	MD_CLOCKTEMP;		SPECIFIED BY ITQUAN+1
	L_MD OR T, TASK;
	NWW_L,:NOTIMERINT;

;The rest of MRT, starting at the label CLOCK is unchanged

