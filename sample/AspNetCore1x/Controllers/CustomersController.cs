namespace ODataSample.Web.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNet.OData;
    using ODataSample.Web.Models;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData;

    public class CustomersController : ODataController
    {
        private readonly SampleContext _sampleContext;

        public CustomersController(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        // GET: odata/Customers
        [EnableQuery]
        //[HttpGet]
        public IEnumerable<Customer> Get()
        {
            return _sampleContext.Customers;
        }

        // GET: odata/Customers(5)
        //[HttpGet("{customerId}")]
        public IActionResult Get(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        // GET: odata/Customers/FindCustomersWithProduct(productId=1)
        //[ODataRoute("Customers/Default.FindCustomersWithProductId(productId={productId})")]
        public IActionResult FindCustomersWithProductAny(int productId)
        {
            var customer = _sampleContext.FindCustomersWithProduct(productId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        // GET: odata/Customers(customerId=1)/GetCustomerName(productId=1)
        //[ODataRoute("Customers({customerId})/Default.GetCustomerName(format={format})")]
        public IActionResult GetCustomerNameAny(int customerId, string format)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            string name = customer.FirstName + format + customer.LastName;
            return Ok(name);
        }
    }
}
