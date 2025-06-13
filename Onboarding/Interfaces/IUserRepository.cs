namespace Onboarding.Interfaces
{
	/// <summary>
	/// This interface is for example register form data validation.
	/// </summary>
	public interface IUserRepository
	{
		Task<bool> UserExistsByLoginAsync(string login);
		Task<bool> UserExistsByEmailAsync(string email);
	}
}
