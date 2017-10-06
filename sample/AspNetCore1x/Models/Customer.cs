// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace ODataSample.Web.Models
{
    public class Customer
    {
        private List<Product> _products;

        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Contained]
        public List<Product> Products
        {
            get
            {
                return this._products ?? (this._products = new List<Product>());
            }
            set
            {
                _products = value;
            }
        }
    }
}
