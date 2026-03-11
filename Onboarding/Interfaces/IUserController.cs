using Microsoft.AspNetCore.Mvc;
using Onboarding.Models;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IUserController
    {
        IActionResult MainPage();
        IActionResult UserPanel();
        Task<IActionResult> MyAccount();
        Task<IActionResult> MyAccount(string name, string lastname, string email, string phone, string dept, string pos);
        Task<IActionResult> UserSettings();
        Task<IActionResult> UserSettings(User model);
    }
}
