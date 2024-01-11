using Ecom_Book_main_DataAccess;
using Ecom_Book_main_DataAccess.Repository;
using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(Options =>
{
    Options.LoginPath = $"/Identity/Account/Login";
    Options.AccessDeniedPath= $"/Identity/Account/AccessDenied";
    Options.LogoutPath= $"/Identity/Account/Logout";
});
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "1084284582596163";
    options.AppSecret = "a45d3326affc0da7463e47e1e3e96cd3";
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "47055320776-i5pgcgbirc2ri65u5s1il18n2sh0392p.apps.googleusercontent.com";
    options.ClientSecret = "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";
});
//builder.Services.AddAuthentication().AddTwitter(options =>
//{
//    options.ConsumerKey = "token";
//    options.ConsumerSecret = "token";
//});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings")["Secretkey"];
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
