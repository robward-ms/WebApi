// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using 
using 
using System.Net.Http;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Builder;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.Test.OData.WebApi.AspNet.TestCommon.Models;
using Microsoft.Test.OData.WebApi.AspNet.TestCommon;
using Microsoft.Test.OData.WebApi.AspNet.TestCommon.Models;
using Microsoft.Test.OData.WebApi.TestCommon;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        private readonly ODataMediaTypeFormatter _formatter;

        public ComplexTypeTest()
        {
            _formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Resource }) { Request = GetSampleRequest() };
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
        }

        [Fact]
        public void ComplexTypeSerializesAsOData()
        {
            // Arrange
            ObjectContent<Person> content = new ObjectContent<Person>(new Person(0, new ReferenceDepthContext(7)),
                _formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.PersonComplexType, content.ReadAsStringAsync().Result);
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            request.EnableODataDependencyInjectionSupport(GetSampleModel());
            request.GetConfiguration().Routes.MapFakeODataRoute();
            return request;
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Person>();

            // Employee is derived from Person. Employee has a property named manager it's Employee type.
            // It's not allowed to build inheritance complex type because a recursive loop of complex types is not allowed.
            builder.Ignore<Employee>();
            return builder.GetEdmModel();
        }
    }
}
