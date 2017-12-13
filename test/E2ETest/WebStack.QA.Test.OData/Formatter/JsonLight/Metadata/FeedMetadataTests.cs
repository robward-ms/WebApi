// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Microsoft.Test.E2E.AspNet.OData.Common.Xunit;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    public class FeedMetadataTests : NuwaTestBase
    {
        public FeedMetadataTests(NuwaClassFixture fixture)
            : base(fixture)
        {
        }

        [NuwaConfiguration]
        internal static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var entitySet = builder.EntitySet<StubEntity>("StubEntity");
            entitySet.EntityType.Collection.Action("Paged").ReturnsCollectionFromEntitySet<StubEntity>("StubEntity");
            return builder.GetEdmModel();
        }

        [NuwaTheory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        public void ODataCountAndNextLinkAnnotationsAppearsOnAllMetadataLevelsWhenSpecified(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, BaseAddress.ToLowerInvariant() + "/StubEntity/Default.Paged");
            message.SetAcceptHeader(acceptHeader);
            string expectedNextLink = new Uri("http://differentServer:5000/StubEntity/Default.Paged?$skip=" + entities.Length).ToString();

            //Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            JsonAssert.PropertyEquals(entities.Length, "@odata.count", result);
            JsonAssert.PropertyEquals(expectedNextLink, "@odata.nextLink", result);
        }

        [NuwaTheory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        public void MetadataAnnotationAppearsOnlyForFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, BaseAddress.ToLowerInvariant() + "/StubEntity/");
            message.SetAcceptHeader(acceptHeader);
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#StubEntity";

            //Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty("@odata.context", result);
            }
            else
            {
                JsonAssert.PropertyEquals(expectedMetadataUrl, "@odata.context", result);
            }
        }

        [NuwaTheory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        public void CanReturnTheWholeResultSetUsingNextLink(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            string nextUrlToQuery = BaseAddress.ToLowerInvariant() + "/StubEntity/";
            JToken token = null;
            JArray returnedEntities = new JArray();
            JObject result = null;

            //Act
            do
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, nextUrlToQuery);
                message.SetAcceptHeader(acceptHeader);
                HttpResponseMessage response = Client.SendAsync(message).Result;
                result = response.Content.ReadAsAsync<JObject>().Result;
                JArray currentResults = (JArray)result["value"];
                for (int i = 0; i < currentResults.Count; i++)
                {
                    returnedEntities.Add(currentResults[i]);
                }
                if (result.TryGetValue("@odata.nextLink", out token))
                {
                    nextUrlToQuery = (string)token;
                }
                else
                {
                    nextUrlToQuery = null;
                }
            }
            while (nextUrlToQuery != null);

            //Assert
            Assert.Equal(entities.Length, returnedEntities.Count);
        }
    }
}
