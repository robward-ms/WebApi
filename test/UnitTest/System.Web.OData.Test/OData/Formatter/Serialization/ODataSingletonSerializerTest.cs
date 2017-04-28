// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Adapters;
using System.Web.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Serialization;
using Microsoft.TestCommon;
using ODataConventionModelBuilder = Microsoft.OData.WebApi.Builder.ODataConventionModelBuilder;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataSingletonSerializerTest
    {
        [Fact]
        public void CanSerializerSingleton()
        {
            // Arrange
            const string expect = "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#Boss\"," +
                "\"EmployeeId\":987,\"EmployeeName\":\"John Mountain\"}";

            IEdmModel model = GetEdmModel();
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Boss");
            HttpRequestMessage request = GetRequest(model, singleton);
            ODataSerializerContext readContext = new ODataSerializerContext()
            {
                Url = new WebApiUrlHelper(new UrlHelper(request)),
                Path = request.ODataProperties().Path,
                Model = model,
                NavigationSource = singleton
            };

            IODataSerializerProvider serializerProvider = DependencyInjectionHelper.GetDefaultODataSerializerProvider();
            EmployeeModel boss = new EmployeeModel {EmployeeId = 987, EmployeeName = "John Mountain"};
            MemoryStream bufferedStream = new MemoryStream();

            // Act
            ODataResourceSerializer serializer = new ODataResourceSerializer(serializerProvider);
            serializer.WriteObject(boss, typeof(EmployeeModel), GetODataMessageWriter(model, bufferedStream), readContext);

            // Assert
            string result = Encoding.UTF8.GetString(bufferedStream.ToArray());
            Assert.Equal(expect, result);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<EmployeeModel>("Boss");
            return builder.GetEdmModel();
        }

        private HttpRequestMessage GetRequest(IEdmModel model, IEdmSingleton singleton)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapODataServiceRoute("odata", "odata", model);
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);
            request.ODataProperties().RouteName = "odata";
            request.ODataProperties().Path = new ODataPath(new[] { new SingletonSegment(singleton) });
            request.RequestUri = new Uri("http://localhost/odata/Boss");
            return request;
        }

        private ODataMessageWriter GetODataMessageWriter(IEdmModel model, MemoryStream bufferedStream)
        {
            HttpContent content = new StringContent("");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
            {
                BaseUri = new Uri("http://localhost/odata"),
                Version = ODataVersion.V4,
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://localhost/odata") }
            };

            IODataResponseMessage responseMessage = new ODataMessageWrapper(bufferedStream, content.Headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(";", kvp.Value)));
            return new ODataMessageWriter(responseMessage, writerSettings, model);
        }

        private sealed class EmployeeModel
        {
            [Key]
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }
    }
}
