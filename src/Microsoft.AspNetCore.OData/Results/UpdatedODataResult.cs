// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Adapters;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header.</remarks>
    public class UpdatedODataResult<T> : IActionResult
    {
        private readonly T _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public UpdatedODataResult(T entity)
        {
            Contract.Assert(entity != null);
            this._innerResult = entity;
        }

        /// <inheritdoc/>
        public virtual Task ExecuteResultAsync(ActionContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            if (RequestPreferenceHelpers.RequestPrefersReturnContent(new WebApiRequestHeaders(request.Headers)))
            {
                ObjectResult objectResult = new ObjectResult(_innerResult)
                {
                    StatusCode = StatusCodes.Status200OK
                };

                return objectResult.ExecuteResultAsync(context);
            }
            else
            {
                return Task.FromResult(new StatusCodeResult((int)HttpStatusCode.NoContent)); // TODO: , _innerResult.Request);
            }
        }
    }
}
