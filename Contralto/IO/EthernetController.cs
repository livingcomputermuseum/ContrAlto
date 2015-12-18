using Contralto.CPU;
using Contralto.Logging;
using System;
using System.Collections.Generic;
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

            // TODO: make this configurable
            _ethernetAddress = 0x22;

            _fifo = new Queue<ushort>();
            Reset();

            _fifoWakeupEvent = new Event(_fifoTransmitDuration, null, FIFOCallback);
        }

        public void Reset()
        {            
            ResetInterface();
        }

        public byte Address
        {
            get { return _ethernetAddress; }
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
            // Latch status before resetting
            _status = (ushort)(
                       (0xffc0) |                        // bits always set                 
                       (_dataLate ? 0x00 : 0x20) |
                       (_collision ? 0x00 : 0x10) |
                       (_crcBad ? 0x00 : 0x08) |
                       ((~_ioCmd & 0x3) << 1) |
                       (_incomplete ? 0x00 : 0x01));

            _ioCmd = 0;
            _oBusy = false;
            _iBusy = false;
            _dataLate = false;
            _collision = false;
            _crcBad = false;
            _incomplete = false;
            _fifo.Clear();

            if (_system.CPU != null)
            {
                _system.CPU.BlockTask(TaskType.Ethernet);
            }

            Log.Write(LogComponent.EthernetController, "Interface reset.");
        }

        public ushort ReadInputFifo(bool lookOnly)
        {
            if (FIFOEmpty)
            {
                Log.Write(LogComponent.EthernetController, "Read from empty Ethernet FIFO, returning 0.");
                return 0;
            }

            if (lookOnly)
            {
                return _fifo.Peek();
            }
            else
            {
                return _fifo.Dequeue();
            }            
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
            if (_fifo.Count == 16)
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
            _fifoWakeupEvent.Context = end;
            _fifoWakeupEvent.TimestampNsec = _fifoTransmitDuration;
            _system.Scheduler.Schedule(_fifoWakeupEvent);
        }

        private void FIFOCallback(ulong timeNsec, ulong skewNsec, object context)
        {
            bool end = (bool)context;
            
            if (!_oBusy)
            {
                // If OBUSY is no longer set then the interface was reset before
                // we got to run; abandon this operation.                
                Log.Write(LogComponent.EthernetController, "FIFO callback after reset, abandoning.");                
                return;
            }
            
            Log.Write(LogComponent.EthernetController, "Sending {0} words from fifo.", _fifo.Count);

            // TODO: actually transmit data in FIFO rather than bit-bucketing it.
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
            }       
        }

        private void InitializeReceiver()
        {
            // TODO: pull next packet off host ethernet interface that's destined for the Alto, and start the
            // process of putting into the FIFO and generating wakeups for the microcode.
        }

        private Queue<ushort> _fifo;

        // Bits in Status register
        private int _ioCmd;
        private bool _dataLate;
        private bool _collision;
        private bool _crcBad;
        private bool _incomplete;
        private ushort _status;

        private byte _ethernetAddress;
        private bool _countdownWakeup;        

        private bool _oBusy;
        private bool _iBusy;        

        // FIFO scheduling
        private ulong _fifoTransmitDuration = 87075;       // ~87000 nsec to transmit 16 words at 3mbit, assuming no collision
        private Event _fifoWakeupEvent;

        private AltoSystem _system;
    }
}
