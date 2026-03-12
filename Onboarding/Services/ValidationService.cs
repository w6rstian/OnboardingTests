using Onboarding.Interfaces;
using System.Text.RegularExpressions;

namespace Onboarding.Services
{
    /// <summary>
    /// This service is for example register form data validation.
    /// It validates login, password, and email using regex and checks for duplicate usernames and emails.
    /// </summary>
    public partial class ValidationService(IUserRepository userRepository) : IValidationService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public bool ValidateLoginFormat(string login)
        {
            var loginRegex = MyRegex();
            return loginRegex.IsMatch(login);
        }

        public bool ValidatePasswordFormat(string password)
        {
            var passwordRegex = MyRegex1();
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

        [GeneratedRegex(@"^[a-zA-Z0-9]{5,50}$")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
        private static partial Regex MyRegex1();
    }
}
