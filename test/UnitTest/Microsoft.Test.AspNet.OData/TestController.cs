// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
#else
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
#endif

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// NonODataController is an abstracted ASP.NET [Core] controller.
    /// </summary>
#if NETCORE
    public class NonODataController : Controller
#else
    public class NonODataController : ApiController
#endif
    {
    }

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
        public new TestStatusCodeResult StatusCode(HttpStatusCode status) { return new TestStatusCodeResult(base.StatusCode(status)); }

        [NonAction]
        public new TestNotFoundResult NotFound() { return new TestNotFoundResult(base.NotFound()); }

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }
#else
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
#endif

        [NonAction]
        public new TestCreatedODataResult<T> Created<T>(T entity) { return new TestCreatedODataResult<T>(entity, this); }

        [NonAction]
        public TestCreatedODataResult<object> Created(string uri, object entity) { return new TestCreatedODataResult<object>(uri, entity, this); }

        [NonAction]
        public new TestUpdatedODataResult<T> Updated<T>(T entity) { return new TestUpdatedODataResult<T>(entity, this); }
    }


    /// <summary>
    /// Wrapper for StatusCodeResult
    /// </summary>
    public class TestStatusCodeResult : TestActionResult
    {
        public TestStatusCodeResult(StatusCodeResult innerResult)
            : base(innerResult)
        {
        }
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
    /// Wrapper for CreatedODataResult
    /// </summary>
#if NETCORE
    public class TestCreatedODataResult<T> : CreatedODataResult<T>, ITestActionResult
    {
        public TestCreatedODataResult(T entity)
            : base(entity)
        {
        }

        public TestCreatedODataResult(string uri, T entity)
            : base(entity)
        {
#if !NETCORE
#endif
        }
    }
#else
    public class TestCreatedODataResult<T> : CreatedODataResult<T>, ITestActionResult
    {
        public TestCreatedODataResult(T entity, ApiController controller)
            : base(entity, controller)
        {
        }

        public TestCreatedODataResult(string uri, T entity, ApiController controller)
            : base(entity, controller)
        {
#if !NETCORE
#endif
        }
    }
#endif

    /// <summary>
    /// Wrapper for UpdatedODataResult
    /// </summary>
#if NETCORE
    public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, ITestActionResult
    {
        public TestUpdatedODataResult(T entity)
            : base(entity)
        {
        }
    }
#else
    public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, ITestActionResult
    {
        public TestUpdatedODataResult(T entity, ApiController controller)
            : base(entity, controller)
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
    /// Platform-agnostic version of IActionHttpMethodProviders.
    /// </summary>
    public class HttpDeleteAttribute : Microsoft.AspNetCore.Mvc.HttpDeleteAttribute { }
    public class HttpGetAttribute : Microsoft.AspNetCore.Mvc.HttpGetAttribute { }
    public class HttpPatchAttribute : Microsoft.AspNetCore.Mvc.HttpPatchAttribute { }
    public class HttpPostAttribute : Microsoft.AspNetCore.Mvc.HttpPostAttribute { }
    public class HttpPutAttribute : Microsoft.AspNetCore.Mvc.HttpPutAttribute { }
    public class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private int? _order;
        public AcceptVerbsAttribute(params string[] methods)
        {
            HttpMethods = methods.Select(method => method.ToUpperInvariant());
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public string Route { get; set; }

        /// <inheritdoc />
        string IRouteTemplateProvider.Template => Route;

        /// <inheritdoc />
        public int Order
        {
            get { return _order ?? 0; }
            set { _order = value; }
        }

        /// <inheritdoc />
        int? IRouteTemplateProvider.Order => _order;

        /// <inheritdoc />
        public string Name { get; set; }
    }
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

    /// <summary>
    /// Platform-agnostic version of formatting attributes.
    /// </summary>
#if NETCORE
    public class FromBodyAttribute : Microsoft.AspNetCore.Mvc.FromBodyAttribute { }
#endif
}
