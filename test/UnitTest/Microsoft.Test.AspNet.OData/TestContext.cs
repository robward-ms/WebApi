// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// A context used with <see cref="TestContextBuilder"/> and <see cref="TestAspNetCoreServer"/>.
    /// </summary>
    public struct TestContext
    {
        public HttpContext HttpContext { get; set; }
        public IDisposable Scope { get; set; }
        public long StartTimestamp { get; set; }
        public bool EventLogEnabled { get; set; }
        public Activity Activity { get; set; }
    }
}
#endif
