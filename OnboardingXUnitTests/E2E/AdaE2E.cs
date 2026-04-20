using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace OnboardingXUnitTests.E2E;

public class AdaE2E : IAsyncLifetime
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
    public async Task ID01_HR_Sidebar_NavigateToCreateEmployee()
    {
        await Login("admin@mail.com", "AdminPassword123!");
        await _page.GotoAsync($"{_baseUrl}/HR/HRPanel");
        await _page.ClickAsync("a:has-text('Dodaj pracownika')");
        await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/HR/CreateEmployee"));
    }

    [Fact]
    public async Task ID02_HR_Panel_Statistics_Visible()
    {
        await Login("admin@mail.com", "AdminPassword123!");
        await _page.GotoAsync($"{_baseUrl}/HR/HRPanel");
        var statsHeader = _page.Locator("h3:has-text('Statystyki')");
        await Assertions.Expect(statsHeader).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ID03_NewEmployee_ShouldNotSee_AddEmployeeButton()
    {
        await Login("nowy1@mail.com", "NowyPassword123!");
        await _page.GotoAsync($"{_baseUrl}/HR/HRPanel");
        var addEmployeeButton = _page.GetByRole(AriaRole.Link, new() { Name = "Dodaj pracownika" });
        await Assertions.Expect(addEmployeeButton).ToBeHiddenAsync();
    }

    [Fact]
    public async Task ID04_HR_BackButton_Works()
    {
        await Login("hr@mail.com", "HrPassword123!");
        await _page.GotoAsync($"{_baseUrl}/HR/HRPanel");
        await _page.ClickAsync("a:has-text('Dodaj pracownika')");
        await _page.ClickAsync("a:has-text('Powrót')");
        await Assertions.Expect(_page).ToHaveURLAsync(new Regex(".*/HR/HRPanel"));
    }

    [Fact]
    public async Task ID05_HR_CreateEmployee_Success()
    {
        await Login("hr@mail.com", "HrPassword123!");
        await _page.GotoAsync($"{_baseUrl}/HR/CreateEmployee");
        await _page.FillAsync("#name", "Test");
        await _page.FillAsync("#lastname", "Pracownik");
        await _page.FillAsync("#email", $"test_{Guid.NewGuid()}@mail.com");

        await _page.ClickAsync("button[type='submit']");
        await Assertions.Expect(_page).ToHaveURLAsync($"{_baseUrl}/");
    }

    [Fact]
    public async Task ID06_Buddy_Panel_Navigation()
    {
        await Login("buddy1@mail.com", "BuddyPassword123!"); 
        await _page.GotoAsync($"{_baseUrl}/Buddy/BuddyPanel");

        await _page.ClickAsync("text='Podgląd nowych'");
        await Assertions.Expect(_page).ToHaveURLAsync(new Regex(".*/Buddy/Newbies"));
    }

    [Fact]
    public async Task ID07_Buddy_TaskStatus_AccordionExpands()
    {
        await Login("buddy1@mail.com", "BuddyPassword123!");
        await _page.GotoAsync($"{_baseUrl}/Buddy/TaskStatus");
        var accordionButton = _page.Locator(".accordion-button").First;

        if (await accordionButton.CountAsync() == 0)
        {
            // Jeśli baza jest pusta test przejdzie jeśli zobaczymy komunikat o braku pracowników
            var noDataMsg = _page.Locator(".no-data");
            await Assertions.Expect(noDataMsg).ToBeVisibleAsync();
        }
        else
        {
            await accordionButton.ClickAsync();
            var accordionCollapse = _page.Locator(".accordion-collapse").First;
            await Assertions.Expect(accordionCollapse).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ID08_Buddy_TaskStatus_BadgeVerification()
    {
        await Login("buddy1@mail.com", "BuddyPassword123!");
        await _page.GotoAsync($"{_baseUrl}/Buddy/TaskStatus");
        var accordionButton = _page.Locator(".accordion-button").First;

        if (await accordionButton.CountAsync() > 0)
        {
            await accordionButton.ClickAsync();
            var badge = _page.Locator(".badge").First;
            await Assertions.Expect(badge).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ID09_Buddy_NoNewbies_Message()
    {
        await Login("buddy2@mail.com", "BuddyPassword123!"); 
        await _page.GotoAsync($"{_baseUrl}/Buddy/TaskStatus");
        var noDataMsg = _page.Locator(".no-data");
        await Assertions.Expect(noDataMsg).ToHaveTextAsync("Nie masz przypisanych żadnych pracowników.");
    }

    [Fact]
    public async Task ID10_Unauthorized_Access_Redirects()
    {
        await Login("nowy1@mail.com", "NowyPassword123!"); 
        await _page.GotoAsync($"{_baseUrl}/Buddy/BuddyPanel");

        await Assertions.Expect(_page).Not.ToHaveURLAsync(new Regex(".*/Buddy/BuddyPanel"));
    }

public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
    
}

