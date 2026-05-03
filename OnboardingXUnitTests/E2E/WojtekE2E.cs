using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace OnboardingXUnitTests.E2E
{
    public class WojtekE2E : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private string _baseUrl = "https://localhost:7231";

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                SlowMo = 100
            });
            _page = await _browser.NewPageAsync(new BrowserNewPageOptions
            {
                IgnoreHTTPSErrors = true
            });
        }

        // helper method
        private async Task Login(string email, string password)
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Login");
            await _page.GetByLabel("Email").FillAsync(email);
            await _page.GetByLabel("Password", new() { Exact = false }).FillAsync(password);
            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Log in|Zaloguj", RegexOptions.IgnoreCase) }).ClickAsync();
            await _page.Locator("#logout").WaitForAsync();
        }

        // Przypadek testowy: ID: 2_WJ
        // Opis: Walidacja formatu adresu e-mail podczas rejestracji użytkownika.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Registration_Invalid_Email_Format_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.GetByLabel("Email").FillAsync("zly_adres.com");
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Haslo123!");
            await _page.GetByLabel("Confirm password", new() { Exact = false }).FillAsync("Haslo123!");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Register|Zarejestruj", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorSpan = _page.GetByText(new Regex("valid e-mail|valid email|prawidłowym adresem|nieprawidłowy|invalid", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(errorSpan).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 8_WJ
        // Opis: Próba utworzenia kursu Onboardingowego bez zdefiniowania nazwy kursu. Sprawdzenie walidacji backendowej.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Admin_Cannot_Create_OnboardingCourse_Without_Name()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            await _page.EvaluateAsync("document.getElementById('CourseName').removeAttribute('required')");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Utwórz|Create", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorMsg = _page.GetByText("Nazwa kursu jest wymagana");
            await Assertions.Expect(errorMsg).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 10_WJ
        // Opis: Próba utworzenia pytania bez powiązania go z istniejącym testem. Weryfikacja błędu walidacji.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Create_Question_Without_Test_Shows_Error()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Questions/Create");

            await _page.GetByLabel(new Regex("Description")).FillAsync("Czym jest C#?");
            await _page.GetByLabel(new Regex("Answer ?A")).FillAsync("Językiem programowania");
            await _page.GetByLabel(new Regex("Answer ?B")).FillAsync("Samochodem");
            await _page.GetByLabel(new Regex("Answer ?C")).FillAsync("Zwierzęciem");
            await _page.GetByLabel(new Regex("Answer ?D")).FillAsync("Planetą");
            await _page.GetByLabel(new Regex("Correct ?Answer")).FillAsync("A");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Create", RegexOptions.IgnoreCase) }).ClickAsync();

            var validationError = _page.GetByText(new Regex("required|wymagane", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(validationError).ToBeVisibleAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}