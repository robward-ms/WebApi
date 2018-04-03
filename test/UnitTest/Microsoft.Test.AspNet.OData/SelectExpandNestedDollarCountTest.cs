// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class SelectExpandNestedDollarCountTest
    {
        private const string AcceptJson = "application/json";

        private HttpClient _client;

        public SelectExpandNestedDollarCountTest()
        {
            Type[] controllers = new[] { typeof(MsCustomersController), typeof(MetadataController), };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(config);
                config.Count().OrderBy().Filter().Expand().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", GetModel(builder));
            });

            _client = TestServerFactory.CreateClient(server);
        }

        [Fact]
        public async Task SelectExpand_WithOneLevelNestedDollarCount_Works()
        {
            // Arrange
            string uri = "/odata/MsCustomers?$expand=Orders($count=true)";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string payload = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain(",\"@odata.count\":7", payload); // Top (Customers)
            Assert.Contains("\"Orders@odata.count\":5,", payload); // Orders
            Assert.DoesNotContain("\"Categories@odata.count\":9", payload); // Categories
        }

        [Fact]
        public async Task SelectExpand_WithTopLevelDollarCount_AndWithOneLevelNestedDollarCount_Works()
        {
            // Arrange
            string uri = "/odata/MsCustomers?$expand=Orders($count=true)&$count=true";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(",\"@odata.count\":7", payload); // Top (Customers)
            Assert.Contains("\"Orders@odata.count\":5,", payload); // Orders
            Assert.DoesNotContain("\"Categories@odata.count\":9", payload); // Categories
        }

        [Fact]
        public async Task SelectExpand_WithDollarFilter_AndWithOneLevelNestedDollarCount_Works()
        {
            // Arrange
            string uri = "/odata/MsCustomers?$expand=Orders($filter=Id ge 3;$expand=Categories;$count=true)";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string payload = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain(",\"@odata.count\":7", payload); // Top (Customers)
            Assert.Contains("\"Orders@odata.count\":3,", payload); // Orders
            Assert.DoesNotContain("\"Categories@odata.count\":9", payload); // Categories
        }

        [Fact]
        public async Task SelectExpand_WithAllDollarCount_Works()
        {
            // Arrange
            string uri = "/odata/MsCustomers?$expand=Orders($expand=Categories($count=true);$count=true)&$count=true";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(",\"@odata.count\":7", payload); // Top (Customers)
            Assert.Contains("\"Orders@odata.count\":5,", payload); // Orders
            Assert.Contains("\"Categories@odata.count\":9", payload); // Categories
        }

        private Task<HttpResponseMessage> GetResponse(string uri, string acceptHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            return _client.SendAsync(request);
        }

        private IEdmModel GetModel(ODataConventionModelBuilder builder)
        {
            builder.EntitySet<MsCustomer>("MsCustomers");
            builder.EntitySet<MsOrder>("MsOrders");
            builder.EntitySet<MsCategory>("MsCategorys");

            return builder.GetEdmModel();
        }
    }

    public class MsCustomersController : TestODataController
    {
        private static IList<MsCustomer> _customers;

        static MsCustomersController()
        {
            _customers = Enumerable.Range(1, 7).Select(i => new MsCustomer
            {
                Id = 42,
                Name = "Name" + i,
                Orders = Enumerable.Range(1, 5).Select(j => new MsOrder
                {
                    Id = j,
                    Title = "Title" + j,
                    Categories = Enumerable.Range(1, 9).Select(k => new MsCategory
                    {
                        Id = k,
                        Category = k % 2 == 0 ? "Book" : "Video"
                    }).ToList()
                }).ToList(),
            }).ToList();
        }

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(_customers);
        }
    }

    public class MsCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<MsOrder> Orders { get; set; }
    }

    public class MsOrder
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public ICollection<MsCategory> Categories { get; set; }
    }

    public class MsCategory
    {
        public int Id { get; set; }

        public string Category { get; set; }
    }
}
