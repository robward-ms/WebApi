// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Common;

namespace Microsoft.OData.WebApi.Routing
{
    /// <summary>
    /// An OData parameter value.
    /// </summary>
    public class ODataParameterValue
    {
        /// <summary>
        /// This prefix is used to identify parameters in [FromODataUri] binding scenario.
        /// </summary>
        public const string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_";

        /// <summary>
        /// Initializes a new instance of the ODataParameterValue class.
        /// </summary>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="paramType">The parameter type.</param>
        public ODataParameterValue(object paramValue, IEdmTypeReference paramType)
        {
            if (paramType == null)
            {
                throw Error.ArgumentNull("paramType");
            }

            Value = paramValue;
            EdmType = paramType;
        }

        /// <summary>
        /// Get the EdmTypeReference.
        /// </summary>
        public IEdmTypeReference EdmType { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public object Value { get; private set; }
    }
}
