// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class CompoundQueryTests : WebHostTestBase
    {
        public CompoundQueryTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CompoundQueryCustomerController));
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("compoundquery", "compoundquery", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<CompoundQueryCustomer>("CompoundQueryCustomers");
            builder.EntitySet<CompoundQueryTransformCustomer>("CompoundQueryTransformCustomers");
            builder.Function("Transform").ReturnsCollectionFromEntitySet<CompoundQueryTransformCustomer>("CompoundQueryTransformCustomers");
            builder.Action("ResetDataSource");
            return builder.GetEdmModel();
        }

        private async Task RestoreData()
        {
            string requestUri = BaseAddress + string.Format("/compoundquery/ResetDataSource");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("/compoundquery/Transform", 10)]
        [InlineData("/compoundquery/Transform?$skip=1&$top=1", 1)]
        [InlineData("/compoundquery/Transform?$filter=Id eq 1", 1)]
        [InlineData("/compoundquery/Transform?$filter=Id gt 2", 8)]
        [InlineData("/compoundquery/Transform?$filter=Id gt 2&$top=2", 2)]
        public async Task QueryForEntitySetCanBeTransformed(string uri, int expectedCount)
        {
            // Arrange
            await RestoreData();
            string queryUrl = string.Format("{0}{1}", BaseAddress, uri);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(expectedCount, "value", result);
        }
    }

    public class CompoundQueryCustomerController : TestODataController
    {
        private readonly CompoundQueryCustomerContext _db = new CompoundQueryCustomerContext();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_db.Customers);
        }


        [HttpGet]
        [ODataRoute("Transform")]
        public ITestActionResult AsEFTransformCustomer(ODataQueryOptions<CompoundQueryCustomer> queryOptions)
        {
            var query = queryOptions.ApplyTo(_db.Customers) as IQueryable<CompoundQueryCustomer>;
            var transformQuery = query.Select(e => new CompoundQueryTransformCustomer
            {
                Id = e.Id,
                Name = "Customer " + e.Id.ToString(),
            });

            return Ok(transformQuery);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate(_db);
            }

            return Ok();
        }

        public static void Generate(CompoundQueryCustomerContext db)
        {
            var customers = Enumerable.Range(1, 10)
                .Select(i =>
                {
                    return new CompoundQueryCustomer
                    {
                        Id = i,
                        Name = "Customer " + i.ToString(),
                    };
                });

            db.Customers.AddRange(customers);
            db.SaveChanges();
        }
    }

    public class CompoundQueryCustomerContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=CompoundQueryCustomerTest";

        public CompoundQueryCustomerContext()
            : base(ConnectionString)
        {
        }

        public DbSet<CompoundQueryCustomer> Customers { get; set; }
    }

    public class CompoundQueryCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CompoundQueryTransformCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
