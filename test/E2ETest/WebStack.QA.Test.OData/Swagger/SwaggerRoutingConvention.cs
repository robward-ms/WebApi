using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.WebApi.Routing;
using Microsoft.OData.WebApi.Routing.Conventions;

namespace WebStack.QA.Test.OData.Swagger
{
    public class SwaggerRoutingConvention : IODataRoutingConvention
    {
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "Swagger";
            }

            return null;
        }

        public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "GetSwagger";
            }

            return null;
        }
    }
}
