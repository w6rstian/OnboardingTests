using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace OnboardingXUnitTests.E2E
{
    public class RegistrationAndFlowTests : IAsyncLifetime
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

        // Przypadek testowy: ID-x
        // Opis: Weryfikacja walidacji formularza rejestracji. Sprawdzenie, czy po wpisaniu dwóch różnych haseł i kliknięciu submit wyświetla się komunikat o błędzie.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Registration_Passwords_Do_Not_Match_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.GetByLabel("Email").FillAsync("nowy_test@mail.com");
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Haslo123!");
            await _page.GetByLabel("Confirm password", new() { Exact = false }).FillAsync("InneHaslo456!"); // Zmień na "Potwierdź hasło" jeśli to polski UI

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Register|Zarejestruj", RegexOptions.IgnoreCase) }).ClickAsync();

            var errorMessage = _page.GetByText(new Regex("match|nie pasują|nie zgadzają", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(errorMessage).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID-x
        // Opis: Weryfikacja walidacji po stronie serwera. Administrator próbuje dodać nowy kurs omijając blokadę HTML, system powinien odrzucić żądanie i wyświetlić błąd.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Admin_Cannot_Create_OnboardingCourse_Without_Name()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            await _page.EvaluateAsync("document.getElementById('CourseName').removeAttribute('required')");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Utwórz" }).ClickAsync();

            var errorMsg = _page.GetByText("Nazwa kursu jest wymagana");
            await Assertions.Expect(errorMsg).ToBeVisibleAsync();
        }

        // Przypadek testowy: ID-x 
        // Opis: Pełna ścieżka logowania i wylogowania użytkownika. Sprawdzenie przekierowań i ukrywania elementów interfejsu (przycisku wylogowania) po sukcesie.
        // Autor: Wojciech Jurkowicz
        [Fact]
        public async Task Logout_User_Redirects_To_Home_Page()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.Locator("#logout").ClickAsync();

            await _page.WaitForURLAsync($"{_baseUrl}/");
            Assert.Equal($"{_baseUrl}/", _page.Url);

            var isLogoutVisible = await _page.Locator("#logout").IsVisibleAsync();
            Assert.False(isLogoutVisible, "Przycisk wylogowania nie powinien być widoczny po wylogowaniu."); ;
        }

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

        [Fact]
        public async Task Registration_Password_Too_Short_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.GetByLabel("Email").FillAsync("nowy_test2@mail.com");
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Krotk");
            await _page.GetByLabel("Confirm password", new() { Exact = false }).FillAsync("Krotk");

            await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Register|Zarejestruj", RegexOptions.IgnoreCase) }).ClickAsync();

            var shortPasswordError = _page.GetByText(new Regex("at least 6|co najmniej 6", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(shortPasswordError).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Guest_Cannot_Access_Protected_Page_Redirects_To_Login()
        {
            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            Assert.Contains("Login", _page.Url);
            Assert.Contains("ReturnUrl", _page.Url);
        }

        [Fact]
        public async Task Create_Question_Form_Loads_Correctly()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Questions/Create");

            var descriptionField = _page.GetByLabel(new Regex("Description|Opis", RegexOptions.IgnoreCase)).First;
            await Assertions.Expect(descriptionField).ToBeVisibleAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}