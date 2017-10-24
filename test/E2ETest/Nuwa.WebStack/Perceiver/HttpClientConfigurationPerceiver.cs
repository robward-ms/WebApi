using System.Collections.Generic;
using System.Linq;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Xunit.Abstractions;

namespace Nuwa.Perceiver
{
    internal class HttpClientConfigurationPerceiver : IRunElementPerceiver
    {
        public IEnumerable<IRunElement> Perceive(ITypeInfo typeUnderTest)
        {
            var attr = typeUnderTest.GetCustomAttributes(typeof(NuwaHttpClientConfigurationAttribute)).SingleOrDefault();

            if (attr != null)
            {
                yield return new ClientConfigurationElement()
                {
                    MessageLog = attr.GetNamedArgument<bool>("MessageLog"),
                    UseProxy = attr.GetNamedArgument<bool>("UseProxy"),
                };
            }
            else
            {
                yield return new ClientConfigurationElement();
            }
        }
    }
}