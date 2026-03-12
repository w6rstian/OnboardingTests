using Microsoft.EntityFrameworkCore;
using Onboarding.Data;
using Microsoft.AspNetCore.Identity;
using Onboarding.Models;
using Microsoft.AspNetCore.SignalR;
using Onboarding.Hubs;
using Onboarding.Services;
using Onboarding.Interfaces;
using Onboarding.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddAuthorizationBuilder();

// Register custom repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IRoleInitializer, RoleInitializer>();
builder.Services.AddScoped<ICourseTaskInitializer, CourseTaskInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

// Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleInitializer = services.GetRequiredService<IRoleInitializer>();
    await roleInitializer.SeedRolesAndAdminAsync(services);

    var courseTaskInitializer = services.GetRequiredService<ICourseTaskInitializer>();
    await courseTaskInitializer.SeedCoursesAndTasksAsync(services);
}

app.Run();
