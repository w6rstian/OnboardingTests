using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onboarding.Data;
using Onboarding.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Onboarding.Controllers
{
    [Authorize]
    public class RewardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RewardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ocena Mentora
        public IActionResult RateMentor(int mentorId, int taskId)
        {
            var mentor = _context.Users.Find(mentorId);
            if (mentor == null)
                return NotFound("Mentor nie znaleziony.");

            ViewBag.PersonType = "Mentora";
            ViewBag.PersonName = $"{mentor.Name} {mentor.Surname}";
            ViewBag.AverageRating = _context.Rewards
                .Where(r => r.Receiver == mentorId)
                .Select(r => (double?)r.Rating)
                .Average() ?? 0;
            ViewBag.TaskId = taskId;

            return View("RatePerson", new Reward { Receiver = mentorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateMentor(int receiver, int rating, string feedback, int taskId)
        {
            return await SaveRating(receiver, rating, feedback, "Execute", "Tasks", new { taskId });
        }

        // Ocena Buddy'ego
        public IActionResult RateBuddy()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.Find(userId);

            if (user?.BuddyId == null)
                return NotFound("Brak przypisanego Buddy'ego.");

            var buddy = _context.Users.Find(user.BuddyId);

            ViewBag.PersonType = "Buddy'ego";
            ViewBag.PersonName = $"{buddy.Name} {buddy.Surname}";
            ViewBag.AverageRating = _context.Rewards
                .Where(r => r.Receiver == buddy.Id)
                .Select(r => (double?)r.Rating)
                .Average() ?? 0;

            return View("RatePerson", new Reward { Receiver = buddy.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateBuddy(int receiver, int rating, string feedback)
        {
            return await SaveRating(receiver, rating, feedback, "Index", "UserCoursesList", null);
        }

        // Wspólna metoda do zapisywania oceny
        private async Task<IActionResult> SaveRating(int receiver, int rating, string feedback, string action, string controller, object routeValues)
        {
            if (string.IsNullOrWhiteSpace(feedback))
                ModelState.AddModelError("Feedback", "Komentarz jest wymagany.");

            if (!ModelState.IsValid)
            {
                var person = _context.Users.Find(receiver);
                ViewBag.PersonName = $"{person.Name} {person.Surname}";
                ViewBag.AverageRating = _context.Rewards
                    .Where(r => r.Receiver == receiver)
                    .Select(r => (double?)r.Rating)
                    .Average() ?? 0;

                return View("RatePerson", new Reward { Receiver = receiver, Rating = rating, Feedback = feedback });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var reward = new Reward
            {
                Receiver = receiver,
                Giver = userId,
                Rating = rating,
                Feedback = feedback,
                CreatedAt = DateTime.Now
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Ocena została dodana!";
            return RedirectToAction(action, controller, routeValues);
        }
    }
}
