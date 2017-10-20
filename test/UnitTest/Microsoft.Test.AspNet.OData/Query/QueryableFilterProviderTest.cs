// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if !NETCORE1x
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
#endif
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Query
{
    public class QueryableFilterProviderTest
    {
#if !NETCORE1x
        [Theory]
        [InlineData("GetQueryable")]
        [InlineData("GetGenericQueryable")]
        [InlineData("GetSingleResult")]
        [InlineData("GetSingleResultOfT")]
        public void GetFilters_ReturnsQueryableFilter_ForQueryableActions(string actionName)
        {
            var config = RoutingConfigurationFactory.Create();;
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod(actionName));

            FilterInfo[] filters = new QueryFilterProvider(new EnableQueryAttribute()).GetFilters(config, actionDescriptor).ToArray();

            Assert.Single(filters);
            Assert.Equal(FilterScope.Global, filters[0].Scope);
            EnableQueryAttribute filter = Assert.IsType<EnableQueryAttribute>(filters[0].Instance);
        }

        [Theory]
        [InlineData("GetEnumerable")]
        [InlineData("GetGenericEnumerable")]
        [InlineData("GetArray")]
        [InlineData("GetList")]
        [InlineData("GetObject")]
        [InlineData("GetGenericQueryableWithODataQueryOption")]
        [InlineData("GetGenericQueryableWithODataQueryOptionOfT")]
        [InlineData("GetGenericQueryableWithODataQueryOption2")]
        public void GetFilters_ReturnsEmptyCollection_ForNonQueryableActions(string actionName)
        {
            var config = RoutingConfigurationFactory.Create();;
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod(actionName));

            FilterInfo[] filters = new QueryFilterProvider(new EnableQueryAttribute()).GetFilters(config, actionDescriptor).ToArray();

            Assert.Empty(filters);
        }
    }

    public class FilterProviderTestController : ODataController
    {
        public IEnumerable GetEnumerable()
        {
            return null;
        }

        public IQueryable GetQueryable()
        {
            return null;
        }

        public IEnumerable<string> GetGenericEnumerable()
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryable()
        {
            return null;
        }

        public string[] GetArray()
        {
            return null;
        }

        public List<string> GetList()
        {
            return null;
        }

        public object GetObject()
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryableWithODataQueryOption(ODataQueryOptions queryOptions)
        {
            return null;
        }

        public IQueryable GetGenericQueryableWithODataQueryOptionOfT(ODataQueryOptions<string> queryOptions)
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryableWithODataQueryOption2(string s, ODataQueryOptions queryOptions)
        {
            return null;
        }

        [EnableQuery(PageSize = 100)]
        public IQueryable GetQueryableWithFilterAttribute()
        {
            return null;
        }

#if !NETCORE1x
        public SingleResult GetSingleResult()
        {
            return null;
        }

        public SingleResult<int> GetSingleResultOfT()
        {
            return null;
        }
#endif
#endif
    }
}
