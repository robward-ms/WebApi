// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class ClrPropertyInfoAnnotationTest
    {
        [Fact]
        public void Ctor_ThrowsForNullPropertyInfo()
        {
            Assert.ThrowsArgumentNull(
                () => new ClrPropertyInfoAnnotation(clrPropertyInfo: null),
                "clrPropertyInfo");
        }
    }
}
