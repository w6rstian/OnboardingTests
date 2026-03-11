using Microsoft.AspNetCore.Mvc;
using Onboarding.ViewModels;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IAdminController
    {
        Task<IActionResult> ManageRoles(string searchTerm);
        Task<IActionResult> UpdateRole(int userId, string selectedRole);
        IActionResult Index();
        IActionResult AdminPanel();
        IActionResult Create();
        Task<IActionResult> ManageUsers(string searchTerm);
        Task<IActionResult> DeleteUser(string id);
        Task<IActionResult> EditUser(int id);
        Task<IActionResult> EditUser(EditUserViewModel model);
    }
}
