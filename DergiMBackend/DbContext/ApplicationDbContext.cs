using DergiMBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserRole> OrganisationRoles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectFile> ProjectFiles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API Configuration
            modelBuilder.Entity<OrganisationMembership>()
                .HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganisationMembership>()
                .HasOne(m => m.Organisation)
                .WithMany(o => o.Members)
                .HasForeignKey(m => m.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganisationMembership>()
                .HasOne(m => m.Role)
                .WithMany()
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Organisation>()
                .HasOne(o => o.Owner)
                .WithMany()
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // You can add more rules later (indexes, unique constraints, etc.)
        }
    }
}