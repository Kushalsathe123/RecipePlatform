using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RecipePlatform.NotificationDashboardService.Application.Services;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1" });
});


// Smtp Settings Key Vault configuration 
var keyVaultUrl = builder.Configuration["KeyVaultUrl"] ?? throw new InvalidOperationException("KeyVaultUrl is not configured");
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

var SmtpHost = (await secretClient.GetSecretAsync("SmtpHost").ConfigureAwait(false)).Value.Value
    ?? throw new InvalidOperationException("JWT secret not found or is null");

var SmtpPort = (await secretClient.GetSecretAsync("SmtpPort").ConfigureAwait(false)).Value.Value
    ?? throw new InvalidOperationException("JWT secret not found or is null");

var SmtpUsernameret = (await secretClient.GetSecretAsync("SmtpUsername").ConfigureAwait(false)).Value.Value
    ?? throw new InvalidOperationException("JWT secret not found or is null");

var SmtpAppPassword = (await secretClient.GetSecretAsync("SmtpAppPassword").ConfigureAwait(false)).Value.Value
    ?? throw new InvalidOperationException("JWT secret not found or is null");

// Configure SMTP settings
builder.Services.Configure<SmtpSettingsDto>(options =>
{
    options.Server = SmtpHost;
    options.Port = int.Parse(SmtpPort);
    options.Username = SmtpUsernameret;
    options.Password = SmtpAppPassword;
});

// Register EmailService
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
#pragma warning disable S6966
app.Run();
#pragma warning disable S6966