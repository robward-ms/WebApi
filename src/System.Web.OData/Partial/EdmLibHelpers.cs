// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http.Dispatcher;
using System.Web.OData.Adapters;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Formatter
{
    internal static partial class EdmLibHelpers
    {
        private static readonly IWebApiAssembliesResolver _defaultAssemblyResolver = new WebApiAssembliesResolver(new DefaultAssembliesResolver());
    }
}