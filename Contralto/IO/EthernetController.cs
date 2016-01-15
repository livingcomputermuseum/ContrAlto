using Contralto.CPU;
using Contralto.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contralto.IO
{
    public class EthernetController
    {
        public EthernetController(AltoSystem system)
        {
            _system = system;

            _receiverLock = new System.Threading.ReaderWriterLockSlim();

            _fifo = new Queue<ushort>();
            Reset();

            _fifoTransmitWakeupEvent = new Event(_fifoTransmitDuration, null, OutputFifoCallback);
            _fifoReceiveWakeupEvent = new Event(_fifoReceiveDuration, null, InputFifoCallback);           

            // Attach real Ethernet device if user has specified one, otherwise leave unattached; output data
            // will go into a bit-bucket.
            if (!String.IsNullOrEmpty(Configuration.HostEthernetInterfaceName))
            {
                _hostEthernet = new HostEthernet(Configuration.HostEthernetInterfaceName);
                _hostEthernet.RegisterReceiveCallback(OnHostPacketReceived);                
            }

            // More words than the Alto will ever send.
            _outputData = new ushort[4096];
        }

        public void Reset()
        {
            _pollEvent = null;

            ResetInterface();            
        }

        public byte Address
        {
            get { return Configuration.HostAddress; }
        }

        /// <summary>
        /// The ICMD and OCMD flip-flops, combined into a single value
        /// as written by STARTF.
        /// (bit 15 = OCMD, bit 14 = ICMD)
        /// </summary>
        public int IOCMD
        {
            get { return _ioCmd; }
        }

        public bool FIFOEmpty
        {
            get { return _fifo.Count == 0; }
        }

        public bool OperationDone
        {
            get { return !_oBusy && !_iBusy; }
        }

        public bool Collision
        {
            get { return _collision; }
        }

        public bool DataLate
        {
            get { return _dataLate; }
        }

        public ushort Status
        {
            get
            {
                return _status;
            }
        }

        public bool CountdownWakeup
        {
            get { return _countdownWakeup; }
            set { _countdownWakeup = value; }
        }

        public void ResetInterface()
        {
            _receiverLock.EnterWriteLock();
            // Latch status before resetting
            _status = (ushort)(
                       (0xffc0) |                        // bits always set                 
                       (_dataLate ? 0x00 : 0x20) |
                       (_collision ? 0x00 : 0x10) |
                       (_crcBad ? 0x00 : 0x08) |
                       ((~0 & 0x3) << 1) |              // TODO: we're clearing the IOCMD bits here early -- validate why this works.
                       (_incomplete ? 0x00 : 0x01));

            _ioCmd = 0;
            _oBusy = false;
            _iBusy = false;
            _dataLate = false;
            _collision = false;
            _crcBad = false;
            _incomplete = false;
            _fifo.Clear();
            _incomingPacket = null;
            _incomingPacketLength = 0;
            _inGone = false;
            //_packetReady = false;
            
            if (_system.CPU != null)
            {
                _system.CPU.BlockTask(TaskType.Ethernet);
            }

            Log.Write(LogComponent.EthernetController, "Interface reset.");
            _receiverLock.ExitWriteLock();
        }

        public ushort ReadInputFifo(bool lookOnly)
        {
            if (FIFOEmpty)
            {
                Log.Write(LogComponent.EthernetController, "Read from empty Ethernet FIFO, returning 0.");
                return 0;
            }

            ushort read = 0;

            if (lookOnly)
            {
                Log.Write(LogComponent.EthernetController, "Peek into FIFO, returning {0} (length {1})", Conversion.ToOctal(_fifo.Peek()), _fifo.Count);
                read = _fifo.Peek();
            }
            else
            {
                read = _fifo.Dequeue();
                Log.Write(LogComponent.EthernetController, "Read from FIFO, returning {0} (length now {1})", Conversion.ToOctal(read), _fifo.Count);

                if (_fifo.Count < 2)
                {                                        
                    if (_inGone)
                    {
                        //
                        // Receiver is done and we're down to the last word (the checksum)
                        // which never gets pulled from the FIFO.
                        // clear IBUSY to indicate to the microcode that we've finished.
                        //
                        _iBusy = false;
                        _system.CPU.WakeupTask(TaskType.Ethernet);
                    }
                    else
                    {
                        //
                        // Still more data, but we block the Ethernet task until it is put
                        // into the FIFO.
                        //
                        _system.CPU.BlockTask(TaskType.Ethernet);
                    }
                }                
            }

            return read;
        }

        public void WriteOutputFifo(ushort data)
        {
            if (_fifo.Count == 16)
            {
                Log.Write(LogComponent.EthernetController, "Write to full Ethernet FIFO, losing first entry.");
                _fifo.Dequeue();
            }
            
            _fifo.Enqueue(data);

            // If the FIFO is full, start transmitting and clear Wakeups            
            if (_fifo.Count == 15)
            {
                if (_oBusy)
                {
                    TransmitFIFO(false /* not end */);
                }
                _system.CPU.BlockTask(TaskType.Ethernet);
            }

            Log.Write(LogComponent.EthernetController, "FIFO written with {0}, length now {1}", data, _fifo.Count);
        }

        public void StartOutput()
        {
            // Sets the OBusy flip-flop in the interface
            _oBusy = true;            

            // Enables wakeups to fill the FIFO
            _system.CPU.WakeupTask(TaskType.Ethernet);

            Log.Write(LogComponent.EthernetController, "Output started.");
        }

        public void StartInput()
        {                  
            InitializeReceiver();

            Log.Write(LogComponent.EthernetController, "Input started.");
        }

        public void EndTransmission()
        {
            // Clear FIFO wakeup and transmit the remainder of the data in the FIFO
            TransmitFIFO(true /* end */);
            _system.CPU.BlockTask(TaskType.Ethernet);
            Log.Write(LogComponent.EthernetController, "Transmission ended.");
        }

        public void STARTF(ushort busData)
        {            
            Log.Write(LogComponent.EthernetController, "Ethernet STARTF {0}", Conversion.ToOctal(busData));

            // 
            // HW Manual, p. 54:
            // "The emulator task sets [the ICMD and OCMD flip flops] from BUS[14 - 15] with 
            // the STARTF function, causing the Ethernet task to wakeup, dispatch on them 
            // and then reset them with EPFCT."
            //
            _ioCmd = busData & 0x3;
            _system.CPU.WakeupTask(TaskType.Ethernet);
        }       

        private void TransmitFIFO(bool end)
        {
            // Schedule a callback to pick up the data and shuffle it out the host interface.
            _fifoTransmitWakeupEvent.Context = end;
            _fifoTransmitWakeupEvent.TimestampNsec = _fifoTransmitDuration;
            _system.Scheduler.Schedule(_fifoTransmitWakeupEvent);
        }

        private void OutputFifoCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            bool end = (bool)context;
            
            if (!_oBusy)
            {
                // If OBUSY is no longer set then the interface was reset before
                // we got to run; abandon this operation.                
                Log.Write(LogComponent.EthernetController, "FIFO callback after reset, abandoning output.");                
                return;
            }
            
            Log.Write(LogComponent.EthernetController, "Sending {0} words from fifo.", _fifo.Count);

            // Copy FIFO to host ethernet output buffer
            _fifo.CopyTo(_outputData, _outputIndex);
            _outputIndex += _fifo.Count;
            _fifo.Clear();

            if (!end)
            {
                // Enable FIFO microcode wakeups for next batch of data                
                _system.CPU.WakeupTask(TaskType.Ethernet);
            }     
            else
            {
                // This is the last of the data, clear the OBUSY flipflop, the transmitter is done.
                Log.Write(LogComponent.EthernetController, "Packet complete.");
                _oBusy = false;                                

                // Wakeup at end of transmission.  ("OUTGONE Post wakeup.")
                _system.CPU.WakeupTask(TaskType.Ethernet);

                // And actually tell the host ethernet interface to send the data.
                if (_hostEthernet != null)
                {
                    _hostEthernet.Send(_outputData, _outputIndex);
                }                
                
                _outputIndex = 0;
            }       
        }

        private void InitializeReceiver()
        {
            // " Sets the IBusy flip flop in the interface..."
            // "...restarting the receiver... causes [the controller] to ignore the current packet and hunt
            //  for the beginning of the next packet."

            //
            // So, two things:
            //  1) Cancel any pending input packets
            //  2) Start listening for more packets if we weren't already doing so.
            //
            _receiverLock.EnterWriteLock();
            if (_iBusy)
            {
                Log.Write(LogComponent.EthernetController, "Receiver initializing, dropping current activity.");
                _system.Scheduler.CancelEvent(_fifoReceiveWakeupEvent);
                _incomingPacket = null;
                _incomingPacketLength = 0;                
            }

            _iBusy = true;
            _receiverLock.ExitWriteLock();

            _system.CPU.BlockTask(TaskType.Ethernet);

            Log.Write(LogComponent.EthernetController, "Receiver initialized.");

            //
            // TODO:
            // This hack is ugly and it wants to die.  Ethernet packets come in asynchronously from another thread.
            // The scheduler is not thread-safe (and making it so incurs a serious performance penalty) so the receiver
            // thread cannot post an event to wake up the rcv FIFO.  Instead we poll periodically and start processing
            // new packets if one has arrived.
            //
            if (_pollEvent == null)
            {
                _pollEvent = new Event(_pollPeriod, null, PacketPoll);
                _system.Scheduler.Schedule(_pollEvent);
            }
        }

        /// <summary>
        /// Invoked when the host ethernet interface receives a packet destined for us.
        /// TODO: determine the best behavior here; we could queue up a number of incoming packets and let
        /// the emulated interface pull them off one by one, or we could only save one packet and discard
        /// any that arrive while the emulated interface is processing the current one.
        /// 
        /// The latter is probably more faithful to the original intent, but the former might be more useful on
        /// busy Ethernets (though the bottom-level filter implemented by HostEthernet might take care
        /// of that already.)
        /// 
        /// For now, we just accept one at a time.
        /// </summary>
        /// <param name="data"></param>
        private void OnHostPacketReceived(MemoryStream data)
        {
            _receiverLock.EnterWriteLock();
            _packetReady = true;
            _nextPacket = data;
            _receiverLock.ExitWriteLock();            
        }

        private void PacketPoll(ulong timeNsec, ulong skewNsec, object context)
        {            
            _receiverLock.EnterUpgradeableReadLock();
            if (_packetReady)
            {
                // Schedule the next word of data.                
                Console.WriteLine("**** hack *****");

                if (_iBusy && _incomingPacket == null)
                {
                    _incomingPacket = _nextPacket;                    

                    // Read the packet length (in words) (first word of the packet).  Convert to bytes.
                    //
                    _incomingPacketLength = ((_incomingPacket.ReadByte()) | (_incomingPacket.ReadByte() << 8)) * 2;

                    // Sanity check:
                    if (_incomingPacketLength > _incomingPacket.Length - 2)
                    {
                        throw new InvalidOperationException("Invalid 3mbit packet length header.");
                    }

                    Log.Write(LogComponent.EthernetController, "Accepting incoming packet (length {0}).", _incomingPacketLength);

                    // From uCode:
                    // "Interface will generate a data wakeup when the first word of the next
                    // "packet arrives, ignoring any packet currently passing."
                    //
                    // Read the first word, place it in the fifo and wake up the ethernet task.
                    //
                    //ushort nextWord = (ushort)((_incomingPacket.ReadByte()) | (_incomingPacket.ReadByte() << 8));
                    //_fifo.Enqueue(nextWord);
                    //_incomingPacketLength -= 2;
                    //_system.CPU.WakeupTask(TaskType.Ethernet);

                    // Wake up the FIFO
                    _fifoReceiveWakeupEvent.TimestampNsec = _fifoReceiveDuration;
                    _system.Scheduler.Schedule(_fifoReceiveWakeupEvent);                    
                }
                else
                {
                    // Drop, we're either already busy with a packet or we're not listening right now.
                    Log.Write(LogComponent.EthernetController, "Dropping incoming packet; controller is currently busy or not active (ibusy {0}, packet {1})", _iBusy, _incomingPacket != null);
                }

                _receiverLock.EnterWriteLock();
                _packetReady = false;
                _nextPacket = null;
                _receiverLock.ExitWriteLock();
            }
            _receiverLock.ExitUpgradeableReadLock();

            // Do it again.
            _pollEvent.TimestampNsec = _pollPeriod;
            _system.Scheduler.Schedule(_pollEvent);            
        }

        private void InputFifoCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            _receiverLock.EnterUpgradeableReadLock();
            if (!_iBusy || _inGone)
            {
                // If IBUSY is no longer set then the interface was reset before
                // we got to run; abandon this operation.                
                Log.Write(LogComponent.EthernetController, "FIFO callback after reset, abandoning input.");
                _incomingPacket = null;
                _incomingPacketLength = 0;
                _receiverLock.ExitUpgradeableReadLock();                
                return;
            }   
            
            if (_fifo.Count >= 16)
            {
                _fifoReceiveWakeupEvent.TimestampNsec = _fifoReceiveDuration - skewNsec;
                _system.Scheduler.Schedule(_fifoReceiveWakeupEvent);

                Log.Write(LogComponent.EthernetController, "Input FIFO full? Scheduling next wakeup.");
                _receiverLock.ExitUpgradeableReadLock();
                return;
            }

            Log.Write(LogComponent.EthernetController, "Processing word from input packet ({0} bytes left in input, {1} words in FIFO.)", _incomingPacketLength, _fifo.Count);

            if (_incomingPacketLength >= 2)
            {
                // Stuff 1 word into the FIFO, if we run out of data to send then we clear _iBusy.                
                ushort nextWord = (ushort)((_incomingPacket.ReadByte()) | (_incomingPacket.ReadByte() << 8));
                _fifo.Enqueue(nextWord);

                _incomingPacketLength -= 2;
            }
            else if (_incomingPacketLength == 1)
            {
                // should never happen
                throw new InvalidOperationException("Packet length not multiple of 2 on receive.");
            }            

            // All out of data?  Finish the receive operation.
            if (_incomingPacketLength == 0)
            {
                _receiverLock.EnterWriteLock();
                _inGone = true;
                _incomingPacket = null;
                _receiverLock.ExitWriteLock();

                // Wakeup for end of data.
                _system.CPU.WakeupTask(TaskType.Ethernet);

                Log.Write(LogComponent.EthernetController, "Receive complete.");
            }
            else
            {
                // Schedule the next wakeup.                
                _fifoReceiveWakeupEvent.TimestampNsec = _fifoReceiveDuration - skewNsec;
                _system.Scheduler.Schedule(_fifoReceiveWakeupEvent);                

                Log.Write(LogComponent.EthernetController, "Scheduling next wakeup.");
            }

            // Wake up the Ethernet task to process this data   
            if (_fifo.Count >= 2)
            {
                _system.CPU.WakeupTask(TaskType.Ethernet);
            }

            _receiverLock.ExitUpgradeableReadLock();
        }

        private Queue<ushort> _fifo;

        // Bits in Status register
        private int _ioCmd;
        private bool _dataLate;
        private bool _collision;
        private bool _crcBad;
        private bool _incomplete;
        private ushort _status;
                
        private bool _countdownWakeup;        

        private bool _oBusy;
        private bool _iBusy;
        private bool _inGone;

        // FIFO scheduling

        // Transmit:
        private ulong _fifoTransmitDuration = 87075;       // ~87000 nsec to transmit 16 words at 3mbit, assuming no collision
        private Event _fifoTransmitWakeupEvent;

        // Receive:
        private ulong _fifoReceiveDuration = 5400;       // ~5400 nsec to receive 1 word at 3mbit
        private Event _fifoReceiveWakeupEvent;

        // Polling (hack)
        private ulong _pollPeriod = 23000;
        private Event _pollEvent;
        private bool _packetReady;

        // The actual connection to a real Ethernet device on the host
        HostEthernet _hostEthernet;

        // Buffer to hold outgoing data to the host ethernet
        ushort[] _outputData;
        int _outputIndex;

        // Incoming data and locking
        private MemoryStream _incomingPacket;
        private MemoryStream _nextPacket;
        private int _incomingPacketLength;
        private System.Threading.ReaderWriterLockSlim _receiverLock;

        private AltoSystem _system;
    }
}
