// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    public static class SecurityHelper
    {
        public static void AddIpListen()
        {
            Run(string.Format(@"netsh http add iplisten ipaddress=::"));
        }

        private static void Run(string command)
        {
            var process = Process.Start(new ProcessStartInfo("cmd", "/c " + command)
            {
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            Console.WriteLine(command);
            Console.Write(process.StandardOutput.ReadToEnd());
            Console.Write(process.StandardError.ReadToEnd());
        }
    }
}
