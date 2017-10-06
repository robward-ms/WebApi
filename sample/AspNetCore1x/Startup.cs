// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();
            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            IEdmModel model = GetEdmModel(app.ApplicationServices);
            app.UseMvc(routeBuilder => routeBuilder.MapODataServiceRoute("name", "prefix", model));
        }

        private static IEdmModel GetEdmModel(IServiceProvider serviceProvider)
        {
            var builder = new ODataConventionModelBuilder(serviceProvider);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Product>("Products");

            // Functions
            //var function = builder.EntityType<Customer>().Collection.Function("FindCustomersWithProductId");
            //function.Parameter<int>("productId");
            //function.ReturnsFromEntitySet<Customer>("Customers");

            //var function = builder.EntityType<Customer>().Function("GetCustomerName");
            //function.Parameter<string>("format");
            //function.Returns<string>();

            return builder.GetEdmModel();
        }
    }
}
