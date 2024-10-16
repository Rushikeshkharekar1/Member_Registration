using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Member_Registration.Models
{
    public partial class iBlueAnts_MembersContext : DbContext
    {
        public iBlueAnts_MembersContext(DbContextOptions<iBlueAnts_MembersContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ClubMember> ClubMembers { get; set; } = null!;
        public virtual DbSet<Hobby> Hobbies { get; set; } = null!;
        public virtual DbSet<Society> Societies { get; set; } = null!;

        // No need for OnConfiguring since the configuration is now in Program.cs
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubMember>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MemberName).HasMaxLength(255);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.HasOne(d => d.Hobby)
                    .WithMany(p => p.ClubMembers)
                    .HasForeignKey(d => d.HobbyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClubMembers_Hobbies");

                entity.HasOne(d => d.Society)
                    .WithMany(p => p.ClubMembers)
                    .HasForeignKey(d => d.SocietyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClubMembers_Society");
            });

            modelBuilder.Entity<Hobby>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.HobbyName).HasMaxLength(255);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Society>(entity =>
            {
                entity.ToTable("Society");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.SocietyName).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
