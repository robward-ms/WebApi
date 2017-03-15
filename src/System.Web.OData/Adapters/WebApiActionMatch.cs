// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.OData.WebApi.Interfaces;

namespace System.Web.OData.Adapters
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
        public WebApiActionMatch(ILookup<string, HttpActionDescriptor> actionMap)
        {
            this.InnerMap = actionMap;
        }

        /// <summary>
        /// The inner map wrapped by this instance.
        /// </summary>
        public ILookup<string, HttpActionDescriptor> InnerMap { get; private set; }

        /// <summary>
        /// Determines whether a specified key exists.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>True if the key exist; false otherwise.</returns>
        public bool Contains(string name)
        {
            return this.InnerMap.Contains(name);
        }
    }
}
