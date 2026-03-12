using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Hubs;
using Onboarding.Models;

namespace Onboarding.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ManagerController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManagerPanel()
        {
            return View();
        }

        public async Task<IActionResult> MyCourses()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var courses = await _context.Courses
                                .Where(c => c.Mentor == currentUser)
                                .ToListAsync();

            return View(courses);
        }
    }
}
