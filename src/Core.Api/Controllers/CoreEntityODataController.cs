using Core.Api.OData;
using Core.Objects;
using Core.Objects.Entities.Security;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers
{
    /// <summary>
    /// Base type for all entity controllers within the Core API context
    /// </summary>
    /// <typeparam name="T">typeof entity being managed</typeparam>
    /// <typeparam name="TKey">type of T's primary Key</typeparam>
    public abstract class CoreEntityODataController<T, TKey> 
        : EntityODataController<T, TKey, User> where T : class
    {
        public CoreEntityODataController(ICoreService<T> service, ICoreAuthInfo auth, ILogger log) 
            : base(service, auth, log) { }

        /// <summary>
        /// Returns the metadata definition of T
        /// </summary>
        /// <returns>metadata</returns>
        [HttpGet]
        public override IActionResult GetMetadata()
        {
            bool isExtendedMetaRequest = Request.Query["extend"] == "true";

            return isExtendedMetaRequest
                ? Ok(GetExtendedMetadataForType("Core", typeof(T), new CoreModelBuilder().Build().EDMModel))
                : base.GetMetadata();
        }
    }
}