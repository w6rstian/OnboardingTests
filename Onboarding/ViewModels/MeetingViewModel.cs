using Microsoft.AspNetCore.Mvc.Rendering;
using Onboarding.Data.Enums;

namespace Onboarding.ViewModels
{
    public class MeetingViewModel
    {
        public List<string> SelectedUsersIds { get; set; } = new();

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string? Title { get; set; }

        public MeetingType Type { get; set; }
        public List<SelectListItem> AllUsers { get; set; } = new();
    }
}
