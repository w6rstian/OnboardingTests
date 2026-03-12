using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Interfaces;
using System.Threading.Tasks;

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

		public async Task<bool> UserExistsByLoginAsync(string login)
		{
			return await _context.Users.AnyAsync(u => u.Login == login);
		}

		public async Task<bool> UserExistsByEmailAsync(string email)
		{
			return await _context.Users.AnyAsync(u => u.Email == email);
		}
	}
}
