// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Buffers;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
#else
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class FormatterFactory
    {
#if NETCORE
        /// <summary>
        /// Create a Json formatter.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A Json formatter.</returns>
        public static OutputFormatter CreateJson(WebRouteConfiguration configuration)
        {
            var options = configuration.ServiceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value;
            var charPool = configuration.ServiceProvider.GetRequiredService<ArrayPool<char>>();
            return new JsonOutputFormatter(options.SerializerSettings, charPool);
        }

        /// <summary>
        /// Create the OData formatters.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use.</param>
        /// <param name="deserializerProvider">The deserializer provider to use.</param>
        /// <returns>A Json formatter.</returns>
        public static IList<ODataOutputFormatter> CreateOData(
            ODataSerializerProvider serializerProvider = null,
            ODataDeserializerProvider deserializerProvider = null)
        {
            // deserializerProvider unused for output formatters.
            if (serializerProvider == null)
            {
                return ODataOutputFormatterFactory.Create();
            }

            return ODataOutputFormatterFactory.Create(serializerProvider);
        }
#else
        /// <summary>
        /// Create a Json formatter.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A Json formatter.</returns>
        public static MediaTypeFormatter CreateJson(WebRouteConfiguration configuration)
        {
            return new JsonMediaTypeFormatter();
        }

        /// <summary>
        /// Create the OData formatters.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use.</param>
        /// <param name="deserializerProvider">The deserializer provider to use.</param>
        /// <returns>A Json formatter.</returns>
        public static IList<ODataOutputFormatter> CreateOData(
            ODataSerializerProvider serializerProvider,
            ODataDeserializerProvider deserializerProvider)
        {
            return ODataMediaTypeFormatters.Create((serializerProvider,deserializerProvider);
        }
#endif
    }
}
