// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Builder;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.Test.OData.WebApi.AspNet.Builder.TestModels;
using Microsoft.Test.OData.WebApi.AspNet.TestCommon.Models;
using Microsoft.Test.OData.WebApi.AspNet.TestCommon;
using using using Microsoft.Test.OData.WebApi.AspNet.TestCommon.Models;
using Microsoft.Test.OData.WebApi.TestCommon;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter.Serialization
{
    public class EntityTypeTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public void EntityTypeSerializesAsODataEntry()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            ObjectContent<Employee> content = new ObjectContent<Employee>(employee, formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.EmployeeEntry, content.ReadAsStringAsync().Result);
        }

        private ODataMediaTypeFormatter CreateFormatter()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Resource });
            formatter.Request = GetSampleRequest();
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            return formatter;
        }

        private HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/employees");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, _model);
            request.SetConfiguration(configuration);
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet));
            request.EnableODataDependencyInjectionSupport(routeName);
            return request;
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Employee>("employees");
            builder.EntitySet<WorkItem>("workitems");
            return builder.GetEdmModel();
        }
    }
}
