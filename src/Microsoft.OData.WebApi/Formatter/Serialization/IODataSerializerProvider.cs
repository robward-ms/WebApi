// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Formatter.Serialization
{
    /// <summary>
    /// An IODataSerializerProvider is a factory for creating <see cref="ODataSerializer"/>s.
    /// </summary>
    public interface IODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataEdmTypeSerializer"/> for the given edmType.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType);

        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <param name="request">The request for which the response is being serialized.</param>
        /// <returns>The <see cref="ODataSerializer"/> for the given type.</returns>
        ODataSerializer GetODataPayloadSerializer(Type type, IWebApiRequestMessage request);
    }
}
