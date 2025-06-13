using Onboarding.Models;

namespace Onboarding.ViewModels
{
    public class ManageRolesViewModel
    {
        public User User { get; set; }
        public IList<string> AvailableRoles { get; set; }
        public IList<string> UserRoles { get; set; }
        public string SelectedRole { get; set; }
    }
}
