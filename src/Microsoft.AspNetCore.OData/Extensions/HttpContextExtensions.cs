// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Interfaces;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extension method to return the <see cref="IODataFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IODataFeature"/>.</returns>
        public static IODataFeature ODataFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            IODataFeature odataFeature = httpContext.Features.Get<IODataFeature>();
            if (odataFeature == null)
            {
                odataFeature = new ODataFeature();
                httpContext.Features.Set<IODataFeature>(odataFeature);
            }

            return odataFeature;
        }

        public static IUrlHelper GetUrlHelper(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            // Get an IUrlHelper from the global service provider.
            ActionContext actionContext = httpContext.RequestServices.GetRequiredService<IActionContextAccessor>().ActionContext;
            return httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }

        public static IETagHandler ETagHandler(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            // Get an IETagHandler from the global service provider.
            return httpContext.RequestServices.GetRequiredService<IETagHandler>();
        }

        //public static IAssemblyProvider AssemblyProvider(this HttpContext httpContext)
        //{
        //    if (httpContext == null)
        //    {
        //        throw Error.ArgumentNull("httpContext");
        //    }

        //    return httpContext.RequestServices.GetRequiredService<IAssemblyProvider>();
        //}
    }
}