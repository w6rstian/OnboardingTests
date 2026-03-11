using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IQuestionsController
    {
        Task<IActionResult> Index();
        Task<IActionResult> Details(int? id);
        IActionResult Create();
        Task<IActionResult> Create(string Description, string AnswerA, string AnswerB, string AnswerC, string AnswerD, string CorrectAnswer, int TestId);
        Task<IActionResult> Edit(int? id);
        Task<IActionResult> Edit(int id, Question question);
        Task<IActionResult> Delete(int? id);
        Task<IActionResult> DeleteConfirmed(int id);
    }
}
