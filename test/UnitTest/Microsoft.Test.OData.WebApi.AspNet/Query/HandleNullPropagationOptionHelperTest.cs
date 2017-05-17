// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi.Query;
using Microsoft.Test.OData.WebApi.TestCommon;

namespace Microsoft.Test.OData.WebApi.AspNet.Query
{
    public class HandleNullPropagationOptionHelperTest : EnumHelperTestBase<HandleNullPropagationOption>
    {
        public HandleNullPropagationOptionHelperTest()
            : base(HandleNullPropagationOptionHelper.IsDefined, HandleNullPropagationOptionHelper.Validate, (HandleNullPropagationOption)999)
        {
        }
    }
}
