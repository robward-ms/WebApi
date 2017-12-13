// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    public class MinimalMetadataSpecificTests : NuwaTestBase
    {
        public MinimalMetadataSpecificTests(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        //[NuwaWebConfig]
        //internal static void UpdateWebConfig(WebConfigHelper webConfig)
        //{
        //    webConfig.AddRAMFAR(true);
        //}

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var pets = builder.EntitySet<Pet>("Pets");
            builder.EntityType<BigPet>();
            return builder.GetEdmModel();
        }

        [NuwaFact]
        public void QueryWithCastDoesntContainODataType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)/Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet");

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            string payload = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"@odata.context\":", payload);
            Assert.Contains("\"Id\":5", payload);
            Assert.DoesNotContain("@odata.type", payload);
            Assert.DoesNotContain("#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet", payload);
        }

        [NuwaFact]
        public void QueryWithoutCastContainsODataType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)");

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            string payload = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"@odata.context\":", payload);
            Assert.Contains("\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet\"", payload);
            Assert.Contains("\"Id\":5", payload);
        }
    }

    public class Pet
    {
        public int Id { get; set; }
    }

    public class BigPet : Pet
    {
    }

    public class PetsController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 10).Select(i =>
            {
                if (i % 2 == 0)
                    return new Pet { Id = i };
                else
                    return new BigPet { Id = i };
            }));
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok(new BigPet { Id = key });
        }
    }
}
