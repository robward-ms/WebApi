// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// An interface used to search for an available action.
    /// </summary>
    public interface IWebApiActionMatch
    {
        /// <summary>
        /// Determines whether a specified key exists.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>True if the key exist; false otherwise.</returns>
        bool Contains(string name);
    }
}
