// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// An interface for WebApi options.
    /// </summary>
    public interface IWebApiOptions
    {
        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        ODataUrlKeyDelimiter UrlKeyDelimiter { get; }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        bool NullDynamicPropertyIsEnabled { get; }
    }
}
