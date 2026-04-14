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
                        // Check roles in order: Admin -> Staff -> Customer
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }

                        if (await _userManager.IsInRoleAsync(user, "Staff"))
                        {
                            return RedirectToAction("Dashboard", "Staff");
                        }

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
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    bool isAdmin = false;

                    if (!_context.Users.Any())
                    {
                        isAdmin = true;
                    }

                    if (model.Email == "admin@gmail.com")
                    {
                        isAdmin = true;
                    }

                    if (isAdmin)
                    {
                        if (!await _roleManager.RoleExistsAsync("Admin"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Admin"));
                        }
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        if (!await _roleManager.RoleExistsAsync("Customer"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Customer"));
                        }
                        await _userManager.AddToRoleAsync(user, "Customer");

                        var client = new Client
                        {
                            UserId = user.Id,
                            FullName = model.FullName,
                            PreferredName = model.PreferredName,
                            Email = model.Email,
                            Phone = model.Phone ?? string.Empty,
                            IdNumber = model.IdNumber ?? string.Empty,
                            Address = model.Address ?? string.Empty,
                            RegistrationDate = System.DateTime.UtcNow
                        };
                        _context.Clients.Add(client);
                        await _context.SaveChangesAsync();

                        await _userManager.AddClaimAsync(user, new Claim("PreferredName", model.PreferredName));
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
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

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var existingAdmin = await _userManager.FindByEmailAsync("admin@gmail.com");
            if (existingAdmin != null)
            {
                if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
                {
                    await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                }

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
    }
}
#nullable restore