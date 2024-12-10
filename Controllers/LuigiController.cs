using Microsoft.AspNetCore.Mvc;

namespace TalkToMeMario.Controllers
{
    public class LuigiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
