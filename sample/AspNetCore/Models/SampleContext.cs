// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ODataSample.Web.Models
{
    public class SampleContext
    {
        private readonly List<Product> _products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Apple",  Price = 10 },
            new Product { ProductId = 2, Name = "Orange", Price = 20 },
            new Product { ProductId = 3, Name = "Banana", Price = 30 },
            new Product { ProductId = 4, Name = "Cherry", Price = 40 },

        };

        private readonly List<Customer> _customers = new List<Customer>
        {
            new Customer { CustomerId = 1, FirstName = "Mark", LastName = "Stand" },
            new Customer { CustomerId = 2, FirstName = "Peter", LastName = "Huward" },
            new Customer { CustomerId = 3, FirstName = "Sam", LastName = "Xu" }
        };

        public SampleContext()
        {
            _customers[0].Products = new List<Product>
            {
                _products[0],
                _products[1]
            };

            _customers[1].Products = new List<Product>
            {
                _products[2],
                _products[3]
            };

            _customers[2].Products = new List<Product>
            {
                _products[1],
                _products[2]
            };
        }

        public IEnumerable<Product> Products => _products;

        public IEnumerable<Customer> Customers => _customers;

        public IEnumerable<Customer> FindCustomersWithProduct(int productId)
        {
            return _customers.Where(c => c.Products.FirstOrDefault(p => p.ProductId == productId) != null);
        }

        public Product FindProduct(int id)
        {
            return _products.SingleOrDefault(p => p.ProductId == id);
        }

        public Customer FindCustomer(int id)
        {
            return _customers.SingleOrDefault(p => p.CustomerId == id);
        }
    }
}
