using Microsoft.AspNetCore.Mvc.Rendering;
using Onboarding.Data.Enums;

namespace Onboarding.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public int OrganizerId { get; set; }
        public User Organizer { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string? Title { get; set; }

        public MeetingType Type { get; set; } // enum MeetingType

        public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    }
}
