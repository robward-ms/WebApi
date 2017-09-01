// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A helper class for comparing Http methods.
    /// </summary>
    internal class HttpMethodHelper
    {
        /// <summary>
        /// "Get"
        /// </summary>
        public const string HttpGet = "GET";

        /// <summary>
        /// "Delete"
        /// </summary>
        public const string HttpDelete = "DELETE";

        /// <summary>
        /// "Merge"
        /// </summary>
        public const string HttpMerge = "MERGE";

        /// <summary>
        /// "Patch"
        /// </summary>
        public const string HttpPatch = "PATCH";

        /// <summary>
        /// "Post"
        /// </summary>
        public const string HttpPost = "POST";

        /// <summary>
        /// "Put"
        /// </summary>
        public const string HttpPut = "PUT";

        /// <summary>
        /// Determine if the method is delete.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is delete; false otherwise.</returns>
        public static bool IsDelete(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == HttpDelete;
        }

        /// <summary>
        /// Determine if the method is put.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is put; false otherwise.</returns>
        public static bool IsPut(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == HttpPut;
        }

        /// <summary>
        /// Determine if the method is post.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is post; false otherwise.</returns>
        public static bool IsPost(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == HttpPost;
        }

        /// <summary>
        /// Determine if the method is patch.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is patch; false otherwise.</returns>
        public static bool IsPatch(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == HttpPatch;
        }

        /// <summary>
        /// Determine if the method is get.
        /// </summary>
        /// <param name="httpMethod">The method to test.</param>
        /// <returns>True if the method is get; false otherwise.</returns>
        public static bool IsGet(string httpMethod)
        {
            return httpMethod.ToUpperInvariant() == HttpGet;
        }
    }
}
