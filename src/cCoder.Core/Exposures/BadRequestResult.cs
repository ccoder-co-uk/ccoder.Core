// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace cCoder.Core.Exposures;

public class BadRequestResult : BadRequestObjectResult
{
    public BadRequestResult(ModelStateDictionary modelState)
        : base(modelState)
    {
        Value = modelState
            .Select(selector: i => new ModelStateError
            {
                Key = i.Key,
                Value = i.Value?.RawValue,
                Errors = i
                    .Value?.Errors?.Select(selector: e => $"{e.ErrorMessage} - {e.Exception?.Message}")
                    .ToArray(),
            })
            .ToArray()
            .ToJsonForOdata();
    }
}