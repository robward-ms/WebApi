// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Extensibility
{
    public class EntityWithPrimitiveAndBinaryProperty
    {
        public int Id { get; set; }
        public long LongProperty { get; set; }
        public byte[] BinaryProperty { get; set; }
        public long? NullableLongProperty { get; set; }
    }

    public class EntityWithPrimitiveAndBinaryPropertyController : ODataController
    {
        private static readonly EntityWithPrimitiveAndBinaryProperty ENTITY;

        static EntityWithPrimitiveAndBinaryPropertyController()
        {
            ENTITY = new EntityWithPrimitiveAndBinaryProperty
            {
                Id = 1,
                LongProperty = long.MaxValue,
                BinaryProperty = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray(),
                NullableLongProperty = null
            };
        }

        public long GetLongProperty(int key)
        {
            return ENTITY.LongProperty;
        }

        public byte[] GetBinaryProperty(int key)
        {
            return ENTITY.BinaryProperty;
        }

        public long? GetNullableLongProperty(int key)
        {
            return ENTITY.NullableLongProperty;
        }
    }

    [NuwaFramework]
    public class SupportDollarValueTest : NuwaTestBase
    {
        public SupportDollarValueTest(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.
                MapODataServiceRoute(
                    routeName: "RawValue",
                    routePrefix: "RawValue",
                    model: GetEdmModel(configuration), pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefault(),
                    defaultHandler: HttpClientFactory.CreatePipeline(innerHandler: new HttpControllerDispatcher(configuration), handlers: new[] { new ODataNullValueMessageHandler() }));
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var parentSet = builder.EntitySet<EntityWithPrimitiveAndBinaryProperty>("EntityWithPrimitiveAndBinaryProperty");
            return builder.GetEdmModel();
        }

        [NuwaFact]
        public void CanExtendTheFormatterToSupportPrimitiveRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/LongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            long result = long.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.Equal(long.MaxValue, result);
        }

        [NuwaFact]
        public void CanExtendTheFormatterToSupportBinaryRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/BinaryProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            byte[] result = response.Content.ReadAsByteArrayAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(new HashSet<byte>(Enumerable.Range(1, 10).Select(x => (byte)x)).SetEquals(result));
        }

        [NuwaFact]
        public void CanExtendTheFormatterToSupportNullRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/NullableLongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
