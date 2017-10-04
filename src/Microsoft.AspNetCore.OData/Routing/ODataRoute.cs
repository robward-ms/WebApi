// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public class ODataRoute : Route
    {
        private static readonly string _escapedHashMark = Uri.EscapeDataString("#");
        private static readonly string _escapedQuestionMark = Uri.EscapeDataString("?");

        private bool _canGenerateDirectLink;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        public ODataRoute(IRouter target, string routePrefix, ODataPathRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : this(target, routePrefix, (IRouteConstraint)routeConstraint, resolver)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        public ODataRoute(IRouter target, string routePrefix, IRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : base(target, GetRouteTemplate(routePrefix), inlineConstraintResolver: resolver)
        {
            RoutePrefix = routePrefix;
            PathRouteConstraint = routeConstraint as ODataPathRouteConstraint;
            RouteConstraint = routeConstraint;

            // We can only use our fast-path for link generation if there are no open brackets in the route prefix
            // that need to be replaced. If there are, fall back to the slow path.
            _canGenerateDirectLink = routePrefix == null || routePrefix.IndexOf('{') == -1;

            if (routeConstraint != null)
            {
                Constraints.Add(ODataRouteConstants.ConstraintName, routeConstraint);
            }

            Constraints.Add(ODataRouteConstants.VersionConstraintName, new ODataVersionConstraint());
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public string RoutePrefix { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataPathRouteConstraint"/> on this route.
        /// </summary>
        public ODataPathRouteConstraint PathRouteConstraint { get; private set; }

        /// <summary>
        /// Gets the <see cref="IRouteConstraint"/> on this route.
        /// </summary>
        public IRouteConstraint RouteConstraint { get; private set; }

        internal bool CanGenerateDirectLink
        {
            get
            {
                return _canGenerateDirectLink;
            }
        }

        /// <inheritdoc />
        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            // Only perform URL generation if the "httproute" key was specified. This allows these
            // routes to be ignored when a regular MVC app tries to generate URLs. Without this special
            // key an HTTP route used for Web API would normally take over almost all the routes in a
            // typical app.
            
            if (context.Values != null && context.Values.Keys.Contains("TODO" /*Route.HttpRouteKey*/, StringComparer.OrdinalIgnoreCase))
            {
                // Fast path link generation where we recognize an OData route of the form "prefix/{*odataPath}".
                // Link generation using HttpRoute.GetVirtualPath can consume up to 30% of processor time
                object odataPathValue;
                if (context.Values.TryGetValue(ODataRouteConstants.ODataPath, out odataPathValue))
                {
                    string odataPath = odataPathValue as string;
                    if (odataPath != null)
                    {
                        // Try to generate an optimized direct link
                        // Otherwise, fall back to the base implementation
                        return _canGenerateDirectLink
                            ? GenerateLinkDirectly(odataPath)
                            : base.GetVirtualPath(context);
                    }
                }
            }

            return null;
        }

        internal VirtualPathData GenerateLinkDirectly(string odataPath)
        {
            Contract.Assert(odataPath != null);
            Contract.Assert(_canGenerateDirectLink);

            string link = CombinePathSegments(RoutePrefix, odataPath);
            link = UriEncode(link);
            return new VirtualPathData(this, link);
        }

        private static string GetRouteTemplate(string prefix)
        {
            return String.IsNullOrEmpty(prefix) ?
                ODataRouteConstants.ODataPathTemplate :
                prefix + '/' + ODataRouteConstants.ODataPathTemplate;
        }

        private static string CombinePathSegments(string routePrefix, string odataPath)
        {
            if (String.IsNullOrEmpty(routePrefix))
            {
                return odataPath;
            }
            else
            {
                return String.IsNullOrEmpty(odataPath) ? routePrefix : routePrefix + '/' + odataPath;
            }
        }

        private static string UriEncode(string str)
        {
            Contract.Assert(str != null);

            string escape = Uri.EscapeUriString(str);
            escape = escape.Replace("#", _escapedHashMark);
            escape = escape.Replace("?", _escapedQuestionMark);
            return escape;
        }
    }
}