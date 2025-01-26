using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RecipePlatform.UserManagementService.Data.Entities;
using System;

namespace RecipePlatform.UserManagementService.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private NullLogger<ApplicationDbContext> instance;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public ApplicationDbContext(DbContextOptions options, NullLogger<ApplicationDbContext> instance) : base(options)
        {
            this.instance = instance;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    b => b.MigrationsAssembly("RecipePlatform.UserManagementService.Data")
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // UserToken Entity Configuration
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.UserTokenId);
                entity.Property(e => e.AccessToken).IsRequired();
                entity.Property(e => e.ExpirationDateTime).IsRequired();
                entity.Property(e => e.TokenType).HasDefaultValue("jwt");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}