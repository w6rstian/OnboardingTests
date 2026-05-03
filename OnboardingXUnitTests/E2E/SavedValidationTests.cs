using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnboardingXUnitTests.E2E
{
    public class SavedValidationTests : PageTest
    {
        [Fact]
        public async Task SetupAuth()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("nowy1@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("NowyPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();


            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" })).ToBeVisibleAsync();


            await Context.StorageStateAsync(new() { Path = "auth.json" });
        }
        [Fact]
        public async Task UserCanEditOwnDataAndSeeChange_SavedValidation()
        {

            var context = await Browser.NewContextAsync(new()
            {
                StorageStatePath = "auth.json"
            });

            var page = await context.NewPageAsync();


            await page.GotoAsync("http://localhost:5021/");


            await page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = " Moje konto" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = " Edytuj dane" }).ClickAsync();

            await page.Locator("#Name").FillAsync("Maciek");
            await page.Locator("#Surname").FillAsync("Super");
            await page.Locator("#PhoneNumber").FillAsync("123456789");
            await page.Locator("#Department").FillAsync("IT");
            await page.Locator("#Position").FillAsync("Coder");

            await page.GetByRole(AriaRole.Button, new() { Name = "Zapisz zmiany" }).ClickAsync();


            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Maciek");
            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Super");
            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("123456789");
            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("IT");
            await Expect(page.GetByRole(AriaRole.Main)).ToContainTextAsync("Coder");
        }

        [Fact]
        public async Task BuddyCanAccessPanelUsingStoredSession_SavedValidation()
        {
            var context = await Browser.NewContextAsync(new()
            {
                StorageStatePath = "auth.json"
            });

            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5021/Buddy/BuddyPanel");

            await Expect(page).Not.ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Account/Login.*"));

            var header = page.Locator("h1.main-title");
            await Expect(header).ToBeVisibleAsync();
            await Expect(header).ToHaveTextAsync("Panel Buddyego");

            await Expect(page.Locator("a[href='/Buddy/Newbies']")).ToBeVisibleAsync();
            await Expect(page.Locator("a[href='/Buddy/TaskStatus']")).ToBeVisibleAsync(); 
    }
}
