// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi options to OData WebApi.
    /// </summary>
    public class WebApiOptions : IWebApiOptions
    {
        /// <summary>
        /// Initializes a new instance of the WebApiOptions class.
        /// </summary>
        /// <param name="feature">The inner feature.</param>
        public WebApiOptions(IODataFeature feature)
        {
            this.InnerFeature = feature;
            if (feature != null)
            {
                this.NullDynamicPropertyIsEnabled = feature.IsNullDynamicPropertyEnabled;
                this.UrlKeyDelimiter = feature.UrlKeyDelimiter;
            }
        }

        /// <summary>
        /// The inner feature wrapped by this instance.
        /// </summary>
        public IODataFeature InnerFeature { get; private set; }

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
