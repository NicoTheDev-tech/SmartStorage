using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Infrastructure.Data;
using System.Threading.Tasks;

namespace SmartStorage.Controllers
{
    public class AdminSetupController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminSetupController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string email, string password, string fullName)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            // Check if admin already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Check if user is already admin
                if (await _userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    ViewBag.Error = "Admin already exists with this email";
                    return View();
                }
                else
                {
                    // Add admin role to existing user
                    await _userManager.AddToRoleAsync(existingUser, "Admin");
                    ViewBag.Success = $"User {email} has been promoted to Admin!";
                    return View();
                }
            }

            // Create new admin user
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Create client profile
                var client = new Core.Entities.Client
                {
                    UserId = user.Id,
                    FullName = fullName ?? "Administrator",
                    Email = email,
                    RegistrationDate = System.DateTime.UtcNow
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                // Add admin role
                await _userManager.AddToRoleAsync(user, "Admin");

                ViewBag.Success = "Admin account created successfully! You can now login with: " + email;
                return View();
            }

            foreach (var error in result.Errors)
            {
                ViewBag.Error = error.Description;
            }
            return View();
        }
    }
}