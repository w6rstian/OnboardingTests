using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IManagerController
    {
        IActionResult Index();
        IActionResult ManagerPanel();
        Task<IActionResult> MyCourses();
    }
}
