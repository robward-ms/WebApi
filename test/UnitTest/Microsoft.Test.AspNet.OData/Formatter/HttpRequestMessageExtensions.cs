// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.Test.AspNet.OData.Formatter
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetFakeODataRouteName(this System.Net.Http.HttpRequestMessage request, string routeName = null)
        {
            request.ODataProperties().RouteName = routeName == null ? HttpRouteCollectionExtensions.RouteName : routeName;
        }
    }
}
