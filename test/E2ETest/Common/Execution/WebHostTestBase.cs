// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
#else
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Owin;
using Xunit;
#endif

// Parallelism in the test framework is a feature that's new for (Xunit) version 2. However,
// since each test will spin up a number of web servers each with a listening port, disabling the
// parallel test with take a bit long but consume fewer resources with more stable results.
//
// By default, each test class is a unique test collection. Tests within the same test class will not run
// in parallel against each other. That means that there may be up to # subclasses of WebHostTestBase
// web servers running at any point during the test run, currently ~500. Without this, there would be a
// web server per test case since Xunit 2.0 spawns a new test class instance for each test case.
//
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]
//[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// The WebHostTestBase is create a web host to be used for a test.
    /// </summary>
    public abstract class WebHostTestBase : IDisposable
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";

        private int _port;
        private bool disposedValue = false;

#if NETCORE
        private IWebHost _selfHostServer = null;
#else
        private IDisposable _selfHostServer = null;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestBase"/> class
        /// which uses Katana/Kestral to host a web service.
        /// </summary>
        public WebHostTestBase()
        {
            this.Initialize();
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// An HttpClient to use with the server.
        /// </summary>
        public HttpClient Client { get; set; }

        /// <summary>
        /// A configuration method for the server.
        /// </summary>
        /// <param name="configuration"></param>
        protected abstract void UpdateConfiguration(WebRouteConfiguration configuration);

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_selfHostServer != null)
                    {
#if NETCORE
                        _selfHostServer.StopAsync().Wait();
                        _selfHostServer.WaitForShutdown();
#endif
                        _selfHostServer.Dispose();
                        _selfHostServer = null;
                    }
                }

                disposedValue = true;
            }
        }

        private bool Initialize()
        {
            int attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    // setup base address
                    // AspNet prefers the machine name
                    // AspNetCore prefers localhost.
                    _port = PortArranger.Reserve();
                    SecurityHelper.AddIpListen();
                    string baseAddress = string.Format(
                        NormalBaseAddressTemplate,
#if NETCORE
                        "localhost",
#else
                        Environment.MachineName,
#endif
                        _port.ToString());

                    this.BaseAddress = baseAddress;

                    // set up the server. If this throws an exception, it will be reported in
                    // the test output.
#if NETCORE
                    _selfHostServer = new WebHostBuilder()
                        .UseKestrel()
                        .UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Loopback, _port);
                        })
                        .UseStartup<WebHostTestStartup>()
                        .ConfigureServices(services =>
                        {
                            // Add ourself to the container so WebHostTestStartup
                            // can call UpdateCOnfiguration.
                            services.AddSingleton<WebHostTestBase>(this);
                        })
                        .Build();

                    _selfHostServer.Start();
#else
                    _selfHostServer = WebApp.Start(baseAddress, DefaultKatanaConfigure);
#endif

                    // setup client, nothing special.
                    this.Client = new HttpClient();

                    return true;
                }
                catch (HttpListenerException)
                {
                    // Retry HttpListenerException up to 3 times.
                }
            }

            throw new TimeoutException(string.Format("Unable to start server after {0} attempts", attempts));
        }

#if NETCORE
        private class WebHostTestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddMvcCore();
                services.AddOData();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseMvc(routeBuilder =>
                {
                    routeBuilder.MapRoute("api default", "api/{controller}/{action?}");

                    WebHostTestBase testBase = routeBuilder.ServiceProvider.GetRequiredService<WebHostTestBase>();
                    testBase?.UpdateConfiguration(new WebRouteConfiguration(routeBuilder));
                });
            }
        }
#else
        private void DefaultKatanaConfigure(IAppBuilder app)
        {
            // Set default principal to avoid OWIN selfhost bug with VS debugger
            app.Use(async (context, next) =>
            {
                Thread.CurrentPrincipal = null;
                await next();
            });

            var configuration = new WebRouteConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.MapHttpRoute("api default", "api/{controller}/{action}", new { action = RouteParameter.Optional });

            var httpServer = new HttpServer(configuration);
            configuration.SetHttpServer(httpServer);

            this.UpdateConfiguration(configuration);

            app.UseWebApi(httpServer: httpServer);
        }
#endif
    }
}
