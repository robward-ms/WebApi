// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class ODataRouteTests : WebHostTestBase
    {
        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            HttpServer server = configuration.GetHttpServer();
            configuration.MapODataServiceRoute("noPrefix", "", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("prefix", "prefix", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("oneParameterInPrefix", "{a}", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            return builder.GetEdmModel();
        }

        internal static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Theory]
        [InlineData("")]
        [InlineData("prefix/")]
        [InlineData("parameter/")]
        public async Task UrlsGeneratedByFastPathAreConsistentWithUrlsGeneratedWithSlowPath(string requestPath)
        {
            Uri serviceUrl = new Uri(BaseAddress + "/" + requestPath);
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Contains("odata.context\":\"" + serviceUrl + "$metadata\"", content);
        }
    }
}
