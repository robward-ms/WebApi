// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Formatter
{
    /// <summary>
    /// Wrapper for IODataRequestMessage and IODataResponseMessage.
    /// </summary>
    public class ODataMessageWrapper : IODataRequestMessage, IODataResponseMessage, IODataPayloadUriConverter, IContainerProvider
    {
        private Stream _stream;
        private Dictionary<string, string> _headers;
        private IDictionary<string, string> _contentIdMapping;
        private static readonly Regex ContentIdReferencePattern = new Regex(@"\$\d", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the ODataMessageWrapper class.
        /// </summary>
        public ODataMessageWrapper()
            : this(stream: null, headers: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ODataMessageWrapper class.
        /// </summary>
        /// <param name="stream">The stream to use for the wrapper./</param>
        public ODataMessageWrapper(Stream stream)
            : this(stream: stream, headers: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ODataMessageWrapper class.
        /// </summary>
        /// <param name="stream">The stream to use for the wrapper./</param>
        /// <param name="headers">The headers to use for the wrapper.</param>
        public ODataMessageWrapper(Stream stream, Dictionary<string, string> headers)
            : this(stream: stream, headers: headers, contentIdMapping: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ODataMessageWrapper class.
        /// </summary>
        /// <param name="stream">The stream to use for the wrapper./</param>
        /// <param name="headers">The headers to use for the wrapper.</param>
        /// <param name="contentIdMapping">The ContentId mapping to use.</param>
        public ODataMessageWrapper(Stream stream, Dictionary<string, string> headers, IDictionary<string, string> contentIdMapping)
        {
            _stream = stream;
            if (headers != null)
            {
                _headers = headers;
            }
            else
            {
                _headers = new Dictionary<string, string>();
            }
            _contentIdMapping = contentIdMapping ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the headers associated with the wrapper.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// Gets the method associated with the wrapper.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        public string Method
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the Url associated with the wrapper.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        public Uri Url
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the status code associated with the wrapper.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        public int StatusCode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the container associated with the wrapper.
        /// </summary>
        public IServiceProvider Container { get; set; }

        /// <summary>
        /// Gets a header associated with the wrapper.
        /// </summary>
        /// <param name="headerName">The header to get.</param>
        /// <returns>The header value.</returns>
        public string GetHeader(string headerName)
        {
            string value;
            if (_headers.TryGetValue(headerName, out value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Gets the stream associated with the wrapper.
        /// </summary>
        public Stream GetStream()
        {
            return _stream;
        }

        /// <summary>
        /// Sets a header associated with the wrapper.
        /// </summary>
        /// <param name="headerName">The header to set.</param>
        /// <param name="headerValue">The header value to set.</param>
        public void SetHeader(string headerName, string headerValue)
        {
            _headers[headerName] = headerValue;
        }

        /// <summary>
        /// Convert a payload Uri resolved against the ContentId mappings.
        /// </summary>
        /// <param name="baseUri">The base Uri.</param>
        /// <param name="payloadUri">The payload Uri.</param>
        /// <returns>A payload Uri resolved against the ContentId mappings.</returns>
        public Uri ConvertPayloadUri(Uri baseUri, Uri payloadUri)
        {
            if (payloadUri == null)
            {
                throw new ArgumentNullException("payloadUri");
            }

            string originalPayloadUri = payloadUri.OriginalString;
            if (ContentIdReferencePattern.IsMatch(originalPayloadUri))
            {
                string resolvedUri = ContentIdHelpers.ResolveContentId(originalPayloadUri, _contentIdMapping);
                return new Uri(resolvedUri, UriKind.RelativeOrAbsolute);
            }

            // Returning null for default resolution.
            return null;
        }
    }
}