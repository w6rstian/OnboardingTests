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

        // Przypadek testowy: ID: 1_WJ
        // Opis: Walidacja niezgodności haseł podczas rejestracji 
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task ID1_Registration_Passwords_Do_Not_Match_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.GetByLabel("Email").FillAsync("nowy_test@mail.com");
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Haslo123!");
            await _page.GetByLabel("Confirm password", new() { Exact = false }).FillAsync("InneHaslo456!");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Register|Zarejestruj", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorMessage = _page.GetByText(new Regex("match|nie pasują|nie zgadzają", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(errorMessage).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 2_WJ
        // Opis: Walidacja formatu adresu e-mail podczas rejestracji użytkownika.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task ID2_Registration_Invalid_Email_Format_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.GetByLabel("Email").FillAsync("zly_adres.com");
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Haslo123!");
            await _page.GetByLabel("Confirm password", new() { Exact = false }).FillAsync("Haslo123!");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Register|Zarejestruj", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorSpan = _page.GetByText(new Regex("valid e-mail|valid email|prawidłowym adresem|nieprawidłowy|invalid", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(errorSpan).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 3_WJ
        // Opis: Próba wystawienia oceny mentorowi bez podania obowiązkowego komentarza (Feedback).
        // Autor: Wojciech Jurkowicz
        // [Fact]
        public async Task ID3_RateMentor_Without_Feedback_Shows_Error()
        {
            await Login("nowy2@mail.com", "NowyPassword123!");
            await _page.GotoAsync($"{_baseUrl}/Rewards/RateMentor?taskId=1");

            await _page.Locator("#Rating").FillAsync("5");
            await _page.Locator("#Feedback").FillAsync("");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Wyślij ocenę|Zapisz", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorMessage = _page.GetByText("wymagany", new() { Exact = false }).First;
            await Assertions.Expect(errorMessage).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 4_WJ
        // Opis: Pomyślne wystawienie oceny i komentarza przypisanemu Buddy'emu.
        // Autor: Wojciech Jurkowicz
        // [Fact]
        public async Task ID4_RateBuddy_With_Valid_Data_Succeeds()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");
            await _page.GotoAsync($"{_baseUrl}/Rewards/RateBuddy");

            await _page.Locator("#Rating").FillAsync("4");
            await _page.Locator("#Feedback").FillAsync("Bardzo pomocny w pierwszych dniach!");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Wyślij ocenę|Zapisz", RegexOptions.IgnoreCase) }).ClickAsync();

            await _page.WaitForURLAsync($"{_baseUrl}/UserCoursesList");
            Assert.Contains("UserCoursesList", _page.Url);
        }

        // Przypadek testowy: ID: 5_WJ
        // Opis: Ochrona dostępu do kreatora kursu Onboardingowego dla użytkowników bez uprawnień.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task ID5_Newbie_Cannot_Access_OnboardingCreate()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");
            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            var content = await _page.InnerTextAsync("body");
            Assert.True(content.Contains("Access denied", StringComparison.OrdinalIgnoreCase) ||
                        _page.Url.Contains("AccessDenied") ||
                        _page.Url.Contains("Login"));
        }

        // Przypadek testowy: ID: 6_WJ
        // Opis: Próba utworzenia kursu Onboardingowego bez zdefiniowania nazwy kursu. Sprawdzenie walidacji backendowej.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task ID6_Admin_Cannot_Create_OnboardingCourse_Without_Name()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            await _page.EvaluateAsync("document.getElementById('CourseName').removeAttribute('required')");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Utwórz|Create", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorMsg = _page.GetByText("Nazwa kursu jest wymagana");
            await Assertions.Expect(errorMsg).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID: 7_WJ
        // Opis: Próba utworzenia pytania bez powiązania go z istniejącym testem. Weryfikacja błędu walidacji.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task ID7_Create_Question_Without_Test_Shows_Error()
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