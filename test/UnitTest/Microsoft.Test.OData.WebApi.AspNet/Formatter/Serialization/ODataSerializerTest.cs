// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData;
using Microsoft.OData.WebApi.Formatter.Serialization;
using Microsoft.Test.OData.WebApi.TestCommon;
using Moq;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter.Serialization
{
    public class ODataSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported).Object;

            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_NotSupported()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported) { CallBase = true }.Object;

            Assert.Throws<NotSupportedException>(
                () => serializer.WriteObject(graph: null, type: typeof(int),messageWriter: null, writeContext: null),
                "ODataSerializerProxy does not support WriteObject.");
        }
    }
}
