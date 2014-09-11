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
        const int State = 0x1;
        const string request = "Bla";

        [TestMethod]
        public async void TestSender()
        {
            var sender = NetworkFactory.Instance.GetSender(1337);

            SimulateClient2();

            var s1 = await sender.SendRequstMessage<string>(new RequstMessage() { InfoState = State }, NetworkInfoBase.IpAddress.ToString());

            Assert.AreEqual(s1, request);
        }

        private void SimulateClient2()
        {
            var reciver = NetworkFactory.Instance.GetReceiver(1337);      
            //register a handler that returns an object that will be given back to the caller
            reciver.RegisterRequstHandler(s => request, State);
        }
    }
}
