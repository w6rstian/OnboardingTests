using Microsoft.AspNetCore.Mvc.Rendering;


    namespace Onboarding.ViewModels
    {
        public class CreateOnboardingViewModel
        {
            public string CourseName { get; set; }
            public int? MentorId { get; set; }
		    public IFormFile ImageFile { get; set; }
		    public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
            public List<TestViewModel> Tests { get; set; } = new List<TestViewModel>();
        }
    }

