using Onboarding.Data.Enums;

namespace Onboarding.Models
{
	public class UserTask
	{
		public int UserTaskId { get; set; }

		public int TaskId { get; set; }
		public Task Task { get; set; }

		public int UserId { get; set; }
		public User user { get; set; }

		public StatusTask Status { get; set; }

		public string Container { get; set; }
		
		public string Grade { get; set; }
	}

}
