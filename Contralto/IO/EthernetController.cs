/*  
    This file is part of ContrAlto.

    ContrAlto is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ContrAlto is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ContrAlto.  If not, see <http://www.gnu.org/licenses/>.
*/

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
    /// <summary>
    /// EthernetController implements the logic for the Alto's 3Mbit Ethernet controller.
    /// </summary>
    public class EthernetController
    {
        public EthernetController(AltoSystem system)
        {
            _system = system;

            _receiverLock = new System.Threading.ReaderWriterLockSlim();

            _fifo = new Queue<ushort>();
            Reset();

            _fifoTransmitWakeupEvent = new Event(_fifoTransmitDuration, null, OutputFifoCallback);


            // Attach real Ethernet device if user has specified one, otherwise leave unattached; output data
            // will go into a bit-bucket.
            try
            {
                switch (Configuration.HostPacketInterfaceType)
                {
                    case PacketInterfaceType.UDPEncapsulation:
                        _hostInterface = new UDPEncapsulation(Configuration.HostPacketInterfaceName);
                        _hostInterface.RegisterReceiveCallback(OnHostPacketReceived);
                        break;

                    case PacketInterfaceType.EthernetEncapsulation:
                        _hostInterface = new HostEthernetEncapsulation(Configuration.HostPacketInterfaceName);
                        _hostInterface.RegisterReceiveCallback(OnHostPacketReceived);
                        break;

                    default:
                        _hostInterface = null;
                        break;
                }
            }
            catch
            {
                _hostInterface = null;
            }

            // More words than the Alto will ever send.
            _outputData = new ushort[4096];

            _nextPackets = new Queue<MemoryStream>();
        }

        public void Reset()
        {
            _inputPollEvent = null;

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

        public IPacketEncapsulation HostInterface
        {
            get { return _hostInterface; }
        }

        public void ResetInterface()
        {            
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
            _inputState = InputState.ReceiverOff;            

            if (_system.CPU != null)
            {
                _system.CPU.BlockTask(TaskType.Ethernet);
            }

            Log.Write(LogComponent.EthernetController, "Interface reset.");
         
            if (_inputPollEvent == null)
            {
                // Kick off the input poll event which will run forever.
                _inputPollEvent = new Event(_inputPollPeriod, null, InputHandler);
                _system.Scheduler.Schedule(_inputPollEvent);
            }
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
                // NOTE: We do not append a checksum to the outgoing 3mbit packet.  See comments on the
                // receiving end for an explanation.
                if (_hostInterface != null)
                {
                    _hostInterface.Send(_outputData, _outputIndex);
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
            //  1) Cancel any pending input packet
            //  2) Start listening for more packets if we weren't already doing so.
            //            
            if (_iBusy)
            {
                Log.Write(LogComponent.EthernetController, "Receiver initializing, dropping current activity.");                
                _incomingPacket = null;
                _incomingPacketLength = 0;                
            }

            _inputState = InputState.ReceiverWaiting;
            _iBusy = true;            

            _system.CPU.BlockTask(TaskType.Ethernet);

            Log.Write(LogComponent.EthernetController, "Receiver initialized.");            
        }

        /// <summary>
        /// Invoked when the host ethernet interface receives a packet destined for us.
        /// NOTE: This runs on the PCap or UDP receiver thread, not the main emulator thread.
        ///       Any access to emulator structures must be properly protected.
        /// 
        /// Due to the nature of the "ethernet" we're simulating, there will never be any collisions or corruption and
        /// everything is completely asynchronous with regard to all receivers, as such it's completely possible
        /// for packets to be received by the host interface when the emulated interface is already sending/receiving
        /// a 3mbit packet (something that could never happen in reality).  There is no reasonable way to change this behavior
        /// without having a distributed synchronization across emulator processes to more accurately simulate the behavior
        /// of a real ethernet, and that seems like complete overkill (and gets even more complicated if we end up using transports
        /// other than raw Ethernet in the future.)
        /// 
        /// To compensate for this somewhat, we queue up received packets (to an upper limit of 32), these will either be consumed or discarded
        /// by InputHandler (which runs periodically on the emulator thread) depending on the current state of the interface.
        /// This reduces the number of dropped packets and seems to work fairly well.
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void OnHostPacketReceived(MemoryStream data)
        {
            _receiverLock.EnterWriteLock();
            if (_nextPackets.Count < _maxQueuedPackets)
            {
                _nextPackets.Enqueue(data);
            }
            else
            {
                Log.Write(LogType.Error, LogComponent.EthernetPacket, "Input packet queue has reached its limit of {0} packets, dropping oldest packet.", _maxQueuedPackets);
                _nextPackets.Dequeue();
                _nextPackets.Enqueue(data);
            }

            _receiverLock.ExitWriteLock();            
        }

        /// <summary>
        /// Runs the input state machine.  This runs periodically (as scheduled by the Scheduler) and:
        ///   1) Ignores incoming packets if the receiver is off.
        ///   2) Pulls incoming packets from the queue if the interface is active
        ///   3) Reads words from incoming packets into the controller's FIFO        
        /// </summary>
        /// <param name="timeNsec"></param>
        /// <param name="skewNsec"></param>
        /// <param name="context"></param>
        private void InputHandler(ulong timeNsec, ulong skewNsec, object context)
        {
            switch(_inputState)
            {
                case InputState.ReceiverOff:
                    // Receiver is off, if we have any incoming packets, they are ignored.
                    // TODO: would it make sense to expire really old packets (say more than a couple of seconds old)
                    // so that the receiver doesn't pick up ancient history the next time it runs?
                    // We already cycle out packets as new ones come in, so this would only be an issue on very quiet networks.
                    // (And even then I don't know if it's really an issue.)
                    _receiverLock.EnterReadLock();

                    if (_nextPackets.Count > 0)
                    {
                        Log.Write(LogComponent.EthernetPacket, "Receiver is off, ignoring incoming packet from packet queue.");
                    }
                    _receiverLock.ExitReadLock();
                    break;

                case InputState.ReceiverWaiting:
                    // Receiver is on, waiting for a new packet.  If we have one now, start an
                    // input operation.
                    _receiverLock.EnterReadLock();
                    if (_nextPackets.Count > 0)
                    {
                        _incomingPacket = _nextPackets.Dequeue();

                        //
                        // Read the packet length (in words) (first word of the packet as provided by the sending emulator).  Convert to bytes.
                        //
                        _incomingPacketLength = ((_incomingPacket.ReadByte() << 8) | (_incomingPacket.ReadByte())) * 2;

                        // Add one word to the count for the checksum.
                        // NOTE: This is not provided by the sending emulator and is not computed here either.
                        // The microcode does not use it and any corrupted packets will be dealt with transparently by the host interface,
                        // not the emulator.
                        // We add the word to the count because the microcode expects to read it in from the input FIFO, it is then dropped.
                        //
                        _incomingPacketLength += 2;                        

                        // Sanity check:
                        if (_incomingPacketLength > _incomingPacket.Length ||
                            (_incomingPacketLength % 2) != 0)
                        {
                           throw new InvalidOperationException(
                                String.Format("Invalid 3mbit packet length header ({0} vs {1}.", _incomingPacketLength, _incomingPacket.Length));                            
                        }

                        Log.Write(LogComponent.EthernetPacket, "Accepting incoming packet (length {0}).", _incomingPacketLength);

                        //LogPacket(_incomingPacketLength, _incomingPacket);

                        // Move to the Receiving state.
                        _inputState = InputState.Receiving;
                    }                    
                    _receiverLock.ExitReadLock();
                    break;

                case InputState.Receiving:
                    Log.Write(LogComponent.EthernetController, "Processing word from input packet ({0} bytes left in input, {1} words in FIFO.)", _incomingPacketLength, _fifo.Count);

                    if (_fifo.Count >= 16)
                    {
                        // This shouldn't happen.
                        Log.Write(LogComponent.EthernetController, "Input FIFO full, Scheduling next wakeup. No words added to the FIFO.");                        
                        break;
                    }                    

                    if (_incomingPacketLength >= 2)
                    {
                        // Stuff 1 word into the FIFO, if we run out of data to send then we clear _iBusy further down.       
                        ushort nextWord = (ushort)((_incomingPacket.ReadByte() << 8) | (_incomingPacket.ReadByte()));
                        _fifo.Enqueue(nextWord);

                        _incomingPacketLength -= 2;
                    }
                    else if (_incomingPacketLength == 1)
                    {
                        // Should never happen.
                        throw new InvalidOperationException("Packet length not multiple of 2 on receive.");
                    }

                    // All out of data?  Finish the receive operation.
                    if (_incomingPacketLength == 0)
                    {                        
                        _inGone = true;
                        _incomingPacket = null;

                        _inputState = InputState.ReceiverDone;                     

                        // Wakeup Ethernet task for end of data.
                        _system.CPU.WakeupTask(TaskType.Ethernet);

                        Log.Write(LogComponent.EthernetController, "Receive complete.");
                    }                   

                    // Wake up the Ethernet task to process data if we have
                    // more than two words in the FIFO.
                    if (_fifo.Count >= 2)
                    {
                        _system.CPU.WakeupTask(TaskType.Ethernet);
                    }

                    break;

                case InputState.ReceiverDone:
                    // Nothing, we just wait in this state for the receiver to be reset by the microcode.
                    break;

            }

            // Schedule the next wakeup.                
            _inputPollEvent.TimestampNsec = _inputPollPeriod - skewNsec;
            _system.Scheduler.Schedule(_inputPollEvent);            
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
        private ulong _inputPollPeriod = 5400;       // ~5400 nsec to receive 1 word at 3mbit
        private Event _inputPollEvent;

        // Input states
        private enum InputState
        {
            ReceiverOff = 0,
            ReceiverWaiting,
            Receiving,
            ReceiverDone,
        }

        private InputState _inputState;

        private const int _maxQueuedPackets = 32;

        // The actual connection to a real network device of some sort on the host
        private IPacketEncapsulation _hostInterface;

        // Buffer to hold outgoing data to the host ethernet
        private ushort[] _outputData;
        private int _outputIndex;

        // Incoming data and locking
        private MemoryStream _incomingPacket;        
        private Queue<MemoryStream> _nextPackets;
        private int _incomingPacketLength;
        private System.Threading.ReaderWriterLockSlim _receiverLock;

        private AltoSystem _system;
    }
}
