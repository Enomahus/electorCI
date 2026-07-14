using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Enum;
using Application.Common.Enums;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.SQLServer.Contexts
{
    public class ApplicationDbContext
        : IdentityDbContext<
            UserDao,
            RoleDao,
            Guid,
            IdentityUserClaim<Guid>,
            UserRoleDao,
            IdentityUserLogin<Guid>,
            IdentityRoleClaim<Guid>,
            IdentityUserToken<Guid>
        >
    {
        public DbSet<AppActionDao> AppActions { get; set; }
        public DbSet<AppPermissionDao> AppPermissions { get; set; }
        public DbSet<CitizenDao> Citizens { get; set; }
        public DbSet<DistrictDao> Districts { get; set; }
        public DbSet<DocumentDao> Documents { get; set; }
        public DbSet<ElectorDao> Electors { get; set; }
        public DbSet<PollingStationDao> PollingStations { get; set; }
        public DbSet<RefreshTokenDao> RefreshTokens { get; set; }
        public DbSet<RegistrationRequestDao> RegistrationRequests { get; set; }
        public DbSet<RegistrationRequestDocumentDao> RegistrationRequestDocuments { get; set; }
        public DbSet<UserDistrictDao> UserDistricts { get; set; }
        public DbSet<AuditLogDao> AuditLogs { get; set; }

        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<RegistrationRequestType>()
                .HaveConversion<EnumToStringConverter<RegistrationRequestType>>();

            configurationBuilder
                .Properties<RegistrationRequestDocumentType>()
                .HaveConversion<EnumToStringConverter<RegistrationRequestDocumentType>>();

            configurationBuilder
                .Properties<PersonTitle>()
                .HaveConversion<EnumToStringConverter<PersonTitle>>();

            configurationBuilder
                .Properties<ElectoralDistrictLevel>()
                .HaveConversion<EnumToStringConverter<ElectoralDistrictLevel>>();

            configurationBuilder
                .Properties<Gender>()
                .HaveConversion<EnumToStringConverter<Gender>>();

            configurationBuilder
                .Properties<MaritalStatus>()
                .HaveConversion<EnumToStringConverter<MaritalStatus>>();

            configurationBuilder
                .Properties<RegistrationStatus>()
                .HaveConversion<EnumToStringConverter<RegistrationStatus>>();

            configurationBuilder
                .Properties<AppAction>()
                .HaveConversion<EnumToStringConverter<AppAction>>();

            configurationBuilder
                .Properties<AppPermission>()
                .HaveConversion<EnumToStringConverter<AppPermission>>();

            configurationBuilder
                .Properties<AuthProvider>()
                .HaveConversion<EnumToStringConverter<AuthProvider>>();

            configurationBuilder
                .Properties<ElectorStatus>()
                .HaveConversion<EnumToStringConverter<ElectorStatus>>();

            configurationBuilder
                .Properties<FiliationType>()
                .HaveConversion<EnumToStringConverter<FiliationType>>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserDao>().Property(e => e.UserName).HasMaxLength(100);
            builder.Entity<UserDao>().Property(e => e.FirstName).HasMaxLength(100);
            builder.Entity<UserDao>().Property(e => e.LastName).HasMaxLength(100);
            builder.Entity<UserDao>().Property(e => e.Email).HasMaxLength(100);

            builder
                .Entity<UserDao>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(r => r.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<UserDao>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<UserRoleDao>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<UserRoleDao>()
                .HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<RoleDao>()
                .HasMany(r => r.Actions)
                .WithMany(r => r.Roles)
                .UsingEntity(e => e.ToTable("RoleAppAction"));

            builder
                .Entity<AppActionDao>()
                .HasMany(e => e.Permissions)
                .WithMany(e => e.Actions)
                .UsingEntity(j => j.ToTable("AppActionAppPermission"));

            builder
                .Entity<UserDistrictDao>()
                .HasMany(e => e.SpecificRoles)
                .WithMany(e => e.UserDistricts)
                .UsingEntity(j => j.ToTable("UserDistrictSpecificRole"));

            builder.Entity<RegistrationRequestDao>(entity =>
            {
                entity
                    .HasOne(l => l.Author)
                    .WithMany(a => a.OwnRegistrationRequests)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(l => l.LastUpdater)
                    .WithMany(a => a.UpdatedRegistrationRequests)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(r => r.District)
                    .WithMany(r => r.RegistrationRequests)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(r => r.Citizen)
                    .WithMany(r => r.RegistrationRequests)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Contraintes Circonscription (Unicité Code/Libelle par Niveau)
            builder.Entity<DistrictDao>(entity =>
            {
                entity.HasIndex(e => new { e.Code, e.Level }).IsUnique();

                entity
                    .HasOne(c => c.Parent)
                    .WithMany(c => c.Subconstituency)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PollingStationDao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StationNumber, e.DistrictId }).IsUnique();
                entity
                    .HasOne(e => e.District)
                    .WithMany() // une circonscription à plusieur bureau
                    .HasForeignKey(e => e.DistrictId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ElectorDao>(entity =>
            {
                entity.HasIndex(e => e.VoterRegistrationNumber).IsUnique();
                // Relation 1:1 Citizen <-> Elector
                entity
                    .HasOne(e => e.Citizen)
                    .WithOne(e => e.ElectorProfil)
                    .HasForeignKey<ElectorDao>(e => e.Id);

                entity
                    .HasOne(e => e.PollingStation)
                    .WithMany(e => e.Electors)
                    .HasForeignKey(e => e.PollingStationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Filiation (Auto-référence Citizen)
            builder.Entity<CitizenDao>(entity =>
            {
                entity
                    .HasOne(c => c.Father)
                    .WithMany()
                    .HasForeignKey(c => c.FatherId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity
                    .HasOne(c => c.Mother)
                    .WithMany()
                    .HasForeignKey(c => c.MotherId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Seeding
            builder.Entity<DistrictDao>().HasData(DistrictData.GetDistrictData);
        }
    }
}
