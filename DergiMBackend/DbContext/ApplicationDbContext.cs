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
        public DbSet<Organisation> Organisations { get; set; } = default!;
        public DbSet<OrganisationMembership> OrganisationMemberships { get; set; } = default!;
        public DbSet<OrganisationRole> OrganisationRoles { get; set; } = default!;
        public DbSet<Project> Projects { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Organisation has many Roles
            modelBuilder.Entity<Organisation>()
                .HasMany(o => o.OrganisationRoles)
                .WithOne(r => r.Organisation)
                .HasForeignKey(r => r.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organisation has many Memberships
            modelBuilder.Entity<Organisation>()
                .HasMany(o => o.OrganisationMemberships)
                .WithOne(m => m.Organisation)
                .HasForeignKey(m => m.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);

            // User has many OrganisationMemberships
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.OrganisationMemberships)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership has one Role
            modelBuilder.Entity<OrganisationMembership>()
                .HasOne(m => m.Role)
                .WithMany()
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
