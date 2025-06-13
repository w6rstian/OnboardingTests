using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Onboarding.Models;
using System.Text.Encodings.Web;
using System.Text;

namespace Onboarding.Controllers
{
	public class UserController : Controller
	{
		private UserManager<User> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IUserStore<User> _userStore;
		
		public UserController(UserManager<User> userManager, IEmailSender emailSender, IUserStore<User> userStore)
		{
			_userManager = userManager;
			_userStore = userStore;
			_emailSender = emailSender;

		}
		
		public IActionResult MainPage()
		{
			return View();
		}
		public IActionResult UserPanel()
		{
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> MyAccount()
		{
			var user = await _userManager.GetUserAsync(User);
			
			return View(user);
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> MyAccount(string name, string lastname,string email,string phone,string dept,string pos)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound();
			}


			user.Name = name;
			user.Surname = lastname;
			user.Department = dept;
			user.Position = pos;
			user.PhoneNumber = phone;
			
			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
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
				return RedirectToAction("MyAccount");
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View();
		}

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserSettings(User model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Department = model.Department;
            user.Position = model.Position;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Dane użytkownika zostały zaktualizowane.";
                return RedirectToAction("MyAccount");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


    }

}
