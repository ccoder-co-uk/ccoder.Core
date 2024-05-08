using cCoder.Core.Objects.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace cCoder.Core.Api.OData.Responses;

public class ValidationFailureResult : IActionResult
{
    private readonly ValidationResult resultInfo;

    public ValidationFailureResult(ValidationResult resultInfo)
    {
        this.resultInfo = resultInfo;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        HttpResponseMessage response = new(HttpStatusCode.NotAcceptable)
        {
            Content = new StringContent(JsonConvert.SerializeObject(resultInfo, ObjectExtensions.GetJSONSettings()))
        };
        return Task.FromResult(response);
    }
}
