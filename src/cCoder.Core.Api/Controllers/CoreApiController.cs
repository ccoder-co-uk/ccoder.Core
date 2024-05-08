using cCoder.Core.Api.OData.Responses;
using cCoder.Core.Objects.Dtos.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace cCoder.Core.Api.Controllers;

public abstract class CoreApiController : Controller
{
    private readonly ILogger log;

    public CoreApiController(ILogger<CoreApiController> log)
    {
        this.log = log;
    }

    protected MetadataContainer GetMetadataForType(Type type) => new(type);

    protected IActionResult Failed<T>(ModelStateDictionary ModelState)
    {
        IEnumerable<string> errors = ModelState.SelectMany(ms => ms.Value.Errors.Select(ExtactFailReason));
        StringBuilder builder = new("The " + typeof(T).Name + " given is not valid because:\n");

        foreach (string err in errors)
            _ = builder.AppendLine(err);

        return new ValidationFailureResult(new ValidationResult(builder.ToString(), ModelState.Select(ms => ms.Key)));
    }

    protected async Task<IActionResult> Failed<TException>(TException ex) where TException : Exception
    {
        // log the problem
        string content = await new StreamReader(Request.Body).ReadToEndAsync();

        var logDetail = new
        {
            Message = "An exception was raised whilst processing this request",
            Url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
            Method = Request.Method.ToString(),
            Request.Headers,
            Body = content
        };

        log.LogError("\n" + JsonConvert.SerializeObject(logDetail, Formatting.Indented), ex);

        // respond
#if DEBUG
        return BadRequest(ex.Message + Environment.NewLine + ex.StackTrace);
#else
            return BadRequest(ex.Message);
#endif
    }

    private string ExtactFailReason(ModelError e)
    {
        if (e.ErrorMessage != string.Empty)
            return e.ErrorMessage;
        else
            return e.Exception != null ? e.Exception.Message : string.Empty;
    }
}