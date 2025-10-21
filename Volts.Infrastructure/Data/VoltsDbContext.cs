using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Infrastructure.Data
{
    public class VoltsDbContext : DbContext
    {
        public VoltsDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftPosition> ShiftPositions { get; set; }
        public DbSet<ShiftVolunteer> ShiftVolunteers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired();
            });

            // Organization Configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("organizations");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedById);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.OrganizationsCreated)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrganizationMember Configuration
            modelBuilder.Entity<OrganizationMember>(entity =>
            {
                entity.ToTable("organization_members");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.OrganizationId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => new { e.OrganizationId, e.Role });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.OrganizationMemberships)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.Members)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.InvitedBy)
                    .WithMany(u => u.OrganizationInvites)
                    .HasForeignKey(e => e.InvitedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Group Configuration
            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("groups");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.CreatedById);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.Icon).HasMaxLength(50);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.Groups)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.GroupsCreated)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GroupMember Configuration
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.ToTable("group_members");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.GroupId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => new { e.GroupId, e.Role });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.GroupMemberships)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AddedBy)
                    .WithMany(u => u.GroupMembershipsAdded)
                    .HasForeignKey(e => e.AddedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Position Configuration
            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("positions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.GroupId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.Positions)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Shift Configuration
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.ToTable("shifts");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => new { e.GroupId, e.Date });
                entity.HasIndex(e => new { e.GroupId, e.Status });
                entity.Property(e => e.Title).HasMaxLength(200);

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.Shifts)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ShiftPosition Configuration
            modelBuilder.Entity<ShiftPosition>(entity =>
            {
                entity.ToTable("shift_positions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ShiftId, e.PositionId }).IsUnique();
                entity.HasIndex(e => e.ShiftId);
                entity.HasIndex(e => e.PositionId);

                entity.HasOne(e => e.Shift)
                    .WithMany(s => s.ShiftPositions)
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Position)
                    .WithMany(p => p.ShiftPositions)
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ShiftVolunteer Configuration
            modelBuilder.Entity<ShiftVolunteer>(entity =>
            {
                entity.ToTable("shift_volunteers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ShiftPositionId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ShiftPositionId);
                entity.HasIndex(e => new { e.ShiftPositionId, e.Status });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ShiftVolunteers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ShiftPosition)
                    .WithMany(sp => sp.Volunteers)
                    .HasForeignKey(e => e.ShiftPositionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var updatedEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && e.State == EntityState.Modified);

            foreach (var entry in updatedEntries)
            {
                ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }


    //public class UsersConfiguration : IEntityTypeConfiguration<User>
    //{
    //    public void Configure(EntityTypeBuilder<User> builder)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
