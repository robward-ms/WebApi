// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class ServerDrivenPaging_ProductsController : InMemoryODataController<Product, int>
    {
        public ServerDrivenPaging_ProductsController()
            : base("ID")
        {
            if (this.LocalTable.Count == 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    this.LocalTable.TryAdd(i, new Product
                    {
                        ID = i,
                        Name = "Test " + i,
                    });
                }
            }
        }

        [EnableQuery(PageSize = 10)]
        public override System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Product>> Get()
        {
            return base.Get();
        }
    }

    public class ServerDrivenPagingTests : NuwaTestBase
    {
        public ServerDrivenPagingTests(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            var product = mb.EntitySet<Product>("ServerDrivenPaging_Products").EntityType;
            product.Ignore(p => p.Family);
            return mb.GetEdmModel();
        }

        // [NuwaFact]
        public void VerifyNextPageLinkAndInlineCountGeneratedCorrect()
        {
            // Arrange & Act
            var result = this.Client.GetStringAsync(this.BaseAddress + "/ServerDrivenPaging_Products?$inlinecount=allPages").Result;

            // Assert
            Assert.Contains("\"@odata.count\":100", result);
            Assert.Contains("\"@odata.nextLink\":", result);
            Assert.Contains("/ServerDrivenPaging_Products?$inlinecount=allPages&$skip=10", result);
        }
    }
}
