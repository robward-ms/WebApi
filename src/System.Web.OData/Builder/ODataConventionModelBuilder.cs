// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http.Dispatcher;
using System.Web.OData.Adapters;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// <see cref="ODataConventionModelBuilder"/> is used to automatically map CLR classes to an EDM model based on a set of.
    /// </summary>
    public class ODataConventionModelBuilder : Microsoft.OData.WebApi.Builder.ODataConventionModelBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        public ODataConventionModelBuilder()
            : base(new WebApiAssembliesResolver(new DefaultAssembliesResolver()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="resolver">The <see cref="IAssembliesResolver"/> to use.</param>
        public ODataConventionModelBuilder(IAssembliesResolver resolver)
            : base(new WebApiAssembliesResolver(resolver))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="resolver">The <see cref="IAssembliesResolver"/> to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        /// <remarks>The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended for use by unit testing only.</remarks>
        public ODataConventionModelBuilder(IAssembliesResolver resolver, bool isQueryCompositionMode)
            : base(new WebApiAssembliesResolver(resolver), isQueryCompositionMode)
        {
        }
    }
}
