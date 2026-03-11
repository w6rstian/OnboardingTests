using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IHRController
    {
        IActionResult CreateEmployee();
        IActionResult HRPanel();
        Task<IActionResult> CreateEmployee(string name, string lastname, string email);
    }
}
