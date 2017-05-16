// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.WebApi;

namespace Microsoft.Test.OData.WebApi.AspNet.Query.Controllers
{
    public class PrimitiveController : ODataController
    {
        [EnableQuery]
        public IQueryable<int> Get()
        {
            return GetIEnumerableOfInt().AsQueryable<int>();
        }

        [EnableQuery]
        public IEnumerable<int> GetIEnumerableOfInt()
        {
            return new List<int>() { 1, 2, 3, 4, 5 };
        }
    }
}
