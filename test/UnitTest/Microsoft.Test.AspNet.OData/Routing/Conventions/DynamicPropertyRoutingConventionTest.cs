// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;
#else
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class DynamicPropertyRoutingConventionTest
    {
        private DynamicPropertyRoutingConvention _routingConvention = new DynamicPropertyRoutingConvention();

        #region Negative Cases
#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(null),
                "routeContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            var request = RequestFactory.Create();
            var routeContext = new RouteContext(request.HttpContext);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(null, controllerContext.Object, emptyMap),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissControllerContext()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(odataPath, null, emptyMap),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissActionMap()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }
#endif

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Customers(10)/Account/Tax");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var emptyActionMap = CreateActionMap();

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Patch")]
        [InlineData("Put")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_IfNotCorrectMethod(string methodName)
        {
            HttpMethod method = new HttpMethod(methodName);
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Orders(7)/DynamicPropertyA");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var actionMap = CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(GetRouteData(request).Values);
        }
        #endregion

        #region Cases for Open Complex Type
        [Theory]
        [InlineData("Customers(7)/Account/Amount")]
        [InlineData("Customers(7)/NS.SpecialCustomer/Account/Amount")]
        public void SelectAction_OnEntitySetPath_OpenComplexType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var actionMap = CreateActionMap("GetDynamicPropertyFromAccount");

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);

            var routeData = GetRouteData(request);
            Assert.Equal(3, routeData.Values.Count);
            Assert.Equal(7, routeData.Values["key"]);
            Assert.Equal("Amount", routeData.Values["dynamicProperty"]);
            Assert.Equal("Amount", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("VipCustomer/Account/Amount")]
        [InlineData("VipCustomer/NS.SpecialCustomer/Account/Amount")]
        public void SelectAction_OnSingletonPath_OpenComplexType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var actionMap = CreateActionMap("GetDynamicPropertyFromAccount");

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);

            var routeData = GetRouteData(request);
            Assert.Equal(2, routeData.Values.Count);
            Assert.Equal("Amount", routeData.Values["dynamicProperty"]);
            Assert.Equal("Amount", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }
        #endregion

        #region Cases for Open Entity Type
        [Theory]
        [InlineData("Orders(7)/DynamicPropertyA")]
        [InlineData("Orders(7)/NS.SpecialOrder/DynamicPropertyA")]
        public void SelectAction_OnEntitySetPath_OpenEntityType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var actionMap = CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);

            var routeData = GetRouteData(request);
            Assert.Equal(3, routeData.Values.Count);
            Assert.Equal(7, routeData.Values["key"]);
            Assert.Equal("DynamicPropertyA", routeData.Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("RootOrder/DynamicPropertyA")]
        [InlineData("RootOrder/NS.SpecialOrder/DynamicPropertyA")]
        public void SelectAction_OnSingltonPath_OpenEntityType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var pathContext = GetPathContext(request, odataPath);
            var controllerContext = GetControllerContext(request);
            var actionMap = CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = _routingConvention.SelectAction(pathContext, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);

            var routeData = GetRouteData(request);
            Assert.Equal(2, routeData.Values.Count);
            Assert.Equal("DynamicPropertyA", routeData.Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }
        #endregion

#if NETCORE
        private RouteContext GetPathContext(HttpRequest request, ODataPath odataPath)
        {
            RouteContext routeContext = new RouteContext(request.HttpContext);
            routeContext.HttpContext.ODataFeature().Path = odataPath;
            return routeContext;
        }

        private SelectControllerResult GetControllerContext(HttpRequest request)
        {
            return new SelectControllerResult("Foo", null); ;
        }

        private IEnumerable<ControllerActionDescriptor> CreateActionMap(string key = null)
        {
            List<ControllerActionDescriptor> actionMap = new List<ControllerActionDescriptor>();
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            actionMap.Add(descriptor);

            if (string.IsNullOrEmpty(key))
            {
                descriptor.ActionName = key;
            }

            return actionMap;
        }

        private RouteData GetRouteData(HttpRequest request)
        {
            return request.HttpContext.GetRouteData();
        }
#else
        private ODataPath GetPathContext(HttpRequestMessage request, ODataPath odataPath)
        {
            return odataPath;
        }

        private HttpControllerContext GetControllerContext(HttpRequestMessage request)
        {
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = request,
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            return controllerContext;
        }

        private ILookup<string, HttpActionDescriptor> CreateActionMap(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            }

            return new HttpActionDescriptor[1].ToLookup(desc => key);
        }

        private IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            return request.GetRouteData();
        }
#endif
    }
}