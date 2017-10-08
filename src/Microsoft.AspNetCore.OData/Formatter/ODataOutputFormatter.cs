// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Adapters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// <see cref="TextOutputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataOutputFormatter : TextOutputFormatter
    {
        private readonly ODataOutputFormatterBase _outputFormatter;

        private HttpRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(ODataSerializerProviderProxy.Instance, payloadKinds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/> to use.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use.</param>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            this._outputFormatter = new ODataOutputFormatterBase(serializerProvider, payloadKinds);

            //_deserializerProvider = deserializerProvider;
            //_serializerProvider = serializerProvider;
            //_payloadKinds = payloadKinds;

            //_version = HttpRequestMessageProperties.DefaultODataVersion;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="ODataOutputFormatter"/> to copy settings from.</param>
        /// <param name="version">The OData version that this formatter supports.</param>
        /// <param name="request">The <see cref="HttpRequest"/> for the per-request formatter instance.</param>
        /// <remarks>This is a copy constructor to be used in <see cref="GetPerRequestFormatterInstance"/>.</remarks>
        //internal ODataOutputFormatter(ODataOutputFormatter formatter, ODataVersion version, HttpRequest request)
        //{
        //    if (request == null)
        //    {
        //        throw Error.ArgumentNull("request");
        //    }

        //    Contract.Assert(formatter._serializerProvider != null);
        //    Contract.Assert(formatter._payloadKinds != null);

        //    // Parameter 1: formatter

        //    // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
        //    // everything on the other instance.

        //    // Copy this class's private fields and internal properties.
        //    _serializerProvider = formatter._serializerProvider;
        //    _payloadKinds = formatter._payloadKinds;

        //    // Parameter 2: version
        //    _version = version;

        //    // Parameter 3: request
        //    Request = request;

        //    if (_serializerProvider.GetType() == typeof(ODataSerializerProviderProxy))
        //    {
        //        _serializerProvider = new ODataSerializerProviderProxy
        //        {
        //            RequestContainer = request.GetRequestContainer(),
        //        };
        //    }
        //}

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataSerializerProvider SerializerProvider
        {
            get
            {
                return this._outputFormatter.SerializerProvider;
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
                ODataSerializerProviderProxy serializerProviderProxy = SerializerProvider as ODataSerializerProviderProxy;
                if (serializerProviderProxy != null && serializerProviderProxy.RequestContainer == null)
                {
                    serializerProviderProxy.RequestContainer = value.GetRequestContainer();
                }

                _request = value;
            }
        }

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
        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context.HttpContext.Request != null)
            {
                Type type = context.Object.GetType();
                return this._outputFormatter.CanWriteType(type, GetODataSerializer);
            }

            return false;
        }

        /// <inheritdoc/>
        //[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            //public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            //    TransportContext transportContext, CancellationToken cancellationToken)
            Type type = context.ObjectType;
            object value = context.Object;
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            try
            {
                IEdmModel model = request.GetModel();
                ODataPath path = request.ODataFeature().Path;
                request.HttpContext.GetUrlHelper();
                new WebApiRequestHeaders(Request.Headers);
                request.GetRequestContainer();
                Uri baseAddress = GetBaseAddressInternal(Request);
                ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
                SelectAndExpand = Request.ODataFeature().SelectExpandClause,
                Apply = Request.ODataFeature().ApplyClause,

                this._outputFormatter.WriteToStream(type, value, response.Body, request, request.ContentType, request.ContentLength);
                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        //[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        //private void WriteToStream(Type type, object value, Stream writeStream, HttpRequest request, string contentType, long? contentLength)
        //{
        //    IEdmModel model = request.GetModel();
        //    if (model == null)
        //    {
        //        throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
        //    }

        //    ODataSerializer serializer = GetSerializer(type, value, _serializerProvider);

        //    ODataPath path = request.ODataFeature().Path;
        //    IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

        //    // serialize a response
        //    string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(new WebApiRequestHeaders(request.Headers));
        //    string annotationFilter = null;
        //    if (!String.IsNullOrEmpty(preferHeader))
        //    {
        //        ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(writeStream, request.Headers);
        //        messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
        //        annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
        //    }

        //    ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(writeStream, request.Headers, request.GetRequestContainer());
        //    IODataResponseMessage responseMessage = responseMessageWrapper;
        //    if (annotationFilter != null)
        //    {
        //        responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
        //    }

        //    Uri baseAddress = GetBaseAddressInternal(Request);
        //    ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
        //    writerSettings.BaseUri = baseAddress;
        //    writerSettings.Version = _version;
        //    writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

        //    string metadataLink = request.HttpContext.GetUrlHelper().CreateODataLink(MetadataSegment.Instance);

        //    if (metadataLink == null)
        //    {
        //        throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
        //    }

        //    writerSettings.ODataUri = new ODataUri
        //    {
        //        ServiceRoot = baseAddress,

        //        // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
        //        SelectAndExpand = Request.ODataFeature().SelectExpandClause,
        //        Apply = Request.ODataFeature().ApplyClause,
        //        Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
        //    };

        //    ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
        //    if (!contentLength.HasValue || contentLength.Value == 0)
        //    {
        //        // TODO: Rewrite the parameter part.
        //        //IEnumerable<KeyValuePair<string, string>> parameters =
        //        //    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
        //        IEnumerable<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
        //        metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType, parameters);
        //    }

        //    using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
        //    {
        //        ODataSerializerContext writeContext = new ODataSerializerContext()
        //        {
        //            Request = Request,
        //            NavigationSource = targetNavigationSource,
        //            Model = model,
        //            RootElementName = GetRootElementName(path) ?? "root",
        //            SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet,
        //            Path = path,
        //            MetadataLevel = metadataLevel,
        //            SelectExpandClause = Request.ODataFeature().SelectExpandClause
        //        };

        //        serializer.WriteObject(value, type, messageWriter, writeContext);
        //    }
        //}

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

            return ODataOutputFormatterBase.GetDefaultBaseAddress(new WebApiUrlHelper(request));
        }

        /// <summary>
        /// Get an instance of ODataSerializer for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An instance of ODataSerializer for a given type.</returns>
        private ODataSerializer GetODataSerializer(Type type)
        {
            return SerializerProvider.GetODataPayloadSerializer(type, Request);
        }
    }
}