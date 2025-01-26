using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Application.Services;
using RecipePlatform.RecipeManagementService.Contracts.Interfaces;
using RecipePlatform.RecipeManagementService.Data.IRepository;
using RecipePlatform.RecipeManagementService.Data.Repository;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
//builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

var keyVaultUrl = builder.Configuration["KeyVaultUrl"] ?? throw new InvalidOperationException("KeyVaultUrl is not configured");
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());


var cloudName = (await secretClient.GetSecretAsync("CloudName").ConfigureAwait(false)).Value.Value?.Trim()
    ?? throw new InvalidOperationException("CloudName not found or is null");

var apiKey = (await secretClient.GetSecretAsync("ApiKey").ConfigureAwait(false)).Value.Value?.Trim()
    ?? throw new InvalidOperationException("ApiKey not found or is null");

var apiSecret = (await secretClient.GetSecretAsync("ApiSecret").ConfigureAwait(false)).Value.Value?.Trim()
    ?? throw new InvalidOperationException("ApiSecret not found or is null");


builder.Services.AddSingleton<ICloudinaryService>(provider =>
    new CloudinaryService(cloudName, apiKey, apiSecret));

//var mongoConnectionString = builder.Configuration.GetSection("MongoDB:ConnectionString").Value;

var mongoConnectionString = (await secretClient.GetSecretAsync("MongoDbConnectionString").ConfigureAwait(false)).Value.Value?.Trim()
    ?? throw new InvalidOperationException("ConnectionString not found or is null");

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase("RecipeDb"); // Replace with your database name
});

// Add Swagger generation service
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Recipe Management API", Version = "v1" });
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

var jwtSecret = (await secretClient.GetSecretAsync("JwtSecret").ConfigureAwait(false)).Value.Value
               ?? throw new InvalidOperationException("JWT secret not found or is null");

// Add the JWT secret to the configuration
builder.Configuration["Jwt:Secret"] = jwtSecret;

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
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role // Explicitly set to interpret roles correctly
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Authentication failed. Token is invalid or expired."
                }));
            },
            OnChallenge = context =>
            {
                context.HandleResponse(); // Prevents the default challenge response
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "You are not authorized to access this resource."
                }));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "You dont have permission to perform this action."
                }));
            }
        };
    });


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

var app = builder.Build();

// Enable Swagger and Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
