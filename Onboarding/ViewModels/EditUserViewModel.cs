using Onboarding.Models;

namespace Onboarding.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int? BuddyId { get; set; }
        public List<User> AvailableBuddies { get; set; }
    }
}
