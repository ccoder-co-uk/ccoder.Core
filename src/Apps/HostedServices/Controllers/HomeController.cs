using Microsoft.AspNetCore.Mvc;

namespace HostedServices.Controllers;

[Route("")]
public sealed class HomeController() : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
