// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Defines the middle ware for handling OData batch requests. This middle ware essentially
    /// acts like branching middle ware, <see cref="MapExtensions "/>, and redirects OData batch
    /// requests to the appropriate ODataBatchHandler.
    /// </summary>
    public class ODataBatchMiddleware
    {
        private readonly RequestDelegate _next;

        public ODataBatchMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            return this._next(context);
        }

        /// <summary>
        /// Configure the middle ware.
        /// </summary>
        /// <param name="applicationBuilder">The application builder to use.</param>
        public void Configure(IApplicationBuilder applicationBuilder)
        {

            return applicationBuilder.UseMiddleware<ODataBatchMiddleware>();

            applicationBuilder.Use(async (context, next) =>
            {
                ODataBatchPathMapping batchMapping = context.RequestServices.GetRequiredService<ODataBatchPathMapping>();

                string routeName;
                if (batchMapping.TryGetRouteName(context.Request.Path, out routeName))
                {
                    IPerRouteContainer perRouteContainer = context.RequestServices.GetRequiredService<IPerRouteContainer>();
                    if (perRouteContainer == null)
                    {
                        throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
                    }

                    IServiceProvider rootContainer = perRouteContainer.GetODataRootContainer(routeName);
                    ODataBatchHandler batchHandler = rootContainer.GetRequiredService<ODataBatchHandler>();
                    await batchHandler.ProcessBatchAsync(context, next);
                }
                else
                {
                    await next.Invoke();
                }
            });
        }
    }
}
