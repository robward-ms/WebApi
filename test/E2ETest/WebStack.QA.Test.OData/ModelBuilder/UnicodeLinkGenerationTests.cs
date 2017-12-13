// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class UnicodeLinkGeneration_Products : InMemoryODataController<Product, int>
    {
        public UnicodeLinkGeneration_Products()
            : base("ID")
        {
        }
    }

    public class UnicodeLinkGenerationTests : NuwaTestBase
    {
        public UnicodeLinkGenerationTests(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetImplicitEdmModel());
        }

        private static IEdmModel GetImplicitEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("UnicodeLinkGeneration_Products");

            return modelBuilder.GetEdmModel();
        }
    }
}
