// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;

namespace Microsoft.Test.OData.WebApi.AspNet.TestCommon.Models
{
    public class NonGenericEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
