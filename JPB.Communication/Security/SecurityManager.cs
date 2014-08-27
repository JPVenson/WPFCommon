using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.Communication.Interface;

namespace JPB.Communication.Security
{
    internal class SecurityManager
    {
        static SecurityManager()
        {
            Provider = new List<ISecureMessage>();
        }

        public static List<ISecureMessage> Provider;

        public static byte[] DecryptMessage(byte[] messageBase, string messageSecType)
        {
            //MEF?

            var resolvedProvide = Provider.FirstOrDefault(s => s.CheckPublicId(messageSecType));
            if (resolvedProvide == null)
            {
                Type secMessageType = typeof (ISecureMessage);
                var providerInThisAsseambly =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(s => s.GetInterfaces().Contains(secMessageType));

                foreach (var type in providerInThisAsseambly)
                {
                    var secureMessage = Activator.CreateInstance(type, true) as ISecureMessage;
                    if(secureMessage == null)
                        continue;

                    if (Provider.Any(s => s.GetType() == type))
                    {
                        Provider.Add(secureMessage);
                    }

                    if (secureMessage.CheckPublicId(messageSecType) && resolvedProvide == null)
                    {
                        resolvedProvide = secureMessage;
                    }
                }
            }

            if (resolvedProvide == null)
                //unable to resolve 
                return messageBase;

            return resolvedProvide.EncryptMessage(messageBase);
        }
    }
}
