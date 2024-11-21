using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using iText.Html2pdf;

namespace cCoder.Core.Api.Controllers.CMS;

public class TemplateController : CoreEntityODataController<Template, int>
{
    protected new ITemplateService Service =>
        base.Service as ITemplateService;

    public TemplateController(ITemplateService service, ICoreAuthInfo auth, ILogger<TemplateController> log)
        : base(service, auth, log) { }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Render(int appId, string name, string culture)
    {
        using var reader = new StreamReader(Request.Body);

        dynamic m = JsonConvert
            .DeserializeObject(await reader.ReadToEndAsync());

        return Content(
            Service.Render(appId, name, culture, m),
            "text/plain", 
            Encoding.UTF8);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> HtmlToPdf(string name)
    {
        using var reader = new StreamReader(Request.Body);
        var htmlContent = await reader.ReadToEndAsync();

        using MemoryStream pdfStream = new();
        HtmlConverter.ConvertToPdf(htmlContent, pdfStream);

        //Response.Headers.Add("Content-Disposition", $"inline; filename={name}.pdf");
        return File(pdfStream.ToArray(), "application/pdf", $"{name}.pdf");
    }
}