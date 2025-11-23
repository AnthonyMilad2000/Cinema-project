using Microsoft.EntityFrameworkCore;
using Cinema.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Cinema.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Cinemas> Cinemas { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Actors> Actors { get; set; }
        public DbSet<Movie> Movie { get; set; }
        public DbSet<MovieImage> MovieImages { get; set; }

        public DbSet<MovieSchedule> MovieSchedules { get; set; }
        public DbSet<MovieActors> MovieActors { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Cinema.Models.ViewModels.ValidateOTP> ValidateOTP { get; set; } = default!;
        public DbSet<Cinema.Models.ViewModels.NewPasswordVM> NewPasswordVM { get; set; } = default!;
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Cinema;Integrated Security=True;TrustServerCertificate=True;");
        //}
    }
}
