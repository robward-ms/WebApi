// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Routing;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// Contains methods to build URLs for ASP.NET within an application.
    /// </summary>
    public interface IWebApiUrlHelper
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        string CreateODataLink(IList<ODataPathSegment> segments);

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        string CreateODataLink(params ODataPathSegment[] segments);

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        string CreateODataLink(string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments);
    }
}
