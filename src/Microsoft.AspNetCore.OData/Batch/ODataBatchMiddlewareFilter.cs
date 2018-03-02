// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// A class for adding a batch middle ware filter for OData routes. This class is used
    /// to decorate the ODataController, enabling batch for all OData controllers. the OData
    /// batch middle ware is responsible for applying the route.
    /// </summary>
    /// <remarks>
    /// MiddlewareFilterAttribute is used as opposed to direct middle ware to simply the configuration
    /// for the user. With middle ware, the call needs to add a .Use() in Startup.Configure(). With the
    /// filter middle ware approach, the middle ware is injected into the filter pipeline without any
    /// additional work on the caller.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class ODataBatchMiddlewareFilterAttribute : MiddlewareFilterAttribute
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="ODataBatchMiddlewareFilterAttribute"/>.
        /// </summary>
        public ODataBatchMiddlewareFilterAttribute()
            : base(typeof(ODataBatchMiddlewareExtension))
        {
        }
    }

    internal class ODataBatchMiddlewareExtension
    {

        /// <summary>
        /// Configure the middle ware.
        /// </summary>
        /// <param name="applicationBuilder">The application builder to use.</param>
        public void Configure(IApplicationBuilder applicationBuilder)
        {

            return applicationBuilder.UseMiddleware<ODataBatchMiddleware>();
        }
    }
}
