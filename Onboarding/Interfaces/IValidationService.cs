using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IValidationService
    {
        bool ValidateLoginFormat(string login);
        bool ValidatePasswordFormat(string password);
        bool ValidateEmailFormat(string email);
        Task<bool> IsLoginUniqueAsync(string login);
        Task<bool> IsEmailUniqueAsync(string email);
    }
}
