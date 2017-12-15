// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Test.E2E.AspNetCore.OData.Common;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies
{
    public class ProductsContext : TestDbContext
    {
        public ProductsContext()
            : base("Products")
        {
        }

        public TestDbSet<Product> Products { get; set; }

        public TestDbSet<ProductFamily> ProductFamilies { get; set; }

        public TestDbSet<Supplier> Suppliers { get; set; }

#if !NETCORE
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<ProductFamily>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Supplier>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Supplier>().Property(p => p.CountryOrRegion).HasColumnName("CountryOrRegion");
            base.OnModelCreating(modelBuilder);
        }
#endif
    }
}
