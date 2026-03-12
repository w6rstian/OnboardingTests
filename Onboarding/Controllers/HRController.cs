using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Onboarding.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace Onboarding.Controllers
{
	public class HRController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly IUserStore<User> _userStore;
		private readonly IEmailSender _emailSender;

		public HRController(UserManager<User> userManager, IEmailSender emailSender, IUserStore<User> userStore)
		{
			_userManager = userManager;
			_userStore = userStore;
			_emailSender = emailSender;
		}

        [Authorize(Roles = "Admin,HR")]
        public IActionResult CreateEmployee()
		{
			return View();
		}
		public IActionResult HRPanel()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> CreateEmployee(string name, string lastname, string email)
		{
			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(lastname) || string.IsNullOrWhiteSpace(email))
			{
				ModelState.AddModelError(string.Empty, "All fields are required.");
				return View();
			}

			var user = CreateUser();

			user.Name = name;
			user.Surname = lastname;
			

			await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
			var result = await _userManager.CreateAsync(user, "DefaultPassword123!");

			if (result.Succeeded)
			{
				await _userManager.SetEmailAsync(user, email);
				user.EmailConfirmed = true;

				var userId = await _userManager.GetUserIdAsync(user);
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Action(
					"ConfirmEmail",
					"Account",
					new { userId = userId, code = code },
					protocol: Request.Scheme);

				await _emailSender.SendEmailAsync(email, "Confirm your email",
					$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

				TempData["SuccessMessage"] = "Employee created successfully. A confirmation email has been sent.";
				return RedirectToAction("CreateEmployee");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View();
		}

		private User CreateUser()
		{
			try
			{
				return Activator.CreateInstance<User>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. Ensure it has a parameterless constructor.");
			}
		}

		private IUserEmailStore<User> GetEmailStore()
		{
			if (!_userManager.SupportsUserEmail)
			{
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<User>)_userStore;
		}
	}
}
