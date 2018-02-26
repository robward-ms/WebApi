// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if EFCORE
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
#else
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class AggregationContext : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=True;Database=AggregationTest";

#if EFCORE
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Database=AggregationTest-EFCore");
        }
#else
        public AggregationContext()
            : base(ConnectionString)
        {
        }
#endif

        public DbSet<Customer> Customers { get; set; }
    }

    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public Order Order { get; set; }

        public Address Address { get; set; }
    }

    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Street { get; set; }
    }
}
