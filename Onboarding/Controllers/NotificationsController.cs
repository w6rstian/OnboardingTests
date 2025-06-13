using Microsoft.AspNetCore.Mvc;
using Onboarding.Data;
using Onboarding.Models;
using System.Security.Claims;

namespace Onboarding.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public ActionResult GetNotificationBell()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId != 0)
            {
                var unreadCount = _context.Notifications
                    .Count(n => n.UserId == userId && !n.IsRead);
                return PartialView("_NotificationBell", unreadCount);
            }
            return PartialView("_NotificationBell", 0);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId != 0)
            {
                var notifications = _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
                return PartialView("_NotificationsList", notifications);
            }
            return PartialView("_NotificationsList", new List<Notification>());
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId != 0)
            {
                var notifications = _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead);
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
