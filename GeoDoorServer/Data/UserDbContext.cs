﻿using GeoDoorServer.Models.DataModels;
using Microsoft.EntityFrameworkCore;

namespace GeoDoorServer.Data
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<ConnectionLog> ConnectionLogs { get; set; }

        public UserDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneId).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name).IsUnique();
        }
    }
}
