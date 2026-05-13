using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Config;
using SmartStorage.Core.Interfaces;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Infrastructure.Services;
using SmartStorage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database Context - Check Azure environment variable first
var connectionString = Environment.GetEnvironmentVariable("AZURE_DB_CONNECTION");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

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

// Background service
builder.Services.AddHostedService<ContractExpiryService>();

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
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "customer",
    pattern: "Customer/{action=Dashboard}/{id?}",
    defaults: new { controller = "Customer" });

app.MapControllerRoute(
    name: "reserve",
    pattern: "Reserve/{action=Index}/{id?}",
    defaults: new { controller = "Reserve" });

app.MapControllerRoute(
    name: "invoice",
    pattern: "Invoice/{action=Index}/{id?}",
    defaults: new { controller = "Invoice" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========== DATABASE MIGRATION ==========
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Customer", "Driver", "WarehouseStaff", "OperationsManager", "InventoryStaff", "CustomerSupport" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

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
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    if (adminUser != null)
    {
        var existingClient = await dbContext.Clients?.FirstOrDefaultAsync(c => c.UserId == adminUser.Id);
        if (existingClient != null)
        {
            dbContext.Clients.Remove(existingClient);
            await dbContext.SaveChangesAsync();
        }
    }
}

app.Run();