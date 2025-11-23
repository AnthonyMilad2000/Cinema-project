using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Utilites.DBSeeder
{
    public class Initializer : IInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Initializer> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public Initializer(ApplicationDbContext context, ILogger<Initializer> logger,
            RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger=logger;
            _roleManager=roleManager;
            _userManager=userManager;
        }
        public void Initialize() {

            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {

                    _context.Database.Migrate();
                }
                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(SD.SUPER_ADMIN_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.ADMIN_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.CUSTOMER_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.EMPLOYEE_ROLE)).GetAwaiter().GetResult();

                    _userManager.CreateAsync(new()
                    {
                        Email = "SuperAdmin@gmail.com",
                        UserName = "SuperAdmin",
                        Name = "SuperAdmin",
                        EmailConfirmed = true,
                    },"Admin1234$").GetAwaiter().GetResult();

                    var user = _userManager.FindByNameAsync("SuperAdmin").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user!,SD.SUPER_ADMIN_ROLE).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex) {
                _logger.LogError($"Error Message: {ex.Message}");
            }



        }

      
    }
}
