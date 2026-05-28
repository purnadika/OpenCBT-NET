using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.Interfaces;
using OpenCBT.Application.Services;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using OpenCBT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Infrastructure.Repositories;
using Serilog;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using OpenCBT.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog for structured logging
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration)
                 .WriteTo.Console());

// 2. Add Database Context with connection pooling for high concurrency, falling back to standard AddDbContext under tests
if (!builder.Environment.IsEnvironment("Testing"))
{
    if (builder.Configuration["UsePooledDbContext"] == "false")
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    }
    else
    {
        builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    }
}

// 3. Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => 
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2PasswordHasher<ApplicationUser>>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 4. Add Cache for stateless architecture, falling back to Memory Cache during testing
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis:Configuration").Value;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. Register Services and Repositories (Clean Architecture DI)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IAdminExamService, AdminExamService>();
builder.Services.AddScoped<IStudentManagementService, StudentManagementService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IGradeService, OpenCBT.Infrastructure.Services.GradeService>();
builder.Services.AddScoped<IClassRoomService, OpenCBT.Infrastructure.Services.ClassRoomService>();
builder.Services.AddScoped<IReportService, OpenCBT.Infrastructure.Services.ReportService>();
builder.Services.AddScoped<ExcelTemplateService>();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<IFileStorageService, OpenCBT.Infrastructure.Services.LocalFileStorageService>();
builder.Services.AddScoped<OpenCBT.Application.Interfaces.ISystemSettingsService, OpenCBT.Infrastructure.Services.SystemSettingsService>();

// Add SignalR for proctoring
builder.Services.AddSignalR();

// 6. Setup UI (Razor Pages)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrTeacher", policy =>
        policy.RequireRole("Admin", "Teacher"));
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en-US", "id-ID" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddRazorPages(options => 
{
    options.Conventions.AuthorizeFolder("/Exams");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOrTeacher");
})
.AddViewLocalization();

builder.Services.AddControllers();

// 7. Rate Limiting to prevent DDoS or abuse
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// Run Migrations and Seeder on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    try 
    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            await context.Database.MigrateAsync();
            await DbSeeder.SeedAsync(context, userManager, roleManager);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Global Exception Handler Middleware
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var error = exceptionHandlerPathFeature?.Error;

        app.Logger.LogError(error, "An unhandled exception occurred.");

        await context.Response.WriteAsJsonAsync(new
        {
            Error = "An unexpected error occurred. Please try again later.",
            Message = app.Environment.IsDevelopment() ? error?.Message : null
        });
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();

app.Use(async (context, next) =>
{
    var settingsService = context.RequestServices.GetRequiredService<ISystemSettingsService>();
    var defaultLang = await settingsService.GetSettingAsync("DefaultLanguage") ?? "en-US";
    var availableLangs = await settingsService.GetSettingAsync("AvailableLanguages") ?? "en-US,id-ID";
    
    var supportedCultures = availableLangs.Split(',').Select(c => c.Trim()).ToList();
    var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

    if (requestCultureFeature != null && !supportedCultures.Contains(requestCultureFeature.RequestCulture.Culture.Name))
    {
        var defaultRequestCulture = new RequestCulture(defaultLang);
        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(defaultRequestCulture, null));
        CultureInfo.CurrentCulture = new CultureInfo(defaultLang);
        CultureInfo.CurrentUICulture = new CultureInfo(defaultLang);
    }
    
    await next();
});

// Use Rate Limiter, Auth and Session
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();
app.MapHub<OpenCBT.Web.Hubs.ProctorHub>("/proctorHub");

app.Run();

public partial class Program { }