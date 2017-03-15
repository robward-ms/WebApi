// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi action map to OData WebApi.
    /// </summary>
    public class WebApiActionMatch : IWebApiActionMatch
    {
        /// <summary>
        /// Initializes a new instance of the WebApiActionMatch class.
        /// </summary>
        /// <param name="actionMap">The inner map.</param>
        public WebApiActionMatch(IEnumerable<ControllerActionDescriptor> actionMap)
        {
            this.InnerMap = actionMap;
        }

        /// <summary>
        /// The inner map wrapped by this instance.
        /// </summary>
        public IEnumerable<ControllerActionDescriptor> InnerMap { get; private set; }

        /// <summary>
        /// Determines whether a specified key exists.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>True if the key exist; false otherwise.</returns>
        public bool Contains(string name)
        {
            return this.InnerMap.Any(a => a.ActionName == name);
        }
    }
}
