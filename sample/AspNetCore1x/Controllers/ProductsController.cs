using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
    public class ProductsController : ODataController
    {
        private readonly SampleContext _sampleContext;

        public ProductsController(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        // GET: odata/Products
        [EnableQuery]
        //[HttpGet]
        public IEnumerable<Product> Get()
        {
            return _sampleContext.Products;
        }

        // GET odata/Products(5)
        //[HttpGet("{productId}")]
        public IActionResult Get(int productId)
        {
            var product = _sampleContext.FindProduct(productId);
            if (product == null)
            {
                return NotFound();
            }

            return new ObjectResult(product);
        }
    }
}
