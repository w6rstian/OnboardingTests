using System;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IRoleInitializer
    {
        Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider);
    }
}
