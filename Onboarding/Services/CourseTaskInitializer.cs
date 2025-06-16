using Microsoft.AspNetCore.Identity;
using Onboarding.Data;
using Onboarding.Models;
using Task = Onboarding.Models.Task;

namespace Onboarding.Services
{
    public class CourseTaskInitializer
    {
        public static async System.Threading.Tasks.Task SeedCoursesAndTasksAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Check if courses already existtt
            if (context.Courses.Any())
            {
                return; // Database has already been seeded
            }

            // Fetch existing users (mentors) to assign to tasks
            var mentor1 = await userManager.FindByEmailAsync("mentor1@mail.com");
            var mentor2 = await userManager.FindByEmailAsync("mentor2@mail.com");

            if (mentor1 == null || mentor2 == null)
            {
                throw new Exception("Mentor users not found. Ensure the RoleInitializer has seeded the users first.");
            }

            // Create sample courses
            var courses = new List<Course>();
			var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "BaseImageCourse.jpg");
			var imageBytes = System.IO.File.ReadAllBytes(imagePath);
			var course1 = new Course
            {
                Name = "Introduction to Programming",
				Image = imageBytes,
				ImageMimeType = "image/jpeg"
			};

            var task1 = new Task
            {
                Title = "Setup Development Environment",
                Description = "Install and configure your development environment.",
                MentorId = mentor1.Id,
                Mentor = mentor1
            };

            var article1 = new Article
            {
                Content = "How to Install Visual Studio? Visit the link and download the installer. Proceed with installation.",
                Task = task1,
                TaskId = mentor1.Id
            };

            task1.Articles.Add(article1);

            var link1 = new Link
            {
                LinkUrl = "https://visualstudio.microsoft.com/",
                Name = "Visual Studio Download",
                Task = task1,
                TaskId = task1.Id
            };

            task1.Links.Add(link1);

            var task2 = new Task
            {
                Title = "Write Your First Program",
                Description = "Create a simple 'Hello, World!' program.",
                MentorId = mentor1.Id,
                Mentor = mentor1
            };

            var article2 = new Article
            {
                Content = "C# Hello World Tutorial",
                Task = task2,
                TaskId = task2.Id
            };

            task2.Articles.Add(article2);

            var link2 = new Link
            {
                Name = "C# tutorial",
                LinkUrl = "https://example.com/csharp-hello-world",
                Task = task2,
                TaskId = task2.Id
            };

            task2.Links.Add(link2);

            course1.Tasks.Add(task1);
            course1.Tasks.Add(task2);

            var course2 = new Course
            {
                Name = "Advanced C# Programming",
				Image = imageBytes,
				ImageMimeType = "image/jpeg"
			};

            var task3 = new Task
            {
                Title = "Understanding LINQ",
                Description = "Learn and practice LINQ queries.",
                MentorId = mentor2.Id,
                Mentor = mentor2
            };

            var article3 = new Article
            {
                Content = "LINQ Basics",
                Task = task3,
                TaskId = task3.Id
            };

            task3.Articles.Add(article3);

            var link3 = new Link
            {
                Name = "LINQ Documentation",
                LinkUrl = "https://docs.microsoft.com/en-us/dotnet/csharp/linq/",
                Task = task3,
                TaskId = task3.Id
            };

            task3.Links.Add(link3);

            course2.Tasks.Add(task3);

            var task4 = new Task
            {
                Title = "Asynchronous Programming",
                Description = "Explore async/await in C#.",
                MentorId = mentor2.Id,
                Mentor = mentor2
            };

            var article4 = new Article
            {
                Content = "Async/Await Tutorial",
                Task = task4,
                TaskId = task4.Id
            };

            task4.Articles.Add(article4);

            course2.Tasks.Add(task4);

            courses.Add(course1);
            courses.Add(course2);

            // Add courses and tasks to the database
            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }
    }
}