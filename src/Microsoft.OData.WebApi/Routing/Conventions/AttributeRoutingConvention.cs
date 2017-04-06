// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Properties;
using Microsoft.OData.WebApi.Routing.Template;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public class AttributeRoutingConvention : IODataRoutingConvention
    {
        private IAttributeMappingProvider _mappingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        public AttributeRoutingConvention(IAttributeMappingProvider mappingProvider)
        {
            this._mappingProvider = mappingProvider;
        }

        /// <inheritdoc />
        public string SelectController(ODataPath odataPath, IWebApiRequestMessage request)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (KeyValuePair<ODataPathTemplate, IWebApiActionDescriptor> attributeMapping in _mappingProvider.AttributeMappings)
            {
                ODataPathTemplate template = attributeMapping.Key;
                IWebApiActionDescriptor action = attributeMapping.Value;

                if (action.SupportedHttpMethods.Contains(request.Method) && template.TryMatch(odataPath, values))
                {
                    values["action"] = action.ActionName;
                    request.Properties["AttributeRouteData"] = values;

                    return action.ControllerDescriptor.ControllerName;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public string SelectAction(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMap actionMap)
        {
            var routeData = controllerContext.Request.GetRouteData();
            var routingConventionsStore = controllerContext.Request.Context.RoutingConventionsStore;

            object value;
            if (controllerContext.Request.Properties.TryGetValue("AttributeRouteData", out value))
            {
                IDictionary<string, object> attributeRouteData = value as IDictionary<string, object>;
                if (attributeRouteData != null)
                {
                    foreach (var item in attributeRouteData)
                    {
                        if (item.Key.StartsWith(ODataParameterValue.ParameterValuePrefix, StringComparison.Ordinal) &&
                            item.Value is ODataParameterValue)
                        {
                            routingConventionsStore.Add(item);
                        }
                        else
                        {
                            routeData.Add(item);
                        }
                    }

                    return attributeRouteData["action"] as string;
                }
            }

            return null;
        }
    }
}
