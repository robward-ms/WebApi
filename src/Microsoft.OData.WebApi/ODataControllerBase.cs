using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi
{
    /// <summary>
    /// The base controller for OData WebApi.
    /// </summary>
    public abstract class ODataControllerBase
    {
        /// <summary>
        /// The Request associated with the controller.
        /// </summary>
        public IWebApiRequestMessage Request { get; internal set; }
    }
}
