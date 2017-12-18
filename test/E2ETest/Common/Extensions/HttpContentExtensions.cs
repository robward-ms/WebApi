// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
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
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
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
