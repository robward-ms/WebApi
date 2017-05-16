// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.OData.WebApi.Extensions;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetFakeODataRouteName(this HttpRequestMessage request)
        {
            request.ODataProperties().RouteName = HttpRouteCollectionExtensions.RouteName;
        }
    }
}
