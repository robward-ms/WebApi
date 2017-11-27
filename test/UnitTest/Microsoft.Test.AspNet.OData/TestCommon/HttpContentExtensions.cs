// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    /// <summary>
    /// A custom extension for AspNetCore to deserialize JSON content as an object.
    /// AspNet provides this in  System.Net.Http.Formatting.
    /// </summary>
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
#endif
