// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.WebApi.Builder
{
    /// <summary>
    /// An annotation for title.
    /// </summary>
    public class OperationTitleAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the OperationTitleAnnotation class.
        /// </summary>
        /// <param name="title">The title associated with the annotation.</param>
        public OperationTitleAnnotation(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Gets the title associated with the annotation.
        /// </summary>
        public string Title { get; private set; }
    }
}
