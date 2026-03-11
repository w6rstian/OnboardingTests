using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface ITasksController
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int? id);
        IActionResult Create();
        Task<IActionResult> Create(int MentorId, string Title, string Description, int CourseId, string ArticleContent, string Links);
        Task<IActionResult> Edit(int? id);
        Task<IActionResult> Edit(int id, int MentorId, string Title, string Description, int CourseId, string ArticleContent, string Links);
        Task<IActionResult> Delete(int? id);
        Task<IActionResult> DeleteConfirmed(int id);
        Task<IActionResult> Execute(int taskId);
        Task<IActionResult> Execute(Onboarding.Models.UserTask model);
    }
}
