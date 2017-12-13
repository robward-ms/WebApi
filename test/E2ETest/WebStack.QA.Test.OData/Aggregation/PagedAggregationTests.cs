// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Newtonsoft.Json.Linq;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class PagedAggregationTests : NuwaTestBase
    {
        private const string AggregationTestBaseUrl = "{0}/pagedaggregation/Customers";

        public PagedAggregationTests(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof (Paged.CustomersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("pagedaggregation", "pagedaggregation",
                AggregationEdmModel.GetEdmModel(configuration));
        }

        [NuwaTheory]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "/filter(Order/Name ne 'Order0')&$orderby=Order/Name")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "&$filter=Order/Name ne 'Order0'&$orderby=Order/Name")]
        public void PagedAggregationWorks(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            var result = response.Content.ReadAsAsync<JObject>().Result;
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
        }
    }
}
