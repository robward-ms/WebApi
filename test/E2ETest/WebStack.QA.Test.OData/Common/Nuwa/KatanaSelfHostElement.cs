// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using WebStack.QA.Common.WebHost;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    internal class KatanaSelfHostElement
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";
        private static readonly string DefaultRouteTemplate = "api/{controller}/{action}";

        private PortArranger _portArranger;
        private string _baseAddress;
        private string _port;
        private AppDomain _sandbox;
        private KatanaSelfHostServerInitiator _serverInitiator;

        public KatanaSelfHostElement(TestTypeDescriptor descriptor)
        {
            TypeDescriptor = descriptor;
            _portArranger = new PortArranger();
        }

        public TestTypeDescriptor TypeDescriptor { get; set; }

        /// <summary>
        /// Initialize the server.
        /// </summary>
        /// <param name="frame">The run frame on which to store state.</param>
        /// <returns>True if the server was setup; false otherwise.</returns>
        public bool Initialize()
        {
            _sandbox = CreateFullTrustAppDomain();

            // load test assembly into sandbox
            if (_sandbox != null && TypeDescriptor.TestAssembly != null)
            {
                _sandbox.Load(TypeDescriptor.TestAssembly.Name);
            }

            // setup base address
            _port = _portArranger.Reserve();
            SecurityHelper.AddIpListen();
            _baseAddress = string.Format(NormalBaseAddressTemplate, Environment.MachineName, _port);

            // create initiator in the sandbox
            if (_sandbox != null)
            {
                _serverInitiator = _sandbox.CreateInstanceAndUnwrap(
                    typeof(KatanaSelfHostServerInitiator).Assembly.FullName,
                    typeof(KatanaSelfHostServerInitiator).FullName)
                    as KatanaSelfHostServerInitiator;
            }
            else
            {
                _serverInitiator = new KatanaSelfHostServerInitiator();
            }
            try
            {
                // set up the server
                _serverInitiator.Setup(
                    _baseAddress,
                    TypeDescriptor.ConfigureMethod?.ToRuntimeMethod(),
                    DefaultRouteTemplate);
            }
            catch (Exception ex)
            {
                EventLog appLog = new System.Diagnostics.EventLog();
                appLog.Source = "Nuwa Katana Self Host Test";
                appLog.WriteEntry(string.Format("base address: {0}\n message: {1}\n stack trace: {2}\n", _baseAddress, ex.Message, ex.StackTrace),
                    EventLogEntryType.Error);
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        /// <param name="frame"></param>
        public void Cleanup()
        {
            if (_serverInitiator != null)
            {
                _serverInitiator.Dispose();
                _serverInitiator = null;
            }

            if (_sandbox != null)
            {
                AppDomain.Unload(_sandbox);
                _sandbox = null;
            }
        }

        /// <summary>
        /// Assign base address uri to the property marked by <paramref name="NuwaBaseAddressAttribute"/>
        /// assuming that there are not more than 1 property is marked by the attribute
        /// </summary>
        public void SetBaseAddress(Type testClassType, object testClassInstance)
        {
            var baseAddressPrpt = testClassType.GetProperties()
                .Where(prop => { return prop.GetCustomAttributes(typeof(NuwaBaseAddressAttribute), false).Length == 1; })
                .FirstOrDefault();

            if (baseAddressPrpt != null && NuwaBaseAddressAttribute.Verify(baseAddressPrpt))
            {
                baseAddressPrpt.SetValue(testClassInstance, _baseAddress, null);
            }
        }

        private static AppDomain CreateFullTrustAppDomain()
        {
            var retval = AppDomain.CreateDomain(
                "Full Trust Sandbox", null,
                new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory });

            return retval;
        }

        private class KatanaSelfHostServerInitiator : MarshalByRefObject, IDisposable
        {
            private IDisposable _katanaSelfHostServer = null;
            private string _defaultRouteTemplate;
            private MethodInfo _httpConfigure;

            public void Setup(string baseAddress, MethodInfo httpConfigure, string defaultRouteTemplate)
            {
                _httpConfigure = httpConfigure;
                _defaultRouteTemplate = defaultRouteTemplate;

                _katanaSelfHostServer = WebApp.Start(baseAddress, DefaultKatanaConfigure);
            }

            public void Dispose()
            {
                if (_katanaSelfHostServer != null)
                {
                    _katanaSelfHostServer.Dispose();
                }
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

                // default map
                configuration.Routes.MapHttpRoute(
                    "api default", _defaultRouteTemplate, new { action = RouteParameter.Optional });

                var httpServer = new HttpServer(configuration);
                configuration.SetHttpServer(httpServer);

                if (_httpConfigure != null)
                {
                    _httpConfigure.Invoke(null, new object[] { configuration });
                }

                app.UseWebApi(httpServer: httpServer);
            }
        }
    }
}
