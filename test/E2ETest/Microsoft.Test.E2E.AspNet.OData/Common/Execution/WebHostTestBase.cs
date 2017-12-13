// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Owin;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// The WebHostTestBase is create a web host to be used for a test.
    /// </summary>
    public abstract class WebHostTestBase : IDisposable
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";
        private static readonly string DefaultRouteTemplate = "api/{controller}/{action}";

        private PortArranger _portArranger = new PortArranger();
        private string _port;
        private IDisposable _katanaSelfHostServer = null;
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestBase"/> class
        /// which uses Katana to host a web service.
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
        protected abstract void UpdateConfiguration(HttpConfiguration configuration);

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
                    if (_katanaSelfHostServer != null)
                    {
                        _katanaSelfHostServer.Dispose();
                        _katanaSelfHostServer = null;
                    }

                    _portArranger.Return(_port);
                }

                disposedValue = true;
            }
        }

        private bool Initialize()
        {
            // setup base address
            _port = _portArranger.Reserve();
            SecurityHelper.AddIpListen();
            string baseAddress = string.Format(NormalBaseAddressTemplate, Environment.MachineName, _port);
            this.BaseAddress = baseAddress.Replace("localhost", Environment.MachineName);

            try
            {
                // set up the server
                WebApp.Start(baseAddress, DefaultKatanaConfigure);

                // setup client
                this.Client = new HttpClient();
            }
            catch (Exception ex)
            {
                EventLog appLog = new System.Diagnostics.EventLog();
                appLog.Source = "OData WebApi Katana Self Host Test";
                appLog.WriteEntry(string.Format("base address: {0}\n message: {1}\n stack trace: {2}\n", this.BaseAddress, ex.Message, ex.StackTrace),
                    EventLogEntryType.Error);
                throw ex;
            }

            return true;
        }

        private void DefaultKatanaConfigure(IAppBuilder app)
        {
            // Set default principal to avoid OWIN selfhost bug with VS debugger
            app.Use(async (context, next) =>
            {
                Thread.CurrentPrincipal = null;
                await next();
            });

            var configuration = new HttpConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.MapHttpRoute("api default", DefaultRouteTemplate, new { action = RouteParameter.Optional });

            var httpServer = new HttpServer(configuration);
            configuration.SetHttpServer(httpServer);

            this.UpdateConfiguration(configuration);

            app.UseWebApi(httpServer: httpServer);
        }
    }
}
