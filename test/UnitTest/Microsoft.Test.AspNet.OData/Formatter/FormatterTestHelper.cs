// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.Test.AspNet.OData.Formatter
{
    internal static class FormatterTestHelper
    {
#if NETCORE
        internal static ODataOutputFormatter GetFormatter(ODataPayloadKind[] payload, IEdmModel model = null)
        {
            ODataOutputFormatter formatter;
            formatter = new ODataOutputFormatter();
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            //request.EnableODataDependencyInjectionSupport();
            //request.GetConfiguration().Routes.MapFakeODataRoute();
            // _formatter.Request = GetSampleRequest();

            return formatter;
        }

        internal static ObjectResult GetContent<T>(T content, ODataOutputFormatter formatter, string mediaType)
        {
            ObjectResult objectResult = new ObjectResult(content);
            objectResult.Formatters.Add(formatter);
            objectResult.ContentTypes.Add(mediaType);

            return objectResult;
        }
#else
        internal static ODataMediaTypeFormatter GetFormatter(
            ODataPayloadKind[] payload,
            IEdmModel model = null,
            string routeName = null,
            ODataPath path = null)
        {
            ODataMediaTypeFormatter formatter;
            formatter = new ODataMediaTypeFormatter(payload);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));

            var config = RoutingConfigurationFactory.Create();
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/property", config);
            if (model != null && routeName != null)
            {
                config.MapODataServiceRoute(routeName, null, model);
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else if (routeName != null)
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else if (model != null)
            {
                config.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
                request.EnableODataDependencyInjectionSupport(model);
                request.GetConfiguration().Routes.MapFakeODataRoute();
            }
            else
            {
                config.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
                request.EnableODataDependencyInjectionSupport();
                request.GetConfiguration().Routes.MapFakeODataRoute();
            }

            if (path != null)
            {
                request.ODataProperties().Path = path;
            }

            formatter.Request = request;
            return formatter;
        }

        internal static ObjectContent<T> GetContent<T>(T content, ODataMediaTypeFormatter formatter, string mediaType)
        {
            return new ObjectContent<T>(content, formatter, MediaTypeHeaderValue.Parse(mediaType));
        }
#endif
    }
}
