// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Adapters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public partial class AttributeRoutingConvention : IODataRoutingConvention
    {
        private static readonly DefaultODataPathHandler _defaultPathHandler = new DefaultODataPathHandler();

        private readonly string _routeName;

        private readonly IServiceProvider _serviceProvider;

        private IODataPathTemplateHandler _odataPathTemplateHandler;

        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> _attributeMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        public AttributeRoutingConvention(string routeName, IServiceProvider serviceProvider)
            : this(routeName, _defaultPathHandler)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        internal AttributeRoutingConvention(string routeName,
            IEnumerable<ControllerActionDescriptor> controllers)
            : this(routeName, controllers, (IODataPathTemplateHandler)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "See note on <see cref=\"ShouldMapController()\"> method.")]
        internal AttributeRoutingConvention(string routeName,
            IEnumerable<ControllerActionDescriptor> controllers,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(routeName, pathTemplateHandler)
        {
            if (controllers == null)
            {
                throw Error.ArgumentNull("controllers");
            }

            _attributeMappings = BuildAttributeMappings(controllers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        private AttributeRoutingConvention(string routeName, IODataPathTemplateHandler pathTemplateHandler)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            _routeName = routeName;
            _odataPathTemplateHandler = pathTemplateHandler;
        }

        /// <summary>
        /// Gets the attribute mappings.
        /// </summary>
        internal IDictionary<ODataPathTemplate, IWebApiActionDescriptor> AttributeMappings
        {
            get
            {
                if (_attributeMappings == null)
                {
                    // Get the IActionDescriptorCollectionProvider from the global services provider.
                    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider =
                            _serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();

                    IEnumerable<ControllerActionDescriptor> actionDescriptors =
                        actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();

                    _attributeMappings = BuildAttributeMappings(actionDescriptors);
                }

                return _attributeMappings;
            }
        }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// This method will execute before the derived type's instance constructor executes. Derived types must
        /// be aware of this and should plan accordingly. For example, the logic in ShouldMapController() should be simple
        /// enough so as not to depend on the "this" pointer referencing a fully constructed object.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        public virtual bool ShouldMapController(ControllerActionDescriptor controllerAction)
        {
            return true;
        }

        /// <inheritdoc/>
        public ControllerActionDescriptor SelectAction(RouteContext routeContext)
        {
            // Get a IActionDescriptorCollectionProvider from the global service provider.
            IActionDescriptorCollectionProvider actionCollectionProvider =
                routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            Contract.Assert(actionCollectionProvider != null);

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(
                odataPath,
                new WebApiRequestMessage(request),
                this.AttributeMappings);

            if (controllerResult != null)
            {
                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                    string actionName = SelectActionImpl(
                        odataPath,
                        new WebApiControllerContext(routeContext, controllerResult),
                        new WebApiActionMap(actionDescriptors));

                if (!String.IsNullOrEmpty(actionName))
                {
                    return actionDescriptors.FirstOrDefault(
                        c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                }
            }

            return null;
        }

        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> BuildAttributeMappings(IEnumerable<ControllerActionDescriptor> controllerActions)
        {
            Dictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, IWebApiActionDescriptor>();

            foreach (ControllerActionDescriptor controllerAction in controllerActions)
            {
                if (IsODataController(controllerAction) && ShouldMapController(controllerAction))
                {
                    foreach (string prefix in GetODataRoutePrefixes(controllerAction))
                    {
                        IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, controllerAction);
                        foreach (ODataPathTemplate pathTemplate in pathTemplates)
                        {
                            attributeMappings.Add(pathTemplate, new WebApiActionDescriptor(controllerAction));
                        }
                    }
                }
            }

            return attributeMappings;
        }

        private static bool IsODataController(ControllerActionDescriptor controllerAction)
        {
            return typeof(ODataController).IsAssignableFrom(controllerAction.ControllerTypeInfo.AsType());
        }

        private static IEnumerable<string> GetODataRoutePrefixes(ControllerActionDescriptor controllerAction)
        {
            Contract.Assert(controllerAction != null);

            IEnumerable<ODataRoutePrefixAttribute> prefixAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false);
            if (!prefixAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (ODataRoutePrefixAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.Prefix;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix,
                            controllerAction.ControllerTypeInfo.FullName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, ControllerActionDescriptor controllerAction)
        {
            Contract.Assert(controllerAction != null);

            IEnumerable<ODataRouteAttribute> routeAttributes =
                controllerAction.MethodInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false);

            return
                routeAttributes
                    .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, controllerAction))
                    .Where(template => template != null);
        }

        private ODataPathTemplate GetODataPathTemplate(string prefix, string pathTemplate,
            ControllerActionDescriptor controllerAction)
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
                // We are NOT in a request but establishing the attribute routing convention.
                // So use the root container rather than the request container.
                IPerRouteContainer perRouteContainer = _serviceProvider.GetRequiredService<IPerRouteContainer>();
                odataPathTemplate = _odataPathTemplateHandler.ParseTemplate(pathTemplate,
                    perRouteContainer.GetODataRootContainer(_routeName));
            }
            catch (ODataException e)
            {
                throw Error.InvalidOperation(SRResources.InvalidODataRouteOnAction, pathTemplate, controllerAction.ActionName,
                    controllerAction.ControllerName, e.Message);
            }

            return odataPathTemplate;
        }
    }
}
