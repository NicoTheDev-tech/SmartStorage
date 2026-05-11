#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.ViewModels;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Core.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace SmartStorage.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        // Check roles in order of specificity
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }

                        if (await _userManager.IsInRoleAsync(user, "OperationsManager"))
                        {
                            return RedirectToAction("Dashboard", "Operations");
                        }

                        if (await _userManager.IsInRoleAsync(user, "WarehouseStaff"))
                        {
                            return RedirectToAction("Dashboard", "Warehouse");
                        }

                        if (await _userManager.IsInRoleAsync(user, "InventoryStaff"))
                        {
                            return RedirectToAction("Dashboard", "Inventory");
                        }

                        if (await _userManager.IsInRoleAsync(user, "CustomerSupport"))
                        {
                            return RedirectToAction("Dashboard", "Support");
                        }

                        if (await _userManager.IsInRoleAsync(user, "Driver"))
                        {
                            return RedirectToAction("Dashboard", "Driver");
                        }

                        if (await _userManager.IsInRoleAsync(user, "Staff"))
                        {
                            return RedirectToAction("Dashboard", "Staff");
                        }

                        // Customer should be the last check
                        if (await _userManager.IsInRoleAsync(user, "Customer"))
                        {
                            // Only handle client claims for regular customers
                            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id);

                            if (client != null && !string.IsNullOrEmpty(client.PreferredName))
                            {
                                var existingClaims = await _userManager.GetClaimsAsync(user);
                                var preferredNameClaim = existingClaims.FirstOrDefault(c => c.Type == "PreferredName");
                                if (preferredNameClaim != null)
                                {
                                    await _userManager.RemoveClaimAsync(user, preferredNameClaim);
                                }

                                await _userManager.AddClaimAsync(user, new Claim("PreferredName", client.PreferredName));
                                await _signInManager.RefreshSignInAsync(user);
                            }

                            return RedirectToAction("Dashboard", "Customer");
                        }

                        // Default fallback
                        return RedirectToLocal(returnUrl);
                    }

                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                // Create new user
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to Customer role by default
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Add claim for preferred name
                    await _userManager.AddClaimAsync(user, new Claim("PreferredName", model.PreferredName));

                    // Create Client record
                    var client = new Client
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PreferredName = model.PreferredName,
                        Email = model.Email,
                        RegistrationDate = DateTime.Now,
                        Phone = string.Empty,
                        Address = string.Empty,
                    };

                    await _context.Clients.AddAsync(client);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to customer dashboard
                    return RedirectToAction("Dashboard", "Customer");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterStaff(RegisterViewModel model, string role = "Staff")
        {
            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                // Create new user
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to selected role
                    await _userManager.AddToRoleAsync(user, role);

                    // Add claim for preferred name
                    await _userManager.AddClaimAsync(user, new Claim("PreferredName", model.PreferredName));

                    // Create Client record for staff (optional)
                    var client = new Client
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PreferredName = model.PreferredName,
                        Email = model.Email,
                        RegistrationDate = DateTime.Now,
                        Phone = string.Empty,
                        Address = string.Empty,
                    };

                    await _context.Clients.AddAsync(client);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Staff account created successfully with role: {role}";
                    return RedirectToAction("Dashboard", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = client?.FullName ?? string.Empty,
                PreferredName = client?.PreferredName ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (client != null)
                {
                    client.FullName = model.FullName;
                    client.PreferredName = model.PreferredName;

                    // Update claim
                    var existingClaims = await _userManager.GetClaimsAsync(user);
                    var preferredNameClaim = existingClaims.FirstOrDefault(c => c.Type == "PreferredName");
                    if (preferredNameClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, preferredNameClaim);
                    }
                    await _userManager.AddClaimAsync(user, new Claim("PreferredName", model.PreferredName));

                    await _context.SaveChangesAsync();
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdmin()
        {
            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Ensure other roles exist
            string[] roles = { "Customer", "Driver", "WarehouseStaff", "OperationsManager", "InventoryStaff", "CustomerSupport", "Staff" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var existingAdmin = await _userManager.FindByEmailAsync("admin@gmail.com");
            if (existingAdmin != null)
            {
                if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
                {
                    await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                }

                // Remove client record if exists
                var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == existingAdmin.Id);
                if (existingClient != null)
                {
                    _context.Clients.Remove(existingClient);
                    await _context.SaveChangesAsync();
                }

                return Content("✅ Admin ready! Login: admin@gmail.com / Admin@123!");
            }

            var user = new IdentityUser { UserName = "admin@gmail.com", Email = "admin@gmail.com" };
            var result = await _userManager.CreateAsync(user, "Admin@123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Content("✅ Admin created! Login: admin@gmail.com / Admin@123!");
            }

            return Content("❌ Error: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
#nullable restore