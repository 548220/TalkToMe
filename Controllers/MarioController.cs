using Microsoft.AspNetCore.Mvc;

namespace TalkToMeMario.Controllers
{
    public class MarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
