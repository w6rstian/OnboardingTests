using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Services;
using Onboarding.ViewModels;

namespace Onboarding.Controllers
{
    [Authorize(Roles = "Admin,Buddy,Mentor,Manager,HR")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRoles(string searchTerm)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Jeśli podano wyszukiwane hasło, filtrujemy użytkowników
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                usersQuery = usersQuery.Where(u => u.Email.ToLower().Contains(searchTerm) ||
                                                   u.Name.ToLower().Contains(searchTerm) ||
                                                   u.Surname.ToLower().Contains(searchTerm));
            }

            var users = await usersQuery.ToListAsync();  // Pobieramy wyniki po filtrowaniu
            var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new List<ManageRolesViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                model.Add(new ManageRolesViewModel
                {
                    User = user,
                    AvailableRoles = availableRoles,
                    UserRoles = userRoles
                });
            }

            ViewData["SearchTerm"] = searchTerm; // Przekazujemy wartość wyszukiwania do widoku
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Sprawdzamy, czy użytkownik ma rolę "Admin" i nie pozwalamy na jej usunięcie
            var isUserAdmin = currentRoles.Contains("Admin");
            var isRemovingAdmin = isUserAdmin && selectedRole != "Admin";

            if (isRemovingAdmin)
            {
                var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                var currentUserId = int.Parse(_userManager.GetUserId(User));

                var isOnlyAdmin = allAdmins.Count == 1;
                var isCurrentUser = currentUserId == userId;

                if (isOnlyAdmin)
                {
                    TempData["Error"] = "Nie można usunąć roli 'Admin' ostatniemu administratorowi.";
                    return RedirectToAction("ManageRoles");
                }

                if (isCurrentUser)
                {
                    TempData["Error"] = "Nie możesz usunąć sobie roli administratora.";
                    return RedirectToAction("ManageRoles");
                }
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, selectedRole);

            return RedirectToAction("ManageRoles");
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AdminPanel()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ManageUsers(string searchTerm)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u => u.Email.ToLower().Contains(searchTerm) ||
                                         u.Name.ToLower().Contains(searchTerm) ||
                                         u.Surname.ToLower().Contains(searchTerm));
            }

            var userList = await users.ToListAsync();
            ViewData["SearchTerm"] = searchTerm;

            return View(userList);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var isNowy = userRoles.Contains("Nowy");

            var buddies = await _userManager.GetUsersInRoleAsync("Buddy");

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                BuddyId = user.BuddyId,
                AvailableBuddies = isNowy ? buddies.ToList() : new List<User>() // Pokazujemy Buddich tylko dla roli "Nowy"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null) return NotFound();
            
            var isBuddyModified = user.BuddyId != model.BuddyId;

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.BuddyId = model.BuddyId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (isBuddyModified)
                {
                    var buddy = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.BuddyId);
                    if (buddy != null)
                    {
                        _context.Notifications.Add(
                            new Notification {
                                UserId = user.Id,
                                User = user,
                                Message = $"Przypisano ci nowego buddiego! Twój nowy buddy to {buddy.Name} {buddy.Surname}"
                            }
                        );
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

    }
}
