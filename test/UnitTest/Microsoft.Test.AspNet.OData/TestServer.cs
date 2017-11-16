// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// An implmentation of IServer design to allow request from an HttpClient to be
    /// passed to an AspNetCore implmentation for processing.
    /// </summary>
    public class TestAspNetCoreServer : IServer
    {
        private const string ServerName = nameof(TestAspNetCoreServer);
        private IWebHost _hostInstance;
        private bool _disposed = false;
        private IHttpApplication<TestContext> _application;

        public TestAspNetCoreServer(IWebHostBuilder builder)
            : this(builder, new FeatureCollection())
        {
        }

        public TestAspNetCoreServer(IWebHostBuilder builder, IFeatureCollection featureCollection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (featureCollection == null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            Features = featureCollection;

            var host = builder.UseServer(this).Build();
            host.StartAsync().GetAwaiter().GetResult();
            _hostInstance = host;
        }

        public Uri BaseAddress { get; set; } = new Uri("http://localhost/");

        public IWebHost Host
        {
            get
            {
                return _hostInstance;
            }
        }

        public IFeatureCollection Features { get; }

        public HttpMessageHandler CreateHandler()
        {
            var pathBase = BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(BaseAddress);
            return new ClientHandler(pathBase, _application);
        }

        public HttpClient CreateClient()
        {
            return new HttpClient(CreateHandler()) { BaseAddress = BaseAddress };
        }

        /// <summary>
        /// Creates, configures, sends, and returns a <see cref="HttpContext"/>. This completes as soon as the response is started.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpContext> SendAsync(Action<HttpContext> configureContext, CancellationToken cancellationToken)
        {
            if (configureContext == null)
            {
                throw new ArgumentNullException(nameof(configureContext));
            }

            // ContextBuilder: https://github.com/aspnet/Hosting/blob/05fd382b93cb51668d1d8858e79292601ca71b18/src/Microsoft.AspNetCore.TestHost/HttpContextBuilder.cs
            var builder = new TestContextBuilder(_application);
            builder.Configure(context =>
            {
                var request = context.Request;
                request.Scheme = BaseAddress.Scheme;
                request.Host = HostString.FromUriComponent(BaseAddress);
                if (BaseAddress.IsDefaultPort)
                {
                    request.Host = new HostString(request.Host.Host);
                }
                var pathBase = PathString.FromUriComponent(BaseAddress);
                if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
                {
                    pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
                }
                request.PathBase = pathBase;
            });
            builder.Configure(configureContext);
            return await builder.SendAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _hostInstance.Dispose();
            }
        }

        Task IServer.StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            _application = new ApplicationWrapper<TestContext>((IHttpApplication<TestContext>)application, () =>
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            });

            return Task.CompletedTask;
        }

        Task IServer.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private class ApplicationWrapper<TContext> : IHttpApplication<TContext>
        {
            private readonly IHttpApplication<TContext> _application;
            private readonly Action _preProcessRequestAsync;

            public ApplicationWrapper(IHttpApplication<TContext> application, Action preProcessRequestAsync)
            {
                _application = application;
                _preProcessRequestAsync = preProcessRequestAsync;
            }

            public TContext CreateContext(IFeatureCollection contextFeatures)
            {
                return _application.CreateContext(contextFeatures);
            }

            public void DisposeContext(TContext context, Exception exception)
            {
                _application.DisposeContext(context, exception);
            }

            public Task ProcessRequestAsync(TContext context)
            {
                _preProcessRequestAsync();
                return _application.ProcessRequestAsync(context);
            }
        }
    }
}
#endif
