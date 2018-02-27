// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Factories;
using Moq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Batch;
using Microsoft.Test.AspNet.OData.Common;
using Moq;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Batch
{
    public class ChangeSetRequestItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            var contexts = CreateRequestContext(0);
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            Assert.Same(contexts, GetRequestContext(requestItem));
        }

        [Fact]
        public void Constructor_NullRequests_Throws()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ChangeSetRequestItem(null),
#if NETCORE
                "contexts");
#else
                "requests");
#endif
        }

        [Fact]
        public async Task SendRequestAsync_NullInvoker_Throws()
        {
            var contexts = CreateRequestContext(0);
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            await ExceptionAssert.ThrowsArgumentNullAsync(
#if NETCORE
                () => requestItem.RouteAsync(null),
                "router");
#else
                () => requestItem.SendRequestAsync(null, CancellationToken.None),
                "invoker");
#endif
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsChangeSetResponse()
        {
            int itemCount = 2;
            var requestContexts = CreateRequestContext(itemCount);
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(requestContexts);

#if NETCORE
            Mock<IRouter> router = new Mock<IRouter>();
            router.Setup(i => i.RouteAsync(It.IsAny<RouteContext>()));

            var response = await requestItem.RouteAsync(router.Object);
#else
            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    return Task.FromResult(new HttpResponseMessage());
                });

            var response = await requestItem.SendRequestAsync(invoker.Object, CancellationToken.None);
#endif

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Equal(itemCount, GetResponseContext(changesetResponse).Count());
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsSingleErrorResponse()
        {
            int itemCount = 3;
            var requestContexts = CreateRequestContext(itemCount);
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(requestContexts);

#if NETCORE
            Mock<IRouter> router = new Mock<IRouter>();
            router.Setup(i => i.RouteAsync(It.IsAny<RouteContext>()))
                .Callback((RouteContext c) =>
                {
                    if (c.HttpContext.Request.Method == "POST")
                    {
                        c.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                });

            var response = await requestItem.RouteAsync(router.Object);
#else
            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns<HttpRequestMessage, CancellationToken>((req, c) =>
                {
                    if (req.Method == HttpMethod.Post)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                    }
                    return Task.FromResult(new HttpResponseMessage());
                });

            var response = await requestItem.SendRequestAsync(invoker.Object, CancellationToken.None);
#endif

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Single(GetResponseContext(changesetResponse));
            Assert.Equal((int)HttpStatusCode.BadRequest, GetResponseStatusCode(GetResponseContext(changesetResponse).First()));
        }

#if NETFX // Only requests in AspNet as disposable.
        [Fact]
        public async Task SendRequestAsync_DisposesResponseInCaseOfException()
        {
            List<MockHttpResponseMessage> responses = new List<MockHttpResponseMessage>();
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.com"),
                new HttpRequestMessage(HttpMethod.Put, "http://example.com")
            });
            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns<HttpRequestMessage, CancellationToken>((req, cancel) =>
                {
                    if (req.Method == HttpMethod.Put)
                    {
                        throw new InvalidOperationException();
                    }
                    var response = new MockHttpResponseMessage();
                    responses.Add(response);
                    return Task.FromResult<HttpResponseMessage>(response);
                });

            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => requestItem.SendRequestAsync(invoker.Object, CancellationToken.None));

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public void GetResourcesForDisposal_ReturnsResourcesRegisteredForDispose()
        {
            var disposeObject1 = new StringContent("foo");
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            request1.RegisterForDispose(disposeObject1);
            var disposeObject2 = new StringContent("bar");
            var request2 = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            request2.RegisterForDispose(disposeObject2);

            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new HttpRequestMessage[] { request1, request2 });

            var resourcesForDisposal = requestItem.GetResourcesForDisposal();

            Assert.Equal(2, resourcesForDisposal.Count());
            Assert.Contains(disposeObject1, resourcesForDisposal);
            Assert.Contains(disposeObject2, resourcesForDisposal);
        }

        [Fact]
        public void Dispose_DisposesAllHttpRequestMessages()
        {
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new MockHttpRequestMessage[]
            {
                new MockHttpRequestMessage(),
                new MockHttpRequestMessage(),
                new MockHttpRequestMessage()
            });

            requestItem.Dispose();

            Assert.Equal(3, requestItem.Requests.Count());
            foreach (var request in requestItem.Requests)
            {
                Assert.True(((MockHttpRequestMessage)request).IsDisposed);
            }
        }
#endif

#if NETCORE
        private IEnumerable<HttpContext> CreateRequestContext(int itemCount)
        {
            HttpMethod[] methods = { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            List<HttpContext> contexts = new List<HttpContext>();
            for (int i = 0; i < itemCount; i++)
            {
                DefaultHttpContext context = new DefaultHttpContext();
                HttpRequest request = RequestFactory.Create(methods[i], "http://example.com");
                context.Request.Method = methods[i].ToString();
                context.Request.Scheme = request.Scheme;
                context.Request.Host = request.Host;
                context.Request.QueryString = request.QueryString;
                context.Request.Path = request.Path;
                contexts.Add(context);
            }

            return contexts;
        }
#else
        private IEnumerable<HttpRequestMessage> CreateRequestContext(int itemCount)
        {
            HttpMethod[] methods = { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            List<HttpRequestMessage> contexts = new List<HttpRequestMessage>();
            for (int i = 0; i < itemCount; i++)
            {
                HttpRequestMessage request = new HttpRequestMessage(methods[i], "http://example.com");
                contexts.Add(request);
            }

            return contexts;
        }
#endif

#if NETCORE
        private IEnumerable<HttpContext> GetRequestContext(ChangeSetRequestItem batchRequest)
        {
            return batchRequest.Contexts;
        }
#else
        private IEnumerable<HttpRequestMessage> GetRequestContext(ChangeSetRequestItem batchRequest)
        {
            return batchRequest.Requests;
        }
#endif

#if NETCORE
        private IEnumerable<HttpContext> GetResponseContext(ChangeSetResponseItem batchResponse)
        {
            return batchResponse.Contexts;
        }
#else
        private IEnumerable<HttpResponseMessage> GetResponseContext(ChangeSetResponseItem batchResponse)
        {
            return batchResponse.Responses;
        }
#endif

#if NETCORE
        private int GetResponseStatusCode(HttpContext context)
        {
            return context.Response.StatusCode;
        }
#else
        private int GetResponseStatusCode(HttpResponseMessage response)
        {
            return (int)response.StatusCode;
        }
#endif
    }
}