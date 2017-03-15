// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.WebApi.Routing.Template;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// An interface used to supply attribute mappings.
    /// </summary>
    public interface IAttributeMappingProvider
    {
        /// <summary>
        /// Gets the attribute mapping for the system.
        /// </summary>
        IDictionary<ODataPathTemplate, IWebApiActionDescriptor> AttributeMappings { get; }
    }
}
