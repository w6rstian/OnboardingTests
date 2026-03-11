using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface ITestsController
    {
        Task<IActionResult> Index();
        IActionResult Create();
        Task<IActionResult> Create(Test test, List<Question> Questions);
        Task<IActionResult> Details(int? id);
        Task<IActionResult> Edit(int? id);
        Task<IActionResult> Edit(int id, Test test, List<Question> Questions);
        Task<IActionResult> Delete(int? id);
        Task<IActionResult> DeleteConfirmed(int id);
        Task<IActionResult> Execute(int id);
        Task<IActionResult> Execute(TestViewModel model);
    }
}
