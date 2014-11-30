using System;

namespace JPB.Communication.Shared
{
    public interface IUniqItem : IComparable
    {
        object Guid { get; set; }
    }
}