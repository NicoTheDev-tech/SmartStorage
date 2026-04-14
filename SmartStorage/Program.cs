using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Config;
using SmartStorage.Core.Interfaces;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add email configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<PdfGeneratorService>();

// Register Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICartageService, CartageService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDeliveryScheduleService, DeliveryScheduleService>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ========== ROUTES ==========
// Admin route - MUST come before default route
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

// Customer routes
app.MapControllerRoute(
    name: "customer",
    pattern: "Customer/{action=Dashboard}/{id?}",
    defaults: new { controller = "Customer" });

// Reserve route
app.MapControllerRoute(
    name: "reserve",
    pattern: "Reserve/{action=Index}/{id?}",
    defaults: new { controller = "Reserve" });

// Invoice routes
app.MapControllerRoute(
    name: "invoice",
    pattern: "Invoice/{action=Index}/{id?}",
    defaults: new { controller = "Invoice" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========== CREATE DATABASE AND ROLES ==========
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Customer", "Staff" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Optional: Seed initial admin if none exists
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        var createResult = await userManager.CreateAsync(admin, "Admin@123!");
        
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            Console.WriteLine("✅ Admin account created: admin@gmail.com / Admin@123!");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        Console.WriteLine("✅ Admin role added to existing admin account");
    }
    
    // Ensure admin doesn't have a client record
    if (adminUser != null)
    {
        var existingClient = await dbContext.Clients.FirstOrDefaultAsync(c => c.UserId == adminUser.Id);
        if (existingClient != null)
        {
            dbContext.Clients.Remove(existingClient);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Removed client record from admin user");
        }
    }
}

app.Run();