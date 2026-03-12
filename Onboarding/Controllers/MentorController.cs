using Microsoft.AspNetCore.Mvc;

namespace Onboarding.Controllers
{
    public class MentorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MentorPanel()
        {
            return View();
        }
    }
}
