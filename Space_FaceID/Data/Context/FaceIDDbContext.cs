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

        public DbSet<CameraSetting> CameraSettings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<FaceData> FaceDatas { get; set; }
        public DbSet<FaceDetectionSetting> FaceDetectionSettings { get; set; }
        public DbSet<FaceRecognizeSetting> FaceRecognizeSettings { get; set; }
        public DbSet<AuthenticationLog> AuthenticationLogs { get; set; }
        public DbSet<FaceAuthenticationSetting> FaceAuthenticationSettings { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }

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

            // ความสัมพันธ์ User - Role (N:1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ความสัมพันธ์ User - UserProfile (1:1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // ความสัมพันธ์ User - AuthenticationLog (1:N)
            modelBuilder.Entity<AuthenticationLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .IsRequired(false);

            // ความสัมพันธ์ User - SystemAuditLog (1:N)
            modelBuilder.Entity<SystemAuditLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .IsRequired(false);

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


            // กำหนด Required Fields และขนาด
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.RoleId).IsRequired();
                entity.Property(u => u.IsActive).IsRequired();
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(up => up.UserId).IsRequired();
                entity.Property(up => up.FirstName).HasMaxLength(50);
                entity.Property(up => up.LastName).HasMaxLength(50);
                entity.Property(up => up.PhoneNumber).HasMaxLength(20);
                entity.Property(up => up.Gender).HasMaxLength(10);
            });

            modelBuilder.Entity<FaceData>(entity =>
            {
                entity.Property(fd => fd.UserId).IsRequired();
                entity.Property(fd => fd.FaceEncoding).IsRequired();
                entity.Property(fd => fd.FaceImage).IsRequired();
                entity.Property(fd => fd.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<AuthenticationLog>(entity =>
            {
                entity.Property(atl => atl.Username).IsRequired().HasMaxLength(50);
                entity.Property(atl => atl.IsSuccessful).IsRequired();
                entity.Property(atl => atl.Timestamp).IsRequired();
                entity.Property(atl => atl.FailureReason).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<FaceAuthenticationSetting>(entity =>
            {
                entity.Property(fats => fats.MatchThreshold).IsRequired();
                entity.Property(fats => fats.RequireLivenessCheck).IsRequired();
                entity.Property(fats => fats.MaxAttempts).IsRequired();
                entity.Property(fats => fats.IsEnabled).IsRequired();
                entity.Property(fats => fats.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<FaceDetectionSetting>(entity =>
            {
                entity.Property(fats => fats.FaceSize).IsRequired();
                entity.Property(fats => fats.DetectionThreshold).HasPrecision(10, 2).IsRequired();
                entity.Property(fats => fats.MaxWidth).IsRequired();
                entity.Property(fats => fats.MaxHeight).IsRequired();
                entity.Property(fats => fats.IsEnabled).IsRequired();
                entity.Property(fats => fats.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<FaceRecognizeSetting>(entity =>
            {
                entity.Property(fats => fats.Name).IsRequired();
                entity.Property(fats => fats.RecognizeThreshold).HasPrecision(10, 2).IsRequired();
                entity.Property(fats => fats.LandmarkType).IsRequired();
                entity.Property(fats => fats.RecognizerType).IsRequired();
                entity.Property(fats => fats.IsEnabled).IsRequired();
                entity.Property(fats => fats.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).IsRequired();
                entity.Property(r => r.Description).HasMaxLength(200);
                entity.Property(r => r.IsDefault).IsRequired();
                entity.Property(r => r.IsSystem).IsRequired();
                entity.Property(r => r.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<SystemAuditLog>(entity =>
            {
                entity.Property(p => p.Action).IsRequired().HasMaxLength(100);
                entity.Property(p => p.UserId).IsRequired();
                entity.Property(p => p.Description).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Details).IsRequired();
                entity.Property(p => p.Timestamp).IsRequired();
            });

            modelBuilder.Entity<CameraSetting>(entity =>
            {
                entity.Property(p => p.CameraIndex).IsRequired();
                entity.Property(p => p.FrameRate).IsRequired();
                entity.Property(p => p.FrameWidth).IsRequired();
                entity.Property(p => p.FrameHeight).IsRequired();
                entity.Property(p => p.UpdatedBy).IsRequired().HasMaxLength(50);
            });
        }
    }
}