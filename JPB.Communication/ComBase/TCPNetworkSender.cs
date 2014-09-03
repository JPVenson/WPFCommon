#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:11

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using JPB.Communication.Interface;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkSender : Networkbase
    {
        internal TCPNetworkSender(short port)
        {
            Port = port;
        }

        public short Port { get; private set; }

        //private TcpClient Client { get; set; }

        public static async Task SendMessage(MessageBase message, string ip, short port)
        {
            var sender = NetworkFactory.Instance.GetSender(port);
            await sender.SendMessageAsync(message, ip);
        }

        public static async Task SendMessageAsync(MessageBase message, string ip, short port)
        {
            var sender = NetworkFactory.Instance.GetSender(port);
            sender.SendMessageAsync(message, ip);
        }

        /// <summary>
        /// Sends a message to multible Hosts
        /// 
        /// </summary>
        /// <returns>all non reached hosts</returns>
        public IEnumerable<string> SendMultiMessage(MessageBase message, params string[] ips)
        {
            var failedMessages = new List<string>();

            var runningMessages = ips.Select(ip => SendMessageAsync(message, ip)).ToArray();

            for (var i = 0; i < runningMessages.Length; i++)
            {
                try
                {
                    var task = runningMessages[i];
                    task.Wait();
                    if (!task.Result)
                    {
                        failedMessages.Add(ips[i]);
                    }
                }
                catch (Exception)
                {
                    failedMessages.Add(ips[i]);
                }
            }

            return failedMessages;
        }

        public bool SendMessage(MessageBase message, string ip)
        {
            var sendMessageAsync = SendMessageAsync(message, ip);
            sendMessageAsync.Wait();
            var b = sendMessageAsync.Result;
            return b;
        }

        public Task<bool> SendMessageAsync(MessageBase message, string ip)
        {
            var task = new Task<bool>(() =>
            {
                var client = CreateClientSockAsync(ip, Port);
                var tcpMessage = PrepareMessage(message, ip);
                client.Wait();
                var result = client.Result;
                if (result == null)
                    return false;
                SendBaseAsync(tcpMessage, result);

                return true;
            });
            task.Start();
            return task;
        }

        public async void SendFile(Stream stream, string ip)
        {
            var client = await CreateClientSockAsync(ip, Port);
            if (client == null)
                return;
            SendBase(stream, client);
        }

        #region Base Methods

        private TcpMessage PrepareMessage(MessageBase message, string ip, ISecureMessage provider = null)
        {
            message.SendAt = DateTime.Now;
            message.Sender = NetworkInfoBase.IpAddress.ToString();
            message.Reciver = ip;
            return Wrap(message, provider);
        }

        private async Task<TcpClient> CreateClientSockAsync(string ip, int port)
        {
            try
            {
                var Client = new TcpClient();
                await Client.ConnectAsync(ip, port);
                Client.NoDelay = true;
                return Client;

                //if (Client == null)
                //{
                //    Client = new TcpClient();
                //    await Client.ConnectAsync(ip, port);
                //    Client.NoDelay = true;
                //    return Client;
                //}
                //else
                //{
                //    if (Client.Connected)
                //    {
                //        return Client;
                //    }
                //    else
                //    {
                //        await Client.ConnectAsync(ip, port);
                //        return Client;
                //    }
                //}
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void SendBaseAsync(TcpMessage message, TcpClient client)
        {
            byte[] stream = Serialize(message);
            using (var memstream = new MemoryStream(stream))
            {
                SendBase(memstream, client);
            }
        }

        private void SendBase(Stream stream, TcpClient client)
        {
            using (var networkStream = client.GetStream())
            {
                int bufSize = client.ReceiveBufferSize;
                var buf = new byte[bufSize];

                int bytesRead;
                while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
                {
                    networkStream.Write(buf, 0, bytesRead);
                }
            }
        }

        #endregion

    }
}










//private TcpMessage PrepareMessage(MessageBase message, string ip)
//{
//    message.Sender = NetworkInfoBase.IpAddress.ToString();
//    message.Reciver = ip;
//    return Wrap(message);
//}

//public async void SendMessage(MessageBase message, string ip)
//{
//    this.SendMessage(message, ip, Port);
//}

//public async void SendMessage(MessageBase message, string ip, short port)
//{
//    var client = CreateClientSock(ip, port);
//    var tcpMessage = PrepareMessage(message, ip);
//    SendBase(tcpMessage, await client);
//}

//public Task SendMessageAsync(MessageBase message, string ip)
//{
//    return SendMessageAsync(message, ip);
//}

//public Task SendMessageAsync(MessageBase message, string ip, short port)
//{
//    var task = new Task(() =>
//    {
//        var client = CreateClientSock(ip, port);
//        var tcpMessage = PrepareMessage(message, ip);
//        client.Wait();
//        SendBase(tcpMessage, client.Result);
//    });
//    task.Start();
//    return task;
//}

//public async void SendFile(Stream stream, string ip, short port)
//{
//    var client = CreateClientSock(ip, port);
//    if (client == null)
//        return;
//    await SendBase(stream, await client);
//}

//public void SendFile(Stream stream, string ip)
//{
//    SendFile(stream, ip, Port);
//}

//private async Task<TcpClient> CreateClientSock(string ip, int port)
//{
//    try
//    {
//        var client = new TcpClient();
//        await client.ConnectAsync(ip, port);
//        client.NoDelay = true;
//        return client;
//    }
//    catch (Exception e)
//    {
//        return null;
//    }
//}

//public async void SendMessageBack(MessageBase message, string ip, short port)
//{
//    SendMessageBack(message, ip, port);
//}

//public async void SendMessageBack(MessageBase message, string ip)
//{
//    var client = CreateClientSock(ip, Port);
//    var tcpMessage = PrepareMessage(message, ip);
//    tcpMessage.TcpInfoState = TcpInfoState.NotifyMessageRecived;
//    SendBase(tcpMessage, await client);
//}

//public Task<bool> SendAndReceiveAsync(MessageBase message, string ip)
//{
//    return SendAndReceiveAsync(message, ip, Port);
//}

//public Task<bool> SendAndReceiveAsync(MessageBase message, string ip, short port)
//{
//    var task = new Task<bool>(() =>
//    {
//        try
//        {
//            var sendMessageAsync = SendMessageAsync(message, ip, port);
//            sendMessageAsync.Wait();
//            return true;
//        }
//        catch (Exception)
//        {
//            return false;
//        }
//    });
//    return task;
//}

//public void SendAndReceive(MessageBase message, string ip, Action<bool> hostReached)
//{
//    try
//    {
//        SendMessage(message, ip);
//        hostReached(true);
//    }
//    catch (Exception)
//    {
//        hostReached(false);
//    }
//}

//private async void SendBase(TcpMessage message, TcpClient client)
//{
//    if (client == null)
//        return;
//    byte[] stream = Serialize(message);
//    using (var memstream = new MemoryStream(stream))
//    {
//        await SendBase(memstream, client);
//    }
//}

//private async Task SendBase(Stream stream, TcpClient client)
//{
//    if (client == null)
//        return;

//    using (var networkStream = client.GetStream())
//    {
//        int bufSize = client.ReceiveBufferSize;
//        var buf = new byte[bufSize];

//        long totalBytes = 0;
//        int bytesRead = 0;

//        while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
//        {
//            await networkStream.WriteAsync(buf, 0, bytesRead);
//            totalBytes += bytesRead;
//        }
//    }
//}