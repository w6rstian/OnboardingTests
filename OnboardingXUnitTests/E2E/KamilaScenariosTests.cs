using Microsoft.Playwright;
using System.Text.Json;
using System.Linq;

namespace OnboardingXUnitTests.E2E
{
    public class KamilaScenariosTests : IAsyncLifetime
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
        public async Task ID1_Meeting_Participant_Filtering()
        {
            await Login("admin@mail.com", "AdminPassword123!");
            await _page.GotoAsync($"{_baseUrl}/Calendar/Index");
            await _page.ClickAsync("a:has-text('Zaplanuj spotkanie')");

            var options = await _page.Locator("select[name='SelectedUsersIds'] option").AllInnerTextsAsync();
            Assert.DoesNotContain("Admin User (admin@mail.com)", options);
            Assert.Contains("Nowy1 Nowak (nowy1@mail.com)", options);
        }

        [Fact]
        public async Task ID2_Create_Meeting_Multiple_Participants()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Calendar/CreateMeeting?type=General");

            string meetingTitle = $"Ważne spotkanie {Guid.NewGuid()}";
            await _page.FillAsync("input[name='Title']", meetingTitle);

            var start = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm");
            var end = DateTime.Now.AddDays(1).AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            await _page.FillAsync("input[name='Start']", start);
            await _page.FillAsync("input[name='End']", end);

            // Nowy1 i Nowy2
            await _page.SelectOptionAsync("select[name='SelectedUsersIds']", new[]
            {
                new SelectOptionValue { Label = "Nowy1 Nowak (nowy1@mail.com)" },
                new SelectOptionValue { Label = "Nowy2 Nowak (nowy2@mail.com)" }
            });

            await _page.ClickAsync("button[type='submit']:has-text('Zaplanuj spotkanie')");

            await _page.WaitForURLAsync($"{_baseUrl}/Calendar");
            Assert.Equal($"{_baseUrl}/Calendar", _page.Url);

            // weryfikacja API
            var response = await _page.APIRequest.GetAsync($"{_baseUrl}/Calendar/GetEvents");
            Assert.True(response.Ok);

            var json = await response.JsonAsync();
            var events = json?.EnumerateArray();

            Assert.NotNull(events);

            var createdMeeting = events.Value.FirstOrDefault(
                e => e.GetProperty("title").GetString() == meetingTitle);
            Assert.NotEqual(default, createdMeeting);

            var participants = createdMeeting.GetProperty("participants")
                .EnumerateArray().Select(p => p.GetString())
                .ToList();
            Assert.Contains("Nowy1 Nowak (nowy1@mail.com)", participants);
            Assert.Contains("Nowy2 Nowak (nowy2@mail.com)", participants);
        }

        [Fact]
        public async Task ID3_Filter_Calendar_Events_By_Type()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            var request = _page.APIRequest;

            var response = await request.GetAsync($"{_baseUrl}/Calendar/GetEvents?type=General");
            Assert.True(response.Ok);

            var json = await response.JsonAsync();
            Assert.NotNull(json);
        }

        [Fact]
        public async Task ID4_Chat_Visibility_Isolation()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Chat/UserList");
            await _page.ClickAsync("a:has-text('nowy2@mail.com')");
            string msg1To2 = $"{Guid.NewGuid()} Żwirek kręci z muchomorkiem!!!";
            await _page.FillAsync("#messageInput", msg1To2);
            await _page.ClickAsync("#messageForm button[type='submit']");

            await _page.ClickAsync("#logout");

            await Login("admin@mail.com", "AdminPassword123!");
            await _page.GotoAsync($"{_baseUrl}/Chat/UserList");
            await _page.ClickAsync("a:has-text('nowy1@mail.com')");
            string msg3To1 = $"{Guid.NewGuid()} Żwirek wcale nie kręci z muchomorkiem";
            await _page.FillAsync("#messageInput", msg3To1);
            await _page.ClickAsync("#messageForm button[type='submit']");

            await _page.ClickAsync("#logout");

            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Chat/UserList");
            await _page.ClickAsync("a:has-text('nowy2@mail.com')");

            var chatContent = await _page.InnerTextAsync("#messagesList");
            Assert.Contains(msg1To2, chatContent);

            Assert.DoesNotContain(msg3To1, chatContent);
        }

        [Fact]
        public async Task ID6_Chat_Empty_Message_Prevention()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Chat/UserList");
            await _page.ClickAsync("a:has-text('nowy2@mail.com')");

            var initialCount = await _page.Locator("#messagesList li").CountAsync();

            await _page.FillAsync("#messageInput", "");
            await _page.ClickAsync("#messageForm button[type='submit']");

            await _page.ReloadAsync();

            var afterCount = await _page.Locator("#messagesList li").CountAsync();

            Assert.Equal(initialCount, afterCount);
        }

        [Fact]
        public async Task ID7_Edit_Course_With_Image()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Courses");

            var editLink = _page.Locator("a:has-text('Edytuj')").First;
            var href = await editLink.GetAttributeAsync("href");
            var courseId = href.Split('/').Last();

            await editLink.ClickAsync();

            await _page.SetInputFilesAsync("input[name='ImageFile']", "../../../Assets/kurs.jpg");

            await _page.ClickAsync("button[type='submit']:has-text(' Zapisz zmiany ')");

            await _page.WaitForURLAsync($"{_baseUrl}/Courses");
            Assert.Contains("Courses", _page.Url);

            var courseImage = _page.Locator($"img[src*='/Courses/GetCourseImage/{courseId}']");
            await Assertions.Expect(courseImage).ToBeVisibleAsync();

            var response = await _page.APIRequest.GetAsync($"{_baseUrl}/Courses/GetCourseImage/{courseId}");
            Assert.True(response.Ok);
            Assert.Equal("image/jpeg", response.Headers["content-type"]);
        }

        [Fact]
        public async Task ID8_Newbie_Cannot_Create_Course()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Onboarding/Create");

            var content = await _page.InnerTextAsync("body");

            Assert.True(content.Contains("Access denied") || _page.Url.Contains("AccessDenied") ||
                        _page.Url.Contains("Login"));
        }

        [Fact]
        public async Task ID9_Course_Details_Integrity()
        {
            await Login("admin@mail.com", "AdminPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Courses");

            await _page.ClickAsync("a:has-text('Szczegóły')");

            var body = await _page.InnerTextAsync("body");

            Assert.DoesNotContain("NullReferenceException", body);
        }

        [Fact]
        public async Task ID10_Chat_Message_Sorting()
        {
            await Login("nowy1@mail.com", "NowyPassword123!");

            await _page.GotoAsync($"{_baseUrl}/Chat/UserList");
            await _page.ClickAsync("a:has-text('nowy2@mail.com')");
            string msgA = $"{Guid.NewGuid()} W Józka strzelił pierun";
            await _page.FillAsync("#messageInput", msgA);
            await _page.ClickAsync("#messageForm button[type='submit']");

            await _page.WaitForTimeoutAsync(500);

            string msgB = $"{Guid.NewGuid()} Nie wiem czym nie nagrał tego";
            await _page.FillAsync("#messageInput", msgB);
            await _page.ClickAsync("#messageForm button[type='submit']");

            var messages = (await _page.Locator("#messagesList li").AllInnerTextsAsync()).ToList();

            int indexA = messages.FindIndex(m => m.Contains(msgA));
            int indexB = messages.FindIndex(m => m.Contains(msgB));

            Assert.True(indexA < indexB);
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}