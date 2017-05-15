// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http.Dispatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.WebApi.Adapters;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    internal static partial class ODataQueryContextExtensions
    {
        public static IWebApiAssembliesResolver GetAssembliesResolver(this ODataQueryContext context)
        {
            IAssembliesResolver resolver = context.RequestContainer.GetRequiredService<IAssembliesResolver>();
            return new WebApiAssembliesResolver(resolver);
        }
    }
}
