// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// Helper methods for request preference headers.
    /// </summary>
    public static class RequestPreferenceHelpers
    {
        /// <summary>
        /// The request preference header name.
        /// </summary>
        public const string PreferHeaderName = "Prefer";

        /// <summary>
        /// The request preference header name return content value.
        /// </summary>
        public const string ReturnContentHeaderValue = "return=representation";

        /// <summary>
        /// The request preference header name return no content value.
        /// </summary>
        public const string ReturnNoContentHeaderValue = "return=minimal";

        /// <summary>
        /// Determine if the request preference header specifies return content.
        /// </summary>
        /// <param name="headers">The header collection.</param>
        /// <returns>True if the request preference header specifies return content; false otherwise.</returns>
        public static bool RequestPrefersReturnContent(IWebApiHeaderCollection headers)
        {
            IEnumerable<string> preferences = null;
            if (headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        /// <summary>
        /// Determine if the request preference header specifies return no content.
        /// </summary>
        /// <param name="headers">The header collection.</param>
        /// <returns>True if the request preference header specifies return no content; false otherwise.</returns>
        public static bool RequestPrefersReturnNoContent(IWebApiHeaderCollection headers)
        {
            IEnumerable<string> preferences = null;
            if (headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }

        /// <summary>
        /// Get the request preference header.
        /// </summary>
        /// <param name="headers">The header collection.</param>
        /// <returns>The request preference header.</returns>
        public static string GetRequestPreferHeader(IWebApiHeaderCollection headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues(PreferHeaderName, out values))
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
