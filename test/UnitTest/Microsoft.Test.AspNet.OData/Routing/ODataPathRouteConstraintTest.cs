// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Routing
{
    public class ODataPathRouteConstraintTest
    {
        IEdmModel _model = ODataConventionModelBuilderFactory.Create().GetEdmModel();
        string _routeName = "name";
        IEnumerable<IODataRoutingConvention> _conventions = ODataRoutingConventions.CreateDefault();
        HttpRequestMessage _request = new HttpRequestMessage();
        IServiceProvider _rootContainer;
        IODataPathHandler _pathHandler;

#if NETCORE
        AspNetCore.Routing.RouteDirection UriGeneration = AspNetCore.Routing.RouteDirection.UrlGeneration;
#else
        System.Web.Http.Routing.HttpRouteDirection UriGeneration = System.Web.Http.Routing.HttpRouteDirection.UriGeneration;
#endif

        private static IList<string> _stringsWithUnescapedSlashes = new List<string>
        {
            { "virtualRoot/odata" },
            { "virtualRoot/prefix/odata" }
        };

        private static IList<string> _stringsLegalEverywhere = new List<string>
        {
            { "some%23hashes" },                                // "some#hashes"
            { "some%2fslashes" },                               // "some/slashes"
            { "some%3Fquestion%3Fmarks" },                      // "some?question?marks"
            { "some%3flower%23escapes" },                       // "some?lower#escapes"
            { "" },
            { "odata" },
            { "some%20spaces" },                                // "some spaces"
            { "_some+plus+signs_" },
            { "some(sub)and&other=delims" },
            { "some(delims)but%2Bupper:escaped" },              // "some(delims)but+upper:escaped"
            { "some(delims)but%2blower:escaped" },              // "some(delims)but+lower:escaped"
            { ":[]@" },                                         // general delimeters that work
            { "Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars" },      // "Chinese西雅图Chars"
            { "Unicode%D8%83Format%D8%83Char" },                // "Unicode؃Format؃Char", class Cf
            { "Unicode%E1%BF%BCTitlecase%E1%BF%BCChar" },       // "UnicodeῼTitlecaseῼChar", class Lt
            { "Unicode%E0%A4%83Combining%E0%A4%83Char" },       // "UnicodeःCombiningःChar", class Mc
        };

        public ODataPathRouteConstraintTest()
        {
            _rootContainer = new MockContainer(_model, _conventions);
            _pathHandler = _rootContainer.GetRequiredService<IODataPathHandler>();
        }

        public static TheoryDataSet<string> PrefixStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string>();
                foreach (var item in _stringsWithUnescapedSlashes)
                {
                    dataSet.Add(item);
                }

                foreach (var item in _stringsLegalEverywhere)
                {
                    dataSet.Add(item);
                }

                return dataSet;
            }
        }

        // Cross product of prefixes (all of _stringsWithUnescapedSlashes and all _stringsLegalEverywhere) with OData paths (all
        // of _stringsLegalEverywhere).
        public static TheoryDataSet<string, string> PrefixAndODataStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string, string>();
                foreach (var prefix in _stringsWithUnescapedSlashes)
                {
                    foreach (var oDataPath in _stringsLegalEverywhere)
                    {
                        dataSet.Add(prefix, oDataPath);
                    }
                }

                foreach (var prefix in _stringsLegalEverywhere)
                {
                    foreach (var oDataPath in _stringsLegalEverywhere)
                    {
                        dataSet.Add(prefix, oDataPath);
                    }
                }

                return dataSet;
            }
        }

        [Fact]
        public void Match_ReturnsTrue_ForUriGeneration()
        {
            var values = new Dictionary<string, object>();

            var constraint = CreatePathRouteConstraint();
            Assert.True(constraint.Match(_request, null, null, values, UriGeneration));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathRouteValueMissing()
        {
            var values = new Dictionary<string, object>();

            var constraint = CreatePathRouteConstraint();
            Assert.False(constraint.Match(_request, null, null, values, UriResolution));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathCannotBeParsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/NotAnODataPath");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            var configuration = new HttpConfiguration(httpRouteCollection);
            configuration.EnableODataDependencyInjectionSupport(_routeName);
            request.SetConfiguration(configuration);

            var values = new Dictionary<string, object>() { { "odataPath", "NotAnODataPath" } };
            var constraint = CreatePathRouteConstraint();

            // Act & Assert
            Assert.False(constraint.Match(request, null, null, values, UriResolution));
        }

        [Fact]
        public void Match_ReturnsTrue_IfODataPathCanBeParsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/odata/$metadata");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            var configuration = new HttpConfiguration(httpRouteCollection);
            PerRouteContainer perRouteContainer = configuration.GetPerRouteContainer() as PerRouteContainer;
            perRouteContainer.SetODataRootContainer(_routeName, _rootContainer);
            request.SetConfiguration(configuration);

            var values = new Dictionary<string, object>() { { "odataPath", "$metadata" } };
            var constraint = CreatePathRouteConstraint();

            // Act & Assert
            Assert.True(constraint.Match(request, null, null, values, UriResolution));

            Assert.Equal("Metadata", values["controller"]);
            Assert.Same(_model, request.GetModel());
            Assert.Same(_routeName, request.ODataProperties().RouteName);
            Assert.Equal(_conventions, request.GetRoutingConventions());
            Assert.Same(_pathHandler, request.GetPathHandler());
        }

        [Theory]
        [MemberData(nameof(PrefixStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForMetadata(string prefixString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var pathHandler = new TestPathHandler();
            var request = new HttpRequestMessage(HttpMethod.Get, expectedRoot + "$metadata");
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            var configuration = new HttpConfiguration(httpRouteCollection);
            configuration.EnableODataDependencyInjectionSupport(_routeName, pathHandler);
            request.SetConfiguration(configuration);

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, "$metadata" },
            };

            // Act
            var matched = constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal("$metadata", pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForMetadataWithEscapedSeparator(string prefixString)
        {
            // Arrange
            var originalRoot = "http://any/" + prefixString;
            var expectedRoot = originalRoot;
            if (!String.IsNullOrEmpty(prefixString))
            {
                originalRoot += "%2F";  // Escaped '/'
            }

            var pathHandler = new TestPathHandler();
            var request = new HttpRequestMessage(HttpMethod.Get, originalRoot + "$metadata");
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            var configuration = new HttpConfiguration(httpRouteCollection);
            configuration.EnableODataDependencyInjectionSupport(_routeName, pathHandler);
            request.SetConfiguration(configuration);

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, "$metadata" },
            };

            // Act
            var matched = constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal("$metadata", pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixAndODataStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForFunctionCall(string prefixString, string oDataString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var request = new HttpRequestMessage(HttpMethod.Get, expectedRoot + oDataPath);
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            var configuration = new HttpConfiguration(httpRouteCollection);
            configuration.EnableODataDependencyInjectionSupport(_routeName, model, pathHandler);
            request.SetConfiguration(configuration);

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
            };

            // Act
            var matched = constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal(oDataPath, pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixAndODataStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForFunctionCallWithEscapedSeparator(
            string prefixString,
            string oDataString)
        {
            // Arrange
            var originalRoot = "http://any/" + prefixString;
            var expectedRoot = originalRoot;
            if (!String.IsNullOrEmpty(prefixString))
            {
                originalRoot += "%2F";  // Escaped '/'
            }

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var request = new HttpRequestMessage(HttpMethod.Get, originalRoot + oDataPath);
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            var configuration = new HttpConfiguration(httpRouteCollection);
            configuration.EnableODataDependencyInjectionSupport(_routeName, model, pathHandler);
            request.SetConfiguration(configuration);

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
            };

            // Act
            var matched = ConstraintMatch(constraint.Match, request, values, RouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal(oDataPath, pathHandler.ODataPath);
        }

        private ODataPathRouteConstraint CreatePathRouteConstraint()
        {
            return new ODataPathRouteConstraint(_routeName);
        }

        // Wrap a PathHandler to allow us to check serviceRoot the constraint calculates.
        private class TestPathHandler : DefaultODataPathHandler
        {
            public string ServiceRoot { get; private set; }
            public string ODataPath { get; private set; }

            public override ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer)
            {
                ServiceRoot = serviceRoot;
                ODataPath = odataPath;
                return base.Parse(serviceRoot, odataPath, requestContainer);
            }
        }

        /// <summary>
        /// Test class for abstracting the version request.
        /// </summary>
        private class TestVersionRequest
        {
            public TestVersionRequest(HttpMethod method, string uri)
            {
                this.Method = method;
                this.Uri = uri;
                this.Headers = new Dictionary<string, string>();
            }

            public HttpMethod Method { get; private set; }
            public string Uri { get; private set; }
            public Dictionary<string, string> Headers { get; private set; }
        }

        /// <summary>
        /// Abstraction for route direction.
        /// </summary>
        private enum RouteDirection
        {
            UriResolution = 0,
            UriGeneration
        }

        /// <summary>
        /// Test method to call constraint.Match using the proper arguments for each platform.
        /// </summary>
        /// <param name="constraint">The constraint object.</param>
        /// <param name="versionRequest">The abstracted request.</param>
        /// <param name="direction">The abstracted route direction.</param>
        /// <returns>Result from constraint.Match,</returns>
        private bool ConstraintMatch(ODataPathRouteConstraint constraint, TestVersionRequest versionRequest, Dictionary<string, object> values, RouteDirection direction)
        {
#if NETCORE
            AspNetCore.Http.HttpContext context = new AspNetCore.Http.DefaultHttpContext();
            AspNetCore.Http.HttpRequest request = context.Request;
            foreach (KeyValuePair<string, string> kvp in versionRequest.Headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }

            System.Uri requestUri = new System.Uri(versionRequest.Uri);
            request.Method = versionRequest.Method.ToString();
            request.Host = new AspNetCore.Http.HostString(requestUri.Host, requestUri.Port);
            request.Scheme = requestUri.Scheme;

            AspNetCore.Routing.RouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? AspNetCore.Routing.RouteDirection.IncomingRequest
                : AspNetCore.Routing.RouteDirection.UrlGeneration;

            return constraint.Match(context, null, null, null, routeDirection);
#else
            HttpRequestMessage request = new HttpRequestMessage(versionRequest.Method, versionRequest.Uri);
            foreach (KeyValuePair<string,string> kvp in versionRequest.Headers)
            {
                request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            System.Web.Http.Routing.HttpRouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? System.Web.Http.Routing.HttpRouteDirection.UriResolution
                : System.Web.Http.Routing.HttpRouteDirection.UriGeneration;

            return constraint.Match(request, null, null, values, routeDirection);
#endif
        }
    }
}
