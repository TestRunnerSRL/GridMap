using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GridMap
{
    public class Connection : BindableBase
    {
        public enum ConnectionStatus { CLOSED, JOINED, HOSTING }

        private class ReceiveObject
        {
            public TcpClientInfo tcpclientinfo = null;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize];
            public List<byte> data = new List<byte>();
            public int dataLength = 0;
        }

        public delegate void MessageReceivedHandler(Message message);
        public event MessageReceivedHandler MessageReceivedEvent;

        private readonly int PingInterval = 10 * 1000; // 10 seconds
        private readonly int PingTimeout = 30 * 1000;  // 30 seconds
                
        ConnectionStatus status = ConnectionStatus.CLOSED;
        List<TcpClientInfo> clients = new List<TcpClientInfo>();
        TcpListener server;
        public Config config;

        public bool CanJoin { get { return status == ConnectionStatus.CLOSED; } }
        public bool CanHost { get { return status == ConnectionStatus.CLOSED; } }
        public bool CanLeave { get { return status != ConnectionStatus.CLOSED; } }


        public Connection()
        {
            config = new Config();
            new Timer(Ping, null, PingInterval, PingInterval);
        }

        public void Host()
        {

            try
            {
                server = new TcpListener(IPAddress.Any, config.Port);
                server.Start();
                SetProperty(ref status, ConnectionStatus.HOSTING, new List<string>() { "CanJoin", "CanHost", "CanLeave" });
                server.BeginAcceptTcpClient(Listen, server);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentOutOfRangeException)
                {
                    Logger.Log("Invalid port", e.ToString());
                }
                else if (e is SocketException)
                {
                    Logger.Log("Socket cannot be opened", e.ToString());
                }
                else if (e is ObjectDisposedException)
                {
                    Logger.Log("Socket is already closed", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
            }
        }

        private void Listen(IAsyncResult ar)
        {
            if (status == ConnectionStatus.CLOSED) { return;}

            var server = ar.AsyncState as TcpListener;
            try {
                var new_client = new TcpClientInfo(server.EndAcceptTcpClient(ar));
                clients.Add(new_client);
                server.BeginAcceptTcpClient(Listen, server);
            }
            catch (SocketException e)
            {
                Logger.Log("Socket cannot be read from", e.ToString());
                Shutdown();
            }
            catch (ObjectDisposedException e)
            {
                Logger.Log("Socket is already closed", e.ToString());
                Shutdown();
            }
        }

        public void Join()
        {
            TcpClientInfo new_client = null;

            try
            {
                new_client = new TcpClientInfo(new TcpClient(config.IP, config.Port))
                {
                    status = ConnectionStatus.HOSTING
                };
                clients.Add(new_client);
                SetProperty(ref status, ConnectionStatus.JOINED, new List<string>() { "CanJoin", "CanHost", "CanLeave" });
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentOutOfRangeException)
                {
                    Logger.Log("Invalid port", e.ToString());
                }
                else if (e is SocketException)
                {
                    Logger.Log("Socket cannot be opened", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
                return;
            }

            var receiveObject = new ReceiveObject()
            {
                tcpclientinfo = new_client
            };

            try
            { 
                new_client.tcpclient.Client.BeginReceive(receiveObject.buffer, 0, ReceiveObject.BufferSize, SocketFlags.None, Receive, receiveObject);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentOutOfRangeException)
                {
                    Logger.Log("Invalid port", e.ToString());
                }
                else if (e is SocketException)
                {
                    Logger.Log("Socket cannot be read from", e.ToString());
                }
                else if (e is ObjectDisposedException)
                {
                    Logger.Log("Socket is already closed", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
            }
        }

        private void Receive(IAsyncResult ar)
        {
            var receiveObject = ar.AsyncState as ReceiveObject;
            int bytesReceived = 0;

            if (receiveObject.tcpclientinfo.status == ConnectionStatus.CLOSED || status == ConnectionStatus.CLOSED) { return; }

            try
            {
                bytesReceived = receiveObject.tcpclientinfo.tcpclient.Client.EndReceive(ar);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                {
                    Logger.Log("Socket cannot be read from", e.ToString());
                }
                else if (e is ObjectDisposedException)
                {
                    Logger.Log("Socket is already closed", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
                return;
            }

            if (bytesReceived > 0)
            {
                receiveObject.data.AddRange(receiveObject.buffer.Take(bytesReceived));
                receiveObject.dataLength += bytesReceived;
            }

            if (receiveObject.dataLength > 0)
            {
                int length;
                Message message = Message.ParseRaw(receiveObject.data, receiveObject.dataLength, out length);
                if (message != null)
                {
                    receiveObject.data.RemoveRange(0, length);
                    receiveObject.dataLength -= length;

                    receiveObject.tcpclientinfo.user = message.sender;

                    switch (message.type)
                    {
                        case Message.Type.CLOSE:
                            Logger.Log($"{message.sender} disconnected.");
                            if (status == ConnectionStatus.JOINED)
                            {
                                Shutdown();
                            }
                            else
                            {
                                receiveObject.tcpclientinfo.tcpclient.Close();
                                receiveObject.tcpclientinfo.status = ConnectionStatus.CLOSED;
                                clients.Remove(receiveObject.tcpclientinfo);
                            }
                            return;
                        case Message.Type.PING:
                            SendMessageTo(receiveObject.tcpclientinfo, new Message(Message.Type.PONG, new byte[0]));
                            break;
                        case Message.Type.PONG:
                            receiveObject.tcpclientinfo.lastping = DateTime.Now;
                            break;
                        default:
                            MessageReceivedEvent?.Invoke(message);
                            break;
                    }
                }
            }

            try
            {
                receiveObject.tcpclientinfo.tcpclient.Client.BeginReceive(receiveObject.buffer, 0, ReceiveObject.BufferSize, SocketFlags.None, Receive, receiveObject);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is ArgumentOutOfRangeException)
                {
                    Logger.Log("Invalid port", e.ToString());
                }
                else if (e is SocketException)
                {
                    Logger.Log("Socket cannot be read from", e.ToString());
                }
                else if (e is ObjectDisposedException)
                {
                    Logger.Log("Socket is already closed", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
            }
        }

        public void SendMessage(string text)
        {
            Message message = new Message(Message.Type.TEXT, text);
            SendMessage(message);
        }

        public void SendMessage(byte[] data)
        {
            Message message = new Message(Message.Type.DATA, data);
            SendMessage(message);
        }

        public void SendMessage(Message message)
        {
            if (status == ConnectionStatus.CLOSED) { return; }

            message.sender = config.Name;
            byte[] data = message.GetRaw();

            foreach(var client in clients)
            {
                if (client.status == ConnectionStatus.CLOSED) { continue; }

                try
                {
                    client.tcpclient.Client.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception e)
                {
                    if (e is SocketException)
                    {
                        Logger.Log("Socket cannot be written to", e.ToString());
                    }
                    else if (e is ObjectDisposedException)
                    {
                        Logger.Log("Socket is already closed", e.ToString());
                    }
                    else
                    {
                        Logger.Log("Unexpected error", e.ToString());
                    }

                    Shutdown();
                    return;
                }
            }
        }

        private void SendMessageTo(TcpClientInfo tcpClientInfo, Message message)
        {
            if (status == ConnectionStatus.CLOSED || tcpClientInfo.status == ConnectionStatus.CLOSED) { return; }

            message.sender = config.Name;
            byte[] data = message.GetRaw();

            try
            {
                tcpClientInfo.tcpclient.Client.Send(data, 0, data.Length, SocketFlags.None);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                {
                    Logger.Log("Socket cannot be written to", e.ToString());
                }
                else if (e is ObjectDisposedException)
                {
                    Logger.Log("Socket is already closed", e.ToString());
                }
                else
                {
                    Logger.Log("Unexpected error", e.ToString());
                }

                Shutdown();
                return;
            }
        }

        private void Ping(object prop)
        {
            if (status == ConnectionStatus.CLOSED) { return; }

            var now = DateTime.Now;
            var remove_list = new List<TcpClientInfo>();
            foreach(var tcpclientinfo in clients)
            {
                if (tcpclientinfo.lastping.AddMilliseconds(PingTimeout).CompareTo(now) < 0)
                {
                    Logger.Log($"{tcpclientinfo.user ?? "A client"} timed out.");
                    if (status == ConnectionStatus.JOINED)
                    {
                        Shutdown();
                        return;
                    }
                    else
                    {
                        tcpclientinfo.tcpclient.Close();
                        tcpclientinfo.status = ConnectionStatus.CLOSED;
                        remove_list.Add(tcpclientinfo);
                    }
                }
            }

            foreach(var client in remove_list)
            {
                clients.Remove(client);
            }

            var message = new Message(Message.Type.PING, new byte[0]);
            SendMessage(message);
        }

        public void Leave()
        {
            Message message = new Message(Message.Type.CLOSE, new byte[0]);
            SendMessage(message);
            Shutdown();
        }

        public void Shutdown()
        {
            Logger.Log("Shutting down connections.");

            foreach (var client in clients)
            {
                client.tcpclient.Close();
                client.status = ConnectionStatus.CLOSED;
            }
            clients.Clear();

            server?.Stop();
            server = null;
            SetProperty(ref status, ConnectionStatus.CLOSED, new List<string>() { "CanJoin", "CanHost", "CanLeave" });
        }

        ~Connection()
        {
            Shutdown();
        }
    }
}
