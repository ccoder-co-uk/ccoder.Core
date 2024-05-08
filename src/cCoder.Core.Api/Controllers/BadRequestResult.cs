using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace cCoder.Core.Api.Controllers;

public class BadRequestResult : BadRequestObjectResult
{
    public BadRequestResult(ModelStateDictionary modelState) : base(modelState)
    {
        Value = modelState
            .Select(i => new ModelStateError
            {
                Key = i.Key,
                Value = i.Value?.RawValue,
                Errors = i.Value?.Errors?.Select(e => $"{e.ErrorMessage} - {e.Exception?.Message}").ToArray()
            })
            .ToArray().ToJsonForOdata();
    }
}