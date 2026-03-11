using Microsoft.AspNetCore.Mvc;

namespace Onboarding.Interfaces
{
    public interface IMentorController
    {
        IActionResult Index();
        IActionResult MentorPanel();
    }
}
