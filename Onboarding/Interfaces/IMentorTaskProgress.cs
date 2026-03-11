using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IMentorTaskProgress
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int id);
        Task<IActionResult> Grade(int userId, int taskId);
        Task<IActionResult> Grade(int userTaskId, string grade);
    }
}
