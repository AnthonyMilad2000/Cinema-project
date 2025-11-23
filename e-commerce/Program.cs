using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories;
using Cinema.Repositories.IRepository;
using Cinema.Services;
using Cinema.Utilites;
using Cinema.Utilites.DBSeeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace e_commerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.Password.RequiredLength = 8;
                option.Password.RequireNonAlphanumeric = false;
                option.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login"; // Customize login path
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Customize access denied path
                                                                             // ... other cookie options ...
            });

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IRepository<Movie>, Repository<Movie>>();
            builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
            builder.Services.AddScoped<IRepository<Cinemas>, Repository<Cinemas>>();
            builder.Services.AddScoped<IRepository<Actors>, Repository<Actors>>();
            builder.Services.AddScoped<IRepository<Cart>, Repository<Cart>>();
            builder.Services.AddScoped<IRepository<Promotion>, Repository<Promotion>>();
            builder.Services.AddScoped<IRepository<MovieImage>, Repository<MovieImage>>();
            builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
            builder.Services.AddScoped<IMovieScheduleRepository, MovieSchedulesRepository>();
            builder.Services.AddScoped<IMovieActorsRepository, MovieActorsRepository>();
            builder.Services.AddScoped<AccountHelper>();
            builder.Services.AddScoped<IInitializer, Initializer>();


            // External Login With Google
            builder.Services.AddAuthentication()
           .AddGoogle("google", opt =>
           {
               var googleAuth = builder.Configuration.GetSection("Authentication:Google");
               opt.ClientId = googleAuth["ClientId"]??"";
               opt.ClientSecret = googleAuth["ClientSecret"]?? "";
               opt.SignInScheme = IdentityConstants.ExternalScheme;
           });

            // External Login With FaceBook
            builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"]??"";
                facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
            });
            var app = builder.Build();

            using (var scop = app.Services.CreateScope())
            {
                var initializer = scop.ServiceProvider.GetRequiredService<IInitializer>();
                initializer.Initialize();

            }
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

            app.MapControllerRoute(
              name: "areas",
              pattern: "{area:exists}/{controller=Account}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=IndexHome}/{id?}");


            app.Run();
        }
    }
}
