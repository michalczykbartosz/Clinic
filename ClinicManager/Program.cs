using ClinicManager.Data;
using ClinicManager.Mappers;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;  


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//dodanie serwisu bazy danych
builder.Services.AddDbContext<ClinicDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//dodanie serwisu ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options=>
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
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


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
        }
    }
    
    string adminEmail = "admin@wp.pl"; 
    var user = await userManager.FindByEmailAsync(adminEmail);
    
    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}
app.Run();
