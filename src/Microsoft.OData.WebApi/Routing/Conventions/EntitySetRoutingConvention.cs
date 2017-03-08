// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public class EntitySetRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMatch">The action match.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        public override string SelectAction(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMatch actionMatch)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionMatch == null)
            {
                throw Error.ArgumentNull("actionMatch");
            }

            if (odataPath.PathTemplate == "~/entityset")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                if (HttpMethodHelper.IsGet(controllerContext.Request.Method))
                {
                    // e.g. Try GetCustomers first, then fall back to Get action name
                    return actionMatch.FindMatchingAction(
                        "Get" + entitySet.Name,
                        "Get");
                }
                else if (HttpMethodHelper.IsPost(controllerContext.Request.Method))
                {
                    // e.g. Try PostCustomer first, then fall back to Post action name
                    return actionMatch.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name,
                        "Post");
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/$count" &&
                HttpMethodHelper.IsGet(controllerContext.Request.Method))
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                // e.g. Try GetCustomers first, then fall back to Get action name
                return actionMatch.FindMatchingAction(
                    "Get" + entitySet.Name,
                    "Get");
            }
            else if (odataPath.PathTemplate == "~/entityset/cast")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                if (HttpMethodHelper.IsGet(controllerContext.Request.Method))
                {
                    // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                    return actionMatch.FindMatchingAction(
                        "Get" + entitySet.Name + "From" + entityType.Name,
                        "GetFrom" + entityType.Name);
                }
                else if (HttpMethodHelper.IsPost(controllerContext.Request.Method))
                {
                    // e.g. Try PostCustomerFromSpecialCustomer first, then fall back to PostFromSpecialCustomer
                    return actionMatch.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name + "From" + entityType.Name,
                        "PostFrom" + entityType.Name);
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/cast/$count" &&
                HttpMethodHelper.IsGet(controllerContext.Request.Method))
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.Segments[1].EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                return actionMatch.FindMatchingAction(
                    "Get" + entitySet.Name + "From" + entityType.Name,
                    "GetFrom" + entityType.Name);
            }

            return null;
        }
    }
}
