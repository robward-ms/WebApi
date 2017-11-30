// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
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
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Get the OData path.
        /// </summary>
        /// <returns>The OData path</returns>
        public static ODataPath GetODataPath(this HttpRequest request)
        {
            return request.ODataFeature().Path;
        }

        /// <summary>
        /// Get the OData path.
        /// </summary>
        /// <returns>The OData path</returns>
        public static void SetODataPath(this HttpRequest request, ODataPath path)
        {
            request.ODataFeature().Path = path;
        }
    }
#else
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
       /// <summary>
        /// Get the OData path.
        /// </summary>
        /// <returns>The OData path</returns>
        public static ODataPath GetODataPath(this HttpRequestMessage request)
        {
            return request.ODataProperties().Path;
        }

        /// <summary>
        /// Get the OData path.
        /// </summary>
        /// <returns>The OData path</returns>
        public static void SetODataPath(this HttpRequestMessage request, ODataPath path)
        {
            request.ODataProperties().Path = path;
        }
    }
#endif
}
