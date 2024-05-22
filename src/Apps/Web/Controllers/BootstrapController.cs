using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("")]
    public class BootstrapController : Controller
    {
        [HttpGet("css/bootstrap.css")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
