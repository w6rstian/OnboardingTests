using Microsoft.AspNetCore.Mvc;

using Onboarding.Interfaces;

namespace Onboarding.Controllers
{
    public class MentorController : Controller, IMentorController
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
