// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    [ODataFormatting]
    [ODataRouting]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract partial class ODataController : ControllerBase
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        /// <summary>
        /// Creates an action result with the specified values that is a response to a POST operation with an entity
        /// to an entity set.
        /// </summary>
        /// <typeparam name="TEntity">The created entity type.</typeparam>
        /// <param name="entity">The created entity.</param>
        /// <returns>A <see cref="CreatedODataResult{TEntity}"/> with the specified values.</returns>
        protected virtual CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new CreatedODataResult<TEntity>(entity);
        }

        /// <summary>
        /// Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation
        /// on an OData entity.
        /// </summary>
        /// <typeparam name="TEntity">The updated entity type.</typeparam>
        /// <param name="entity">The updated entity.</param>
        /// <returns>An <see cref="UpdatedODataResult{TEntity}"/> with the specified values.</returns>
        protected virtual UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new UpdatedODataResult<TEntity>(entity);
        }

        /// <summary>
        /// Get the Edm model associated with the controller.
        /// </summary>
        /// <returns></returns>
        protected IEdmModel GetModel()
        {
            IEdmModel model = GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
