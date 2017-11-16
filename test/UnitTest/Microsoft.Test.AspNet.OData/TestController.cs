// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
#else
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.OData;
#endif


namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// TestController is a controller designed to be used in UnitTests to abstract the controller
    /// semantics between AspNet and AspNet core. TestController implments (and hides) the convienience
    /// methods for generating responses and surfaces those as a common type, ITestActionResult.
    /// ITestActionResult is derived from the AspNet/AspNetCore and implments the correct ActionResult
    /// interface.
    /// </summary>
    public class TestController : ODataController
    {
        [NonAction]
        public new TestNotFoundResult NotFound() { return new TestNotFoundResult(base.NotFound()); }

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(base.Ok(value)); }
#else
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
#endif
    }

    /// <summary>
    /// Wrapper for NotFoundResult
    /// </summary>
    public class TestNotFoundResult : TestActionResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for OkResult
    /// </summary>
    public class TestOkResult : TestActionResult
    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for OkObjectResult
    /// </summary>
#if NETCORE
    public class TestOkObjectResult : TestActionResult
    {
        public TestOkObjectResult(OkObjectResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestOkObjectResult<T> : TestActionResult
    {
        public TestOkObjectResult(OkNegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }
    }
#endif

/// <summary>
/// Platform-agnostic version of action result.
/// </summary>
#if NETCORE
    public interface ITestActionResult : IActionResult { }
#else
    public interface ITestActionResult : IHttpActionResult { }
#endif

    /// <summary>
    /// Wrapper for NotFoundObjectResult
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
#if NETCORE
        private IActionResult innerResult;

        public TestActionResult(IActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            return innerResult.ExecuteResultAsync(context);
        }
#else
        private IHttpActionResult innerResult;

        public TestActionResult(IHttpActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return innerResult.ExecuteAsync(cancellationToken);
        }
#endif
    }
}
