using Enocean.Core.Constants;
using Enocean.Core.Packets;
using Microsoft.Extensions.Logging;

namespace Enocean.Core.Communicators
{
    /// <summary>
    ///   Communicator base-class for EnOcean.
    ///   Not to be used directly, only serves as base class for SerialCommunicator etc.
    /// </summary>
    public abstract class Communicator
    {
        protected ILogger? logger;
        // Declare the delegate (if using non-generic pattern).
        public delegate void StopFlageEventHandler(object sender, EventArgs e);

        // Declare the event.
        public event StopFlageEventHandler? StopFlagEvent;
        protected bool _StopFlag;
        protected List<byte> _Buffer;
        protected readonly Queue<Packet> _Transmit;
        protected readonly Queue<Packet> _Receive;
        protected readonly Action<Packet>? _Callback;
        protected List<byte>? _BaseId;
        public bool _TeachIn;

        public Queue<Packet> Transmit => _Transmit;
        public Queue<Packet> Receive => _Receive;
        public List<byte> Buffer { get => _Buffer; set => _Buffer = value; }
        public bool StopFlag => _StopFlag;

        public bool TeachIn { get => _TeachIn; set => _TeachIn = value; }

        public Communicator(Action<Packet>? callback = null, bool teachIn = true)
        {

            //  Create an event to stop the thread
            _Buffer = new List<byte>();
            _Transmit = new Queue<Packet>();
            _Receive = new Queue<Packet>();
            // Set the callback method
            _Callback = callback;
            // Internal variable for the Base ID of the module.
            _BaseId = null;
            //  Should new messages be learned automatically? Defaults to True.
            // TODO: Not sure if we should use CO_WR_LEARNMODE??
            _TeachIn = teachIn;
        }

        public Packet? GetFromSendQueue()
        {
            // Get message from send queue, if one exists
            try
            {
                var packet = _Transmit.Dequeue();
                logger?.LogInformation("Sending packet");
                logger?.LogDebug(packet.ToString());
                return packet;
            }
            catch
            {
                return null;
            }
        }

        public bool Send(Packet packet)
        {
            if (packet is not Packet || packet == null)
            {
                logger?.LogError("Object to send must be an instance of Packet");
                return false;
            }
            _Transmit.Enqueue(packet);
            return true;
        }
        public void Stop()
        {
            StopFlagEvent?.Invoke(this, new EventArgs());
            _StopFlag = true;
        }

        public PARSE_RESULT Parse()
        {
            // Parses messages and puts them to receive queue
            // Loop while we get new messages
            while (true)
            {
                var (status, buffer, packet) = Packet.ParseMsg(_Buffer);
                // If message is incomplete -> break the loop
                if (status == PARSE_RESULT.INCOMPLETE)
                {
                    return status;
                }

                // If message is OK, add it to receive queue or send to the callback method
                if (status == PARSE_RESULT.OK && packet != null)
                {
                    packet.Received = DateTime.Now;

                    if (packet is UTETeachInPacket && _TeachIn && _BaseId != null)
                    {
                        var responsePacket = ((UTETeachInPacket)packet).CreateResponsePacket(_BaseId);
                        logger?.LogInformation($"Sending response to UTE teach-in. {responsePacket.SenderToHex}");
                        Send(responsePacket);
                    }

                    if (_Callback == null)
                    {
                        _Receive.Enqueue(packet);
                    }
                    else
                    {
                        _Callback(packet);
                    }

                    _Buffer = buffer;
                    logger?.LogDebug(packet.ToString());
                }
            }
        }

        public List<byte>? BaseId
        {
            get
            {
                // Fetches Base ID from the transmitter, if required. Otherwise returns the currently set Base ID.
                // If base id is already set, return it.
                if (_BaseId != null)
                {
                    return _BaseId;
                }

                // Send COMMON_COMMAND 0x08, CO_RD_IDBASE request to the module
                Send(new Packet(PACKET.COMMON_COMMAND, new List<byte> { 0x08 }));

                // Loop over 10 times, to make sure we catch the response.
                // Thanks to timeout, shouldn't take more than a second.
                // Unfortunately, all other messages received during this time are ignored.
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        var packet = _Receive.Dequeue();
                        // We're only interested in responses to the request in question.
                        if (packet.PacketType == PACKET.RESPONSE && packet is ResponsePacket && ((ResponsePacket)packet).Response == (int)RETURN_CODE.OK && ((ResponsePacket)packet).ResponseData.Count == 4)
                        {
                            // Base ID is set in the response data.
                            _BaseId = ((ResponsePacket)packet).ResponseData;
                            // Put packet back to the Queue, so the user can also react to it if required...
                            _Receive.Enqueue(packet);
                            break;
                        }
                        // Put other packets back to the Queue.
                        _Receive.Enqueue(packet);
                    }
                    catch (InvalidOperationException)
                    {
                        // Queue is empty, continue to the next iteration
                        continue;
                    }
                    Thread.Sleep(1000); // Simulate timeout
                }
                // Return the current Base ID (might be null).
                return _BaseId;
            }
            set
            {
                // Sets the Base ID manually, only for testing purposes. '''
                _BaseId = value;
            }
        }

    }
}
