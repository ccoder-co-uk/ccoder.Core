using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;

namespace cCoder.Core.Api
{
    /// <summary>
    /// Base controller for all odata controllers that expose the metadata for the type of data of interest
    /// </summary>
    public abstract partial class CoreODataController : ODataController
    {
        readonly ILogger log;

        protected virtual ICoreAuthInfo AuthInfo { get; }

        protected CoreODataController(ICoreAuthInfo auth, ILogger<CoreODataController> log)
        {
            AuthInfo = auth;
            this.log = log;
        }

        protected MetadataContainer GetMetadataForType(Type type, bool hasEndpoint, bool isEntity)
        {
            try
            {
                return new MetadataContainer(type, isEntity, hasEndpoint);
            }
            catch (Exception ex)
            {
                log.LogError("Failure in acquiring metadata for type " + type.Name, ex);
                throw;
            }
        }

        protected ExtendedMetadataContainer GetExtendedMetadataForType(string context, Type type, IEdmModel model) => 
            model.GetExtendedMetadataForType(context, type);

        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState) => 
            new BadRequestResult(modelState);
    }
}