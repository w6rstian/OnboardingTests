using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Data.Enums;
using Onboarding.Hubs;
using Onboarding.Models;
using System.Security.Claims;

namespace Onboarding.Controllers
{
    [Authorize(Roles = "Admin,Buddy")]
    public class BuddyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<ChatHub> _chatHub;

        public BuddyController(ApplicationDbContext context, UserManager<User> userManager, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _userManager = userManager;
            _chatHub = chatHub;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BuddyPanel()
        {
            return View();
        }

        public async Task<IActionResult> Newbies(string searchTerm)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var newbies = _context.Users
                .Where(n => n.Buddy == currentUser)
                .Include(n => n.UserCourses)
                    .ThenInclude(uc => uc.Course)
                        .ThenInclude(c => c.Mentor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                newbies = newbies.Where(n => n.Email.ToLower().Contains(searchTerm) ||
                                             n.Name.ToLower().Contains(searchTerm) ||
                                             n.Surname.ToLower().Contains(searchTerm));
            }

            var newbiesList = await newbies.ToListAsync();
            ViewData["SearchTerm"] = searchTerm;

            return View(newbiesList);
        }

        [Authorize(Roles = "Admin,Buddy")]
        [HttpPost]
        public async Task<IActionResult> SendFeedbackToMentor(int newbieId, int courseId, string feedbackContent)
        {
            if (string.IsNullOrEmpty(feedbackContent))
            {
                TempData["Error"] = "Feedback nie może być pusty.";
                return RedirectToAction("Newbies");
            }

            var course = await _context.Courses
                .Include(c => c.Mentor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null || course.Mentor == null)
            {
                TempData["Error"] = "Kurs lub mentor nie istnieje.";
                return RedirectToAction("Newbies");
            }

            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var receiverId = course.Mentor.Id;

            var message = new Message
            {
                Content = feedbackContent,
                SentAt = DateTime.Now,
                SenderId = senderId,
                ReceiverId = receiverId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var groupName = $"{Math.Min(senderId, receiverId)}_{Math.Max(senderId, receiverId)}";
            await _chatHub.Clients.Group(groupName)
                .SendAsync("ReceiveMessage", message.Content, message.SentAt.ToString("g"), message.SenderId);

            TempData["Success"] = "Feedback został wysłany do mentora.";
            return RedirectToAction("Newbies");
        }

        public async Task<IActionResult> TaskStatus()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var tasks = await _context.UserTasks
                .Where(t => t.user.Buddy == currentUser)
                .Include(t => t.Task)
                    .ThenInclude(task => task.Course)
                .Include(t => t.user)
                .ToListAsync();



            // DEBUG
            var nowi = await _userManager.GetUsersInRoleAsync("Nowy");
            var nowy1 = nowi.FirstOrDefault(t => t.Email == "nowy1@mail.com");


            var tempUserTask = new UserTask
            {
                user = nowy1,
                Status = StatusTask.InProgress,
                Task = new Models.Task
                {
                    Title = "Testowy task",
                    Description = "Ten task jest sztuczny"
                }
            };

            if (currentUser == nowy1.Buddy)
            {
                tasks.Add(tempUserTask);
            }

            // DEBUG END

            return View(tasks);
        }
    }
}
