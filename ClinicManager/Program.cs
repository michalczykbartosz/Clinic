using ClinicManager.BackgroundServices;
using ClinicManager.Data;
using ClinicManager.Mappers;
using ClinicManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using QuestPDF.Infrastructure;

var nlogConfigPath = Path.Combine(AppContext.BaseDirectory, "nlog.config");
var logger = LogManager.Setup()
    .LoadConfigurationFromFile(nlogConfigPath)
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    //dodanie serwisu bazy danych
    builder.Services.AddDbContext<ClinicDbContext>(x => x
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
    //dodanie serwisu ASP.NET Identity
    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<ClinicDbContext>();

    // Mapperly mappers
    builder.Services.AddScoped<PatientMapper>();
    builder.Services.AddScoped<DoctorMapper>();
    builder.Services.AddScoped<VisitMapper>();
    builder.Services.AddScoped<MedicationMapper>();
    builder.Services.AddScoped<ClinicalNoteMapper>();

    // Business services
    builder.Services.AddScoped<IPatientService, PatientService>();
    builder.Services.AddScoped<IDoctorService, DoctorService>();
    builder.Services.AddScoped<IVisitService, VisitService>();
    builder.Services.AddScoped<IMedicationService, MedicationService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<IClinicalNoteService, ClinicalNoteService>();
    builder.Services.AddScoped<IProcedureService, ProcedureService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    //BackgroundServices
    builder.Services.AddHostedService<NextDayReportAutomationService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change it for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception exception)
        {
            var requestLogger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionMiddleware");

            requestLogger.LogError(
                exception,
                "Nieobsłużony wyjątek podczas żądania {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            throw;
        }
    });

    QuestPDF.Settings.License = LicenseType.Community;

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    using (var scope = scopeFactory.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] requiredRoles = ["Admin", "Lekarz", "Rejestratorka", "Pacjent"];

        foreach (var role in requiredRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                app.Logger.LogInformation("Utworzono brakującą rolę {RoleName}", role);
            }
        }

        string adminEmail = "admin@wp.pl";
        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
            app.Logger.LogInformation("Nadano rolę Admin użytkownikowi {Email}", adminEmail);
        }
    }

    app.Logger.LogInformation("Aplikacja ClinicManager została uruchomiona.");
    await app.RunAsync();
}
catch (Exception exception)
{
    logger.Error(exception, "Aplikacja ClinicManager zakończyła działanie z błędem.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
