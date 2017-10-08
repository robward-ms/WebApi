// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="TextOutputFormatter"/> class to handle OData.
    /// </summary>
    internal class ODataOutputFormatterBase
    {
        private readonly ODataVersion _version;

        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly ODataSerializerProvider _serializerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use.</param>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatterBase(ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
            _version = HttpRequestExtensions.DefaultODataVersion;
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
        public Func<Uri> BaseAddressFactory { get; set; }

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
        public bool CanWriteType(Type type, Func<Type,ODataSerializer> getODataSerializer)
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
                payloadKind = GetEdmObjectPayloadKind(type, getODataSerializer);
            }
            else
            {
                ODataSerializer serializer = getODataSerializer(type);
                payloadKind = serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
            }

            return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        public void WriteToStream(Type type, object value, Stream writeStream, IEdmModel model, ODataPath path, IWebApiUrlHelper urlHelper, IWebApiHeaders header, IServiceProvider requestContainer, ODataMessageWriterSettings writerSettings , SelectExpandClause selectExpandClause, ApplyClause applyClause, ODataSerializerContext writeContext, string contentType, long? contentLength, Func<Type, ODataSerializer> getODataSerializer)
        {
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            // Get the serializer - needs type and value
            ODataSerializer serializer = GetSerializer(type, value, _serializerProvider);

            // Get annoatation filter from pref header - preferHeader, stream, and headers.
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(header);
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = new ODataMessageWrapper(writeStream, header.AsDictionary());
                messageWrapper.Container = requestContainer;

                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            // Create message wrapper. - needs streams, headers, request container.
            ODataMessageWrapper responseMessageWrapper = new ODataMessageWrapper(writeStream, header.AsDictionary());
            responseMessageWrapper.Container = requestContainer;

            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            // Create writer settings and add values. - needs base address and version.
            /// If the consumer has provided a delegate for overriding our default implementation,
            /// we call that, otherwise we default to existing behavior below.
            Uri baseAddress = (BaseAddressFactory != null) ? BaseAddressFactory() : GetDefaultBaseAddress(urlHelper);
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = _version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            // Get metadata link - needs  url helpers.
            string metadataLink = urlHelper.CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            // Add odata Url to settings - needs base address and other stuff from request.
            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = selectExpandClause,
                Apply = applyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (!contentLength.HasValue || contentLength.Value == 0)
            {
                // TODO: Rewrite the parameter part.
                //IEnumerable<KeyValuePair<string, string>> parameters =
                //    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
                IEnumerable<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                writeContext.NavigationSource = path == null ? null : path.NavigationSource; ;
                writeContext.Model = model;
                writeContext.RootElementName = GetRootElementName(path) ?? "root";
                writeContext.SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet;
                writeContext.Path = path;
                writeContext.MetadataLevel = metadataLevel;
                writeContext.SelectExpandClause = selectExpandClause;

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        /// <summary>
        /// This method is to get payload kind for untyped scenario.
        /// </summary>
        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type, IWebApiRequestMessage request)
        {
            if (request.IsCountRequest())
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

        private ODataSerializer GetSerializer(Type type, object value, ODataSerializerProvider serializerProvider, IWebApiRequestMessage request)
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
                // get the most appropriate serializer given that we support inheritance.
                ApplyClause applyClause = request.ApplyClause;
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
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IWebApiUrlHelper"/> object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(IWebApiUrlHelper urlHelper)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            string baseAddress = urlHelper.CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        /// <summary>
        /// This function is used to determine whether an OData path includes operation (import) path segments.
        /// We use this function to make sure the value of ODataUri.Path in ODataMessageWriterSettings is null
        /// when any path segment is an operation. ODL will try to calculate the context URL if the ODataUri.Path
        /// equals to null.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
