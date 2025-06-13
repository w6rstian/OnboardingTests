using System.ComponentModel.DataAnnotations;

namespace Onboarding.Models
{
	/// <summary>
	/// This model is for example register form data validation.
	/// </summary>
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Login is required")]
		[StringLength(50, ErrorMessage = "Login can have a maximum of 50 characters")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[StringLength(100, MinimumLength = 8, ErrorMessage = "Password must have between 8-100 characters")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Incorrect email format")]
		public string Email { get; set; }
	}
}
