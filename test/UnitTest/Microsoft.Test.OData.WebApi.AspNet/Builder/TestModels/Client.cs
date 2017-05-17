// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Test.OData.WebApi.AspNet.Builder.TestModels;

namespace Microsoft.Test.OData.WebApi.AspNet.Builder.TestModelss
{
    public class Client
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<MyOrder> MyOrders { get; set; }
    }
}
