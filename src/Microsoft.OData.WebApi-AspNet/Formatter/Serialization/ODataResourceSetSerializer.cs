// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Extensions;

namespace Microsoft.OData.WebApi.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" /> or <see cref="IEdmComplexType"/>
    /// </summary>
    public partial class ODataResourceSetSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Get the next page link for a given Uri and page size.
        /// </summary>
        /// <param name="requestUri">The Uri</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        public static Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            return HttpRequestMessageExtensions.GetNextPageLink(requestUri, pageSize);
        }
    }
}
