using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;
using Xunit.Abstractions;

namespace Nuwa.Perceiver
{
    internal class TracePerceiver : IRunElementPerceiver
    {
        public TracePerceiver()
        {
        }

        public IEnumerable<IRunElement> Perceive(ITypeInfo typeUnderTest)
        {
            var traceSetting = ConfigurationManager.AppSettings["global.trace.setting"];
            var strDefaultTraceType = ConfigurationManager.AppSettings["global.trace.default"];

            Type defaultTraceType = null;
            if (!string.IsNullOrEmpty(strDefaultTraceType))
            {
                defaultTraceType = Type.GetType(strDefaultTraceType);
            }

            /// here's the process of deciding trace writer
            /// 1. if the NuwaTraceAttribute exists and the AlwaysOff property is true, then generate no run element
            /// 2. if the NuwaTraceAttribute exists and specific a TraceWriter type, always use it.
            /// 3. if the NuwaTraceAttribute doesn't exist, check the Default Settings.
            /// 4. (incorrect path, but just case it is reached). if the NuwaTraceAttribute exists, and the
            ///    TraceWriter type is null

            var attr = typeUnderTest.GetCustomAttributes(typeof(NuwaTraceAttribute)).FirstOrDefault();
            if (attr != null)
            {
                if (attr.GetNamedArgument<bool>("AlwaysOff"))
                {
                    return Enumerable.Empty<IRunElement>();
                }
                else if (attr.GetNamedArgument<Type>("TraceWriter") != null)
                {
                    return new IRunElement[] { new TraceElement(attr.GetNamedArgument<Type>("TraceWriter")) };
                }
                else
                {
                    return new IRunElement[] { new TraceElement(defaultTraceType) };
                }
            }
            else
            {
                if (traceSetting == "off")
                {
                    return Enumerable.Empty<IRunElement>();
                }
                else if (traceSetting == "on")
                {
                    return new IRunElement[] { new TraceElement(defaultTraceType) };
                }
                else
                {
                    // indicate traceSetting == "both"
                    return new IRunElement[] { new TraceElement(null), new TraceElement(defaultTraceType) };
                }
            }
        }
    }
}