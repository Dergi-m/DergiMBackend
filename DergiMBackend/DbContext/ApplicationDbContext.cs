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
        public DbSet<ProjectInvitation> ProjectInvitations { get; set; }


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

            // Project
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Creator)
                .WithMany() // You can add .WithMany(u => u.CreatedProjects) if needed
                .HasForeignKey(p => p.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invitations
            modelBuilder.Entity<ProjectInvitation>()
                .HasOne(pi => pi.Project)
                .WithMany(p => p.Invitations)
                .HasForeignKey(pi => pi.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectInvitation>()
                .HasOne(pi => pi.SenderUser)
                .WithMany()
                .HasForeignKey(pi => pi.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectInvitation>()
                .HasOne(pi => pi.TargetUser)
                .WithMany(u => u.ProjectInvitations) // this is good as-is
                .HasForeignKey(pi => pi.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enforce unique invitation per user per project
            modelBuilder.Entity<ProjectInvitation>()
                .HasIndex(pi => new { pi.TargetUserId, pi.ProjectId })
                .IsUnique();

            // Many-to-many: Projects <-> Users
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Members)
                .WithMany(u => u.Projects)
                .UsingEntity(j => j.ToTable("ProjectUsers"));
        }
    }
}
