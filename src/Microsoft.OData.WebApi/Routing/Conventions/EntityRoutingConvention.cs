// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public class EntityRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
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

            if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast")
            {
                string httpMethodName;

                switch (controllerContext.Request.Method.ToUpperInvariant())
                {
                    case HttpMethodHelper.HttpGet:
                        httpMethodName = "Get";
                        break;
                    case HttpMethodHelper.HttpPut:
                        httpMethodName = "Put";
                        break;
                    case HttpMethodHelper.HttpPatch:
                        httpMethodName = "Patch";
                        break;
                    case HttpMethodHelper.HttpDelete:
                        httpMethodName = "Delete";
                        break;
                    default:
                        return null;
                }

                Contract.Assert(httpMethodName != null);

                IEdmEntityType entityType = (IEdmEntityType)odataPath.EdmType;

                // e.g. Try GetCustomer first, then fallback on Get action name
                string actionName = actionMatch.FindMatchingAction(
                    httpMethodName + entityType.Name,
                    httpMethodName);

                if (actionName != null)
                {
                    KeySegment keySegment = (KeySegment)odataPath.Segments[1];
                    controllerContext.AddKeyValueToRouteData(keySegment);
                    return actionName;
                }
            }
            return null;
        }
    }
}
