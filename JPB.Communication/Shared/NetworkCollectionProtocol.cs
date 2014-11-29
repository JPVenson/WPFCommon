namespace JPB.Communication.Shared
{
    /// <summary>
    /// A static collection of the Network Contracts that are used be the Collection classes
    /// </summary>
    public static class NetworkCollectionProtocol
    {
        public static string CollectionAdd
        {
            get { return "ADD-70A71976-7610-4100-99D6-97E09A53C79A"; }
        }

        public static string CollectionReset
        {
            get { return "RES-4732A847-A949-4CFA-8464-28651F0E5F67"; }
        }

        public static string CollectionRemove
        {
            get { return "REM-A956886F-985F-4D24-825B-97259B144506"; }
        }

        public static string CollectionRegisterUser
        {
            get { return "RUS-8E76ADE0-CF05-4E50-B4A1-C9C074444ADD"; }
        }

        public static string CollectionUnRegisterUser
        {
            get { return "URS-411141B2-DCA6-4C20-B06F-0B7302C84A02"; }
        }

        public static string CollectionGetCollection
        {
            get { return "GCO-EEED881A-032B-4DB1-857A-5359B65CAB4C"; }
        }

        public static string CollectionGetUsers
        {
            get { return "GUS-ABB1D663-B6BF-42BD-90A6-DFBC196AE1CE"; }
        }

        public static string CollectionUpdateItem
        {
            get { return "UPI-1A0AE184-3C66-4B73-9E0A-6C2B9C393EFC"; }
        }
    }
}
