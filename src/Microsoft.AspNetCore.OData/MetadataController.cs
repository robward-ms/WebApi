// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a controller for generating OData servicedoc and metadata document ($metadata).
    /// </summary>
    public partial class MetadataController
    {
        /// <remarks>This function uses types that are AspNetCore-specific.</remarks>
        private IEdmModel GetModelFromRequest()
        {
            return Request.GetModel();
        }
    }
}
