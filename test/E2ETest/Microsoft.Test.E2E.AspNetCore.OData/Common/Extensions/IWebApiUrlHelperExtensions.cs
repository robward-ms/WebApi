// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
    /// <summary>
    /// Extensions for IWebApiUrlHelper.
    /// </summary>
    internal static class IWebApiUrlHelperExtensions
    {
        public static string Link(this IWebApiUrlHelper urlHelper, string routeName, object values)
        {
            WebApiUrlHelper internalUrlHelper = urlHelper as WebApiUrlHelper;
            return internalUrlHelper.innerHelper.Link(routeName, values);
        }
    }
}
