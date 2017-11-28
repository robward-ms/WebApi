﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProviderProxy"/>.
    /// </summary>
    internal partial class ODataDeserializerProviderProxy : ODataDeserializerProvider
    {
        private static readonly ODataDeserializerProviderProxy _instance = new ODataDeserializerProviderProxy();

        private IServiceProvider _requestContainer;

        /// <summary>
        /// Gets the default instance of the <see cref="ODataDeserializerProviderProxy"/>.
        /// </summary>
        public static ODataDeserializerProviderProxy Instance
        {
            get
            {
                return _instance;
            }
        }

        public IServiceProvider RequestContainer
        {
            get { return _requestContainer; }
            set
            {
                Contract.Assert(_requestContainer == null, "Cannot set request container twice.");

                _requestContainer = value;
            }
        }

        /// <inheritdoc />
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            return RequestContainer.GetRequiredService<ODataDeserializerProvider>()
                .GetEdmTypeDeserializer(edmType);
        }
    }
}
