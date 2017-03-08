// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// A helper class for comparing Http methods.
    /// </summary>
    public class HttpMethodHelper
    {
        /// <summary>
        /// Determine if the method is delete.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is delete; false otherwise.</returns>
        public static bool IsDelete(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == "DELETE";
        }

        /// <summary>
        /// Determine if the method is put.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is put; false otherwise.</returns>
        public static bool IsPut(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == "PUT";
        }

        /// <summary>
        /// Determine if the method is post.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is post; false otherwise.</returns>
        public static bool IsPost(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == "POST";
        }

        /// <summary>
        /// Determine if the method is patch.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is patch; false otherwise.</returns>
        public static bool IsPatch(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == "PATCH";
        }

        /// <summary>
        /// Determine if the method is get.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is get; false otherwise.</returns>
        public static bool IsGet(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == "GET";
        }
    }
}
