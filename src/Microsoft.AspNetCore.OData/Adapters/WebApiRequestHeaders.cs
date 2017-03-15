// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request headers to OData WebApi.
    /// </summary>
    public class WebApiRequestHeaders : IWebApiHeaderCollection
    {
        /// <summary>
        /// The inner collection wrapped by this instance.
        /// </summary>
        private RequestHeaders requestHeader;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="headers">The inner collection.</param>
        public WebApiRequestHeaders(IHeaderDictionary headers)
        {
            this.InnerCollection = headers;
            this.requestHeader = new RequestHeaders(headers);
        }

        /// <summary>
        /// The inner collection wrapped by this instance.
        /// </summary>
        public IHeaderDictionary InnerCollection { get; private set; }

        /// <summary>
        /// Gets the value of the If-None-Match header for an HTTP request.
        /// </summary>
        public IEnumerable<WebApiEntityTagHeaderValue> IfNoneMatch
        {
            get
            {
                List<WebApiEntityTagHeaderValue> convertedValues = new List<WebApiEntityTagHeaderValue>();
                foreach (EntityTagHeaderValue value in this.requestHeader.IfNoneMatch)
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
                foreach (EntityTagHeaderValue value in this.requestHeader.IfMatch)
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
            StringValues stringValues;
            bool found = this.InnerCollection.TryGetValue(key, out stringValues);

            values = stringValues.AsEnumerable();
            return found;
        }
    }
}
