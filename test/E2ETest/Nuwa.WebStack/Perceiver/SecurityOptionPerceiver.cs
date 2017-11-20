using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Xunit.Abstractions;

namespace Nuwa.Perceiver
{
    internal class SecurityOptionPerceiver : IRunElementPerceiver
    {
        public IEnumerable<IRunElement> Perceive(ITypeInfo typeUnderTest)
        {
            var securityAttribute = typeUnderTest.GetCustomAttributes(typeof(NuwaServerCertificateAttribute)).FirstOrDefault();
            if (securityAttribute != null)
            {
                var elem = new SecurityOptionElement(securityAttribute.GetNamedArgument<X509Certificate2>("Certificate"));
                return this.ToArray(elem);
            }
            else
            {
                return this.Empty();
            }
        }
    }
}