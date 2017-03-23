// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using Microsoft.TestCommon;
using Microsoft.OData.WebApi.Routing.Conventions;
using System.Web.OData.Adapters;

namespace System.Web.OData.Routing.Conventions
{
    public class ODataRoutingConventionsTest
    {
        [Fact]
        public void CreateDefaultWithAttributeRouting_ContainsAttributeRoutingConvention()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", new AttributeMappingProvider("odata", config));

            // Assert
            Assert.Single(conventions.OfType<AttributeRoutingConvention>());
        }
    }
}
