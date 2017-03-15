// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi errors to OData WebApi.
    /// </summary>
    public class WebApiErrorHelper : IWebApiErrorHelper
    {
        /// <summary>
        /// Get the type of an Http error.
        /// </summary>
        public Type HttpErrorType
        {
            get { return typeof(SerializableError); }
        }

        /// <summary>
        /// Return true of the object is an HttpError.
        /// </summary>
        /// <param name="error">The error to test.</param>
        /// <returns>true of the object is an HttpError</returns>
        public bool IsHttpError(object error)
        {
            return error is SerializableError;
        }

        /// <summary>
        /// Create an ODataError from an HttpError.
        /// </summary>
        /// <param name="error">The error to use.</param>
        /// <returns>an ODataError.</returns>
        public ODataError CreateODataError(object error)
        {
            SerializableError httpError = error as SerializableError;
            return httpError.CreateODataError();
        }
    }
}
