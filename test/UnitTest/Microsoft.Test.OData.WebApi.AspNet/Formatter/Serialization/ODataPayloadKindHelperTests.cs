// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.WebApi.Formatter.Serialization;
using Microsoft.Test.OData.WebApi.TestCommon;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter.Serialization
{
    public class ODataPayloadKindHelperTests : EnumHelperTestBase<ODataPayloadKind>
    {
        public ODataPayloadKindHelperTests()
            : base(ODataPayloadKindHelper.IsDefined, ODataPayloadKindHelper.Validate, (ODataPayloadKind)999)
        {
        }
    }
}
