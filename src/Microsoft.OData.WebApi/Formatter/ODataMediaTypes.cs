// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.OData.WebApi.Formatter
{
    /// <summary>
    /// Contains media types used by the OData formatter.
    /// </summary>
    public static class ODataMediaTypes
    {
        private static readonly string _applicationJson = "application/json";
        private static readonly string _applicationJsonODataFullMetadata = "application/json;odata.metadata=full";
        private static readonly string _applicationJsonODataFullMetadataStreamingFalse = "application/json;odata.metadata=full;odata.streaming=false";
        private static readonly string _applicationJsonODataFullMetadataStreamingTrue = "application/json;odata.metadata=full;odata.streaming=true";
        private static readonly string _applicationJsonODataMinimalMetadata = "application/json;odata.metadata=minimal";
        private static readonly string _applicationJsonODataMinimalMetadataStreamingFalse = "application/json;odata.metadata=minimal;odata.streaming=false";
        private static readonly string _applicationJsonODataMinimalMetadataStreamingTrue = "application/json;odata.metadata=minimal;odata.streaming=true";
        private static readonly string _applicationJsonODataNoMetadata = "application/json;odata.metadata=none";
        private static readonly string _applicationJsonODataNoMetadataStreamingFalse = "application/json;odata.metadata=none;odata.streaming=false";
        private static readonly string _applicationJsonODataNoMetadataStreamingTrue = "application/json;odata.metadata=none;odata.streaming=true";
        private static readonly string _applicationJsonStreamingFalse = "application/json;odata.streaming=false";
        private static readonly string _applicationJsonStreamingTrue = "application/json;odata.streaming=true";
        private static readonly string _applicationXml = "application/xml";

        /// <summary>
        /// Gets "application/json".
        /// </summary>
        public static string ApplicationJson
        {
            get { return _applicationJson; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=full".
        /// </summary>
        public static string ApplicationJsonODataFullMetadata
        {
            get { return _applicationJsonODataFullMetadata; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=full;odata.streaming=false".
        /// </summary>
        public static string ApplicationJsonODataFullMetadataStreamingFalse
        {
            get { return _applicationJsonODataFullMetadataStreamingFalse; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=full;odata.streaming=true".
        /// </summary>
        public static string ApplicationJsonODataFullMetadataStreamingTrue
        {
            get { return _applicationJsonODataFullMetadataStreamingTrue; }
        }

        /// <summary>
        /// Gets  "application/json;odata.metadata=minimal".
        /// </summary>
        public static string ApplicationJsonODataMinimalMetadata
        {
            get { return _applicationJsonODataMinimalMetadata; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=minimal;odata.streaming=false".
        /// </summary>
        public static string ApplicationJsonODataMinimalMetadataStreamingFalse
        {
            get
            {
                return _applicationJsonODataMinimalMetadataStreamingFalse;
            }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=minimal;odata.streaming=true".
        /// </summary>
        public static string ApplicationJsonODataMinimalMetadataStreamingTrue
        {
            get
            {
                return _applicationJsonODataMinimalMetadataStreamingTrue;
            }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=none".
        /// </summary>
        public static string ApplicationJsonODataNoMetadata
        {
            get { return _applicationJsonODataNoMetadata; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=none;odata.streaming=false".
        /// </summary>
        public static string ApplicationJsonODataNoMetadataStreamingFalse
        {
            get { return _applicationJsonODataNoMetadataStreamingFalse; }
        }

        /// <summary>
        /// Gets "application/json;odata.metadata=none;odata.streaming=true".
        /// </summary>
        public static string ApplicationJsonODataNoMetadataStreamingTrue
        {
            get { return _applicationJsonODataNoMetadataStreamingTrue; }
        }

        /// <summary>
        /// Gets "application/json;odata.streaming=false".
        /// </summary>
        public static string ApplicationJsonStreamingFalse
        {
            get { return _applicationJsonStreamingFalse; }
        }

        /// <summary>
        /// Gets "application/json;odata.streaming=true".
        /// </summary>
        public static string ApplicationJsonStreamingTrue
        {
            get { return _applicationJsonStreamingTrue; }
        }

        /// <summary>
        /// Gets "application/xml".
        /// </summary>
        public static string ApplicationXml
        {
            get { return _applicationXml; }
        }

        /// <summary>
        /// Get the meta-data level for a given media type.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="parameters">The media type parameters.</param>
        /// <returns>The meta-data level for a given media type.</returns>
        public static ODataMetadataLevel GetMetadataLevel(string mediaType, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (mediaType == null)
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            if (!String.Equals(ODataMediaTypes.ApplicationJson, mediaType,
                StringComparison.Ordinal))
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            Contract.Assert(parameters != null);
            KeyValuePair<string,string> odataParameter =
                parameters.FirstOrDefault(
                    (p) => String.Equals("odata.metadata", p.Key, StringComparison.OrdinalIgnoreCase));

            if (!odataParameter.Equals(default(KeyValuePair<string, string>)))
            {
                if (String.Equals("full", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.FullMetadata;
                }
                if (String.Equals("none", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.NoMetadata;
                }
            }

            // Minimal is the default metadata level
            return ODataMetadataLevel.MinimalMetadata;
        }
    }
}
