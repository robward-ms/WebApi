using System;

namespace Nuwa.Sdk
{
    /// <summary>
    /// A default implementation of IRunElement.
    /// 
    /// This class is supposed to help reducing duplicate codes of empty 
    /// override method. DO NOT put common logic here, because it is not
    /// supposed to and it's a bad practice. 
    /// </summary>
    public abstract class AbstractRunElement : IRunElement
    {
        public virtual string Name
        {
            get;
            protected set;
        }

        public virtual void Initialize(RunFrame frame)
        {
        }

        public virtual void Recover(RunFrame frame, Type testClassType, object testClassInstance, NuwaTestCase testCommand)
        {
        }

        public virtual void Cleanup(RunFrame frame)
        {
        }
    }
}
