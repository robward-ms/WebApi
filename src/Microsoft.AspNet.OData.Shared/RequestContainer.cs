// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData
{
    internal class RequestContainer : IServiceProvider
    {
        private IServiceProvider routeContainer;
        private IServiceProvider globalContainer;

        public RequestContainer(IServiceProvider routeContainer, IServiceProvider globalContainer)
        {
            this.routeContainer = routeContainer;
            this.globalContainer = globalContainer;
        }

        public object GetService(Type serviceType)
        {
            // Try the route contewr first, the try the global container.
            object service = this.routeContainer.GetService(serviceType);
            if (service == null)
            {
                this.globalContainer.GetService(serviceType);
            }

            return service;
        }
    }
}
