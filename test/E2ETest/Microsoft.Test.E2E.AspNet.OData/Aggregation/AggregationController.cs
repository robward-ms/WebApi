// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE && EFCORE
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#elif NETCORE
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#else
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class BaseCustomersController : TestController
    {
        protected readonly AggregationContext _db = new AggregationContext();

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i % 2,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i % 2,
                        Price = i * 100
                    },
                    Address = new Address
                    {
                        Name = "City" + i % 2,
                        Street = "Street" + i % 2,
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.Customers.Add(new Customer()
            {
                Id = 10,
                Name = null,
                Address = new Address
                {
                    Name = "City1",
                    Street = "Street",
                },
                Order = new Order
                {
                    Id = 10,
                    Name = "Order" + 10 % 2,
                    Price = 0
                },
            });

            _db.SaveChanges();
        }

        protected void ResetDataSource()
        {
#if EFCORE
            _db.Database.EnsureCreated();
#endif
            if (!_db.Customers.Any())
            {
                Generate();
            }
        }
    }

    public class CustomersController : BaseCustomersController
    {
        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
#if EFCORE
            // EFCore does not yet support lazy loading, making this scenario
            // difficult to achieve.
            // See: https://docs.microsoft.com/en-us/ef/core/querying/related-data
            //
            // When returning just Customers, there are no Orders and no way to get them
            // since lazy loading is not supported. So when returning Customers, there is
            // no Orders to aggregate so that results in nulls and issues in serialization.
            //
            // When adding order with .Include(), the Linq queires generated throw an exception
            // in AggregationBinder::Bind after the on the IQueryable grouping variable.
            // The exception is "One object should implment IComparable" but I tried implmenting
            // IComparable on Customer and Order with no effect.
            //
            // Finally, this little gem works as desired by disconncting out Linq queries from
            // EFCore entirely. However, the sort order is different that is was an EF so the
            // test still fails in this case.
            //
            // In short, lack of lazy loading makes it super hard to implment [EnableQuery]
            // since you have no idea what navigation need to be included. A better
            // option would be to use ODataQueryOptions but it still requires loading of
            // the correct navigation props and still likely has a problem in executing
            // the currently generated Linq queries.
            return db.Customers.Include(c => c.Order).ToList().AsQueryable();
#else
            return db.Customers;
#endif
        }

        [EnableQuery]
        public Customer Get(int key)
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers.Where(c => c.Id == key).FirstOrDefault();
        }
    }
}
