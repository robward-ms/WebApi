// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    internal class ODataMediaTypeInputFormatter
    {
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;
        private readonly ODataDeserializerProvider _deserializerProvider;

        public ODataMediaTypeInputFormatter(ODataDeserializerProvider deserializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            _deserializerProvider = deserializerProvider;
            _payloadKinds = payloadKinds;
        }

        /// <summary>
        /// Gets the <see cref="ODataDeserializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider
        {
            get
            {
                return _deserializerProvider;
            }
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequestMessage, Uri> BaseAddressFactory { get; set; }

        /// <inheritdoc/>
        public bool CanReadType(Type type, HttpRequestMessage request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            IEdmModel model = request.GetModel();
            IEdmTypeReference expectedPayloadType;
            ODataDeserializer deserializer = GetDeserializer(type, request, request.ODataProperties().Path, model,
                _deserializerProvider, out expectedPayloadType);
            if (deserializer != null)
            {
                return _payloadKinds.Contains(deserializer.ODataPayloadKind);
            }

            return false;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ODataMessageReader disposed later with request.")]
        public object ReadFromStream(Type type, object defaultValue, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, HttpRequestMessage request)
        {
            object result;

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders == null || contentHeaders.ContentLength == 0)
            {
                result = defaultValue;
            }
            else
            {
                IEdmModel model = request.GetModel();
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, request, request.ODataProperties().Path, model, _deserializerProvider, out expectedPayloadType);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                try
                {
                    ODataMessageReaderSettings oDataReaderSettings = request.GetReaderSettings();
                    oDataReaderSettings.BaseUri = GetBaseAddressInternal(request);
                    oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

                    IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(readStream, contentHeaders, request.GetODataContentIdMapping(), request.GetRequestContainer());
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);

                    request.RegisterForDispose(oDataMessageReader);
                    ODataPath path = request.ODataProperties().Path;
                    ODataDeserializerContext readContext = new ODataDeserializerContext
                    {
                        Path = path,
                        Model = model,
                        Request = request,
                        ResourceType = type,
                        ResourceEdmType = expectedPayloadType,
                    };

                    result = deserializer.Read(oDataMessageReader, type, readContext);
                }
                catch (Exception e)
                {
                    if (formatterLogger == null)
                    {
                        throw;
                    }

                    formatterLogger.LogError(String.Empty, e);
                    result = defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequestMessage object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequestMessage request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataMediaTypeFormatter.GetDefaultBaseAddress(request);
            }
        }
        
        private ODataDeserializer GetDeserializer(Type type, HttpRequestMessage request, ODataPath path, IEdmModel model,
            ODataDeserializerProvider deserializerProvider, out IEdmTypeReference expectedPayloadType)
        {
            expectedPayloadType = EdmLibHelpers.GetExpectedPayloadType(type, path, model);

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type, request);
            if (deserializer == null && expectedPayloadType != null)
            {
                // we are in typeless mode, get the deserializer using the edm type from the path.
                deserializer = deserializerProvider.GetEdmTypeDeserializer(expectedPayloadType);
            }

            return deserializer;
        }
    }
}
