using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IUserCoursesListController
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int id);
    }
}
