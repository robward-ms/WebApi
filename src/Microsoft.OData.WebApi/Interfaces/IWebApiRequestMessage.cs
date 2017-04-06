// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Deserialization;
using Microsoft.OData.WebApi.Routing;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// Represents a HTTP request message.
    /// </summary>
    public interface IWebApiRequestMessage
    {
        /// <summary>
        /// Gets the contents of the HTTP message. 
        /// </summary>
        IWebApiContext Context { get; }

        /// <summary>
        /// Gets the collection of HTTP request headers.
        /// </summary>
        IWebApiHeaderCollection Headers { get; }


        /// <summary>
        /// Gets a value indicating if this is a raw request.
        /// </summary>
        /// <returns></returns>
        bool IsRawValueRequest();

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        bool IsCountRequest();

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        string Method {get; }

        /// <summary>
        /// Get the options associated with the request.
        /// </summary>
        IWebApiOptions Options { get; }

        /// <summary>
        /// The request container associated with the request.
        /// </summary>
        IServiceProvider RequestContainer { get; }

        /// <summary>
        /// Gets the Uri used for the HTTP request.
        /// </summary>
        Uri RequestUri{ get; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        IWebApiUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// get the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        ODataDeserializerProvider GetDeserializerProvider();

        /// <summary>
        /// Get the entity tag associated with the request.
        /// </summary>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        ETag GetETag(WebApiEntityTagHeaderValue etagHeaderValue);

        /// <summary>
        /// Get a specific type of entity tage associated with the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="etagHeaderValue"></param>
        /// <returns></returns>
        ETag GetETag<T>(WebApiEntityTagHeaderValue etagHeaderValue);

        /// <summary>
        /// Get the Edm model associated with the request.
        /// </summary>
        /// <returns></returns>
        IEdmModel GetModel();

        /// <summary>
        /// Get the next page link for a given page size.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <returns></returns>
        Uri GetNextPageLink(int pageSize);

        /// <summary>
        /// Get the next page link for a given Uri and page size.
        /// </summary>
        /// <param name="requestUri">The Uri</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        Uri GetNextPageLink(Uri requestUri, int pageSize);

        /// <summary>
        /// Get a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetODataContentIdMapping();

        /// <summary>
        /// Get the path handler associated with the request.
        /// </summary>
        /// <returns></returns>
        IODataPathHandler GetPathHandler();

        /// <summary>
        /// Get the name value pairs from the query.
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs();

        /// <summary>
        /// Get the reader settings associated with the request.
        /// </summary>
        /// <returns></returns>
        ODataMessageReaderSettings GetReaderSettings();

        /// <summary>
        /// Retrieves the route data for the given request or null if not available.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetRouteData();
    }
}
