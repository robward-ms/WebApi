// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// Helper function for timezones.
    /// </summary>
    public class TimeZoneInfoHelper
    {
        private static TimeZoneInfo _defaultTimeZoneInfo;

        /// <summary>
        /// Get the default timezone.
        /// </summary>
        public static TimeZoneInfo TimeZone
        {
            get
            {
                if (_defaultTimeZoneInfo == null)
                {
                    return TimeZoneInfo.Local;
                }

                return _defaultTimeZoneInfo;
            }
            set { _defaultTimeZoneInfo = value; }
        }
    }
}
