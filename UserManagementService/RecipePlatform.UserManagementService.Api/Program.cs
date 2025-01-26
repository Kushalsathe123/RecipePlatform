using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Repositories;
using System.Text;

namespace RecipePlatform.UserManagementService
{
    public class Program
    {
        // Private constructor to prevent instantiation
        private Program() { }
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

#pragma warning disable S125
            // Key Vault configuration (commented out for now)
            var keyVaultUrl = builder.Configuration["KeyVaultUrl"] ?? throw new InvalidOperationException("KeyVaultUrl is not configured");
            var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var jwtSecret = (await secretClient.GetSecretAsync("JwtSecret").ConfigureAwait(false)).Value.Value
                ?? throw new InvalidOperationException("JWT secret not found or is null");

            // Add the JWT secret to the configuration
            builder.Configuration["Jwt:Secret"] = jwtSecret;


            var connectionString = (await secretClient.GetSecretAsync("JwtSecret").ConfigureAwait(false)).Value.Value;
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
#pragma warning restore S125
            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger/OpenAPI with JWT authentication
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Configure Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUpdateProfileService, UpdateProfileService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();

            // Configure Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<INewUserRepository, NewUserRepository>();
            builder.Services.AddScoped<IDeleteUserRepository, DeleteUserRepository>();
            builder.Services.AddScoped<IUpdateUserProfileRepository, UpdateUserProfileRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<PasswordResetUrlGenerator>();

            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT secret key is not configured");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            var app = builder.Build();

            // Apply migrations at startup
            await ApplyMigrationsAsync(app);

            // Enable Swagger and Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            // Enable CORS
            app.UseCors("AllowAllOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
#pragma warning disable S6667 // Logging in a catch clause should pass the caught exception as a parameter.
                logger?.LogError("An error occurred while migrating the database: {ErrorMessage}", ex.Message);
#pragma warning restore S6667 // Logging in a catch clause should pass the caught exception as a parameter.
            }
        }
    }
}