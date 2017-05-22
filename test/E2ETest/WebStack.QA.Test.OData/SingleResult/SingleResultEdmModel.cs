using System.Web.Http;
using Microsoft.OData.WebApi.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.SingleResultTest
{
    public class SingleResultEdmModel
    {
        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }
}
