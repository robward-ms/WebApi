//// Copyright (c) Microsoft Corporation.  All rights reserved.
//// Licensed under the MIT License.  See License.txt in the project root for license information.

//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.AspNet.OData.Formatter;
//using Microsoft.AspNet.OData.Formatter.Deserialization;
//using Microsoft.AspNet.OData.Formatter.Serialization;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.OData;

//namespace Microsoft.AspNetCore.OData.Formatter
//{
//    /// <summary>
//    /// Factory for <see cref="ODataInputFormatter"/> and <see cref="ODataOutputFormatter"/> classes to handle OData.
//    /// </summary>
//    public static class ODataFormatterFactory
//    {
//        private const string DollarFormat = "$format";

//        private const string JsonFormat = "json";

//        private const string XmlFormat = "xml";

//        /// <summary>
//        /// Creates a list of media type formatters to handle OData.
//        /// The default deserializer provider is <see cref="ODataDeserializerProviderProxy"/>.
//        /// </summary>
//        /// <returns>A list of media type formatters to handle OData.</returns>
//        public static IList<ODataInputFormatter> CreateInputerFormatters()
//        {
//            return CreateInputerFormatters(ODataDeserializerProviderProxy.Instance);
//        }

//        /// <summary>
//        /// Creates a list of media type formatters to handle OData with the given <paramref name="deserializerProvider"/>.
//        /// </summary>
//        /// <param name="deserializerProvider">The deserializer provider to use.</param>
//        /// <returns>A list of media type formatters to handle OData.</returns>
//        public static IList<ODataInputFormatter> CreateInputerFormatters(ODataDeserializerProvider deserializerProvider)
//        {
//            Func<ODataDeserializerProvider, IEnumerable<ODataPayloadKind>, ODataInputFormatter> createFormatter = (provider, payloadKinds) =>
//            {
//                return new ODataInputFormatter(provider, payloadKinds);
//            };

//            return new List<ODataInputFormatter>()
//            {
//                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
//                CreateApplicationJson(createFormatter, deserializerProvider),
//                CreateApplicationXml(createFormatter, deserializerProvider),
//                CreateRawValue(createFormatter, deserializerProvider)
//            };
//        }

//        /// <summary>
//        /// Creates a list of media type formatters to handle OData.
//        /// The default serializer provider is <see cref="ODataSerializerProviderProxy"/>.
//        /// </summary>
//        /// <returns>A list of media type formatters to handle OData.</returns>
//        public static IList<ODataOutputFormatter> CreateOutputFormatters()
//        {
//            return CreateOutputFormatters(ODataSerializerProviderProxy.Instance);
//        }

//        /// <summary>
//        /// Creates a list of media type formatters to handle OData with the given <paramref name="serializerProvider"/> and
//        /// <paramref name="deserializerProvider"/>.
//        /// </summary>
//        /// <param name="serializerProvider">The serializer provider to use.</param>
//        /// <returns>A list of media type formatters to handle OData.</returns>
//        public static IList<ODataOutputFormatter> CreateOutputFormatters(ODataSerializerProvider serializerProvider)
//        {
//            Func<ODataSerializerProvider, IEnumerable<ODataPayloadKind>, ODataOutputFormatter> createFormatter = (provider, payloadKinds) =>
//            {
//                return new ODataOutputFormatter(provider, payloadKinds);
//            };

//            return new List<ODataOutputFormatter>()
//            {
//                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
//                CreateApplicationJson(createFormatter, serializerProvider),
//                CreateApplicationXml(createFormatter, serializerProvider),
//                CreateRawValue(createFormatter, serializerProvider)
//            };
//        }

//        private static TFormatter CreateRawValue<TFormatter, TProvider>(Func<TProvider, IEnumerable<ODataPayloadKind>, TFormatter> createFormatter, TProvider provider)
//        {
//            TFormatter formatter = CreateFormatterWithoutMediaTypes(createFormatter, provider, ODataPayloadKind.Value);
//            //formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
//            //formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
//            //formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
//            //formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
//            return formatter;
//        }

//        private static TFormatter CreateApplicationJson<TFormatter, TProvider>(Func<TProvider, IEnumerable<ODataPayloadKind>, TFormatter> createFormatter, TProvider provider)
//        {
//            TFormatter formatter = CreateFormatterWithoutMediaTypes(
//                createFormatter,
//                provider,
//                ODataPayloadKind.ResourceSet,
//                ODataPayloadKind.Resource,
//                ODataPayloadKind.Property,
//                ODataPayloadKind.EntityReferenceLink,
//                ODataPayloadKind.EntityReferenceLinks,
//                ODataPayloadKind.Collection,
//                ODataPayloadKind.ServiceDocument,
//                ODataPayloadKind.Error,
//                ODataPayloadKind.Parameter,
//                ODataPayloadKind.Delta);

//            // Add minimal metadata as the first media type so it gets used when the request doesn't
//            // ask for a specific content type
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadata);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadata);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingTrue);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingFalse);
//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJson);

//            //formatter.AddDollarFormatQueryStringMappings();
//            //formatter.AddQueryStringMapping(DollarFormat, JsonFormat, ODataMediaTypes.ApplicationJson);

//            return formatter;
//        }

//        private static TFormatter CreateApplicationXml<TFormatter, TProvider>(Func<TProvider, IEnumerable<ODataPayloadKind>, TFormatter> createFormatter, TProvider provider)
//        {
//            TFormatter formatter = CreateFormatterWithoutMediaTypes(
//                createFormatter,
//                provider,
//                ODataPayloadKind.MetadataDocument);

//            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

//            //formatter.AddDollarFormatQueryStringMappings();
//            //formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);

//            return formatter;
//        }

//        private static TFormatter CreateFormatterWithoutMediaTypes<TFormatter, TProvider>(
//            Func<TProvider, IEnumerable<ODataPayloadKind>, TFormatter> createFormatter,
//            TProvider provider,
//            params ODataPayloadKind[] payloadKinds)
//        {
//            TFormatter formatter = createFormatter(provider, payloadKinds as IEnumerable<ODataPayloadKind>);
//            AddSupportedEncodings(formatter.SupportedEncodings);
//            return formatter;
//        }

//        private static void AddSupportedEncodings(IList<Encoding> supportedEncodings)
//        {
//            supportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
//            supportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
//        }

//        private static void AddDollarFormatQueryStringMappings<TFormatter>(this TFormatter formatter)
//        {
//            MediaTypeCollection supportedMediaTypes = formatter.SupportedMediaTypes;
//            foreach (string supportedMediaType in supportedMediaTypes)
//            {
//                //QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(DollarFormat, supportedMediaType);
//                //formatter.MediaTypeMappings.Add(mapping);
//            }
//        }
//    }
//}
