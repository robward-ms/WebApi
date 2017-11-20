// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest
{
    public class OrderByAttributeTest : NuwaTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string CarBaseUrl = "{0}/enablequery/Cars";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";
        private const string ModelBoundCarBaseUrl = "{0}/modelboundapi/Cars";

        public OrderByAttributeTest(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController),
                    typeof(CarsController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                OrderByAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                OrderByAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [NuwaTheory]
        [InlineData(CustomerBaseUrl + "?$orderby=Id")]
        [InlineData(CustomerBaseUrl + "?$orderby=Id,Name")]
        [InlineData(OrderBaseUrl + "?$expand=UnSortableCustomers($orderby=Id,Name)")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Id")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Id,Name")]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=UnSortableCustomers($orderby=Id,Name)")]
        public void NonSortableByDefault(string url)
        {
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("cannot be used in the $orderby query option.", result);
        }

        [NuwaTheory]
        [InlineData(OrderBaseUrl + "?$orderby=Name", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$orderby=Id", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$orderby=Id,Name", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=Name",
            (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=Price",
            (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=SpecialName",
            (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($orderby=Id,Name)", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($orderby=CarNumber)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CarBaseUrl + "?$orderby=Id,Name", (int)HttpStatusCode.OK)]
        [InlineData(CarBaseUrl + "?$orderby=CarNumber", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$orderby=Name", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$orderby=Id", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$orderby=Id,Name", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=Name",
            (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=Price",
            (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.OrderByAttributeTest.SpecialOrder?$orderby=SpecialName",
            (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($orderby=Id,Name)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($orderby=CarNumber)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCarBaseUrl + "?$orderby=Id,Name", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCarBaseUrl + "?$orderby=CarNumber", (int)HttpStatusCode.BadRequest)]
        public void OrderByOnEntityType(string entitySetUrl, int statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $orderby query option.", result);
            }
        }

        [NuwaTheory]
        [InlineData(OrderBaseUrl + "?$expand=Customers($orderby=Id,Name)", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "(1)/Customers?$orderby=Id,Name", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($orderby=Name)", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$orderby=Name", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($orderby=Id)", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$orderby=Id", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($orderby=Id,Name)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "(1)/Customers?$orderby=Id,Name", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($orderby=Name)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$orderby=Name", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($orderby=Id)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$orderby=Id", (int)HttpStatusCode.OK)]
        public void OrderByOnProperty(string entitySetUrl, int statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $orderby query option.", result);
            }
        }

        [NuwaTheory]
        [InlineData(CustomerBaseUrl + "?$orderby=AutoExpandOrder/Name", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$orderby=AutoExpandOrder/Id", (int)HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$orderby=Address/Name", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$orderby=Address/Street", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($orderby=AutoExpandOrder/Id)", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($orderby=AutoExpandOrder/Name)", (int)HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($orderby=Address/Name)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=AutoExpandOrder/Name", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=AutoExpandOrder/Id", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Address/Name", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Address/Street", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($orderby=AutoExpandOrder/Id)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($orderby=AutoExpandOrder/Name)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($orderby=Address/Name)", (int)HttpStatusCode.OK)]
        public void OrderBySingleNavigationOrComplexProperty(string entitySetUrl, int statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $orderby query option.", result);
            }
        }
    }
}