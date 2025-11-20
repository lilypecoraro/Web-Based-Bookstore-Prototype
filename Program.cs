//add a using statement for currency
using System.Globalization;
//add a using statement for Entity Framework Core (uncomment this line for HW3 and beyond)
using Microsoft.EntityFrameworkCore;
//add a using statement for Identity (uncomment this line for HW4 and beyond)
using Microsoft.AspNetCore.Identity;

using Pecoraro_Lillian_HW5.DAL;
using Pecoraro_Lillian_HW5.Models;

//create a web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


string connectionString = "Server=tcp:fa25pecorarolilyhw3.database.windows.net,1433;" +
    "Initial Catalog=PecoraroLilyHW5;" +
    "Persist Security Info=False;" +
    "User ID=MISAdmin;" +
    "Password=Password123;" + 
    "MultipleActiveResultSets=True;" +
    "Encrypt=True;" +
    "TrustServerCertificate=False;" +
    "Connection Timeout=30;";

//Configure the context to use SQL Server -- this is needed for EF Core to work
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));


builder.Services.AddDefaultIdentity<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

//build the app
var app = builder.Build();

//These lines allow you to see more detailed error messages
app.UseDeveloperExceptionPage();
app.UseStatusCodePages();

//This line allows you to use static pages like style sheets and images
app.UseStaticFiles();

//This marks the position in the middleware pipeline where a routing decision
//is made for a URL.
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

//This allows the data annotations for currency to work on Macs
app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
    CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

    await next.Invoke();
});

//This method maps the controllers and their actions to a patter for
//requests that's known as the default route. This route identifies
//the Home controller as the default controller and the Index() action
//method as the default action. The default route also identifies a 
//third segment of the URL that's a parameter named id.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();