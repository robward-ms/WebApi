// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Routing.Conventions;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// Contains information for a single HTTP operation.
    /// </summary>
    public interface IWebApiControllerContext
    {
        /// <summary>
        /// The selected controller result.
        /// </summary>
        SelectControllerResult ControllerResult { get; }

        /// <summary>
        /// Gets the request.
        /// </summary>
        IWebApiRequestMessage Request { get; }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        IDictionary<string, object> RouteData { get; }
    }
}
