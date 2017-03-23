// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData
{
    internal class ODataContext
    {
        public IEdmModel Model { get; }

        public ODataContext(IServiceCollection serviceCollection, Type t)
        {
            Model = DefaultODataModelProvider.BuildEdmModel(serviceCollection, t);
        }
    }
}
