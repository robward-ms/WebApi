﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public class ODataPathRouteConstraint : IRouteConstraint
    {
        // "%2F"
        private static readonly string _escapedSlash = Uri.EscapeDataString("/");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        public ODataPathRouteConstraint(string routeName)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            RouteName = routeName;
        }

        /// <summary>
        /// Gets the name of the route this constraint is associated with.
        /// </summary>
        public string RouteName
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="routeKey">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            if (routeDirection == RouteDirection.IncomingRequest)
            {
                object oDataPathValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out oDataPathValue))
                {
                    string oDataPathString = oDataPathValue as string;
                    ODataPath path;

                    try
                    {
                        // Service root is the current RequestUri, less the query string and the ODataPath (always the
                        // last portion of the absolute path).  ODL expects an escaped service root and other service
                        // root calculations are calculated using AbsoluteUri (also escaped).  But routing exclusively
                        // uses unescaped strings, determined using
                        //    address.GetComponents(UriComponents.Path, UriFormat.Unescaped)
                        //
                        // For example if the AbsoluteUri is
                        // <http://localhost/odata/FunctionCall(p0='Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars')>, the
                        // oDataPathString will contain "FunctionCall(p0='Chinese西雅图Chars')".
                        //
                        // Due to this decoding and the possibility of unecessarily-escaped characters, there's no
                        // reliable way to determine the original string from which oDataPathString was derived.
                        // Therefore a straightforward string comparison won't always work.  See RemoveODataPath() for
                        // details of chosen approach.
                        HttpRequest request = httpContext.Request;

                        StringBuilder requestLeftPartBuilder = new StringBuilder(request.Scheme);
                        requestLeftPartBuilder.Append("://");
                        requestLeftPartBuilder.Append(request.Host);
                        requestLeftPartBuilder.Append("/");
                        requestLeftPartBuilder.Append(request.Path);
                        string requestLeftPart = requestLeftPartBuilder.ToString();
                        string serviceRoot = requestLeftPart;

                        if (!String.IsNullOrEmpty(oDataPathString))
                        {
                            serviceRoot = RemoveODataPath(serviceRoot, oDataPathString);
                        }

                        // As mentioned above, we also need escaped ODataPath.
                        // The requestLeftPart and request.QueryString are both escaped.
                        // The ODataPath for service documents is empty.
                        string oDataPathAndQuery = requestLeftPart.Substring(serviceRoot.Length);

                        if (request.QueryString.HasValue)
                        {
                            // Ensure path handler receives the query string as well as the path.
                            oDataPathAndQuery += request.QueryString.ToString();
                        }

                        // Leave an escaped '/' out of the service route because DefaultODataPathHandler will add a
                        // literal '/' to the end of this string if not already present. That would double the slash
                        // in response links and potentially lead to later 404s.
                        if (serviceRoot.EndsWith(_escapedSlash, StringComparison.OrdinalIgnoreCase))
                        {
                            serviceRoot = serviceRoot.Substring(0, serviceRoot.Length - _escapedSlash.Length);
                        }

                        //IServiceProvider requestContainer = request.CreateRequestContainer(RouteName);
                        IServiceProvider requestContainer = httpContext.RequestServices;
                        IODataPathHandler pathHandler = requestContainer.GetRequiredService<IODataPathHandler>();
                        path = pathHandler.Parse(serviceRoot, oDataPathAndQuery, requestContainer);
                    }
                    catch (ODataException)
                    {
                        path = null;
                    }

                    if (path != null)
                    {
                        // Set all the properties we need for routing, querying, formatting
                        IODataFeature odataFeature = httpContext.ODataFeature();
                        odataFeature.Path = path;
                        odataFeature.RouteName = RouteName;
                        return true;
                    }
                }

                // The request doesn't match this route.
                return false;
            }
            else
            {
                // This constraint only applies to incomming request.
                return true;
            }
        }

        /// <summary>
        /// Gets the service root from a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The service root from a request.</returns>
        private string GetServiceRoot(HttpRequest request)
        {
            StringBuilder sb = new StringBuilder(request.Scheme);
            sb.Append("://")
                .Append(request.Host);

            if (!String.IsNullOrEmpty(this.RouteName))
            {
                sb.Append("/" + this.RouteName);
            }

            sb.Append("/");

            return sb.ToString();
        }

        // Find the substring of the given URI string before the given ODataPath.  Tests rely on the following:
        // 1. ODataPath comes at the end of the processed Path
        // 2. Virtual path root, if any, comes at the beginning of the Path and a '/' separates it from the rest
        // 3. OData prefix, if any, comes between the virtual path root and the ODataPath and '/' characters separate
        //    it from the rest
        // 4. Even in the case of Unicode character corrections, the only differences between the escaped Path and the
        //    unescaped string used for routing are %-escape sequences which may be present in the Path
        //
        // Therefore, look for the '/' character at which to lop off the ODataPath.  Can't just unescape the given
        // uriString because subsequent comparisons would only help to check wehther a match is _possible_, not where
        // to do the lopping.
        private static string RemoveODataPath(string uriString, string oDataPathString)
        {
            // Potential index of oDataPathString within uriString.
            int endIndex = uriString.Length - oDataPathString.Length - 1;
            if (endIndex <= 0)
            {
                // Bizarre: oDataPathString is longer than uriString.  Likely the values collection passed to Match()
                // is corrupt.
                throw Error.InvalidOperation(SRResources.RequestUriTooShortForODataPath, uriString, oDataPathString);
            }

            string startString = uriString.Substring(0, endIndex + 1);  // Potential return value.
            string endString = uriString.Substring(endIndex + 1);       // Potential oDataPathString match.
            if (String.Equals(endString, oDataPathString, StringComparison.Ordinal))
            {
                // Simple case, no escaping in the ODataPathString portion of the Path.  In this case, don't do extra
                // work to look for trailing '/' in startString.
                return startString;
            }

            while (true)
            {
                // Escaped '/' is a derivative case but certainly possible.
                int slashIndex = startString.LastIndexOf('/', endIndex - 1);
                int escapedSlashIndex =
                    startString.LastIndexOf(_escapedSlash, endIndex - 1, StringComparison.OrdinalIgnoreCase);
                if (slashIndex > escapedSlashIndex)
                {
                    endIndex = slashIndex;
                }
                else if (escapedSlashIndex >= 0)
                {
                    // Include the escaped '/' (three characters) in the potential return value.
                    endIndex = escapedSlashIndex + 2;
                }
                else
                {
                    // Failure, unable to find the expected '/' or escaped '/' separator.
                    throw Error.InvalidOperation(SRResources.ODataPathNotFound, uriString, oDataPathString);
                }

                startString = uriString.Substring(0, endIndex + 1);
                endString = uriString.Substring(endIndex + 1);

                // Compare unescaped strings to avoid both arbitrary escaping and use of lowercase 'a' through 'f' in
                // %-escape sequences.
                endString = Uri.UnescapeDataString(endString);
                if (String.Equals(endString, oDataPathString, StringComparison.Ordinal))
                {
                    return startString;
                }

                if (endIndex == 0)
                {
                    // Failure, could not match oDataPathString after an initial '/' or escaped '/'.
                    throw Error.InvalidOperation(SRResources.ODataPathNotFound, uriString, oDataPathString);
                }
            }
        }
    }
}