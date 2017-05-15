// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// Represents an entity-tag header value.
    /// </summary>
    public class WebApiEntityTagHeaderValue
    {
        /// <summary>
        /// Initializes a new instance of the EntityTagHeaderValue class.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="isWeak"></param>
        public WebApiEntityTagHeaderValue(string tag, bool isWeak)
        {
            this.Tag = tag;
            this.IsWeak = isWeak;
        }

        /// <summary>
        /// Gets a valie indicating if the entity set is weak.
        /// </summary>
        public bool IsWeak { get; private set; }

        /// <summary>
        /// Gets the tag for the header value.
        /// </summary>
        public string Tag { get; private set; }
    }
}
