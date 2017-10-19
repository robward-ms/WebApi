// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.Test.AspNet.OData
{
    internal static class ODataRouteBuilderExtensions
    {
        public static void MapODataServiceRoute(this IRouteBuilder routeBuilder, IEdmModel model)
        {
            routeBuilder.MapODataServiceRoute("IgnoredRouteName", null, model);
        }
    }
}
