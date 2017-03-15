// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract class NavigationSourceRoutingConvention : IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        public virtual SelectControllerResult SelectController(ODataPath odataPath, IWebApiRequestMessage request)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // entity set
            EntitySetSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetSegment;
            if (entitySetSegment != null)
            {
                return new SelectControllerResult(entitySetSegment.EntitySet.Name);
            }

            // singleton
            SingletonSegment singletonSegment = odataPath.Segments.FirstOrDefault() as SingletonSegment;
            if (singletonSegment != null)
            {
                return new SelectControllerResult(singletonSegment.Singleton.Name);
            }

            return null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMatch">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        public abstract string SelectAction(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMatch actionMatch);
    }
}
