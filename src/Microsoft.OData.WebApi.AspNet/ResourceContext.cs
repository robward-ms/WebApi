// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Deserialization;
using Microsoft.OData.WebApi.Formatter.Serialization;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// An instance of <see cref="ResourceContext"/> gets passed to the self link (
    /// <see cref="M:NavigationSourceConfiguration.HasIdLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasEditLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasReadLink"/>
    /// ) and navigation link (
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertyLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertiesLink"/>
    /// ) builders and can be used by the link builders to generate links.
    /// </summary>
    public partial class ResourceContext
    {
        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequestMessage Request
        {
            get
            {
                return SerializerContext.Request;
            }
            set
            {
                SerializerContext.Request = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="IWebApiUrlHelper"/> that may be used to generate links while serializing this resource
        /// instance.
        /// </summary>
        public UrlHelper Url
        {
            get
            {
                return SerializerContext.Url;
            }
            set
            {
                SerializerContext.Url = value;
            }
        }
    }
}
