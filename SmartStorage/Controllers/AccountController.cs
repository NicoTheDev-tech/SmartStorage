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
                if (await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
                {
                    return Content("✅ Admin already exists! Login with: admin@gmail.com / Admin@123!");
                }

                await _userManager.AddToRoleAsync(existingAdmin, "Admin");

                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == existingAdmin.Id);
                if (client != null)
                {
                    client.FullName = "Storage Administrator";
                    client.PreferredName = "Admin";
                    await _context.SaveChangesAsync();
                }

                return Content("✅ User promoted to Admin! Login with: admin@gmail.com / Admin@123!");
            }

            var user = new IdentityUser { UserName = "admin@gmail.com", Email = "admin@gmail.com" };
            var result = await _userManager.CreateAsync(user, "Admin@123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");

                var client = new SmartStorage.Core.Entities.Client
                {
                    UserId = user.Id,
                    FullName = "Storage Administrator",
                    PreferredName = "Admin",
                    Email = "admin@gmail.com",
                    Phone = "0666666666",
                    IdNumber = "0202020202020",
                    Address = "5 Govinthu Place",
                    RegistrationDate = DateTime.UtcNow
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                return Content("✅ Admin created successfully! Login with: admin@gmail.com / Admin@123!");
            }

            return Content("❌ Error creating admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpGet("FixAdmin")]
        public async Task<IActionResult> FixAdmin()
        {
            var user = await _userManager.FindByEmailAsync("admin@gmail.com");

            if (user == null)
            {
                user = new IdentityUser { UserName = "admin@gmail.com", Email = "admin@gmail.com" };
                var createResult = await _userManager.CreateAsync(user, "Admin@123!");
                if (!createResult.Succeeded)
                {
                    return Content("Failed to create user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (client == null)
            {
                client = new SmartStorage.Core.Entities.Client
                {
                    UserId = user.Id,
                    FullName = "Storage Administrator",
                    PreferredName = "Admin",
                    Email = "admin@gmail.com",
                    Phone = "0666666666",
                    IdNumber = "0202020202020",
                    Address = "5 Govinthu Place",
                    RegistrationDate = DateTime.UtcNow
                };
                _context.Clients.Add(client);
            }
            else
            {
                client.FullName = "Storage Administrator";
                client.PreferredName = "Admin";
                client.Email = "admin@gmail.com";
                client.Phone = "0666666666";
                client.IdNumber = "0202020202020";
                client.Address = "5 Govinthu Place";
            }
            await _context.SaveChangesAsync();

            var existingClaims = await _userManager.GetClaimsAsync(user);
            var preferredNameClaim = existingClaims.FirstOrDefault(c => c.Type == "PreferredName");
            if (preferredNameClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, preferredNameClaim);
            }
            await _userManager.AddClaimAsync(user, new Claim("PreferredName", "Admin"));

            await _signInManager.SignOutAsync();

            return Content(@"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; padding: 50px; text-align: center; background: #f5f5f5; }
                    .container { max-width: 500px; margin: 0 auto; background: white; padding: 40px; border-radius: 15px; box-shadow: 0 5px 20px rgba(0,0,0,0.1); }
                    h2 { color: #D4AF37; margin-bottom: 20px; }
                    .btn { display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #D4AF37 0%, #B8860B 100%); color: #2c2c2c; text-decoration: none; border-radius: 50px; font-weight: bold; margin-top: 20px; }
                    .btn:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(212,175,55,0.3); }
                    .details { background: #f8f9fa; padding: 15px; border-radius: 10px; margin: 20px 0; text-align: left; }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>✅ Admin Account Fixed!</h2>
                    <p>Your admin account has been successfully configured.</p>
                    <div class='details'>
                        <p><strong>Email:</strong> admin@gmail.com</p>
                        <p><strong>Password:</strong> Admin@123!</p>
                        <p><strong>Role:</strong> Admin</p>
                    </div>
                    <a href='/Account/Login' class='btn'>Login as Admin</a>
                </div>
            </body>
            </html>
            ", "text/html");
        }

        [HttpGet("CheckRole")]
        public async Task<IActionResult> CheckRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("Not logged in");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Content($"Logged in as: {user.Email}\nRoles: {(roles.Any() ? string.Join(", ", roles) : "No roles assigned")}");
        }

        [HttpGet("MakeMeAdmin")]
        public async Task<IActionResult> MakeMeAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("You are not logged in.");
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _signInManager.RefreshSignInAsync(user);
                return Content("✅ Admin role added! Please refresh the page.");
            }

            return Content("You are already an admin.");
        }

        [HttpGet("CreateStaffAccount")]
        public async Task<IActionResult> CreateStaffAccount()
        {
            // Create Staff role if doesn't exist
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Staff"));
            }

            // Check if staff already exists
            var existingStaff = await _userManager.FindByEmailAsync("staff@gmail.co.za");
            if (existingStaff != null)
            {
                if (!await _userManager.IsInRoleAsync(existingStaff, "Staff"))
                {
                    await _userManager.AddToRoleAsync(existingStaff, "Staff");
                }
                return Content("Staff account already exists. Login with: staff@gmail.co.za / Staff@123!");
            }

            // Create new staff user
            var user = new IdentityUser { UserName = "staff@gmail.co.za", Email = "staff@gmail.co.za" };
            var result = await _userManager.CreateAsync(user, "Staff@123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Staff");

                // Create client profile for staff
                var client = new SmartStorage.Core.Entities.Client
                {
                    UserId = user.Id,
                    FullName = "Storage Staff",
                    PreferredName = "Staff",
                    Email = "staff@gmail.co.za",
                    Phone = "0676767676",
                    IdNumber = "0101010101010",
                    Address = "123 West Street, Durban",
                    RegistrationDate = DateTime.UtcNow
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                // Add claim for preferred name
                await _userManager.AddClaimAsync(user, new Claim("PreferredName", "Staff"));

                return Content("✅ Staff account created successfully! Login with: staff@gmail.co.za / Staff@123!");
            }

            return Content("Error creating staff: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpGet("RegisterStaff")]
        public IActionResult RegisterStaff(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("RegisterStaff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStaff(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Staff"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Staff"));
                    }

                    await _userManager.AddToRoleAsync(user, "Staff");

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
    }
}