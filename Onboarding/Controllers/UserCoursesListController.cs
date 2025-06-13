using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Onboarding.Controllers
{
    [Authorize]
    public class UserCoursesListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserCoursesListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var userCourses = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Course)
                .Select(uc => uc.Course)
                .ToListAsync();

            return View(userCourses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.UserCourses)
                    .ThenInclude(uc => uc.User)
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.Links) 
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.Mentor) 
                .Include(c => c.Tests)
					.ThenInclude(t => t.Questions)
				.FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var results = await _context.UserTestResults
				.Where(r => r.UserId == userId)
				.ToListAsync();

			ViewBag.UserTestResults = results;

			return View(course);
        }
    }
}
