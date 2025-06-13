using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Onboarding.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OnboardingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            var mentors = _context.Users
            .Select(u => new { u.Id, FullName = u.Name + " " + u.Surname })
            .ToList();

            if (!mentors.Any())
            {
                ModelState.AddModelError("", "Brak dostępnych mentorów w systemie. Dodaj użytkowników przed utworzeniem kursu.");
            }

            Console.WriteLine("Mentors loaded for Create (GET): " + string.Join(", ", mentors.Select(m => $"Id={m.Id}, Name={m.FullName}")));
            ViewData["Mentors"] = new SelectList(mentors, "Id", "FullName");
            return View(new CreateOnboardingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CreateOnboardingViewModel viewModel)
        {
            if (viewModel == null)
            {
                ModelState.AddModelError("", "Model jest pusty.");
            }
            else
            {
                if (string.IsNullOrEmpty(viewModel.CourseName))
                    ModelState.AddModelError("CourseName", "Nazwa kursu jest wymagana.");

                if (viewModel.MentorId.HasValue && viewModel.MentorId.Value > 0)
                {
                    var mentorExists = await _context.Users.AnyAsync(u => u.Id == viewModel.MentorId.Value);
                    if (!mentorExists)
                        ModelState.AddModelError("MentorId", $"Mentor o ID {viewModel.MentorId.Value} nie istnieje w bazie danych.");
                }

                if (viewModel.Tasks != null)
                {
                    Console.WriteLine($"Tasks Count: {viewModel.Tasks.Count}");
                    for (int i = 0; i < viewModel.Tasks.Count; i++)
                    {
                        var task = viewModel.Tasks[i];
                        Console.WriteLine($"Task[{i}]: Title={task.Title}, MentorId={task.MentorId}, Type={task.MentorId.GetType().Name}");

                        if (string.IsNullOrEmpty(task.Title))
                            ModelState.AddModelError($"Tasks[{i}].Title", "Tytuł zadania jest wymagany.");
                        if (string.IsNullOrEmpty(task.Description))
                            ModelState.AddModelError($"Tasks[{i}].Description", "Opis zadania jest wymagany.");
                        if (task.MentorId <= 0)
                            ModelState.AddModelError($"Tasks[{i}].MentorId", "Wybierz mentora.");
                        else
                        {
                            var mentorExists = await _context.Users.AnyAsync(u => u.Id == task.MentorId);
                            Console.WriteLine($"Checking MentorId={task.MentorId}, Exists={mentorExists}");
                            if (!mentorExists)
                                ModelState.AddModelError($"Tasks[{i}].MentorId", $"Mentor o ID {task.MentorId} nie istnieje w bazie danych.");
                        }
                        if (string.IsNullOrEmpty(task.ArticleContent))
                            ModelState.AddModelError($"Tasks[{i}].ArticleContent", "Treść artykułu jest wymagana.");
                        if (string.IsNullOrEmpty(task.Links))
                            ModelState.AddModelError($"Tasks[{i}].Links", "Linki są wymagane.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Validation Error: Key={error.Key}, Errors={string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                var mentors = await _context.Users
                .Select(u => new { u.Id, FullName = u.Name + " " + u.Surname })
                .ToListAsync();
                Console.WriteLine("Available Mentors (POST): " + string.Join(", ", mentors.Select(m => $"Id={m.Id}, Name={m.FullName}")));
                ViewData["Mentors"] = new SelectList(mentors, "Id", "FullName");
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
				byte[] imageBytes = null;
				string imageMimeType = null;

				if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
				{
					using (var ms = new MemoryStream())
					{
						await viewModel.ImageFile.CopyToAsync(ms);
						imageBytes = ms.ToArray();
						imageMimeType = viewModel.ImageFile.ContentType;
					}
				}

				var course = new Course
                {
                    Name = viewModel.CourseName,
                    MentorId = viewModel.MentorId,
					Image = imageBytes,
					ImageMimeType = imageMimeType
				};
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                if (viewModel.Tasks != null && viewModel.Tasks.Any())
                {
                    foreach (var taskVM in viewModel.Tasks)
                    {
                        var mentor = await _context.Users.FindAsync(taskVM.MentorId);
                        if (mentor == null)
                        {
                            ModelState.AddModelError("", $"Mentor o ID {taskVM.MentorId} nie istnieje.");
                            ViewData["Mentors"] = new SelectList(
                            await _context.Users.Select(u => new { u.Id, FullName = u.Name + " " + u.Surname }).ToListAsync(),
                            "Id", "FullName");
                            return View(viewModel);
                        }

                        var task = new Onboarding.Models.Task
                        {
                            Title = taskVM.Title,
                            Description = taskVM.Description,
                            MentorId = taskVM.MentorId,
                            CourseId = course.Id,
                            Mentor = mentor,
                            Course = course
                        };

                        if (!string.IsNullOrEmpty(taskVM.Links))
                        {
                            var links = taskVM.Links.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                            foreach (var url in links)
                            {
                                task.Links.Add(new Link
                                {
                                    LinkUrl = url,
                                    Name = url,
                                    Task = task
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(taskVM.ArticleContent))
                        {
                            task.Articles.Add(new Article
                            {
                                Content = taskVM.ArticleContent,
                                Task = task
                            });
                        }

                        course.Tasks.Add(task);
                        _context.Tasks.Add(task);
                    }
                }

                if (viewModel.Tests != null && viewModel.Tests.Any())
                {
                    foreach (var testVM in viewModel.Tests)
                    {
                        var test = new Test
                        {
                            Name = testVM.Name,
                            CourseId = course.Id,
                            Course = course,
                            Questions = new List<Question>()
                        };

                        if (testVM.Questions != null && testVM.Questions.Any())
                        {
                            foreach (var questionVM in testVM.Questions)
                            {
                                test.Questions.Add(new Question
                                {
                                    Description = questionVM.Description,
                                    AnswerA = questionVM.AnswerA,
                                    AnswerB = questionVM.AnswerB,
                                    AnswerC = questionVM.AnswerC,
                                    AnswerD = questionVM.AnswerD,
                                    CorrectAnswer = questionVM.CorrectAnswer
                                });
                            }
                        }

                        _context.Tests.Add(test);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction("Index", "Courses");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Wystąpił błąd: {ex.Message}");
                ViewData["Mentors"] = new SelectList(
                await _context.Users.Select(u => new { u.Id, FullName = u.Name + " " + u.Surname }).ToListAsync(),
                "Id", "FullName");
                return View(viewModel);
            }
        }
    }
}