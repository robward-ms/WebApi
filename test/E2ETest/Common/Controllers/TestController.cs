// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#else
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using Microsoft.AspNet.OData;
#endif

namespace Microsoft.Test.E2E.AspNetCore.OData.Common.Controllers
{
    /// <summary>
    /// TestController is a controller designed to be used in UnitTests to abstract the controller
    /// semantics between AspNet and AspNet core. TestController implements (and hides) the convenience
    /// methods for generating responses and surfaces those as a common type, ITestActionResult.
    /// ITestActionResult is derived from the AspNet/AspNetCore and implements the correct ActionResult
    /// interface.
    /// </summary>
    public class TestController : ODataController
    {
        [NonAction]
        public new TestNotFoundResult NotFound() { return new TestNotFoundResult(base.NotFound()); }

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
        public new TestBadRequestResult BadRequest() { return new TestBadRequestResult(base.BadRequest()); }


        [NonAction]
#if NETCORE
        public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState) { return new TestBadRequestObjectResult(base.BadRequest(modelState)); }
#else
        public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState) { return new TestBadRequestObjectResult(base.BadRequest(modelState)); }
#endif

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }
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
    public class TestOkObjectResult : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
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
    /// Wrapper for BadRequestResult
    /// </summary>
    public class TestBadRequestResult : TestActionResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for BadRequestObjectResult
    /// </summary>
#if NETCORE
    public class TestBadRequestObjectResult : TestObjectResult
    {
        public TestBadRequestObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 400;
        }
    }
#else
    public class TestBadRequestObjectResult : TestActionResult
    {
        public TestBadRequestObjectResult(InvalidModelStateResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

#if NETCORE
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    public interface ITestActionResult : IActionResult { }

    /// <summary>
    /// Wrapper for platform-agnostic version of action result.
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
        private IActionResult innerResult;

        public TestActionResult(IActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            return innerResult.ExecuteResultAsync(context);
        }
    }

    /// <summary>
    /// Wrapper for platform-agnostic version of object result.
    /// </summary>
    public class TestObjectResult : ObjectResult, ITestActionResult
    {
        public TestObjectResult(object innerResult)
            : base(innerResult)
        {
        }
    }
#else
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    public interface ITestActionResult : IHttpActionResult { }

    /// <summary>
    /// Wrapper for platform-agnostic version of action result.
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
        private IHttpActionResult innerResult;

        public TestActionResult(IHttpActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return innerResult.ExecuteAsync(cancellationToken);
        }

    }
#endif

    /// <summary>
    /// Platform-agnostic version of HttpMethod attributes. AspNetCore attributes are not sealed
    /// so they are used as a base class. AspNet has sealed attributes so the code is copied.
    /// </summary>
#if NETCORE
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    public class HttpGetAttribute : Microsoft.AspNetCore.Mvc.HttpGetAttribute { }
    public class HttpPatchAttribute : Microsoft.AspNetCore.Mvc.HttpPatchAttribute { }
    public class HttpPostAttribute : Microsoft.AspNetCore.Mvc.HttpPostAttribute { }
#else
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpGetAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Get }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPatchAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { new HttpMethod("PATCH") }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Post }); }
        }
    }
#endif
}