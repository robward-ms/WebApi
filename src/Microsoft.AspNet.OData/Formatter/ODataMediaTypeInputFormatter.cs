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
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    internal class ODataMediaTypeInputFormatter
    {
        /// <inheritdoc/>
        public static bool CanReadType(Type type, IEdmModel model, ODataPath path, IEnumerable<ODataPayloadKind> payloadKinds, Func<IEdmTypeReference, ODataDeserializer> getEdmTypeDeserializer, Func<Type, ODataDeserializer> getODataPayloadDeserializer)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            IEdmTypeReference expectedPayloadType;
            ODataDeserializer deserializer = GetDeserializer(type, path, model,
                getEdmTypeDeserializer, getODataPayloadDeserializer, out expectedPayloadType);
            if (deserializer != null)
            {
                return payloadKinds.Contains(deserializer.ODataPayloadKind);
            }

            return false;
        }

        // content only needed for content headers.
        // factor out formatterLogger by try/catch outside.
        // request is used for registering for disposal
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ODataMessageReader disposed later with request.")]
        public object ReadFromStream(Type type, object defaultValue, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, IEdmModel model, Uri baseAddress, HttpRequestMessage request, IWebApiRequestMessage internalRequest, Func<IEdmTypeReference, ODataDeserializer> getEdmTypeDeserializer, Func<Type, ODataDeserializer> getODataPayloadDeserializer, Func<ODataDeserializerContext> getODataDeserializerContext)
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
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, internalRequest.Context.Path, model, getEdmTypeDeserializer, getODataPayloadDeserializer, out expectedPayloadType);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                try
                {
                    ODataMessageReaderSettings oDataReaderSettings = internalRequest.ReaderSettings;
                    oDataReaderSettings.BaseUri = baseAddress;
                    oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

                    IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(readStream, contentHeaders, internalRequest.ODataContentIdMapping, internalRequest.RequestContainer);
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);
                    request.RegisterForDispose(oDataMessageReader);

                    ODataPath path = internalRequest.Context.Path;
                    ODataDeserializerContext readContext = getODataDeserializerContext();
                    readContext.Path = path;
                    readContext.Model = model;
                    readContext.ResourceType = type;
                    readContext.ResourceEdmType = expectedPayloadType;

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

        private static ODataDeserializer GetDeserializer(Type type, ODataPath path, IEdmModel model,
            Func<IEdmTypeReference, ODataDeserializer> getEdmTypeDeserializer, Func<Type, ODataDeserializer> getODataPayloadDeserializer, out IEdmTypeReference expectedPayloadType)
        {
            expectedPayloadType = EdmLibHelpers.GetExpectedPayloadType(type, path, model);

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = getODataPayloadDeserializer(type);
            if (deserializer == null && expectedPayloadType != null)
            {
                // we are in typeless mode, get the deserializer using the edm type from the path.
                deserializer = getEdmTypeDeserializer(expectedPayloadType);
            }

            return deserializer;
        }
    }
}
