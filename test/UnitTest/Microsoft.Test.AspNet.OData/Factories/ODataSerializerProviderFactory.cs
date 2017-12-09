// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A factory for creating <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public static class ODataSerializerProviderFactory
    {
        /// <summary>
        /// Create an <see cref="ODataSerializerProvider"/>.
        /// </summary>
        /// <returns>An ODataSerializerProvider.</returns>
        public static ODataSerializerProvider Create()
        {
#if NETCORE
            var config = RoutingConfigurationFactory.Create();
            return config.ServiceProvider.GetRequiredService<ODataSerializerProvider>();
#else
            return new MockContainer().GetRequiredService<ODataSerializerProvider>();
#endif
        }
    }
}
