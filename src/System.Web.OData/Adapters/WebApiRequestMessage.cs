// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Deserialization;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing;
using HttpRequestMessageExtensions = System.Web.OData.Extensions.HttpRequestMessageExtensions;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request message to OData WebApi.
    /// </summary>
    public class WebApiRequestMessage : IWebApiRequestMessage
    {
        /// <summary>
        /// The inner request wrapped by this instance.
        /// </summary>
        private HttpRequestMessage innerRequest;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="request">The inner request.</param>
        public WebApiRequestMessage(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            this.innerRequest = request;

            HttpRequestMessageProperties context = request.ODataProperties();
            if (context != null)
            {
                this.Context = new WebApiContext(context);
            }

            HttpRequestHeaders headers = request.Headers;
            if (headers != null)
            {
                this.Headers = new WebApiRequestHeaders(headers);
            }

            UrlHelper uriHelper = request.GetUrlHelper();
            if (uriHelper != null)
            {
                this.UrlHelper = new WebApiUrlHelper(uriHelper);
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration != null)
            {
                this.Options = new WebApiOptions(configuration);
            }
        }

        /// <summary>
        /// Gets the contents of the HTTP message. 
        /// </summary>
        public IWebApiContext Context { get; private set; }

        /// <summary>
        /// Gets the collection of HTTP request headers.
        /// </summary>
        public IWebApiHeaders Headers { get; private set; }

        /// <summary>
        /// Gets a value indicating if this is a raw request.
        /// </summary>
        /// <returns></returns>
        public bool IsRawValueRequest()
        {
            return ODataRawValueMediaTypeMapping.IsRawValueRequest(this.innerRequest);
        }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        public bool IsCountRequest()
        {
            return ODataCountMediaTypeMapping.IsCountRequest(this.innerRequest);
        }

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        public string Method
        {
            get { return this.innerRequest.Method.ToString().ToUpperInvariant(); }
        }

        /// <summary>
        /// Get the options associated with the request.
        /// </summary>
        public IWebApiOptions Options { get; private set; }

        /// <summary>
        /// The request container associated with the request.
        /// </summary>
        public IServiceProvider RequestContainer
        {
            get { return this.innerRequest.GetRequestContainer(); }
        }

        /// <summary>
        /// Gets the Uri used for the HTTP request.
        /// </summary>
        public Uri RequestUri
        {
            get { return this.innerRequest.RequestUri; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        public IWebApiUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataDeserializerProvider DeserializerProvider
        {
            get { return this.innerRequest.GetDeserializerProvider(); }
        }

        /// <summary>
        /// Get the entity tag associated with the request.
        /// </summary>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        public ETag GetETag(WebApiEntityTagHeaderValue etagHeaderValue)
        {
            EntityTagHeaderValue value = etagHeaderValue.AsEntityTagHeaderValue();
            return this.innerRequest.GetETag(value);
        }

        /// <summary>
        /// Get a specific type of entity tage associated with the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        public ETag GetETag<T>(WebApiEntityTagHeaderValue etagHeaderValue)
        {
            EntityTagHeaderValue value = etagHeaderValue.AsEntityTagHeaderValue();
            return this.innerRequest.GetETag<T>(value);
        }

        /// <summary>
        /// Gets the Edm model associated with the request.
        /// </summary>
        /// <returns></returns>
        public IEdmModel Model
        {
            get { return this.innerRequest.GetModel(); }
        }

        /// <summary>
        /// Get the next page link for a given page size.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <returns></returns>
        public Uri GetNextPageLink(int pageSize)
        {
            return this.innerRequest.GetNextPageLink(pageSize);
        }

        /// <summary>
        /// Get the next page link for a given Uri and page size.
        /// </summary>
        /// <param name="requestUri">The Uri</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        public Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            return HttpRequestMessageExtensions.GetNextPageLink(requestUri, pageSize);
        }

        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="properties">The input property names and values.</param>
        /// <returns>The generated ETag string.</returns>
        public WebApiEntityTagHeaderValue CreateETag(IDictionary<string, object> properties)
        {
            HttpConfiguration configuration = this.innerRequest.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            return configuration.GetETagHandler().CreateETag(properties);
        }

        /// <summary>
        /// Gets a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ODataContentIdMapping
        {
            get { return this.innerRequest.GetODataContentIdMapping(); }
        }

        /// <summary>
        /// Gets the path handler associated with the request.
        /// </summary>
        /// <returns></returns>
        public IODataPathHandler PathHandler
        {
            get { return this.innerRequest.GetPathHandler(); }
        }

        /// <summary>
        /// Gets the OData query parameters from the query.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ODataQueryParameters
        {
            get
            {
                return this.innerRequest.GetQueryNameValuePairs()
                    .Where(p => p.Key.StartsWith("$", StringComparison.Ordinal) ||
                    p.Key.StartsWith("@", StringComparison.Ordinal))
                    .ToDictionary(p => p.Key, p => p.Value);
            }
        }

        /// <summary>
        /// Gets the reader settings associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataMessageReaderSettings ReaderSettings
        {
            get { return this.innerRequest.GetReaderSettings(); }
        }

        /// <summary>
        /// Gets the route data for the given request or null if not available.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> RouteData
        {
            get { return this.innerRequest.GetRouteData().Values; }
        }
    }
}
