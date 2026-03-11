using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IChatController
    {
        IActionResult UserList();
        IActionResult Index(int receiverId);
        Task<IActionResult> SendMessage(int receiverId, string content);
    }
}
