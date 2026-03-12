using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.ViewModels;

namespace Onboarding.Controllers
{
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        // GET: Tests
        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests
                                        .Include(t => t.Course)
                                        .Include(t => t.Questions)
                                        .ToListAsync();

            Console.WriteLine($"Liczba testów w bazie: {tests.Count}");

            foreach (var test in tests)
            {
                Console.WriteLine($"Test: {test.Id} - {test.Name}");
            }

            return View(tests);
        }


        // GET: Tests/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name");
            return View(new Test()); return View();
        }

        // POST: Tests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Test test, [FromForm] List<Question> Questions)
        {
            Console.WriteLine("Rozpoczęto dodawanie testu.");
            Console.WriteLine($"CourseId: {test.CourseId}, Name: {test.Name}");

            var course = await _context.Courses.FindAsync(test.CourseId);
            if (course == null)
            {
                Console.WriteLine("Nie znaleziono kursu.");
                return NotFound();
            }

            test.Course = course;

            if (Questions != null && Questions.Count > 0)
            {
                test.Questions = new List<Question>();
                foreach (var question in Questions)
                {
                    Console.WriteLine($"Pytanie: {question.Description}");
                    test.Questions.Add(new Question
                    {
                        Description = question.Description,
                        AnswerA = question.AnswerA,
                        AnswerB = question.AnswerB,
                        AnswerC = question.AnswerC,
                        AnswerD = question.AnswerD,
                        CorrectAnswer = question.CorrectAnswer
                    });
                }
            }

             _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Test dodany do bazy: {test.Name}, ID: {test.Id}");
            return RedirectToAction(nameof(Index));
        }


        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.Course)       // Ładujemy powiązany kurs
                .Include(t => t.Questions)    // Ładujemy pytania
                .FirstOrDefaultAsync(m => m.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.Course)       // Ładujemy powiązany kurs
                .Include(t => t.Questions)    // Ładujemy pytania
                .FirstOrDefaultAsync(m => m.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            // Przekazujemy test do widoku
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return View(test);
        }

        // POST: Tests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Test test, List<Question> Questions)
        {
            if (id != test.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTest = await _context.Tests
                        .Include(t => t.Questions)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTest == null)
                    {
                        return NotFound();
                    }

                    // Aktualizujemy nazwę testu i przypisanie do kursu
                    existingTest.Name = test.Name;
                    existingTest.CourseId = test.CourseId;

                    // Aktualizacja pytań
                    existingTest.Questions.Clear();
                    if (Questions != null && Questions.Count > 0)
                    {
                        foreach (var question in Questions)
                        {
                            existingTest.Questions.Add(new Question
                            {
                                Description = question.Description,
                                AnswerA = question.AnswerA,
                                AnswerB = question.AnswerB,
                                AnswerC = question.AnswerC,
                                AnswerD = question.AnswerD,
                                CorrectAnswer = question.CorrectAnswer
                            });
                        }
                    }

                    _context.Update(existingTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tests.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return View(test);
        }


        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                 .Include(t => t.Course) 
                 .Include(t => t.Questions)
                 .FirstOrDefaultAsync(m => m.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);  // Pass the test with questions to the view
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Questions) // Ładujemy powiązane pytania
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test != null)
            {
                try
                {
                    // Usuwamy pytania związane z testem
                    _context.Questions.RemoveRange(test.Questions);
                    // Usuwamy test
                    _context.Tests.Remove(test);

                    // Zapisujemy zmiany w bazie danych
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // W przypadku błędu, np. problemów z relacjami lub zależnościami
                    Console.WriteLine($"Błąd podczas usuwania testu: {ex.Message}");
                    return StatusCode(500, "Wystąpił błąd podczas usuwania testu.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

		[HttpGet]
		public async Task<IActionResult> Execute(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var alreadyTaken = await _context.UserTestResults
				.AnyAsync(r => r.TestId == id && r.UserId == userId);
			if (alreadyTaken)
			{
				TempData["Error"] = "Już rozwiązałeś ten test.";
				return RedirectToAction("Details", new { id });
			}

			var test = await _context.Tests
				.Include(t => t.Questions)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (test == null) return NotFound();

			var vm = new TestViewModel
			{
				TestId = test.Id,
				Name = test.Name,
				CourseId = test.CourseId,
				Questions = test.Questions.Select(q => new QuestionViewModel
				{
					Id = q.Id,
					Description = q.Description,
					AnswerA = q.AnswerA,
					AnswerB = q.AnswerB,
					AnswerC = q.AnswerC,
					AnswerD = q.AnswerD,
					CorrectAnswer = q.CorrectAnswer
				}).ToList(),
				Answers = test.Questions.Select(q => new AnswerSubmissionModel
				{
					QuestionId = q.Id
				}).ToList()
			};

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Execute(TestViewModel model)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var alreadyTaken = await _context.UserTestResults
				.AnyAsync(r => r.TestId == model.TestId && r.UserId == userId);
			if (alreadyTaken)
			{
				TempData["Error"] = "Już rozwiązałeś ten test.";
				return RedirectToAction("Details", new { id = model.TestId });
			}

			var questions = await _context.Questions
				.Where(q => q.TestId == model.TestId)
				.ToListAsync();

			int correct = 0;
			foreach (var ans in model.Answers)
			{
				var q = questions.FirstOrDefault(x => x.Id == ans.QuestionId);
				if (q != null && ans.SelectedAnswer == q.CorrectAnswer)
				{
					correct++;
				}
			}

			var result = new UserTestResult
			{
				UserId = userId,
				TestId = model.TestId,
				TakenDate = DateTime.Now,
				CorrectAnswers = correct
			};

			_context.UserTestResults.Add(result);
			await _context.SaveChangesAsync();

			TempData["Message"] = $"Twój wynik: {correct} poprawnych odpowiedzi.";
			return RedirectToAction("Details", "UserCoursesList", new { id = model.CourseId });

		}
	}
}
