using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface INotificationsController
    {
        ActionResult GetNotificationBell();
        Task<IActionResult> GetNotifications();
        Task<IActionResult> MarkNotificationsAsRead();
        IActionResult Delete(int id);
    }
}
