using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Data.Enums;
using Onboarding.Models;
using Onboarding.ViewModels;

namespace Onboarding.Controllers
{
    public class CalendarController(ApplicationDbContext context, UserManager<User> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateMeeting(MeetingType type)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var users = await _context.Users
                .Where(u => u.Id != currentUser.Id)
                .ToListAsync();

            var model = new MeetingViewModel
            {
                Type = type,
                AllUsers = [.. users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} {u.Surname} ({u.Email})"
                })]
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(MeetingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                model.AllUsers = await _context.Users
                    .Where(u => u.Id != currentUser.Id)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.Name} {u.Surname} ({u.Email})"
                    }).ToListAsync();

                return View(model);
            }

            var organizer = await _userManager.GetUserAsync(User);

            var meeting = new Meeting
            {
                OrganizerId = organizer.Id,
                Start = model.Start,
                End = model.End,
                Title = model.Title ?? "Spotkanie",
                Type = model.Type
            };

            foreach (var userId in model.SelectedUsersIds)
            {
                var participant = new MeetingParticipant
                {
                    UserId = int.Parse(userId),
                };
                meeting.Participants.Add(participant);
            }

            _context.Add(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // kalendarz
        }

        [HttpGet]
        public async Task<JsonResult> GetEvents(string? type)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var meetingsQuery = _context.Meetings
                .Include(m => m.Participants).ThenInclude(p => p.User)
                .Where(m =>
                    m.OrganizerId == currentUser.Id ||
                    m.Participants.Any(mp => mp.UserId == currentUser.Id));

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<MeetingType>(type, out var parsedType))
            {
                meetingsQuery = meetingsQuery.Where(m => m.Type == parsedType);
            }

            var meetings = await meetingsQuery.ToListAsync();

            var events = meetings.Select(m => new
            {
                title = m.Title ?? "Spotkanie",
                start = m.Start.ToString("s"),
                end = m.End.ToString("s"),
                type = m.Type.ToString(),
                participants = m.Participants
                    .Select(p => $"{p.User.Name} {p.User.Surname} ({p.User.Email})")
                    .ToList()
            }).ToList();

            // przykładowe spotkania żeby się dało testować
            var exampleEvents = new List<ExampleEvent>
            {
                new() {
                    Title = "Spotkanie zespołu",
                    Start = "2025-05-10T10:00:00",
                    End = "2025-05-10T11:00:00",
                    Type = "General",
                    Participants = ["Anna Nowak (anna@firma.pl)", "Jan Kowalski (jan@firma.pl)"]
                },
                new() {
                    Title = "Lunch z klientem",
                    Start = "2025-05-12T13:00:00",
                    End = "2025-05-12T14:00:00",
                    Type = "General",
                    Participants = ["Magdalena Wójcik (magda@firma.pl)"]
                },
                new() {
                    Title = "Check-in z Basią",
                    Start = "2025-05-15T09:00:00",
                    End = "2025-05-15T10:00:00",
                    Type = "CheckIn",
                    Participants = ["Basia Zielińska (basia@firma.pl)"]
                }
            };

            if (!string.IsNullOrEmpty(type))
            {
                exampleEvents = [.. exampleEvents.Where(e => e.Type == type)];
            }

            events.AddRange(exampleEvents.Select(e => new
            {
                title = e.Title,
                start = e.Start,
                end = e.End,
                type = e.Type,
                participants = e.Participants
            }));

            return Json(events);
        }
    }
    public class ExampleEvent
    {
        public string Title { get; set; } = null!;
        public string Start { get; set; } = null!;
        public string End { get; set; } = null!;
        public string Type { get; set; } = null!;
        public List<string> Participants { get; set; } = [];
    }
}
