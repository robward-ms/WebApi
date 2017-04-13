// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.WebApi.Interfaces;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi options to OData WebApi.
    /// </summary>
    public class WebApiOptions : IWebApiOptions
    {
        /// <summary>
        /// Initializes a new instance of the WebApiOptions class.
        /// </summary>
        /// <param name="configuration">The inner configuration.</param>
        public WebApiOptions(HttpConfiguration configuration)
        {
            this.InnerConfiguration = configuration;
            if (configuration != null)
            {
                this.NullDynamicPropertyIsEnabled = configuration.HasEnabledNullDynamicProperty();
                this.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
            }
        }

        /// <summary>
        /// The inner configuration wrapped by this instance.
        /// </summary>
        public HttpConfiguration InnerConfiguration { get; private set; }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; private set; }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        public bool NullDynamicPropertyIsEnabled { get; private set; }
    }
}
