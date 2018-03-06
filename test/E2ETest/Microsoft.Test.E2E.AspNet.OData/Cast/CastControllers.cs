// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#else
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class ProductsController : TestController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(DataSource.EfProducts);
            }
            else
            {
                return Ok(DataSource.InMemoryProducts);
            }
        }

        [EnableQuery]
        public ITestActionResult GetDimensionInCentimeter(int key)
        {
            if (GetRoutePrefix() == "EF")
            {
                Product product = DataSource.EfProducts.Single(p => p.ID == key);
                return Ok(product.DimensionInCentimeter);
            }
            else
            {
                Product product = DataSource.InMemoryProducts.Single(p => p.ID == key);
                return Ok(product.DimensionInCentimeter);
            }
        }
    }

}
