// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
#endif

namespace Microsoft.Test.AspNet.OData.Extensions
{
#if NETCORE
    /// <summary>
    /// Extensions for HttpRequest.
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Get the content as the value of ObjectContent.
        /// </summary>
        /// <returns>The content value.</returns>
        public static string AsObjectContentValue(this HttpContent content)
        {
            string json = content.ReadAsStringAsync().Result;
            try
            {
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                return obj["value"].ToString();
            }
            catch (JsonReaderException)
            {
            }

            return json;
        }
    }
#else
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Get the content as the value of ObjectContent.
        /// </summary>
        /// <returns>The content value.</returns>
        public static object AsObjectContentValue(this HttpContent content)
        {
            return (content as ObjectContent<string>).Value;
        }
    }
#endif
}
