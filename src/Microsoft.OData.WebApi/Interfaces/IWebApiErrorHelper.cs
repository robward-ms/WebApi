// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.OData.WebApi.Interfaces
{
    /// <summary>
    /// An interface for determining and creating error information.
    /// </summary>
    public interface IWebApiErrorHelper
    {
        /// <summary>
        /// Get the type of an Http error.
        /// </summary>
        Type HttpErrorType { get; }

        /// <summary>
        /// Return true of the object is an HttpError.
        /// </summary>
        /// <param name="error">The error to test.</param>
        /// <returns>true of the object is an HttpError</returns>
        bool IsHttpError(object error);

        /// <summary>
        /// Create an ODataError from an HttpError.
        /// </summary>
        /// <param name="error">The error to use.</param>
        /// <returns>an ODataError.</returns>
        ODataError CreateODataError(object error);
    }
}
