using Microsoft.AspNetCore.Mvc;

namespace Onboarding.Interfaces
{
    public interface IHomeController
    {
        IActionResult Index();
        IActionResult Privacy();
        IActionResult Error();
    }
}
