// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE1x
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Moq;
#else
using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
#endif

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    public static class ODataModelBuilderMocks
    {
        // Creates a mock of an ODataModelBuilder or any subclass of it that disables model validation
        // in order to reduce verbosity on tests.
        public static T GetModelBuilderMock<T>() where T : ODataModelBuilder
        {
#if NETCORE1x
            ApplicationPartManager applicationPartManager = new ApplicationPartManager();
            AssemblyPart part = new AssemblyPart(typeof(ODataModelBuilderMocks).Assembly);
            applicationPartManager.ApplicationParts.Add(part);

            IContainerBuilder container = new DefaultContainerBuilder();
            container.AddService(ServiceLifetime.Singleton, sp => applicationPartManager);

            IServiceProvider serviceProvider = container.BuildContainer();
            Mock<T> mock = new Mock<T>(serviceProvider);
#else
            Mock<T> mock = new Mock<T>();
#endif

            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            return mock.Object;
        }

        // Creates a mock of an ODataModelBuilder or any subclass of it that disables model validation
        // in order to reduce verbosity on tests.
#if !NETCORE1x
        public static T GetModelBuilderMock<T>(HttpConfiguration configuration) where T : ODataModelBuilder
        {
            Mock<T> mock = new Mock<T>(configuration);
            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            return mock.Object;
        }
#else
        public static T GetModelBuilderMock<T>(IRouteBuilder routeBuilder) where T : ODataModelBuilder
        {
            Mock<T> mock = new Mock<T>(routeBuilder.ApplicationBuilder.ApplicationServices);
            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            return mock.Object;
        }
#endif
    }
}
