// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Extensions
{
    public class ReflectedPropertyRoutingConvention : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        internal override string SelectAction(ODataPath odataPath, WebApiControllerContext controllerContext, WebApiActionMap actionMap)
        {
            if (odataPath.PathTemplate == "~/entityset/key/property" || odataPath.PathTemplate == "~/entityset/key/cast/property")
            {
                var segment = odataPath.Segments.Last() as PropertySegment;
                var property = segment.Property;
                var declareType = property.DeclaringType as IEdmEntityType;
                if (declareType != null)
                {
                    var key = odataPath.Segments[1] as KeySegment;
                    controllerContext.AddKeyValueToRouteData(key);
                    controllerContext.RouteData.Add("property", property.Name);
                    string prefix = ODataHelper.GetHttpPrefix(controllerContext.Request.Method.ToString().ToUpperInvariant());
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    string action = prefix + "Property" + "From" + declareType.Name;
                    return actionMap.Contains(action) ? action : prefix + "Property";
                }
            }

            return null;
        }
    }
}
