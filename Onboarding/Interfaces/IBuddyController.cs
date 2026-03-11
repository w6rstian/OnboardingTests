using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IBuddyController
    {
        IActionResult Index();
        IActionResult BuddyPanel();
        Task<IActionResult> Newbies(string searchTerm);
        Task<IActionResult> SendFeedbackToMentor(int newbieId, int courseId, string feedbackContent);
        Task<IActionResult> TaskStatus();
    }
}
