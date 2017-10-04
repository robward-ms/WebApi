// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Sets up default options for <see cref="ODataOptions"/>.
    /// </summary>
    public class ODataOptionsSetup : IConfigureOptions<ODataOptions>
    {
        private IServiceProvider services;

        public ODataOptionsSetup(IServiceProvider services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            this.services = services;
        }

        public void Configure(ODataOptions options)
        {
            //TODO: routeName?
            string routeName = string.Empty;

            // Set up the default routing conventions
            options.RoutingConventions.Add(new AttributeRoutingConvention(routeName, this.services));
            options.RoutingConventions.Add(new MetadataRoutingConvention());
            options.RoutingConventions.Add(new EntitySetRoutingConvention());
            options.RoutingConventions.Add(new SingletonRoutingConvention());
            options.RoutingConventions.Add(new EntityRoutingConvention());
            options.RoutingConventions.Add(new NavigationRoutingConvention());
            options.RoutingConventions.Add(new PropertyRoutingConvention());
            options.RoutingConventions.Add(new DynamicPropertyRoutingConvention());
            options.RoutingConventions.Add(new RefRoutingConvention());
            options.RoutingConventions.Add(new ActionRoutingConvention());
            options.RoutingConventions.Add(new FunctionRoutingConvention());
            options.RoutingConventions.Add(new UnmappedRequestRoutingConvention());

            // TODO: add more default configuration here
        }
    }
}
