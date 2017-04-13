// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.OData.Extensions;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing.Conventions;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi OData properties to OData WebApi.
    /// </summary>
    public class WebApiContext : IWebApiContext
    {
        /// <summary>
        /// Initializes a new instance of the WebApiContext class.
        /// </summary>
        /// <param name="context">The inner context.</param>
        public WebApiContext(HttpRequestMessageProperties context)
        {
            this.InnerContext = context;
            this.ErrorHelper = new WebApiErrorHelper();
        }

        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        public HttpRequestMessageProperties InnerContext { get; private set; }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get { return this.InnerContext.ApplyClause; }
            set { this.InnerContext.ApplyClause = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiErrorHelper"/> to use for generating OData errors.
        /// </summary>
        public IWebApiErrorHelper ErrorHelper { get; private set; }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        public Uri NextLink
        {
            get { return this.InnerContext.NextLink; }
            set { this.InnerContext.NextLink = value; }
        }

        /// <summary>
        /// Gets or sets the OData path.
        /// </summary>
        public ODataPath Path
        {
            get { return this.InnerContext.Path; }
        }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        public string RouteName
        {
            get { return this.InnerContext.RouteName; }
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore
        {
            get { return this.InnerContext.RoutingConventionsStore; }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get { return this.InnerContext.SelectExpandClause; }
            set { this.InnerContext.SelectExpandClause = value; }
        }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public long? TotalCount
        {
            get { return this.InnerContext.TotalCount; }
        }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        public Func<long> TotalCountFunc
        {
            get { return this.InnerContext.TotalCountFunc; }
            set { this.InnerContext.TotalCountFunc = value; }
        }
    }
}
