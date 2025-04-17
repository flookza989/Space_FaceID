﻿using Microsoft.EntityFrameworkCore;
using Space_FaceID.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using Space_FaceID.Models.Enums;
using System;

namespace Space_FaceID.Data.Context
{
    public class FaceIDDbContext : DbContext
    {
        public FaceIDDbContext(DbContextOptions<FaceIDDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } // เพิ่มถ้าคุณใช้ UserProfile
        public DbSet<FaceData> FaceDatas { get; set; }
        public DbSet<AuthenticationLog> AuthenticationLogs { get; set; }
        public DbSet<FaceAuthenticationSettings> FaceAuthenticationSettings { get; set; }
        public DbSet<FaceRecognitionModel> FaceRecognitionModels { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=faceID.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ความสัมพันธ์ User - FaceData (1:N)
            modelBuilder.Entity<User>()
                .HasMany(u => u.FaceDatas)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ความสัมพันธ์ User - UserProfile (1:1) ถ้าคุณใช้ UserProfile
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // ความสัมพันธ์อื่นๆ
            modelBuilder.Entity<AuthenticationLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .IsRequired(false);

            modelBuilder.Entity<SystemAuditLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .IsRequired(false);

            // ความสัมพันธ์ User - Role (N:N)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // ความสัมพันธ์ Role - Permission (N:N)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // กำหนด Index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            modelBuilder.Entity<FaceData>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<AuthenticationLog>()
                .HasIndex(l => l.Timestamp);

            modelBuilder.Entity<SystemAuditLog>()
                .HasIndex(l => l.Timestamp);

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            // กำหนด Primary Key
            modelBuilder.Entity<User>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UserProfile>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserProfile>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<FaceData>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<FaceData>()
                .Property(f => f.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<AuthenticationLog>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<AuthenticationLog>()
                .Property(l => l.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<SystemAuditLog>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<SystemAuditLog>()
                .Property(l => l.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => ur.Id);

            modelBuilder.Entity<UserRole>()
                .Property(ur => ur.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => rp.Id);

            modelBuilder.Entity<RolePermission>()
                .Property(rp => rp.Id)
                .ValueGeneratedOnAdd();




            // กำหนด Required Fields และขนาด
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).IsRequired();
                entity.Property(r => r.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(200);
                entity.Property(p => p.Category).IsRequired();
            });

            // ข้อมูลเริ่มต้นสามารถเพิ่มได้ที่นี่ (SeedData method)
        }
    }
}