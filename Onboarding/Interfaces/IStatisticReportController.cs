using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IStatisticReportController
    {
        Task<IActionResult> Index();
        Task<IActionResult> GetNewUsersByCourse(int courseId);
        Task<IActionResult> GetUsersByRole(string role);
        Task<IActionResult> GetDetailsByRoleUser(string role, int userId);
        Task<IActionResult> GetCourseTasks(int courseId);
        Task<IActionResult> GetNewUserDetails(int userId);
    }
}
