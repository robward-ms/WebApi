// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties
{
    [NuwaFramework]
    public class DynamicPropertiesTest : NuwaTestBase
    {
        public DynamicPropertiesTest(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { 
                typeof(DynamicCustomersController),
                typeof(DynamicSingleCustomerController),
                typeof(MetadataController),
            };

            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            configuration.GetHttpServer();
            configuration.MapODataServiceRoute(routeName: "odata", routePrefix: "odata",
                model: GetEdmModel());

            configuration.EnsureInitialized();
        }

        [NuwaTheory]
        [InlineData("DynamicCustomers(1)/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_1")]
        [InlineData("DynamicCustomers(2)/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_2")]
        [InlineData("DynamicCustomers(3)/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder_3")]
        [InlineData("DynamicCustomers(4)/Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties.DynamicVipCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_4")]
        [InlineData("DynamicCustomers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties.DynamicVipCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_5")]
        [InlineData("DynamicSingleCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty")]
        [InlineData("DynamicSingleCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount")]
        [InlineData("DynamicSingleCustomer/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder")]
        [InlineData("DynamicCustomers(1)/Id", "Id_1")]
        public void AccessPropertyTest(string uri, string expected)
        {
            string requestUri = string.Format("{0}/odata/{1}", BaseAddress, uri);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, response.Content.ReadAsStringAsync().Result);
        }

        [NuwaTheory]
        [InlineData("Put")]
        [InlineData("Patch")]
        [InlineData("Post")]
        [InlineData("Delete")]
        public void AccessDynamicPropertyWithWrongMethodTest(string method)
        {
            string requestUri = string.Format("{0}/odata/DynamicCustomers(1)/DynamicPropertyName", BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);

            var response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [NuwaTheory]
        [InlineData("DynamicCustomers(2)/SecondAccount/DynamicPropertyName")]
        [InlineData("DynamicSingleCustomer/SecondAccount/DynamicPropertyName")]
        public void AccessDynamicPropertyWithoutImplementMethod(string uri)
        {
            string requestUri = string.Format("{0}/odata/{1}", BaseAddress, uri);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DynamicCustomer>("DynamicCustomers");
            builder.Singleton<DynamicSingleCustomer>("DynamicSingleCustomer");
            return builder.GetEdmModel();
        }
    }
}
