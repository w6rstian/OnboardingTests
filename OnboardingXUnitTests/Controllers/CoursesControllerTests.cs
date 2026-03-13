using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Security.Claims;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class CoursesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CoursesController _controller;

        public CoursesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new CoursesController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfCourses()
        {
            _context.Courses.AddRange(
                new Course { Id = 1, Name = "Course 1" },
                new Course { Id = 2, Name = "Course 2" }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.Index();

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Course>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public async Task Details_IdIsNull_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Details_CourseNotFound_ReturnsNotFound()
        {
            var result = await _controller.Details(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Details_CourseExists_ReturnsViewWithCourse()
        {
            var course = new Course { Id = 1, Name = "Test Course" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = await _controller.Details(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Course>().Subject;
            model.Id.Should().Be(1);
            model.Name.Should().Be("Test Course");
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndexAndAddsCourse()
        {
            var course = new Course { Name = "New Course" };

            var result = await _controller.Create(course);

            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            _context.Courses.Should().HaveCount(1);
            _context.Courses.First().Name.Should().Be("New Course");
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithCourse()
        {
            var course = new Course { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Create(course);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(course);
        }

        [Fact]
        public async Task Edit_Get_CourseNotFound_ReturnsNotFound()
        {
            var result = await _controller.Edit(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Edit_Get_CourseExists_ReturnsViewWithViewModel()
        {
            var course = new Course { Id = 1, Name = "Course to Edit" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = await _controller.Edit(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CourseEditViewModel>().Subject;
            model.Id.Should().Be(1);
            model.Name.Should().Be("Course to Edit");
        }

        [Fact]
        public async Task Edit_Post_IdsDoNotMatch_ReturnsNotFound()
        {
            var model = new CourseEditViewModel { Id = 1 };

            var result = await _controller.Edit(2, model);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            var model = new CourseEditViewModel { Id = 1, Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Edit(1, model);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task Edit_Post_CourseNotFoundInDb_ReturnsNotFound()
        {
            var model = new CourseEditViewModel { Id = 99, Name = "Non-existent" };

            var result = await _controller.Edit(99, model);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesCourseAndRedirects()
        {
            var course = new Course { Id = 1, Name = "Old Name" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var model = new CourseEditViewModel { Id = 1, Name = "New Name" };

            var result = await _controller.Edit(1, model);

            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            var updatedCourse = await _context.Courses.FindAsync(1);
            updatedCourse.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task Edit_Post_WithNewImage_UpdatesImage()
        {
            var course = new Course { Id = 1, Name = "Old Name" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var imageFile = A.Fake<IFormFile>();
            var content = "fake image content";
            var fileName = "test.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            A.CallTo(() => imageFile.OpenReadStream()).Returns(ms);
            A.CallTo(() => imageFile.FileName).Returns(fileName);
            A.CallTo(() => imageFile.Length).Returns(ms.Length);
            A.CallTo(() => imageFile.ContentType).Returns("image/png");

            A.CallTo(() => imageFile.CopyToAsync(A<Stream>._, A<CancellationToken>._))
                .ReturnsLazily(async (Stream s, CancellationToken ct) => 
                {
                    await ms.CopyToAsync(s);
                });

            var model = new CourseEditViewModel 
            { 
                Id = 1, 
                Name = "New Name",
                ImageFile = imageFile
            };

            var result = await _controller.Edit(1, model);

            result.Should().BeOfType<RedirectToActionResult>();
            var updatedCourse = await _context.Courses.FindAsync(1);
            updatedCourse.Image.Should().NotBeNull();
            updatedCourse.ImageMimeType.Should().Be("image/png");
        }

        [Fact]
        public async Task Delete_Get_IdIsNull_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Get_CourseNotFound_ReturnsNotFound()
        {
            var result = await _controller.Delete(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Get_CourseExists_ReturnsViewWithCourse()
        {
            var course = new Course { Id = 1, Name = "Course to Delete" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Course>().Subject;
            model.Id.Should().Be(1);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesCourseAndRedirects()
        {
            var course = new Course { Id = 1, Name = "To be deleted" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteConfirmed(1);

            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            _context.Courses.Should().BeEmpty();
        }

        [Fact]
        public void GetCourseImage_CourseNotFound_ReturnsNotFound()
        {
            var result = _controller.GetCourseImage(99);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetCourseImage_NoImage_ReturnsNotFound()
        {
            var course = new Course { Id = 1, Name = "No Image", Image = null };
            _context.Courses.Add(course);
            _context.SaveChanges();

            var result = _controller.GetCourseImage(1);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetCourseImage_ImageExists_ReturnsFileResult()
        {
            var course = new Course 
            { 
                Id = 1, 
                Name = "With Image", 
                Image = new byte[] { 1, 2, 3 }, 
                ImageMimeType = "image/jpeg" 
            };
            _context.Courses.Add(course);
            _context.SaveChanges();

            var result = _controller.GetCourseImage(1);

            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.FileContents.Should().Equal(new byte[] { 1, 2, 3 });
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
