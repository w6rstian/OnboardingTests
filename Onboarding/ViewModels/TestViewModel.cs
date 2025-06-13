using Onboarding.Models;
namespace Onboarding.ViewModels
{
    public class TestViewModel
    {
		public int TestId { get; set; }
		public string Name { get; set; }
		public int CourseId { get; set; }
		public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
		public List<AnswerSubmissionModel> Answers { get; set; } = new();
	}
}
