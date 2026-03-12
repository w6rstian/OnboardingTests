using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Data.Enums;
using Onboarding.Models;
using Onboarding.ViewModels;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;

namespace Onboarding.Controllers
{
    public class StatisticReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public StatisticReportController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Pobierz wszystkie role
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            // Pobierz wszystkich użytkowników wraz z rolami
            var users = await _context.Users.ToListAsync();

            // Pobierz role użytkowników (asynchronicznie)
            var userRolesDict = new Dictionary<int, IList<string>>();
            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = rolesForUser;
            }

            // Liczba użytkowników na role
            var userCountsByRole = roles.ToDictionary(
                role => role,
                role => userRolesDict.Count(kvp => kvp.Value.Contains(role))
            );

            // Pobierz kursy wraz z mentorem i zadaniami
            var courses = await _context.Courses
                .Include(c => c.Mentor)
                .Include(c => c.Tasks)
                .ToListAsync();

            // Przygotuj model kursów z managerem i liczbą tasków
            var courseModels = courses.Select(c =>
                new CourseReportModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    TaskCount = c.Tasks.Count,
                    ManagerName = c.Mentor != null ? $"{c.Mentor.Name} {c.Mentor.Surname}" : "Brak",
                    Tasks = null
                }).ToList();

            // Pobierz użytkowników o roli Nowy wraz z kursami, do których są przypisani
            var newRoleName = "Nowy";
            var newUserIds = userRolesDict.Where(kvp => kvp.Value.Contains(newRoleName)).Select(kvp => kvp.Key).ToList();

            var newUsersInCourses = await _context.UserCourses
                .Where(uc => newUserIds.Contains(uc.UserId))
                .Include(uc => uc.User)
                .Include(uc => uc.Course)
                .Select(uc => new NewUserCourseModel
                {
                    UserId = uc.UserId,
                    UserName = $"{uc.User.Name} {uc.User.Surname}",
                    CourseName = uc.Course.Name
                })
                .ToListAsync();

            var model = new StatisticReportViewModel
            {
                Roles = roles,
                UserCountsByRole = userCountsByRole,
                Courses = courseModels,
                NewUsersInCourses = newUsersInCourses
            };
            model.NewUsers = await _context.Users
            .Where(u => newUserIds.Contains(u.Id))
            .ToListAsync();

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetNewUsersByCourse(int courseId)
        {
            var newRole = "Nowy";
            var users = await _context.Users.ToListAsync();
            var userRolesDict = new Dictionary<int, IList<string>>();

            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = rolesForUser;
            }

            var newUserIds = userRolesDict.Where(kvp => kvp.Value.Contains(newRole)).Select(kvp => kvp.Key).ToList();

            var newUsersInCourse = await _context.UserCourses
                .Where(uc => uc.CourseId == courseId && newUserIds.Contains(uc.UserId))
                .Include(uc => uc.User)
                .Select(uc => new
                {
                    userId = uc.UserId,
                    userName = uc.User.Name + " " + uc.User.Surname
                })
                .ToListAsync();

            return Json(newUsersInCourse);
        }
        [HttpGet]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var users = await _context.Users.ToListAsync();
            var userRolesDict = new Dictionary<int, IList<string>>();

            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = rolesForUser;
            }

            var filteredUsers = users
                .Where(u => userRolesDict.ContainsKey(u.Id) && userRolesDict[u.Id].Contains(role))
                .Select(u => new { u.Id, Name = $"{u.Name} {u.Surname}", u.Login })
                .ToList();

            return Json(filteredUsers);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsByRoleUser(string role, int userId)
        {
            object result = null;

            switch (role)
            {
                case "Manager":
                    // Kursy przypisane managerowi
                    var courses = await _context.Courses
                        .Where(c => c.MentorId == userId)
                        .Select(c => new { c.Id, c.Name })
                        .ToListAsync();
                    result = courses;
                    break;

                case "Mentor":
                    // Taski mentora
                    var tasks = await _context.Tasks
                        .Where(t => t.MentorId == userId)
                        .Select(t => new { t.Id, t.Title })
                        .ToListAsync();
                    result = tasks;
                    break;

                case "Buddy":
                    // Podopieczni buddy
                    var buddies = await _context.Users
                        .Where(u => u.BuddyId == userId)
                        .Select(u => new { u.Id, Name = $"{u.Name} {u.Surname}" })
                        .ToListAsync();
                    result = buddies;
                    break;

                case "Nowy":
                    // Kursy przypisane nowemu użytkownikowi
                    var userCourses = await _context.UserCourses
                        .Where(uc => uc.UserId == userId)
                        .Include(uc => uc.Course)
                        .Select(uc => new { uc.Course.Id, uc.Course.Name })
                        .ToListAsync();
                    result = userCourses;
                    break;
            }

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseTasks(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Tasks)
                .ThenInclude(t => t.Mentor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return Json(null);

            var tasks = course.Tasks.Select(t => new
            {
                t.Id,
                t.Title,
                MentorName = t.Mentor != null ? $"{t.Mentor.Name} {t.Mentor.Surname}" : "Brak"
            });

            return Json(tasks);
        }
        [HttpGet]
        public async Task<IActionResult> GetNewUserDetails(int userId)
        {
            // 1) Pobierz użytkownika wraz z nawigacją do Buddy i powiązanych kursów
            var user = await _context.Users
                .Include(u => u.Buddy)
                .Include(u => u.UserCourses).ThenInclude(uc => uc.Course)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // 2) Pobierz wszystkie zadania przypisane temu użytkownikowi (UserTasks)
            var userTasks = await _context.UserTasks
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Task)
                .Select(ut => new
                {
                    TaskId = ut.Task.Id,
                    TaskTitle = ut.Task.Title,
                    Status = ut.Status.ToString(),     // przyjmujemy enum StatusTask
                    Grade = ut.Grade                   // ewentualna ocena/feedback
                })
                .ToListAsync();

            // 3) Pobierz wszystkie wyniki testów (UserTestResults) – w DB UserId przechowywany jako string
            var userIdString = user.Id.ToString();
            var userTestResults = await _context.UserTestResults
                .Where(utr => utr.UserId == userIdString)
                .Include(utr => utr.Test)
                .Select(utr => new
                {
                    TestId = utr.Test.Id,
                    TestName = utr.Test.Name,
                    CorrectAnswers = utr.CorrectAnswers
                })
                .ToListAsync();

            // 4) Zbuduj obiekt JSON do zwrócenia
            var result = new
            {
                UserId = user.Id,
                UserName = $"{user.Name} {user.Surname}",
                BuddyName = user.Buddy != null
                    ? $"{user.Buddy.Name} {user.Buddy.Surname}"
                    : "Brak",
                Courses = user.UserCourses
                    .Select(uc => new
                    {
                        CourseId = uc.Course.Id,
                        CourseName = uc.Course.Name
                    })
                    .ToList(),
                Tasks = userTasks,
                TestResults = userTestResults
            };

            return Json(result);
        }
    }
}
