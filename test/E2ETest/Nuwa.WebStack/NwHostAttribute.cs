using System;

namespace Nuwa
{
    /// <summary>
    /// Defines the host strategy of attributed test class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NuwaHostAttribute : Attribute
    {
        public NuwaHostAttribute(HostType type)
        {
            this.HostType = type;
        }

        public HostType HostType
        {
            get;
            private set;
        }
    }
}