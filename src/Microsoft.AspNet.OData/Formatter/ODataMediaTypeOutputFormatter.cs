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
    internal class ODataMediaTypeOutputFormatter
    {
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;
        private readonly ODataSerializerProvider _serializerProvider;

        public ODataMediaTypeOutputFormatter(ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
        }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataSerializerProvider SerializerProvider
        {
            get
            {
                return _serializerProvider;
            }
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequestMessage, Uri> BaseAddressFactory { get; set; }

        /// <inheritdoc/>
        public bool SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            // When the user asks for application/json we really need to set the content type to
            // application/json; odata.metadata=minimal. If the user provides the media type and is
            // application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
            // going to fallback to the default implementation.

            // When calling this formatter as part of content negotiation the content negotiator will always
            // pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
            // media type, we delegate to the base class to rely on the default behavior. It's the user's
            // responsibility to pass in the right media type.

            if (mediaType != null)
            {
                if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                    !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
                {
                    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                }

                headers.ContentType = (MediaTypeHeaderValue)((ICloneable)mediaType).Clone();
                return true;
            }
            else
            {
                // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
                return false;
            }
        }

        public void SetCharSetAndVersion(HttpContentHeaders headers, HttpRequestHeaders requestHeaders, ODataVersion version)
        {
            // In general, in Web API we pick a default charset based on the supported character sets
            // of the formatter. However, according to the OData spec, the service shouldn't be sending
            // a character set unless explicitly specified, so if the client didn't send the charset we chose
            // we just clean it.
            if (headers.ContentType != null &&
                !requestHeaders.AcceptCharset
                    .Any(cs => cs.Value.Equals(headers.ContentType.CharSet, StringComparison.OrdinalIgnoreCase)))
            {
                headers.ContentType.CharSet = String.Empty;
            }

            headers.TryAddWithoutValidation(
                ODataVersionConstraint.ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(version));
        }

        /// <inheritdoc/>
        public bool CanWriteType(Type type, HttpRequestMessage request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataPayloadKind? payloadKind;

            Type elementType;
            if (typeof(IEdmObject).IsAssignableFrom(type) ||
                (TypeHelper.IsCollection(type, out elementType) && typeof(IEdmObject).IsAssignableFrom(elementType)))
            {
                payloadKind = GetEdmObjectPayloadKind(type, request);
            }
            else
            {
                payloadKind = GetClrObjectResponsePayloadKind(type, request);
            }

            return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        public void WriteToStream(Type type, object value, Stream writeStream, HttpContent content, HttpContentHeaders contentHeaders, HttpRequestMessage request, ODataVersion version)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, _serializerProvider, request);

            UrlHelper urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            ODataPath path = request.ODataProperties().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            // serialize a response
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(new WebApiRequestHeaders(request.Headers));
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(writeStream, content.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(writeStream, content.Headers, request.GetRequestContainer());
            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            Uri baseAddress = GetBaseAddressInternal(request);
            ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = urlHelper.CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = request.ODataProperties().SelectExpandClause,
                Apply = request.ODataProperties().ApplyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (contentHeaders != null && contentHeaders.ContentType != null)
            {
                MediaTypeHeaderValue contentType = contentHeaders.ContentType;
                IEnumerable<KeyValuePair<string, string>> parameters =
                    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType.MediaType, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = new ODataSerializerContext()
                {
                    Request = request,
                    Url = urlHelper,
                    NavigationSource = targetNavigationSource,
                    Model = model,
                    RootElementName = GetRootElementName(path) ?? "root",
                    SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet,
                    Path = path,
                    MetadataLevel = metadataLevel,
                    SelectExpandClause = request.ODataProperties().SelectExpandClause
                };

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        private ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type, HttpRequestMessage request)
        {
            // SingleResult<T> should be serialized as T.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SingleResult<>))
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, request);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }

        /// <summary>
        /// This method is to get payload kind for untyped scenario.
        /// </summary>
        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type, HttpRequestMessage request)
        {
            if (ODataCountMediaTypeMapping.IsCountRequest(request))
            {
                return ODataPayloadKind.Value;
            }

            Type elementType;
            if (TypeHelper.IsCollection(type, out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.ResourceSet;
                }
                else if (typeof(IEdmChangedObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Delta;
                }
            }
            else
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Resource;
                }
            }

            return null;
        }

        private ODataSerializer GetSerializer(Type type, object value, ODataSerializerProvider serializerProvider, HttpRequestMessage request)
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
                var applyClause = request.ODataProperties().ApplyClause;
                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = serializerProvider.GetODataPayloadSerializer(type, request);
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
