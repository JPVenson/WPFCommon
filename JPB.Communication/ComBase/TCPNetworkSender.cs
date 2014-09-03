#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:11

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Interface;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkSender : Networkbase
    {
        internal TCPNetworkSender(short port)
        {
            Port = port;
            Timeout = TimeSpan.FromSeconds(15);
        }

        public short Port { get; private set; }
        public TimeSpan Timeout { get; set; }

        #region Message Methods

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

        #endregion

        /// <summary>
        /// Sends a message an awaits a response on the same port from the other side
        /// </summary>
        /// <param name="mess"></param>
        /// <param name="ip"></param>
        /// <returns>Result from other side or default(T)</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<T> SendRequstMessage<T>(RequstMessage mess, string ip)
        {
            var result = default(T);
            var waitForResponsive = new AutoResetEvent(false);
            var reciever = NetworkFactory.Instance.GetReceiver(Port);
            reciever.RegisterRequst(s =>
            {
                result = (T)s.Message;
                waitForResponsive.Set();
            }, mess.Id);
            var isSend = SendMessage(mess, ip);
            if (isSend)
            {
                waitForResponsive.WaitOne(Timeout);
            }
            return result;
        }

        public async void SendFile(FileStream stream, MessageBase mess, string ip)
        {
            throw new NotImplementedException();

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

        private static async Task<TcpClient> CreateClientSockAsync(string ip, int port)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                client.NoDelay = true;
                return client;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void SendBaseAsync(TcpMessage message, TcpClient client)
        {
            using (var memstream = new MemoryStream(Serialize(message)))
            {
                using (SendBase(memstream, client))
                {

                }
            }
        }

        private NetworkStream SendBase(Stream stream, TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();

            int bufSize = client.ReceiveBufferSize;
            var buf = new byte[bufSize];

            int bytesRead;
            while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
            {
                networkStream.Write(buf, 0, bytesRead);
            }

            networkStream.Write(new byte[0], 0, 0);

            return networkStream;
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