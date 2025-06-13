using Microsoft.AspNetCore.Identity;
using Onboarding.Models;

namespace Onboarding.Services
{
    public static class RoleInitializer
    {
        public static async System.Threading.Tasks.Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var roles = new List<string>
            {
                "Admin",
                "Buddy",
                "Mentor",
                "Manager",
                "Nowy",
                "HR",
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            //konto HR
            var hrUser = await userManager.FindByEmailAsync("hr@mail.com");
            if (hrUser == null)
            {
                var newUser = new User
                {
                    UserName = "hr@mail.com",
                    Email = "hr@mail.com",
                    Name = "HR",
                    Surname = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "HrPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "HR");
                }
            }

            //konto Admina
            var adminUser = await userManager.FindByEmailAsync("admin@mail.com");
            if (adminUser == null)
            {
                var newUser = new User
                {
                    UserName = "admin@mail.com",
                    Email = "admin@mail.com",
                    Name = "Admin",
                    Surname = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Admin");
                }
            }

            //konto Buddy1
            var buddyUser1 = await userManager.FindByEmailAsync("buddy1@mail.com");
            if (buddyUser1 == null)
            {
                var newUser = new User
                {
                    UserName = "buddy1@mail.com",
                    Email = "buddy1@mail.com",
                    Name = "Buddy1",
                    Surname = "Chad",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "BuddyPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Buddy");
                }
            }

            //konto Buddy2
            var buddyUser2 = await userManager.FindByEmailAsync("buddy2@mail.com");
            if (buddyUser2 == null)
            {
                var newUser = new User
                {
                    UserName = "buddy2@mail.com",
                    Email = "buddy2@mail.com",
                    Name = "Buddy2",
                    Surname = "Guy",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "BuddyPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Buddy");
                }
            }

            //konto Mentor1
            var mentorUser1 = await userManager.FindByEmailAsync("mentor1@mail.com");
            if (mentorUser1 == null)
            {
                var newUser = new User
                {
                    UserName = "mentor1@mail.com",
                    Email = "mentor1@mail.com",
                    Name = "Mentor1",
                    Surname = "Chad",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "MentorPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Mentor");
                }
            }

            //konto Mentor2
            var mentorUser2 = await userManager.FindByEmailAsync("mentor2@mail.com");
            if (mentorUser2 == null)
            {
                var newUser = new User
                {
                    UserName = "mentor2@mail.com",
                    Email = "mentor2@mail.com",
                    Name = "Mentor2",
                    Surname = "Guy",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "MentorPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Mentor");
                }
            }

            //konto Manager1
            var managerUser1 = await userManager.FindByEmailAsync("manager1@mail.com");
            if (managerUser1 == null)
            {
                var newUser = new User
                {
                    UserName = "manager1@mail.com",
                    Email = "manager1@mail.com",
                    Name = "Manager1",
                    Surname = "Chad",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "ManagerPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Manager");
                }
            }

            //konto Manager2
            var managerUser2 = await userManager.FindByEmailAsync("manager2@mail.com");
            if (managerUser2 == null)
            {
                var newUser = new User
                {
                    UserName = "manager2@mail.com",
                    Email = "manager2@mail.com",
                    Name = "Manager2",
                    Surname = "Guy",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "ManagerPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Manager");
                }
            }

            //konto Nowy1
            var nowyUser1 = await userManager.FindByEmailAsync("nowy1@mail.com");
            if (nowyUser1 == null)
            {
                var newUser = new User
                {
                    UserName = "nowy1@mail.com",
                    Email = "nowy1@mail.com",
                    Name = "Nowy1",
                    Surname = "Nowak",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "NowyPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Nowy");
                }
            }

            //konto Nowy2
            var nowyUser2 = await userManager.FindByEmailAsync("nowy2@mail.com");
            if (nowyUser2 == null)
            {
                var newUser = new User
                {
                    UserName = "nowy2@mail.com",
                    Email = "nowy2@mail.com",
                    Name = "Nowy2",
                    Surname = "Nowak",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "NowyPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Nowy");
                }
            }
        }
    }
}
