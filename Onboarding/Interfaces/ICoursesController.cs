using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface ICoursesController
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int? id);
        IActionResult Create();
        Task<IActionResult> Create(Course course);
        Task<IActionResult> Edit(int id);
        Task<IActionResult> Edit(int id, CourseEditViewModel model);
        Task<IActionResult> Delete(int? id);
        Task<IActionResult> DeleteConfirmed(int id);
        IActionResult GetCourseImage(int id);
    }
}
