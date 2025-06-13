using Onboarding.Interfaces;
using System.Threading.Tasks;

namespace Onboarding.Services
{
    public class IValidationService
    {
        public bool ValidateLoginFormat(string login)
        {
            return !string.IsNullOrEmpty(login) && login.Length >= 5 && login.Length <= 50 && login.All(char.IsLetterOrDigit);
        }

        public bool ValidatePasswordFormat(string password)
        {
            return password.Length >= 8 && password.Any(char.IsLetter) &&
                   password.Any(char.IsDigit) && password.Any(c => !char.IsLetterOrDigit(c));
        }

        public bool ValidateEmailFormat(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        public Task<bool> IsLoginUniqueAsync(string login)
        {
            // Tutaj dodaj logikę sprawdzania unikalności loginu, np. w bazie danych
            return Task.FromResult(true);
        }

        public Task<bool> IsEmailUniqueAsync(string email)
        {
            // Tutaj dodaj logikę sprawdzania unikalności emaila
            return Task.FromResult(true);
        }
    }
}
