// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Common;

namespace System.Web.OData.Extensions
{
    /// <summary>
    /// Extensions to convert to/from WebApiEntityTagHeaderValue from/to EntityTagHeaderValue.
    /// </summary>
    public static class EntityTagHeaderValueExtensions
    {
        /// <summary>
        /// Convert to WebApiEntityTagHeaderValue
        /// </summary>
        /// <param name="value">The EntityTagHeaderValue to convert.</param>
        /// <returns>A WebApiEntityTagHeaderValue.</returns>
        public static WebApiEntityTagHeaderValue AsWebApiEntityTagHeaderValue(this EntityTagHeaderValue value)
        {
            WebApiEntityTagHeaderValue newValue = null;
            if (value != null)
            {
                newValue = new WebApiEntityTagHeaderValue(value.Tag, value.IsWeak);
            }

            return newValue;
        }

        /// <summary>
        /// Convert to EntityTagHeaderValue
        /// </summary>
        /// <param name="value">The WebApiEntityTagHeaderValue to convert.</param>
        /// <returns>A EntityTagHeaderValue.</returns>
        public static EntityTagHeaderValue AsEntityTagHeaderValue(this WebApiEntityTagHeaderValue value)
        {
            EntityTagHeaderValue newValue = null;
            if (value != null)
            {
                newValue = new EntityTagHeaderValue(value.Tag, value.IsWeak);
            }

            return newValue;
        }
    }
}
