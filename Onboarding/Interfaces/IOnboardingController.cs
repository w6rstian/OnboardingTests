using Microsoft.AspNetCore.Mvc;
using Onboarding.ViewModels;
using System.Threading.Tasks;

namespace Onboarding.Interfaces
{
    public interface IOnboardingController
    {
        IActionResult Create();
        Task<IActionResult> Create(CreateOnboardingViewModel viewModel);
    }
}
