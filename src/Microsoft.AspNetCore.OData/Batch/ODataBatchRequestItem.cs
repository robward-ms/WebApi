// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents an OData batch request.
    /// </summary>
    public abstract class ODataBatchRequestItem
    {
        /// <summary>
        /// Routes a single OData batch request.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="context">The context.</param>
        /// <param name="contentIdToLocationMapping">The Content-ID to Location mapping.</param>
        /// <returns></returns>
        public static async Task RouteAsync(IRouter router, HttpContext context, Dictionary<string, string> contentIdToLocationMapping)
        {
            if (router == null)
            {
                throw Error.ArgumentNull("router");
            }
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (contentIdToLocationMapping != null)
            {
                string queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : String.Empty;
                string resolvedRequestUrl = ContentIdHelpers.ResolveContentId(queryString, contentIdToLocationMapping);
                context.Request.CopyAbsoluteUrl(new Uri(resolvedRequestUrl));

                context.Request.SetODataContentIdMapping(contentIdToLocationMapping);
            }

            RouteContext routeContext = new RouteContext(context);
            routeContext.RouteData.Routers.Add(router);
            await router.RouteAsync(routeContext);

            string contentId = context.Request.GetODataContentId();

            if (contentIdToLocationMapping != null && contentId != null)
            {
                AddLocationHeaderToMapping(context.Response, contentIdToLocationMapping, contentId);
            }
        }

        private static void AddLocationHeaderToMapping(
            HttpResponse response,
            IDictionary<string, string> contentIdToLocationMapping,
            string contentId)
        {
            Contract.Assert(response != null);
            Contract.Assert(response.Headers != null);
            Contract.Assert(contentIdToLocationMapping != null);
            Contract.Assert(contentId != null);

            var headers = response.GetTypedHeaders();
            if (headers.Location != null)
            {
                contentIdToLocationMapping.Add(contentId, headers.Location.AbsoluteUri);
            }
        }

        /// <summary>
        /// Routes the request.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <returns>A <see cref="ODataBatchResponseItem"/>.</returns>
        public abstract Task<ODataBatchResponseItem> RouteAsync(IRouter router);

    }
}