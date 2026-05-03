using Microsoft.AspNetCore.Routing;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OnboardingXUnitTests.E2E
{
    public class MikeleScenariosTests : PageTest
    {
       
        [Fact]
        public async Task RegisterShouldWork()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
            await Page.GetByRole(AriaRole.Heading, new() { Name = "Confirm email" }).ClickAsync();
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        }

        [Fact]
        public async Task NewCanGotoOwnCourses()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = " Kursy" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Moje kursy");
        }

     
        [Fact]
        public async Task UserCanEditOwnDataAndSeeChange()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Moje konto" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Edytuj dane" }).ClickAsync();
            await Page.Locator("#Name").ClickAsync();
            await Page.Locator("#Name").FillAsync("Maciek");
            await Page.Locator("#Name").PressAsync("Tab");
            await Page.Locator("#Surname").FillAsync("Super");
            await Page.Locator("#PhoneNumber").ClickAsync();
            await Page.Locator("#PhoneNumber").FillAsync("123456789");
            await Page.Locator("#Department").ClickAsync();
            await Page.Locator("#Department").FillAsync("IT");
            await Page.Locator("#Position").ClickAsync();
            await Page.Locator("#Position").FillAsync("Coder");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Zapisz zmiany" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Maciek");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Super");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("123456789");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("IT");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Coder");
        }

      
        [Fact]
        public async Task HrCanMakeUser()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("hr@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("HrPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Dodaj pracownika" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Imię" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Imię" }).FillAsync("Marek");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Nazwisko" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Nazwisko" }).FillAsync("Malinowski");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adres email" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adres email" }).FillAsync("malinowskimarek@mail.com");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Utwórz konto" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Wyświetl listę" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Marek");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Malinowski");
            await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("malinowskimarek@mail.com");
        }

        [Fact]
        public async Task RegisterWithMismatchedPasswordsShouldFail()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("nowy@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).FillAsync("DobreHaslo123!");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).FillAsync("ZleHaslo123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

          
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register" })).ToBeVisibleAsync();
        }

    
        [Fact]
        public async Task UserCannotLoginWithWrongPassword()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("CalkowicieBledneHaslo123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Log in" })).ToBeVisibleAsync();
        }


        [Fact]
        public async Task UserCanLogoutSuccessfully()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            
            await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

           
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        }

   
        [Fact]
        public async Task StandardUserCannotSeeHrOptions()
        {
            await Page.GotoAsync("http://localhost:5021/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("maciek@mail.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("MaciekPassword123!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Przejdź do panelu" }).ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = " Dodaj pracownika" })).Not.ToBeVisibleAsync();
        }
        //mocki
        private const string BaseUrl = "http://localhost:5021";

        [Fact]
        public async Task ShouldMockSuccessfulEmployeeCreationAndShowSuccess()
        {
            await Page.GotoAsync($"{BaseUrl}/HR/CreateEmployee*");

            await Page.RouteAsync("**/HR/CreateEmployee", async route =>
            {
                if (route.Request.Method == "POST")
                {
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "text/html",
                        Body = @"
                            <html>
                                <body>
                                    <div class=""alert alert-success"" id=""success-message"">
                                        Employee created successfully. A confirmation email has been sent.
                                    </div>
                                </body>
                            </html>"
                    });
                }
                else
                {
                    await route.ContinueAsync();
                }
            });

            await Page.FillAsync("input[name=\"name\"]", "Jan");
            await Page.FillAsync("input[name=\"lastname\"]", "Kowalski");
            await Page.FillAsync("input[name=\"email\"]", "jan.kowalski@firma.pl");

           await Page.ClickAsync("button[type=\"submit\"]");

            var successAlert = Page.Locator("#success-message");
            await Expect(successAlert).ToBeVisibleAsync();
            await Expect(successAlert).ToContainTextAsync("Employee created successfully");
        }

        [Fact]
        public async Task ShouldMockValidationErrorWhenDataIsMissing()
        {
            await Page.GotoAsync($"{BaseUrl}/HR/CreateEmployee");

            await Page.RouteAsync("**/HR/CreateEmployee", async route =>
            {
                if (route.Request.Method == "POST")
                {
                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = 400,
                        ContentType = "text/html",
                        Body = @"
                            <html>
                                <body>
                                    <div class=""validation-summary-errors"">
                                        <ul><li>All fields are required.</li></ul>
                                    </div>
                                </body>
                            </html>"
                    });
                }
                else
                {
                    await route.ContinueAsync();
                }
            });

            await Page.ClickAsync("button[type=\"submit\"]");

            var validationSummary = Page.Locator(".validation-summary-errors");
            await Expect(validationSummary).ToBeVisibleAsync();
            await Expect(validationSummary).ToContainTextAsync("All fields are required.");
        }

        [Fact]
        public async Task ShouldBlockNavigationWhenServerReturns500()
        {
            await Page.GotoAsync($"{BaseUrl}/HR/HRPanel");

            await Expect(Page.Locator("h1.main-title")).ToHaveTextAsync("Panel HR");

            await Page.RouteAsync("**/HR/CreateEmployee", async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    Status = 500,
                    ContentType = "text/html",
                    Body = "<h1>Wewnetrzny blad serwera (500)</h1><p>Baza danych nie odpowiada.</p>"
                });
            });

            await Page.ClickAsync("a[href=\"/HR/CreateEmployee\"]");

            await Expect(Page.Locator("h1")).ToHaveTextAsync("Wewnetrzny blad serwera (500)");
            await Expect(Page.Locator("p")).ToHaveTextAsync("Baza danych nie odpowiada.");
        }

        //stan
        
        

    }

}
