#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 11:31

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JPB.Communication.ComBase;

namespace JPB.Communication
{
    public class NetworkFactory
    {
        private static NetworkFactory _instance = new NetworkFactory();
        private Dictionary<short, TCPNetworkReceiver> _receivers;
        private Dictionary<short, TCPNetworkSender> _senders;
        private TCPNetworkReceiver _commonReciever;
        private TCPNetworkSender _commonSender;
        private Object _mutex;

        private NetworkFactory()
        {
            _receivers = new Dictionary<short, TCPNetworkReceiver>();
            _senders = new Dictionary<short, TCPNetworkSender>();
            _mutex = new object();
        }

        public static NetworkFactory Instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        public TCPNetworkReceiver Reciever
        {
            get
            {
                if (_commonReciever == null)
                    throw new ArgumentException("There is no port supplied. call InitCommonSenderAndReciver first");
                return _commonReciever;
            }
            set { _commonReciever = value; }
        }

        public TCPNetworkSender Sender
        {
            get
            {
                if (_commonSender == null)
                    throw new ArgumentException("There is no port supplied. call InitCommonSenderAndReciver first");
                return _commonSender;
            }
            set { _commonSender = value; }
        }

        public void InitCommonSenderAndReciver(short listeningPort = -1, short sendingPort = -1)
        {
            if (listeningPort != -1)
            {
                Reciever = GetReceiver(listeningPort);
            }

            if (sendingPort != -1)
            {
                Sender = GetSender(sendingPort);
            }
        }

        /// <summary>
        /// Gets or Creates a Network sender for a given port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkSender GetSender(short port)
        {
            lock (_mutex)
            {
                var element = _senders.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateSender(port);
            }
        }

        private TCPNetworkSender CreateSender(short port)
        {
            var sender = new TCPNetworkSender(port);
            _senders.Add(port, sender);
            return sender;
        }

        /// <summary>
        /// Gets or Creats a network Reciever for a given port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TCPNetworkReceiver GetReceiver(short port)
        {
            lock (_mutex)
            {
                var element = _receivers.FirstOrDefault(s => s.Key == port);

                if (!element.Equals(null) && element.Value != null)
                {
                    return element.Value;
                }

                return CreateReceiver(port);
            }
        }

        private TCPNetworkReceiver CreateReceiver(short port)
        {
            var receiver = new TCPNetworkReceiver(port);
            _receivers.Add(port, receiver);
            return receiver;
        }
    }
}