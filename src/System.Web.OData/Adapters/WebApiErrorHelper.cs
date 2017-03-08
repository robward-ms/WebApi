// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.WebApi.Interfaces;
using System.Web.Http;
using System.Web.OData.Extensions;

namespace System.Web.OData.Adapters
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
            get { return typeof (HttpError); }
        }

        /// <summary>
        /// Return true of the object is an HttpError.
        /// </summary>
        /// <param name="error">The error to test.</param>
        /// <returns>true of the object is an HttpError</returns>
        public bool IsHttpError(object error)
        {
            return error is HttpError;
        }

        /// <summary>
        /// Create an ODataError from an HttpError.
        /// </summary>
        /// <param name="error">The error to use.</param>
        /// <returns>an ODataError.</returns>
        public ODataError CreateODataError(object error)
        {
            HttpError httpError = error as HttpError;
            return httpError.CreateODataError();
        }
    }
}
