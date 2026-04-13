using Microsoft.Playwright;

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

        private async Task Login(string email, string password)
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Login");
            await _page.FillAsync("#Input_Email", email);
            await _page.FillAsync("#Input_Password", password);
            await _page.ClickAsync("button[type='submit']");
            await _page.WaitForSelectorAsync("#logout");
        }

        [Fact]
        public async Task Registration_Passwords_Do_Not_Match_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.FillAsync("#Input_Email", "nowy_test@mail.com");
            await _page.FillAsync("#Input_Password", "Haslo123!");
            await _page.FillAsync("#Input_ConfirmPassword", "InneHaslo456!");

            await _page.ClickAsync("#registerSubmit");

            var errorMessage = await _page.Locator("span[data-valmsg-for='Input.ConfirmPassword']").InnerTextAsync();
            Assert.Contains("match", errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Registration_Invalid_Email_Format_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.FillAsync("#Input_Email", "zly_adres.com");
            await _page.FillAsync("#Input_Password", "Haslo123!");
            await _page.FillAsync("#Input_ConfirmPassword", "Haslo123!");

            await _page.ClickAsync("#registerSubmit");

            var errorSpan = _page.Locator("span[data-valmsg-for='Input.Email']");
            await Assertions.Expect(errorSpan).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Registration_Password_Too_Short_Shows_Error()
        {
            await _page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

            await _page.FillAsync("#Input_Email", "nowy_test2@mail.com");
            await _page.FillAsync("#Input_Password", "Krotk");
            await _page.FillAsync("#Input_ConfirmPassword", "Krotk");

            await _page.ClickAsync("#registerSubmit");

            var bodyText = await _page.InnerTextAsync("body");
            Assert.True(bodyText.Contains("at least 6"),
                "System powinien zablokować hasło krótsze niż 6 znaków.");
        }

        [Fact]
        public async Task Guest_Cannot_Access_Protected_Page_Redirects_To_Login()
        {
            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            Assert.Contains("Login", _page.Url);
            Assert.Contains("ReturnUrl", _page.Url);
        }

        [Fact]
        public async Task Logout_User_Redirects_To_Home_Page()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.ClickAsync("#logout");

            await _page.WaitForURLAsync($"{_baseUrl}/");
            Assert.Equal($"{_baseUrl}/", _page.Url);

            var isLogoutVisible = await _page.IsVisibleAsync("#logout");
            Assert.False(isLogoutVisible, "Przycisk wylogowania nie powinien być widoczny po wylogowaniu.");
        }

        [Fact]
        public async Task Admin_Cannot_Create_OnboardingCourse_Without_Name()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            await _page.EvaluateAsync("document.getElementById('CourseName').removeAttribute('required')");

            await _page.ClickAsync("button[type='submit']:has-text('Utwórz')");

            var bodyLocator = _page.Locator("body");
            await Assertions.Expect(bodyLocator).ToContainTextAsync("Nazwa kursu jest wymagana");
        }

        [Fact]
        public async Task Create_Question_Form_Loads_Correctly()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Questions/Create");

            var isDescriptionInputVisible = await _page.IsVisibleAsync("input[name='Description'], textarea[name='Description']");
            Assert.True(isDescriptionInputVisible, "Pole 'Description' pytania powinno być widoczne na formularzu.");
        }

        [Fact]
        public async Task API_StatisticReport_GetUsersByRole_Returns_Success()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            var response = await _page.APIRequest.GetAsync($"{_baseUrl}/StatisticReport/GetUsersByRole?role=Nowy");

            Assert.True(response.Ok);

            var jsonResponse = await response.JsonAsync();
            Assert.NotNull(jsonResponse);
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}