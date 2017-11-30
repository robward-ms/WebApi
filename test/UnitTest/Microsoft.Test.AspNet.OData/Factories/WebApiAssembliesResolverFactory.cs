// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
#else
using System.Reflection;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Moq;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create WebApiAssembliesResolver.
    /// </summary>
    public class WebApiAssembliesResolverFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE
        internal static IWebApiAssembliesResolver CreateFake()
        {
            IRouteBuilder builder = RoutingConfigurationFactory.Create();

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Clear();

            return new WebApiAssembliesResolver(applicationPartManager);
        }
#else
        internal static IWebApiAssembliesResolver CreateFake()
        {
            Mock<IAssembliesResolver> mockAssembliesResolver = new Mock<IAssembliesResolver>();
            mockAssembliesResolver
                .Setup(r => r.GetAssemblies())
                .Returns(new Assembly[0]);

            return new WebApiAssembliesResolver(mockAssembliesResolver.Object);
        }
#endif
    }
}
