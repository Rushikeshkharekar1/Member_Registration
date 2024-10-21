using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Member_Registration.Models
{
    public partial class UserAdminDBContext : DbContext
    {
        public UserAdminDBContext()
        {
        }

        public UserAdminDBContext(DbContextOptions<UserAdminDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ClubUser> ClubUsers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubUser>(entity =>
            {
                entity.ToTable("ClubUser");

                entity.Property(e => e.ClubUserId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.UserName).HasMaxLength(225);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
