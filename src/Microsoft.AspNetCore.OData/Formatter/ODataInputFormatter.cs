// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// <see cref="TextInputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataInputFormatter : TextInputFormatter
    {
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept in CanReadType.
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly ODataDeserializerProvider _deserializerProvider;

        private HttpRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataInputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(ODataDeserializerProviderProxy.Instance, payloadKinds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInputFormatter"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/> to use.</param>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataInputFormatter(ODataDeserializerProvider deserializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _deserializerProvider = deserializerProvider;
            _payloadKinds = payloadKinds;

            _version = HttpRequestExtensions.DefaultODataVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInputFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="ODataInputFormatter"/> to copy settings from.</param>
        /// <param name="version">The OData version that this formatter supports.</param>
        /// <param name="request">The <see cref="HttpRequest"/> for the per-request formatter instance.</param>
        /// <remarks>This is a copy constructor to be used in <see cref="GetPerRequestFormatterInstance"/>.</remarks>
        internal ODataInputFormatter(ODataInputFormatter formatter, ODataVersion version, HttpRequest request)
            //: base(formatter)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Contract.Assert(formatter._deserializerProvider != null);
            Contract.Assert(formatter._payloadKinds != null);

            // Parameter 1: formatter

            // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.

            // Copy this class's private fields and internal properties.
            _deserializerProvider = formatter._deserializerProvider;
            _payloadKinds = formatter._payloadKinds;

            // Parameter 2: version
            _version = version;

            // Parameter 3: request
            Request = request;

            if (_deserializerProvider.GetType() == typeof(ODataDeserializerProviderProxy))
            {
                _deserializerProvider = new ODataDeserializerProviderProxy
                {
                    RequestContainer = request.GetRequestContainer()
                };
            }
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
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// The request message associated with the per-request formatter instance.
        /// </summary>
        public HttpRequest Request
        {
            get { return _request; }
            set
            {
                ODataDeserializerProviderProxy deserializerProviderProxy = _deserializerProvider as ODataDeserializerProviderProxy;
                if (deserializerProviderProxy != null && deserializerProviderProxy.RequestContainer == null)
                {
                    deserializerProviderProxy.RequestContainer = value.GetRequestContainer();
                }

                _request = value;
            }
        }

        /// <inheritdoc/>
        //public override TextOutputFormatter GetPerRequestFormatterInstance(Type type, HttpRequest request, MediaTypeHeaderValue mediaType)
        //{
        //    // call base to validate parameters
        //    base.GetPerRequestFormatterInstance(type, request, mediaType);

        //    if (Request != null && Request == request)
        //    {
        //        // If the request is already set on this formatter, return itself.
        //        return this;
        //    }
        //    else
        //    {
        //        ODataVersion version = GetODataResponseVersion(request);
        //        return new ODataInputFormatter(this, version, request);
        //    }
        //}

        /// <inheritdoc/>
        //public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        //{
        //    if (type == null)
        //    {
        //        throw Error.ArgumentNull("type");
        //    }
        //    if (headers == null)
        //    {
        //        throw Error.ArgumentNull("headers");
        //    }

        //    // When the user asks for application/json we really need to set the content type to
        //    // application/json; odata.metadata=minimal. If the user provides the media type and is
        //    // application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
        //    // going to fallback to the default implementation.

        //    // When calling this formatter as part of content negotiation the content negotiator will always
        //    // pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
        //    // media type, we delegate to the base class to rely on the default behavior. It's the user's
        //    // responsibility to pass in the right media type.

        //    if (mediaType != null)
        //    {
        //        if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
        //            !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
        //        {
        //            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
        //        }

        //        headers.ContentType = (MediaTypeHeaderValue)((ICloneable)mediaType).Clone();
        //    }
        //    else
        //    {
        //        // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
        //        base.SetDefaultContentHeaders(type, headers, mediaType);
        //    }

        //    // In general, in Web API we pick a default charset based on the supported character sets
        //    // of the formatter. However, according to the OData spec, the service shouldn't be sending
        //    // a character set unless explicitly specified, so if the client didn't send the charset we chose
        //    // we just clean it.
        //    if (headers.ContentType != null &&
        //        !Request.Headers.AcceptCharset
        //            .Any(cs => cs.Value.Equals(headers.ContentType.CharSet, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        headers.ContentType.CharSet = String.Empty;
        //    }

        //    headers.TryAddWithoutValidation(
        //        HttpRequestProperties.ODataServiceVersionHeader,
        //        ODataUtils.ODataVersionToString(_version));
        //}

        /// <inheritdoc/>
        public override bool CanRead(InputFormatterContext context)
        {
            Type type = context.ModelType;

            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (Request != null)
            {
                IEdmModel model = Request.GetModel();
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, Request.ODataFeature().Path, model,
                    _deserializerProvider, out expectedPayloadType);
                if (deserializer != null)
                {
                    return _payloadKinds.Contains(deserializer.ODataPayloadKind);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        //[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            //public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
            Type type = context.ModelType;
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            try
            {
                object content = ReadFromStream(type, request.Body, request.Headers, request.ContentType, request.ContentLength);
                return InputFormatterResult.SuccessAsync(content);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError<InputFormatterResult>(ex);
            }
        }

        /// <inheritdoc/>
        //[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ODataMessageReader disposed later with request.")]
        private object ReadFromStream(Type type, Stream readStream, IHeaderDictionary requestHeaders, string contentType, long? contentLength)
        {
            object result;

            // If content length is 0 then return default value for this type
            if (!contentLength.HasValue || contentLength.Value == 0)
            {
                result = GetDefaultValueForType(type);
            }
            else
            {
                IEdmModel model = Request.GetModel();
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, Request.ODataFeature().Path, model, _deserializerProvider, out expectedPayloadType);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                try
                {
                    ODataMessageReaderSettings oDataReaderSettings = Request.GetReaderSettings();
                    oDataReaderSettings.BaseUri = GetBaseAddressInternal(Request);
                    oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

                    IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(readStream, requestHeaders, Request.GetODataContentIdMapping(), Request.GetRequestContainer());
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);

                    ODataPath path = Request.ODataFeature().Path;
                    ODataDeserializerContext readContext = new ODataDeserializerContext
                    {
                        Path = path,
                        Model = model,
                        Request = Request,
                        ResourceType = type,
                        ResourceEdmType = expectedPayloadType,
                    };

                    result = deserializer.Read(oDataMessageReader, type, readContext);
                }
                catch (Exception)
                {
                    result = GetDefaultValueForType(type);
                }
            }

            return result;
        }

        /// <summary>
        /// This method is to get payload kind for untyped scenario.
        /// </summary>
        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type)
        {
            if (Request.IsCountRequest())
            {
                return ODataPayloadKind.Value;
            }

            Type elementType;
            if (TypeHelper.IsCollection(type, out elementType))
            {
                if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmComplexObject), elementType) || TypeHelper.IsTypeAssignableFrom(typeof(IEdmEnumObject), elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmEntityObject), elementType))
                {
                    return ODataPayloadKind.ResourceSet;
                }
                else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmChangedObject), elementType))
                {
                    return ODataPayloadKind.Delta;
                }
            }
            else
            {
                if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmComplexObject), elementType) || TypeHelper.IsTypeAssignableFrom(typeof(IEdmEnumObject), elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmEntityObject), elementType))
                {
                    return ODataPayloadKind.Resource;
                }
            }

            return null;
        }

        private ODataDeserializer GetDeserializer(Type type, ODataPath path, IEdmModel model,
            ODataDeserializerProvider deserializerProvider, out IEdmTypeReference expectedPayloadType)
        {
            expectedPayloadType = EdmLibHelpers.GetExpectedPayloadType(type, path, model);

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type, Request);
            if (deserializer == null && expectedPayloadType != null)
            {
                // we are in typeless mode, get the deserializer using the edm type from the path.
                deserializer = deserializerProvider.GetEdmTypeDeserializer(expectedPayloadType);
            }

            return deserializer;
        }

        private ODataSerializer GetSerializer(Type type, object value, ODataSerializerProvider serializerProvider)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(Error.Format(SRResources.EdmTypeCannotBeNull,
                        edmObject.GetType().FullName, typeof(IEdmObject).Name));
                }

                serializer = serializerProvider.GetEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString());
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = Request.ODataFeature().ApplyClause;
                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = serializerProvider.GetODataPayloadSerializer(type, Request);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }

        private static string GetRootElementName(ODataPath path)
        {
            if (path != null)
            {
                ODataPathSegment lastSegment = path.Segments.LastOrDefault();
                if (lastSegment != null)
                {
                    OperationSegment actionSegment = lastSegment as OperationSegment;
                    if (actionSegment != null)
                    {
                        IEdmAction action = actionSegment.Operations.Single() as IEdmAction;
                        if (action != null)
                        {
                            return action.Name;
                        }
                    }

                    PropertySegment propertyAccessSegment = lastSegment as PropertySegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequest request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataInputFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            string baseAddress = request.HttpContext.GetUrlHelper().CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequest request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to
            // understand the response. There is no easy way we can figure out the minimum version that the client
            // needs to understand our response. We send response headers much ahead generating the response. So if
            // the requestMessage has a OData-MaxVersion, tell the client that our response is of the same
            // version; else use the DataServiceVersionHeader. Our response might require a higher version of the
            // client and it might fail. If the client doesn't send these headers respond with the default version
            // (V4).
            return request.ODataMaxServiceVersion() ??
                request.ODataServiceVersion() ??
                HttpRequestExtensions.DefaultODataVersion;
        }

        // This function is used to determine whether an OData path includes operation (import) path segments.
        // We use this function to make sure the value of ODataUri.Path in ODataMessageWriterSettings is null
        // when any path segment is an operation. ODL will try to calculate the context URL if the ODataUri.Path
        // equals to null.
        private static bool IsOperationPath(ODataPath path)
        {
            if (path == null)
            {
                return false;
            }

            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is OperationSegment ||
                    segment is OperationImportSegment)
                {
                    return true;
                }
            }

            return false;
        }
    }
}