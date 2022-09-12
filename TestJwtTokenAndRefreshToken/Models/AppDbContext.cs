using Microsoft.EntityFrameworkCore;

namespace TestJwtTokenAndRefreshToken.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                UserName = "datnm",
                Password = "12"
            });
        }
    }
}
