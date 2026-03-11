using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IRewardsController
    {
        IActionResult RateMentor(int mentorId, int taskId);
        Task<IActionResult> RateMentor(int receiver, int rating, string feedback, int taskId);
        IActionResult RateBuddy();
        Task<IActionResult> RateBuddy(int receiver, int rating, string feedback);
    }
}
