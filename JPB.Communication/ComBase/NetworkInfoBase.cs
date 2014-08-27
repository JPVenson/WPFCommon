#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:03

#endregion

using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    public class NetworkInfoBase
    {
        public static IPAddress IpAddress
        {
            get
            {
                return
                    Dns.GetHostEntry(Dns.GetHostName())
                       .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            }
        }
    }
}