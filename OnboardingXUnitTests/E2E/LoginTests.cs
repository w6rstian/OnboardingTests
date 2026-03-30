using Microsoft.Playwright;

namespace OnboardingXUnitTests.E2E
{
    public class LoginTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                SlowMo = 100
            });

            _page = await _browser.NewPageAsync();
        }

        [Fact]
        public async Task Login_Should_Work()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");


            await _page.FillAsync("#Input_Email", "nowy1@mail.com");
            await _page.FillAsync("#Input_Password", "NowyPassword123!");


            await _page.ClickAsync("button[type='submit']");


            await _page.WaitForSelectorAsync("#logout");


            Assert.True(await _page.IsVisibleAsync("#logout"));
        }

        [Fact]
        public async Task Login_Should_Not_Work()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");


            await _page.FillAsync("#Input_Email", "nowy1@mail.com");
            await _page.FillAsync("#Input_Password", "ZlyPassword123!");


            await _page.ClickAsync("button[type='submit']");

            await _page.WaitForSelectorAsync("text=Invalid login attempt.");

            Assert.True(await _page.IsVisibleAsync("text=Invalid login attempt."));

            Assert.Contains("Login", _page.Url);
        }

        [Fact]
        public async Task Admin_Can_Add_New_Course_And_Delete()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");

            await _page.FillAsync("#Input_Email", "admin@mail.com");
            await _page.FillAsync("#Input_Password", "AdminPassword123!");


            await _page.ClickAsync("button[type='submit']");


            await _page.WaitForSelectorAsync("#logout");


            Assert.True(await _page.IsVisibleAsync("#logout"));


            await _page.ClickAsync("a[href='/Admin/AdminPanel']");

            await _page.ClickAsync("a[href='/Onboarding/Create']");

            await _page.SetInputFilesAsync("input[name='ImageFile']", "../../../Assets/kurs.jpg");

            await _page.FillAsync("#CourseName", "KursTestPlaywright1337");

            await _page.ClickAsync("button:has-text('Utwórz')");

            await _page.WaitForURLAsync("https://localhost:7231/Courses");

            var courseVisible = await _page.IsVisibleAsync("h5:has-text('KursTestPlaywright1337')");
            Assert.True(courseVisible, "Nowy kurs 'KursTest' nie pojawił się na liście.");

            await _page.ClickAsync("h5.card-title.fw-bold:text('KursTest') >> xpath=../.. >> text=Usuń");

            await _page.WaitForURLAsync("**/Courses/Delete/**");

            await _page.ClickAsync("input[type='submit'][value='Usuń']");

            await _page.WaitForURLAsync("https://localhost:7231/Courses");

            var courseStillVisible = await _page.IsVisibleAsync("h5.card-title.fw-bold:text('KursTestPlaywright1337')");
            Assert.False(courseStillVisible, "Kurs powinien zostać usunięty z listy.");
        }

        [Fact]
        public async Task Admin_Can_View_Statistics_And_Report()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");
            await _page.FillAsync("#Input_Email", "admin@mail.com");
            await _page.FillAsync("#Input_Password", "AdminPassword123!");
            await _page.ClickAsync("button[type='submit']");
            await _page.WaitForSelectorAsync("#logout");
            Assert.True(await _page.IsVisibleAsync("#logout"));

            await _page.ClickAsync("a[href='/Admin/AdminPanel']");
            await _page.ClickAsync("a[href='/StatisticReport/Index']");

            var chartVisible = await _page.IsVisibleAsync("#usersByRoleChart");
            Assert.True(chartVisible, "Wykres użytkowników powinien być widoczny.");

            await _page.Locator("h2:text('Użytkownicy o roli \"Nowy\"')").ScrollIntoViewIfNeededAsync();

            var userRow = _page.Locator("tbody tr").Filter(new() { HasText = "Nowy1 Nowak" });
            await userRow.Locator("button.show-new-user-report").ClickAsync();

            var reportVisible = await _page.IsVisibleAsync("#newUserReport");
            Assert.True(reportVisible, "Sekcja raportu użytkownika powinna być widoczna.");

            var reportHeader = await _page.TextContentAsync("#reportHeader");
            Assert.Contains("Nowy1 Nowak", reportHeader);
        }

        [Fact]
        public async Task Admin_Cannot_Add_Task_Without_Name()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");
            await _page.FillAsync("#Input_Email", "admin@mail.com");
            await _page.FillAsync("#Input_Password", "AdminPassword123!");
            await _page.ClickAsync("button[type='submit']");
            await _page.WaitForSelectorAsync("#logout");

            await _page.ClickAsync("a[href='/Admin/AdminPanel']");
            await _page.ClickAsync("a[href='/Tasks/Create']");

            await _page.ClickAsync("input[type='submit']");

            var errorMessage = await _page.Locator("#Title-error").InnerTextAsync();
            Assert.Equal("The Title field is required.", errorMessage.Trim());
        }

        [Fact]
        public async Task RegularUser_Cannot_Access_AdminPanel()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");

            await _page.FillAsync("#Input_Email", "nowy1@mail.com");
            await _page.FillAsync("#Input_Password", "NowyPassword123!");
            await _page.ClickAsync("button[type='submit']");

            await _page.WaitForSelectorAsync("#logout");
            Assert.True(await _page.IsVisibleAsync("#logout"), "Użytkownik powinien być zalogowany");

            await _page.GotoAsync("https://localhost:7231/Admin/AdminPanel");

            var hasAccessDeniedMessage = await _page.Locator("body").InnerTextAsync();

            Assert.True(hasAccessDeniedMessage.Contains("Access denied", StringComparison.OrdinalIgnoreCase),
                "Powinien pojawić się komunikat o braku uprawnień do panelu administratora.");

            Assert.DoesNotMatch("https://localhost:7231/Admin/AdminPanel", _page.Url);

            Assert.False(await _page.IsVisibleAsync("h1:text('Panel Administratora')"),
                "Elementy panelu administratora nie powinny być widoczne.");
        }

        [Fact]
        public async Task RegularUser_Cannot_RateBuddy_When_NotAssigned()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");

            await _page.FillAsync("#Input_Email", "nowy2@mail.com");
            await _page.FillAsync("#Input_Password", "NowyPassword123!");
            await _page.ClickAsync("button[type='submit']");

            await _page.WaitForSelectorAsync("#logout");
            Assert.True(await _page.IsVisibleAsync("#logout"), "Użytkownik powinien być zalogowany");

            await _page.ClickAsync("a.nav-link[href='/UserCoursesList']");

            await _page.ClickAsync("a[href='/Rewards/RateBuddy']");

            var messageText = await _page.Locator("pre").InnerTextAsync();

            Assert.Contains("Brak przypisanego Buddy'ego", messageText);
            Assert.Equal("Brak przypisanego Buddy'ego.", messageText.Trim());
        }

        [Fact]
        public async Task RegularUser_Course_List_Is_Empty()
        {
            await _page.GotoAsync("https://localhost:7231/Identity/Account/Login");

            await _page.FillAsync("#Input_Email", "nowy2@mail.com");
            await _page.FillAsync("#Input_Password", "NowyPassword123!");
            await _page.ClickAsync("button[type='submit']");

            await _page.WaitForSelectorAsync("#logout");
            Assert.True(await _page.IsVisibleAsync("#logout"), "Użytkownik powinien być zalogowany");

            await _page.ClickAsync("a.nav-link[href='/UserCoursesList']");

            Assert.True(await _page.IsVisibleAsync(".no-data.text-center.mt-4"),
                "Sekcja 'no-data' powinna być widoczna.");

            Assert.True(await _page.IsVisibleAsync(".no-data i.bi.bi-emoji-frown"),
                "Ikona bi-emoji-frown powinna być widoczna.");

            var messageText = await _page.Locator(".no-data p").InnerTextAsync();

            Assert.Equal("Nie jesteś przypisany do żadnego kursu.", messageText.Trim());

            Assert.False(await _page.IsVisibleAsync(".card"), "Nie powinny być widoczne karty kursów.");
            Assert.False(await _page.IsVisibleAsync("table"), "Nie powinna być widoczna tabela z kursami.");
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}