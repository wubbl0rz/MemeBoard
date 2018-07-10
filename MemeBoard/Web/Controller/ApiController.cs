using Microsoft.AspNetCore.Mvc;

namespace MemeBoard
{
    public class ApiController : Controller
    {
        public ApiController()
        {
        }

        public IActionResult Index()
        {
            return Json("123");
        }
    }
}
