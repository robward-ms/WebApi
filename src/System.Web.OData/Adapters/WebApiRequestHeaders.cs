// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Interfaces;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request headers to OData WebApi.
    /// </summary>
    public class WebApiRequestHeaders : IWebApiHeaderCollection
    {
        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="headers">The inner collection.</param>
        public WebApiRequestHeaders(HttpRequestHeaders headers)
        {
            this.InnerCollection = headers;
        }

        /// <summary>
        /// The inner collection wrapped by this instance.
        /// </summary>
        public HttpRequestHeaders InnerCollection { get; private set; }

        /// <summary>
        /// Gets the value of the If-None-Match header for an HTTP request.
        /// </summary>
        public IEnumerable<WebApiEntityTagHeaderValue> IfNoneMatch
        {
            get
            {
                List<WebApiEntityTagHeaderValue> convertedValues = new List<WebApiEntityTagHeaderValue>();
                foreach (EntityTagHeaderValue value in this.InnerCollection.IfNoneMatch)
                {
                    convertedValues.Add(new WebApiEntityTagHeaderValue(value.Tag, value.IsWeak));
                }

                return convertedValues;
            }
        }

        /// <summary>
        /// Gets the value of the If-Match header for an HTTP request.
        /// </summary>
        public IEnumerable<WebApiEntityTagHeaderValue> IfMatch
        {
            get
            {
                List<WebApiEntityTagHeaderValue> convertedValues = new List<WebApiEntityTagHeaderValue>();
                foreach (EntityTagHeaderValue value in this.InnerCollection.IfMatch)
                {
                    convertedValues.Add(new WebApiEntityTagHeaderValue(value.Tag, value.IsWeak));
                }

                return convertedValues;
            }
        }

        /// <summary>
        /// Return if a specified header and specified values are stored in the collection.
        /// </summary>
        /// <param name="key">The specified header.</param>
        /// <param name="values">The specified header values.</param>
        /// <returns>true is the specified header name and values are stored in the collection; otherwise false.</returns>
        public bool TryGetValues(string key, out IEnumerable<string> values)
        {
            return this.InnerCollection.TryGetValues(key, out values);
        }
    }
}
