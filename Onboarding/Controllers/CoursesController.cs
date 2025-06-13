using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses.ToListAsync());
        }

        
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
            .Include(c => c.UserCourses)
            .ThenInclude(uc => uc.User)
            .Include(c => c.Tasks)
            .ThenInclude(t => t.Mentor)
            .Include(c => c.Tests)
            .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }



        [Authorize(Roles = "Admin,Manager")]
        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Course course)
        {   
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

		// GET: Courses/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var course = await _context.Courses.FindAsync(id);
			if (course == null)
				return NotFound();

			var model = new CourseEditViewModel
			{
				Id = course.Id,
				Name = course.Name,
				ExistingImage = course.Image,
				ExistingImageMimeType = course.ImageMimeType
			};

			return View(model);
		}


		// POST: Courses/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, CourseEditViewModel model)
		{
			Console.WriteLine(">>> Edit POST wywołane <<<");

			if (id != model.Id)
				return NotFound();

			if (!ModelState.IsValid)
			{
				Console.WriteLine("ModelState = invalid");
				foreach (var entry in ModelState)
				{
					foreach (var error in entry.Value.Errors)
					{
						Console.WriteLine($"[VALIDATION ERROR] {entry.Key}: {error.ErrorMessage}");
					}
				}

				return View(model);
			}

			var course = await _context.Courses.FindAsync(id);
			if (course == null)
				return NotFound();

			Console.WriteLine($"Stara nazwa: {course.Name}, Nowa: {model.Name}");

			course.Name = model.Name;

			if (model.ImageFile != null && model.ImageFile.Length > 0)
			{
				using var ms = new MemoryStream();
				await model.ImageFile.CopyToAsync(ms);
				course.Image = ms.ToArray();
				course.ImageMimeType = model.ImageFile.ContentType;
			}

			await _context.SaveChangesAsync();

			var confirm = await _context.Courses.FindAsync(model.Id);
			Console.WriteLine($"Zapisano jako: {confirm.Name}");

			return RedirectToAction(nameof(Index));
		}


		// GET: Courses/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
		public IActionResult GetCourseImage(int id)
		{
			var course = _context.Courses.FirstOrDefault(c => c.Id == id);
			if (course == null || course.Image == null)
				return NotFound();

			return File(course.Image, course.ImageMimeType);
		}

	}
}
