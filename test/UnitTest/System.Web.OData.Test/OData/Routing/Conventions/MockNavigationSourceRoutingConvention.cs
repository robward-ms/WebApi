// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing;
using Microsoft.OData.WebApi.Routing.Conventions;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.OData.Routing.Conventions
{
    class MockNavigationSourceRoutingConvention : NavigationSourceRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMatch actionMatch)
        {
            return null;
        }
    }
}
