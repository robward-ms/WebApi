// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData
{
    internal class PerRouteContainer : IPerRouteContainer
    {
        private ConcurrentDictionary<string, IServiceProvider> _perRouteContainers;

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Create a root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public IServiceProvider CreateODataRootContainer(string routeName, Action<IContainerBuilder> configureAction)
        {
            IServiceProvider rootContainer = this.CreateODataRootContainer(configureAction);
            this._perRouteContainers.AddOrUpdate(routeName, rootContainer, (k, v) => rootContainer);

            return rootContainer;
        }

        /// <summary>
        /// Get the root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <returns>The root container for the route name.</returns>
        public IServiceProvider GetODataRootContainer(string routeName)
        {
            IServiceProvider rootContainer;
            if (_perRouteContainers.TryGetValue(routeName, out rootContainer))
            {
                return rootContainer;
            }

            throw Error.InvalidOperation(SRResources.NullContainer);
        }

        /// <summary>
        /// Create a root container not associated with a route.
        /// </summary>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public IServiceProvider CreateODataRootContainer(Action<IContainerBuilder> configureAction)
        {
            IContainerBuilder builder = CreateContainerBuilderWithDefaultServices();

            if (configureAction != null)
            {
                configureAction(builder);
            }

            IServiceProvider rootContainer = builder.BuildContainer();
            if (rootContainer == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            return rootContainer;
        }

        /// <summary>
        /// Create a container builder with the default OData services.
        /// </summary>
        /// <returns>An instance of <see cref="IContainerBuilder"/> to manage services.</returns>
        private IContainerBuilder CreateContainerBuilderWithDefaultServices()
        {
            // Construct the IContainerBuilder.
            IContainerBuilder builder;
            if (this.BuilderFactory != null)
            {
                builder = this.BuilderFactory();
                if (builder == null)
                {
                    throw Error.InvalidOperation(SRResources.NullContainerBuilder);
                }
            }
            else
            {
                builder = new DefaultContainerBuilder();
            }

            // TODO: Add these in AspNet version only?
            //builder.AddService(ServiceLifetime.Singleton, sp => configuration);
            //builder.AddService(ServiceLifetime.Singleton, sp => configuration.GetDefaultQuerySettings());

            // Add default ODataLib services.
            builder.AddDefaultODataServices();

            // Path handler.
            builder.AddService<IODataPathHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);

            // ReaderSettings and WriterSettings are registered as prototype services.
            // There will be a copy (if it is accessed) of each prototype for each request.
            builder.AddServicePrototype(new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            builder.AddServicePrototype(new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            // QueryValidators.
            builder.AddService<CountQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<FilterQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<ODataQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<OrderByQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SelectExpandQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SkipQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<TopQueryValidator>(ServiceLifetime.Singleton);

            // SerializerProvider and DeserializerProvider.
            builder.AddService<ODataSerializerProvider, DefaultODataSerializerProvider>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeserializerProvider, DefaultODataDeserializerProvider>(ServiceLifetime.Singleton);

            // Deserializers.
            builder.AddService<ODataResourceDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEnumDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataActionPayloadDeserializer>(ServiceLifetime.Singleton);

            // Serializers.
            builder.AddService<ODataEnumSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeltaFeedSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataServiceDocumentSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinksSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataErrorSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataMetadataSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataRawValueSerializer>(ServiceLifetime.Singleton);

            // Binders.
            builder.AddService<ODataQuerySettings>(ServiceLifetime.Scoped);
            builder.AddService<FilterBinder>(ServiceLifetime.Transient);

            // Add ETag handler.
            builder.AddService<IETagHandler, DefaultODataETagHandler>(ServiceLifetime.Singleton);

            // Routing.
            builder.AddService<IODataPathTemplateHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);
            builder.AddService<IActionSelector, ODataActionSelector>(ServiceLifetime.Singleton);

            return builder;
        }
    }
}
