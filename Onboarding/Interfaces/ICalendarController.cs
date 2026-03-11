using Microsoft.AspNetCore.Mvc;
using Onboarding.Data.Enums;
using Onboarding.ViewModels;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface ICalendarController
    {
        IActionResult Index();
        Task<IActionResult> CreateMeeting(MeetingType type);
        Task<IActionResult> CreateMeeting(MeetingViewModel model);
    }
}
