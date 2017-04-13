// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using Microsoft.OData.WebApi.Routing.Conventions;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// General context for WebApi.
    /// </summary>
    public interface IWebApiContext
    {
        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        ApplyClause ApplyClause { get; set; }

        /// <summary>
        /// Get the type of an Http error.
        /// </summary>
        Type HttpErrorType { get; }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        Uri NextLink { get; set; }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        ODataPath Path { get; }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        string RouteName { get; }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        IDictionary<string, object> RoutingConventionsStore { get; }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        SelectExpandClause SelectExpandClause { get; set; }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        long? TotalCount { get; }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        Func<long> TotalCountFunc { get; set; }
    }
}
