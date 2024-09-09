using Enocean.Core.Packets;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Enocean.Core.Communicators
{
    public class SerialCommunicator : Communicator
    {
        string _port = string.Empty;
        SerialPort _serialPort;

        public SerialCommunicator(string port = "/dev/ttyAMA0", Action<Packet>? callback = null, ILogger? logger = null) : base(callback)
        {
            // Initialize serial port
            this.logger = logger;
            Packet.SetLogger(logger);
            _serialPort = new SerialPort(port, 57600)
            {
                Handshake = Handshake.None
            };
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs ee)
        {
            // Read chars from serial port as hex numbers
            try
            {
                byte[] bt = new byte[_serialPort.ReadBufferSize];

                var nbBytes = _serialPort.Read(bt, 0, _serialPort.ReadBufferSize);
                this._Buffer.AddRange(bt.Take(nbBytes));
            }
            catch (Exception e)
            {
                logger?.LogError($"Serial port exception! (device disconnected or multiple access on port?) {e.Message}");
                CloseConnection();
                Stop();
            }

            Parse();
        }

        public bool OpenConnection()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                logger?.LogInformation("SerialCommunicator connected");
            }

            return _serialPort.IsOpen;
        }
        public void CloseConnection()
        {
            _serialPort?.Close();
        }

        public void Run()
        {
            logger?.LogInformation("SerialCommunicator started");
            while (!_StopFlag)
            {
                // If there's messages in transmit queue
                // send them
                var packet = GetFromSendQueue();
                if (packet is not null)
                {
                    try
                    {
                        var msg = packet.Build().ToArray();
                        logger?.LogInformation($"send -> {Utils.ToHexString(msg.ToList())}");
                        _serialPort.Write(msg, 0, msg.Length);
                    }
                    catch (Exception e)
                    {
                        logger?.LogError($"Message send error {e.Message}");
                        Stop();
                    }
                }
            }

            _serialPort.Close();
            Stop();
            logger?.LogInformation("SerialCommunicator stopped");
        }
    }
}
