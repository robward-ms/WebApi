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
    internal static class ODataMediaTypes
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

        public static string ApplicationJson
        {
            get { return _applicationJson; }
        }

        public static string ApplicationJsonODataFullMetadata
        {
            get { return _applicationJsonODataFullMetadata; }
        }

        public static string ApplicationJsonODataFullMetadataStreamingFalse
        {
            get { return _applicationJsonODataFullMetadataStreamingFalse; }
        }

        public static string ApplicationJsonODataFullMetadataStreamingTrue
        {
            get { return _applicationJsonODataFullMetadataStreamingTrue; }
        }

        public static string ApplicationJsonODataMinimalMetadata
        {
            get { return _applicationJsonODataMinimalMetadata; }
        }

        public static string ApplicationJsonODataMinimalMetadataStreamingFalse
        {
            get
            {
                return _applicationJsonODataMinimalMetadataStreamingFalse;
            }
        }

        public static string ApplicationJsonODataMinimalMetadataStreamingTrue
        {
            get
            {
                return _applicationJsonODataMinimalMetadataStreamingTrue;
            }
        }

        public static string ApplicationJsonODataNoMetadata
        {
            get { return _applicationJsonODataNoMetadata; }
        }

        public static string ApplicationJsonODataNoMetadataStreamingFalse
        {
            get { return _applicationJsonODataNoMetadataStreamingFalse; }
        }

        public static string ApplicationJsonODataNoMetadataStreamingTrue
        {
            get { return _applicationJsonODataNoMetadataStreamingTrue; }
        }

        public static string ApplicationJsonStreamingFalse
        {
            get { return _applicationJsonStreamingFalse; }
        }

        public static string ApplicationJsonStreamingTrue
        {
            get { return _applicationJsonStreamingTrue; }
        }

        public static string ApplicationXml
        {
            get { return _applicationXml; }
        }

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
