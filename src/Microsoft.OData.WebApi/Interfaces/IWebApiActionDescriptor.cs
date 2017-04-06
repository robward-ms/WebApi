// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// Provides information about the action methods.
    /// </summary>
    public interface IWebApiActionDescriptor
    {
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        string ActionName { get; }

        /// <summary>
        /// Gets the information that describes the controller of the action.
        /// </summary>
        IWebApiControllerDescriptor ControllerDescriptor { get; }

        /// <summary>
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        IList<string> SupportedHttpMethods { get; }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        IList<T> GetCustomAttributes<T>(bool inherit) where T : class;

        /// <summary>
        /// Get the service provider for OData for a given route.
        /// </summary>
        /// <param name="routeName">The route name for which toi get a service provider.</param>
        /// <returns>A service provider for OData for a given route</returns>
        IServiceProvider GetODataRootContainer(string routeName);
    }
}
