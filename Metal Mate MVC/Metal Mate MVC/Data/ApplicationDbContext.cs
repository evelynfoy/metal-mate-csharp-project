using Metal_Mate_MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Metal_Mate_MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AlertRequest> AlertRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Important: let Identity configure its tables first
            base.OnModelCreating(builder);

            builder.Entity<AlertRequest>()
                .HasOne(p => p.User)
                .WithMany(u => u.AlertRequests)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
