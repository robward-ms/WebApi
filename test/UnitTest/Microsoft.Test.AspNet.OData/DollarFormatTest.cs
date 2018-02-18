// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;
#else
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData
{
    public class DollarFormatTest
    {
        [Theory]
        [InlineData("FormatCustomers", "application/json;odata.metadata=full")]
        [InlineData("This", "application/json;odata.metadata=full")]
        public async Task DollarFormat_Applies_IfPresent(string path, string mediaTypeFormat)
        {
            // Arrange
            MediaTypeHeaderValue expected = MediaTypeHeaderValue.Parse(mediaTypeFormat);
            string url = string.Format("http://localhost/{0}?$format={1}", path, mediaTypeFormat);

            Type[] controllers = new[] { typeof(FormatCustomersController), typeof(ThisController), };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(config);
                builder.EntitySet<Customer>("FormatCustomers");
                builder.Singleton<Customer>("This"); // Singleton

                config.MapODataServiceRoute("odata", routePrefix: null, model: builder.GetEdmModel());
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expected, response.Content.Headers.ContentType);
        }

        public class Customer
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class FormatCustomersController : ODataController
        {
            public Customer[] Get()
            {
                return new Customer[] { };
            }
        }

        public class ThisController : ODataController
        {
            public Customer Get()
            {
                return new Customer();
            }
        }
    }
}
