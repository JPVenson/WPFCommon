using System;
using System.Collections.Generic;
using JPB.Communication;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testing
{
    [TestClass]
    public class NetworkTest
    {
        [TestMethod]
        public async void TestSender()
        {
            var request = new List<User>();
            request.Add(new User());
            request.Add(new User());
            request.Add(new User());
            request.Add(new User());

            const int state = 0x1;

            var reciver = NetworkFactory.Instance.GetReceiver(1337);
            var sender = NetworkFactory.Instance.GetSender(1337);

            //register a handler that returns an object that will be given back to the caller
            reciver.RegisterRequstHandler(s => request, state);

            var s1 = await sender.SendRequstMessage<List<User>>(new RequstMessage() { InfoState = state }, NetworkInfoBase.IpAddress.ToString());

            Assert.AreEqual(s1, request);
        }
    }
}
