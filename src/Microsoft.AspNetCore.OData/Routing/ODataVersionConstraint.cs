// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches a specific OData protocol 
    /// version. This constraint won't match incoming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public class ODataVersionConstraint : IRouteConstraint
    {
        // The header names used for versioning in the versions 1.0 to 3.0 of the OData protocol.
        private const string PreviousODataVersionHeaderName = "DataServiceVersion";
        private const string PreviousODataMaxVersionHeaderName = "MaxDataServiceVersion";
        private const string PreviousODataMinVersionHeaderName = "MinDataServiceVersion";

        /// <summary>
        /// Creates a new instance of the <see cref="ODataVersionConstraint"/> class that will have a default version
        /// of 4.0.
        /// </summary>
        public ODataVersionConstraint()
        {
            Version = ODataVersion.V4;
            IsRelaxedMatch = true;
        }

        /// <summary>
        /// The version of the OData protocol that an OData-Version or OData-MaxVersion request header must have
        /// in order to be processed by the OData service with this route constraint.
        /// </summary>
        public ODataVersion Version { get; private set; }

        /// <summary>
        /// If set to true, allow passing in both OData V4 and previous version headers.
        /// </summary>
        public bool IsRelaxedMatch { get; set; }

        /// <inheritdoc />
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // The match behaviour depends on value of IsRelaxedMatch.
            // If users select using relaxed match logic, the header contains both V3 (or before) and V4 style version
            // will be regarded as valid. While under non-relaxed match logic, both version headers presented will be
            // regarded as invalid. The behavior for other situations are the same. When non version headers present,
            // assume using V4 version.

            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            if (routeDirection == RouteDirection.UrlGeneration)
            {
                return true;
            }

            if (!ValidateVersionHeaders(httpContext))
            {
                return false;
            }

            ODataVersion? requestVersion = GetVersion(httpContext);
            return requestVersion.HasValue && requestVersion.Value == Version;
        }

        private bool ValidateVersionHeaders(HttpContext httpContext)
        {
            bool containPreviousVersionHeaders =
                httpContext.Request.Headers.ContainsKey(PreviousODataVersionHeaderName) ||
                httpContext.Request.Headers.ContainsKey(PreviousODataMinVersionHeaderName) ||
                httpContext.Request.Headers.ContainsKey(PreviousODataMaxVersionHeaderName);
            bool containPreviousMaxVersionHeaderOnly =
                httpContext.Request.Headers.ContainsKey(PreviousODataMaxVersionHeaderName) &&
                !httpContext.Request.Headers.ContainsKey(PreviousODataVersionHeaderName) &&
                !httpContext.Request.Headers.ContainsKey(PreviousODataMinVersionHeaderName);
            bool containCurrentMaxVersionHeader = httpContext.Request.Headers.ContainsKey(HttpRequestExtensions.ODataMaxServiceVersionHeader);

            return IsRelaxedMatch
                ? !containPreviousVersionHeaders || (containCurrentMaxVersionHeader && containPreviousMaxVersionHeaderOnly)
                : !containPreviousVersionHeaders;
        }

        private ODataVersion? GetVersion(HttpContext httpContext)
        {
            // The logic is as follows. We check OData-Version first and if not present we check OData-MaxVersion.
            // If both OData-Version and OData-MaxVersion do not present, we assume the version is V4

            int versionHeaderCount = GetHeaderCount(HttpRequestExtensions.ODataServiceVersionHeader, httpContext.Request);
            int maxVersionHeaderCount = GetHeaderCount(HttpRequestExtensions.ODataMaxServiceVersionHeader, httpContext.Request);

            ODataVersion? serviceVersion = httpContext.Request.ODataServiceVersion();
            ODataVersion? maxServiceVersion = httpContext.Request.ODataMaxServiceVersion();

            if ((versionHeaderCount == 1 && serviceVersion != null))
            {
                return serviceVersion;
            }
            else if ((versionHeaderCount == 0 && maxVersionHeaderCount == 1 && maxServiceVersion != null))
            {
                return maxServiceVersion;
            }
            else if (versionHeaderCount == 0 && maxVersionHeaderCount == 0)
            {
                return Version;
            }
            else
            {
                return null;
            }
        }

        private static int GetHeaderCount(string headerName, HttpRequest request)
        {
            StringValues values;
            if (request.Headers.TryGetValue(headerName, out values))
            {
                return values.Count();
            }
            return 0;
        }
    }
}
