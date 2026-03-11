using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IUserCoursesController
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int? id);
        Task<IActionResult> Create();
        Task<IActionResult> Create(int UserID, int CourseID);
        Task<IActionResult> Edit(int? id);
        Task<IActionResult> Edit(int id, UserCourse userCourse);
        Task<IActionResult> Delete(int? id);
        Task<IActionResult> DeleteConfirmed(int id);
    }
}
