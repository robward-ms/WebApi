// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// Provides an abstraction for managing the assemblies of an application.
    /// </summary>
    public class WebApiDefaultAssembliesResolver : IWebApiAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies available for the application. 
        /// </summary>
        /// <returns>A list of assemblies available for the application. </returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return null;
        }
    }
}
