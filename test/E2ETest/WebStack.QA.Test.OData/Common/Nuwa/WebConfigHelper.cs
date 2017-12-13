// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Xml.Linq;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Represents the a web.config file content in xml
    /// </summary>
    public class WebConfigHelper
    {
        private XElement _configElement;

        private WebConfigHelper()
            : this("<configuration/>")
        {
        }

        private WebConfigHelper(string webConfig)
        {
            _configElement = XElement.Parse(webConfig);
        }

        public WebConfigHelper AddAppSection(string key, string value)
        {
            return this;
        }
    }
}