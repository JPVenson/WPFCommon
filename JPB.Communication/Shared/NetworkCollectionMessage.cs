using System;

namespace JPB.Communication.Shared
{
    [Serializable]
    public class NetworkCollectionMessage
    {
        private object _value;
        private string _guid;

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public NetworkCollectionMessage()
        {
        }

        public NetworkCollectionMessage(object value)
        {
            _value = value;
        }


        public object Value
        {
            get { return _value ?? (_value = new object()); }
            set { _value = value; }
        }
    }
}