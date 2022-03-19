using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UPD_Client
{
    internal class BroadcastClient
    {
        int _client;
        IPAddress? _ip;
        IPEndPoint _ep;
        Socket _socket;
        byte[] _buffer = new byte[0];

        byte[] _pingSend;
        byte[] _pingResp;
        bool _ping = false;

        CancellationTokenSource _cts;

        int _FPS = 0;
        int _FPS_count = 0;
        int _FPS_tick = Environment.TickCount;

        public int FPS { get => _FPS; }

        public BroadcastClient()
        {
            _pingSend = new byte[1] { 1 };
        }

        internal async Task ConnectChannel(string host, int port)
        {
            try
            {
                _cts = new CancellationTokenSource();
                _client = new Random(Environment.TickCount).Next(10);

                if (!IPAddress.TryParse(host, out _ip))
                {
                    Uri uri = new(host);
                    _ip = Dns.GetHostEntry(uri.Host).AddressList[0];
                }
                
                _ep = new(_ip, port);
                _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.EnableBroadcast = true;

                await _socket.ConnectAsync(_ep, _cts.Token);

                int threads = 1;

                Task[] task = new Task[threads];
                for (int i = 0; i < 1; i++)
                {
                    task[i] = ThreadReceiveMessagesAsync(i, 1000);
                }
                await Task.WhenAll(task);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        internal async Task ThreadReceiveMessagesAsync(int thread, int bufferLength)
        {
            Array.Resize(ref _buffer, _buffer.Length + bufferLength);
            ArraySegment<byte> segment = new(_buffer, _buffer.Length - bufferLength, bufferLength);
            
            await Task.Factory.StartNew(async () => {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var count = await _socket.ReceiveAsync(segment, SocketFlags.None, _cts.Token);

                        if (count > 0)
                        {
                            if (_buffer[0] == 1) // Ping Send
                            {
                                await _socket.SendAsync(_pingSend, SocketFlags.None, _cts.Token);
                            }
                            else if (_buffer[0] == 2) // Ping Receive
                            {
                                _ping = true;
                            }
                        }

                        CalcFPS();
                    }
                }
                catch (Exception ex)
                {
                }
            }, _cts.Token);
        }

        internal async Task<bool> PingAsync(int timeout)
        {
            try
            {
                _ping = false;
                await _socket.SendToAsync(_pingSend, SocketFlags.None, _ep);

                int tick = Environment.TickCount;
                while (Environment.TickCount - tick <= timeout)
                {
                    await Task.Yield();
                    if (_ping) return _ping;
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        //internal void SendMessage(string msg, ReadOnlySpan<char> channel)
        //{
        //    byte[] buffer = UTF8Encoding.UTF8.GetBytes(msg);
        //    byte[] ch = UTF8Encoding.UTF8.GetBytes(channel);

        //    _socket.SendTo(buffer, _ep);
        //}
        

        internal void SendMessage3(string msg, string channel)
        {
            _socket.SendTo(_pingSend, _ep);
            CalcFPS();
        }

        internal void SendMessage2(string msg, string channel)
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(channel);
            byte[] b_msg = UTF8Encoding.UTF8.GetBytes(msg);

            int length = buffer.Length;
            Array.Resize(ref buffer, length + b_msg.Length);
            Array.Copy(b_msg, 0, buffer, length, b_msg.Length); 

            _socket.SendTo(buffer, _ep);
            CalcFPS();
        }

            internal void SendMessage(string msg, string channel)
        {
            var buffer = new ArrayBufferWriter<byte>();

            SendMessage(buffer, msg, channel);
        }

        internal void SendMessage(IBufferWriter<byte> writer, string msg, string channel)
        {
            writer.Write(UTF8Encoding.UTF8.GetBytes(channel));
            writer.Write(UTF8Encoding.UTF8.GetBytes(msg));
            
            _socket.SendTo(writer.GetSpan(), _ep);
            CalcFPS();
        }

        private void SerializePacket(string channel, string msg)
        {

        }

        private void CalcFPS()
        {
            if (Environment.TickCount - _FPS_tick >= 1000)
            {
                _FPS = _FPS_count;
                _FPS_count = 0;
                _FPS_tick = Environment.TickCount;
                Debug.Print($"FPS: {_FPS}");
                Console.WriteLine($"FPS: {_FPS}");
            }
            Interlocked.Increment(ref _FPS_count);
        }
    }
}
