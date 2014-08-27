using System;

namespace JPB.Communication.Interface
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class ExportSecProviderAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        // This is a positional argument
        public ExportSecProviderAttribute()
        {

        }
    }
}