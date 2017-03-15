// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;
using Microsoft.OData.WebApi.Interfaces;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi assembly resolver to OData WebApi.
    /// </summary>
    public class WebApiAssembliesResolver : IWebApiAssembliesResolver
    {
        /// <summary>
        /// Initializes a new instance of the WebApiAssembliesResolver class.
        /// </summary>
        /// <param name="resolver">The inner resolver.</param>
        public WebApiAssembliesResolver(IAssembliesResolver resolver)
        {
            this.InnerResolver = resolver;
        }

        /// <summary>
        /// The inner resolver wrapped by this instance.
        /// </summary>
        public IAssembliesResolver InnerResolver { get; private set; }

        /// <summary>
        /// Returns a list of assemblies available for the application. 
        /// </summary>
        /// <returns>A list of assemblies available for the application. </returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return this.InnerResolver.GetAssemblies();
        }
    }
}
