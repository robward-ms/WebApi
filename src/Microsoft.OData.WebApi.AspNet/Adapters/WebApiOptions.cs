// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi options to OData WebApi.
    /// </summary>
    internal class WebApiOptions : IWebApiOptions
    {
        /// <summary>
        /// Initializes a new instance of the WebApiOptions class.
        /// </summary>
        /// <param name="configuration">The inner configuration.</param>
        public WebApiOptions(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            this.NullDynamicPropertyIsEnabled = configuration.HasEnabledNullDynamicProperty();
            this.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
        }

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
