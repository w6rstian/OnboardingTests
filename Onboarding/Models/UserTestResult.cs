// Models/UserTestResult.cs
using System;

namespace Onboarding.Models
{
	public class UserTestResult
	{
		public int Id { get; set; }
		public string UserId { get; set; } // zakładamy IdentityUser
		public int TestId { get; set; }
		public DateTime TakenDate { get; set; }
		public int CorrectAnswers { get; set; }

		public Test Test { get; set; }
	}
}
