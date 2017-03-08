// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for querying an action map.
    /// </summary>
    internal static class ActionMapExtensions
    {
        public static string FindMatchingAction(this IWebApiActionMatch actionMatch, params string[] targetActionNames)
        {
            foreach (string targetActionName in targetActionNames)
            {
                if (actionMatch.Contains(targetActionName))
                {
                    return targetActionName;
                }
            }

            return null;
        }
    }
}