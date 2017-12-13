// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class NestedClass_Parent
    {
        public class Nest
        {
            public string Name { get; set; }
            public NestPropertyType NestProperty { get; set; }
        }

        public class NestPropertyType
        {
            public string Name { get; set; }
        }
    }

    public class NestedClassController : ApiController
    {
        [HttpGet]
        public IQueryable<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest> QueryOnNestClass()
        {
            return new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]
            {
                new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest()
                {
                    Name = "aaa",
                    NestProperty = new NestedClass_Parent.NestPropertyType()
                    {
                        Name = "aaa"
                    }
                },
                new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest()
                {
                    Name = "bbb",
                    NestProperty = new NestedClass_Parent.NestPropertyType()
                    {
                        Name = "bbb"
                    }
                }
            }.AsQueryable();
        }
    }

    public class NestedClassTests : WebHostTestBase
    {
        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public void QueryOnNestClassShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=Name eq 'aaa'").Result;
            response.EnsureSuccessStatusCode();

            var actual = response.Content.ReadAsAsync<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]>().Result;

            Assert.Equal("aaa", actual.Single().Name);
        }

        [Fact]
        public void QueryOnPropertyWithNestedTypeShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=NestProperty/Name eq 'aaa'").Result;
            response.EnsureSuccessStatusCode();

            var actual = response.Content.ReadAsAsync<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]>().Result;

            Assert.Equal("aaa", actual.Single().NestProperty.Name);
        }
    }
}
