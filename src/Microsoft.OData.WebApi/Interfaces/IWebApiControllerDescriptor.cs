// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// Represents information that describes the HTTP controller.
    /// </summary>
    public interface IWebApiControllerDescriptor
    {
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        string ControllerName { get; }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        Type ControllerType { get; }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T" /> for this descriptor's controller. 
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;
    }
}
