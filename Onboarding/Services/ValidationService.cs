using Onboarding.Interfaces;
using System.Text.RegularExpressions;

namespace Onboarding.Services
{
    /// <summary>
    /// This service is for example register form data validation.
    /// It validates login, password, and email using regex and checks for duplicate usernames and emails.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IUserRepository _userRepository;

        public ValidationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool ValidateLoginFormat(string login)
        {
            var loginRegex = new Regex(@"^[a-zA-Z0-9]{5,50}$");
            return loginRegex.IsMatch(login);
        }

        public bool ValidatePasswordFormat(string password)
        {
            var passwordRegex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            return passwordRegex.IsMatch(password);
        }

        public bool ValidateEmailFormat(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        public async Task<bool> IsLoginUniqueAsync(string login)
        {
            return !await _userRepository.UserExistsByLoginAsync(login);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _userRepository.UserExistsByEmailAsync(email);
        }
    }
}
