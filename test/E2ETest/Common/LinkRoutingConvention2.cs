// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class LinkRoutingConvention2 : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        internal override string SelectAction(ODataPath odataPath, WebApiControllerContext controllerContext, WebApiActionMap actionMap)
        {
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/navigation/key/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/key/$ref")
            {
                var actionName = string.Empty;
                var requestMethod = controllerContext.Request.Method;
                if ((requestMethod == ODataRequestMethod.Post) || (requestMethod == ODataRequestMethod.Put))
                {
                    actionName += "CreateRefTo";
                }
                else if (requestMethod == ODataRequestMethod.Delete)
                {
                    actionName += "DeleteRefTo";
                }
                else
                {
                    return null;
                }
                var navigationSegment = odataPath.Segments.OfType<NavigationPropertyLinkSegment>().Last();
                actionName += navigationSegment.NavigationProperty.Name;

                var castSegment = odataPath.Segments[2] as TypeSegment;
                
                if (castSegment != null)
                {
                    IEdmType elementType = castSegment.EdmType;
                    if (castSegment.EdmType.TypeKind == EdmTypeKind.Collection)
                    {
                        elementType = ((IEdmCollectionType)castSegment.EdmType).ElementType.Definition;
                    }

                    var actionCastName = string.Format("{0}On{1}", actionName, ((IEdmEntityType)elementType).Name);
                    if (actionMap.Contains(actionCastName))
                    {
                        AddLinkInfoToRouteData(controllerContext, odataPath);
                        return actionCastName;
                    }
                }

                if (actionMap.Contains(actionName))
                {
                    AddLinkInfoToRouteData(controllerContext, odataPath);
                    return actionName;
                }
            }
            return null;
        }

        private static void AddLinkInfoToRouteData(WebApiControllerContext controllerContext, ODataPath odataPath)
        {
            KeySegment keyValueSegment = odataPath.Segments.OfType<KeySegment>().First();
            controllerContext.AddKeyValueToRouteData(keyValueSegment);

            KeySegment relatedKeySegment = odataPath.Segments.Last() as KeySegment;
            if (relatedKeySegment != null)
            {
                controllerContext.AddKeyValueToRouteData(relatedKeySegment, ODataRouteConstants.RelatedKey);
            }
        }
    }
}
