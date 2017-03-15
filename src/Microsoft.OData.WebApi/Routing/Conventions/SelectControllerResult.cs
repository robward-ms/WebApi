// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// An return value for SelectController.
    /// </summary>
    public class SelectControllerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectControllerResult"/> class.
        /// </summary>
        /// <param name="controllerName">The controller name selected.</param>
        public SelectControllerResult(string controllerName)
        {
            this.ControllerName = controllerName;
            this.Values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the controller name selected.
        /// </summary>
        public string ControllerName { get; private set; }

        /// <summary>
        /// Gets or sets the properties associated with the selected controller.4
        /// </summary>
        public IDictionary<string, object> Values { get; set; }
    }
}
