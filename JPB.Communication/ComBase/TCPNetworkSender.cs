#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:11

#endregion

using System;
using System.IO;
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

        public static async Task SendMessage(MessageBase message, string ip, short port)
        {
            var sender = NetworkFactory.Instance.GetSender(port);
            await sender.SendMessageAsync(message, ip);
        }

        public async void SendMessage(MessageBase message, string ip)
        {
            await SendMessageAsync(message, ip);
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
                var sendBaseAsync = SendBaseAsync(tcpMessage, result);
                sendBaseAsync.Wait();
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
            await SendBaseAsync(stream, client);
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

        private Task SendBaseAsync(TcpMessage message, TcpClient client)
        {
            if (client == null)
                return null;
            byte[] stream = Serialize(message);
            using (var memstream = new MemoryStream(stream))
            {
                return SendBaseAsync(memstream, client);
            }
        }

        private Task SendBaseAsync(Stream stream, TcpClient client)
        {
            if (client == null)
                return null;

            using (var networkStream = client.GetStream())
            {
                int bufSize = client.ReceiveBufferSize;
                var buf = new byte[bufSize];

                long totalBytes = 0;
                int bytesRead = 0;
                Task writeAsync = null;

                while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
                {
                    writeAsync = networkStream.WriteAsync(buf, 0, bytesRead);
                    totalBytes += bytesRead;
                }
                return writeAsync;
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