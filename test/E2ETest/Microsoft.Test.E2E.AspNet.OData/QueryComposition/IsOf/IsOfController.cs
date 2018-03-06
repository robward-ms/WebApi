// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public class BillingCustomersController : TestController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(IsofDataSource.EfCustomers);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryCustomers);
            }
        }
    }

    public class BillingsController : TestController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(IsofDataSource.EfBillings);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryBillings);
            }
        }
    }
}
