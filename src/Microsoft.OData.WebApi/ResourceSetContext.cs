// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public class ResourceSetContext
    {
        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public IWebApiRequestMessage Request { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySetBase"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySetBase EntitySetBase { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        public IWebApiUrlHelper Url { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// </summary>
        public object ResourceSetInstance { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        public IEdmModel EdmModel
        {
            get
            {
                return Request.Model;
            }
        }
    }
}
