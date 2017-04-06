// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// An interface used to search for an available action.
    /// </summary>
    public interface IWebApiActionMap
    {
        /// <summary>
        /// Determines whether a specified key exists.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>True if the key exist; false otherwise.</returns>
        bool Contains(string name);

        /// <summary>
        /// Find an action matching a key.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>An action associated with keys.</returns>
        string FindMatchingAction(params string[] keys);
    }
}
