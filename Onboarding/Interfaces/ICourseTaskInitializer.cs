using System;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface ICourseTaskInitializer
    {
        Task SeedCoursesAndTasksAsync(IServiceProvider serviceProvider);
    }
}
