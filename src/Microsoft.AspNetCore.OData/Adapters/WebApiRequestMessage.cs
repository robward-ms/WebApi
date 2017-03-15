// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Deserialization;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request message to OData WebApi.
    /// </summary>
    public class WebApiRequestMessage : IWebApiRequestMessage
    {
        /// <summary>
        /// The inner collection wrapped by this instance.
        /// </summary>
        private RequestHeaders requestHeader;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="request">The inner request.</param>
        public WebApiRequestMessage(HttpRequest request)
        {
            this.InnerRequest = request;
            this.Context = new WebApiContext(request.ODataFeature());
            this.Headers = new WebApiRequestHeaders(request.Headers);
            this.UrlHelper = new WebApiUrlHelper(request.HttpContext.UrlHelper());
            this.Options = new WebApiOptions(request.ODataFeature());
            this.requestHeader = new RequestHeaders(request.Headers);
        }

        /// <summary>
        /// The inner request wrapped by this instance.
        /// </summary>
        public HttpRequest InnerRequest { get; private set; }

        /// <summary>
        /// Gets the contents of the HTTP message. 
        /// </summary>
        public IWebApiContext Context { get; private set; }

        /// <summary>
        /// Gets the collection of HTTP request headers.
        /// </summary>
        public IWebApiHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets a value indicating if this is a raw request.
        /// </summary>
        /// <returns></returns>
        public bool IsRawValueRequest()
        {
            ODataPath path = this.InnerRequest.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is ValueSegment;
        }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        public bool IsCountRequest()
        {
            ODataPath path = this.InnerRequest.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is CountSegment;
        }

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        public string Method
        {
            get { return this.InnerRequest.Method.ToString().ToUpperInvariant(); }
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
            get { return this.InnerRequest.HttpContext.RequestServices; }
        }

        /// <summary>
        /// Gets the Uri used for the HTTP request.
        /// </summary>
        public Uri RequestUri
        {
            get { return new Uri(this.InnerRequest.GetEncodedUrl()); }
        }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        public IWebApiUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// get the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataDeserializerProvider GetDeserializerProvider()
        {
            return this.InnerRequest.HttpContext.RequestServices.GetRequiredService<ODataDeserializerProvider>();
        }

        /// <summary>
        /// Get the entity tag associated with the request.
        /// </summary>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        public ETag GetETag(WebApiEntityTagHeaderValue etagHeaderValue)
        {
            EntityTagHeaderValue value = new EntityTagHeaderValue(etagHeaderValue.Tag, etagHeaderValue.IsWeak);
            return this.InnerRequest.GetETag(value);
        }

        /// <summary>
        /// Get a specific type of entity tage associated with the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        public ETag GetETag<T>(WebApiEntityTagHeaderValue etagHeaderValue)
        {
            EntityTagHeaderValue value = new EntityTagHeaderValue(etagHeaderValue.Tag, etagHeaderValue.IsWeak);
            return this.InnerRequest.GetETag<T>(value);
        }

        /// <summary>
        /// Get the Edm model associated with the request.
        /// </summary>
        /// <returns></returns>
        public IEdmModel GetModel()
        {
            return this.InnerRequest.HttpContext.ODataFeature().Model;
        }

        /// <summary>
        /// Get the next page link for a given page size.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <returns></returns>
        public Uri GetNextPageLink(int pageSize)
        {
            return this.InnerRequest.GetNextPageLink(pageSize);
        }

        /// <summary>
        /// Get the next page link for a given Uri and page size.
        /// </summary>
        /// <param name="requestUri">The Uri</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        public Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            return HttpRequestExtensions.GetNextPageLink(requestUri, pageSize);
        }

        /// <summary>
        /// Get a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetODataContentIdMapping()
        {
            return null;
        }

        /// <summary>
        /// Get the path handler associated with the request.
        /// </summary>
        /// <returns></returns>
        public IODataPathHandler GetPathHandler()
        {
            return this.InnerRequest.HttpContext.ODataPathHandler();
        }

        /// <summary>
        /// Get the name value pairs from the query.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs()
        {
            IQueryCollection colleciton = this.InnerRequest.Query;
            return colleciton.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));
        }

        /// <summary>
        /// Get the reader settings associated with the request.
        /// </summary>;
        /// <returns></returns>
        public ODataMessageReaderSettings GetReaderSettings()
        {
            return this.InnerRequest.GetReaderSettings();
        }

        /// <summary>
        /// Retrieves the route data for the given request or null if not available.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetRouteData()
        {
            return this.InnerRequest.HttpContext.GetRouteData().Values;
        }
    }
}
