// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Microsoft.Test.AspNet.OData.TestCommon.Models;
using Xunit;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Microsoft.Test.AspNet.OData.TestCommon.Models;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        [Fact]
        public void ComplexTypeSerializesAsOData()
        {
            // Arrange
            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = FormatterTestHelper.GetFormatter(payload, GetSampleModel());
            var content = FormatterTestHelper.GetContent(new Person(0, new ReferenceDepthContext(7)), formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.PersonComplexType, content.ReadAsStringAsync().Result);
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Person>();

            // Employee is derived from Person. Employee has a property named manager it's Employee type.
            // It's not allowed to build inheritance complex type because a recursive loop of complex types is not allowed.
            builder.Ignore<Employee>();
            return builder.GetEdmModel();
        }
    }
}
