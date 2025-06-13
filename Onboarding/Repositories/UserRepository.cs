using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Interfaces;

namespace Onboarding.Repositories
{
	/// <summary>
	/// This repository is for example register form data validation.
	/// It looks within the database for user login and email.
	/// </summary>
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext _context;

		public UserRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		async Task<bool> IUserRepository.UserExistsByLoginAsync(string login)
		{
			return await _context.Users.AnyAsync(u => u.Login == login);
		}

		async Task<bool> IUserRepository.UserExistsByEmailAsync(string email)
		{
			return await _context.Users.AnyAsync(u => u.Email == email);
		}
	}
}
