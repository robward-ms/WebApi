// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Properties;
using Microsoft.OData.WebApi.Routing;
using Microsoft.OData.WebApi.Routing.Conventions;
using Microsoft.OData.WebApi.Routing.Template;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi attribute mappings to OData WebApi.
    /// </summary>
    public class AttributeMappingProvider : IAttributeMappingProvider
    {
        private IServiceProvider services;

        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> _attributeMappings;

        public AttributeMappingProvider(IServiceProvider services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            this.services = services;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="pathTemplateHandler"></param>
        /// <param name="actionCollectionProvider"></param>
        public AttributeMappingProvider(IODataPathTemplateHandler pathTemplateHandler, IActionDescriptorCollectionProvider actionCollectionProvider)
        {
            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            if (actionCollectionProvider == null)
            {
                throw Error.ArgumentNull("actionCollectionProvider");
            }

            ODataPathTemplateHandler = pathTemplateHandler;
            ActionDescriptorCollectionProvider = actionCollectionProvider;
        }

        /// <summary>
        /// Gets the options to use.
        /// </summary>
        public ODataOptions Options { get; private set; }

        /// <summary>
        /// Gets the attribute mapping for the system.
        /// </summary>
        public IDictionary<ODataPathTemplate, IWebApiActionDescriptor> AttributeMappings
        {
            get
            {
                if (_attributeMappings == null)
                {
                    if (ActionDescriptorCollectionProvider == null)
                    {
                        ActionDescriptorCollectionProvider =
                            this.services.GetRequiredService<IActionDescriptorCollectionProvider>
                                ();
                    }

                    if (ODataPathTemplateHandler == null)
                    {
                        ODataPathTemplateHandler =
                            this.services.GetRequiredService<IODataPathTemplateHandler>();
                    }

                    IEnumerable<ControllerActionDescriptor> actionDescriptors =
                        ActionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();

                    _attributeMappings = BuildAttributeMappings(actionDescriptors);
                }

                return _attributeMappings;
            }
        }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; private set; }

        /// <summary>
        /// Gets the <see cref="IActionDescriptorCollectionProvider"/> to be used for collecting the controllers.
        /// </summary>
        public IActionDescriptorCollectionProvider ActionDescriptorCollectionProvider { get; private set; }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// This method will execute before the derived type's instance constructor executes. Derived types must
        /// be aware of this and should plan accordingly. For example, the logic in ShouldMapController() should be simple
        /// enough so as not to depend on the "this" pointer referencing a fully constructed object.
        /// </summary>
        /// <param name="controllerActionDescriptor">The controller and action descriptor.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        public virtual bool ShouldMapController(ControllerActionDescriptor controllerActionDescriptor)
        {
            return true;
        }

        private static IEnumerable<string> GetODataRoutePrefixes(ControllerActionDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            ODataRouteAttribute[] prefixAttributes = controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false).ToArray();
            if (!prefixAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (ODataRouteAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.PathTemplate;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerDescriptor.ControllerTypeInfo.FullName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, ControllerActionDescriptor descriptor)
        {
            Contract.Assert(descriptor != null);

            IEnumerable<ODataRouteAttribute> routeAttributes = descriptor.MethodInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false);
            return
                routeAttributes
                .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, descriptor))
                .Where(template => template != null);
        }

        private ODataPathTemplate GetODataPathTemplate(string prefix, string pathTemplate, ControllerActionDescriptor action)
        {
            if (prefix != null && !pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                if (String.IsNullOrEmpty(pathTemplate))
                {
                    pathTemplate = prefix;
                }
                else if (pathTemplate.StartsWith("(", StringComparison.Ordinal))
                {
                    // We don't need '/' when the pathTemplate starts with a key segment.
                    pathTemplate = prefix + pathTemplate;
                }
                else
                {
                    pathTemplate = prefix + "/" + pathTemplate;
                }
            }

            if (pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                pathTemplate = pathTemplate.Substring(1);
            }

            ODataPathTemplate odataPathTemplate;

            try
            {
                odataPathTemplate = ODataPathTemplateHandler.ParseTemplate(pathTemplate, this.services);
            }
            catch (ODataException e)
            {
                throw Error.InvalidOperation(SRResources.InvalidODataRouteOnAction, pathTemplate, action.ActionName,
                    action.ControllerName, e.Message);
            }

            return odataPathTemplate;
        }

        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> BuildAttributeMappings(
            IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            Dictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, IWebApiActionDescriptor>();

            foreach (ControllerActionDescriptor actionDescriptor in actionDescriptors)
            {
                if (!ShouldMapController(actionDescriptor))
                {
                    continue;
                }

                foreach (string prefix in GetODataRoutePrefixes(actionDescriptor))
                {
                    IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, actionDescriptor);
                    foreach (ODataPathTemplate pathTemplate in pathTemplates)
                    {
                        attributeMappings.Add(pathTemplate, new WebApiActionDescriptor(actionDescriptor));
                    }
                }
            }

            return attributeMappings;
        }
    }
}
