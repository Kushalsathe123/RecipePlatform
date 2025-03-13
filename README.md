# RecipePlatform

A microservices-based Recipe Platform built with .NET Core and C#.

## 🌟 Overview

RecipePlatform is a modern, cloud-native application that provides a comprehensive solution for recipe management and sharing. The platform is built using a microservices architecture, ensuring scalability, maintainability, and robust feature delivery.

## 🏗️ Architecture

flowchart LR
    %% Core nodes
    User(["👤"])
    
    %% Main services
    Gateway[("API Gateway")]
    AdminSvc["Admin Service"]
    NotifySvc["Notification Service"]
    
    %% External services
    KeyVault[("Azure KeyVault")]
    SMTP[("SMTP")]
    
    %% Connections
    User --> Gateway
    Gateway --> AdminSvc
    Gateway --> NotifySvc
    AdminSvc -.-> KeyVault
    NotifySvc -.-> KeyVault
    NotifySvc --> SMTP
    
    %% Styling
    classDef user fill:#FF5722,color:#fff,stroke:none
    classDef gateway fill:#00BCD4,color:#fff,stroke:none
    classDef service fill:#333,color:#fff,stroke:none
    classDef external fill:#673AB7,color:#fff,stroke:none
    
    class User user
    class Gateway gateway
    class AdminSvc,NotifySvc service
    class KeyVault,SMTP external

The application is structured into several microservices:

1. **Gateway Service**
   - Entry point for all client requests
   - Handles API routing and request distribution
   - Implements OpenAPI/Swagger for API documentation

2. **Admin Management Service**
   - Manages administrative functions
   - Handles user management and platform configuration
   - Built with ASP.NET Core minimal APIs

3. **Notification Dashboard Service**
   - Handles email notifications and communications
   - Integrates with Azure Key Vault for secure configuration
   - Features include:
     - SMTP email service
     - Email templating system
     - Secure credential management

## 🔒 Security Features

- Azure Key Vault integration for secure secrets management
- HTTPS redirection enabled by default
- Secure SMTP configuration
- Azure Identity integration

## 🛠️ Technical Stack

- **Framework**: .NET Core
- **Language**: C# (100%)
- **Documentation**: OpenAPI/Swagger
- **Cloud Services**: Azure Key Vault
- **Security**: Azure Identity
- **API Style**: REST

## 🚀 Getting Started

### Prerequisites

- .NET Core SDK
- Azure subscription (for Key Vault)
- SMTP server access for notifications

### Configuration

The application requires the following configuration:

1. Azure Key Vault setup with the following secrets:
   ```
   - SmtpHost
   - SmtpPort
   - SmtpUsername
   - SmtpAppPassword
   ```

2. Environment configuration in `appsettings.json`:
   ```json
   {
     "KeyVaultUrl": "your-key-vault-url"
   }
   ```

### Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/Kushalsathe123/RecipePlatform.git
   ```

2. Navigate to each service directory and run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

3. Access the Swagger UI:
   - Gateway Service: https://localhost:[port]/swagger
   - Admin Service: https://localhost:[port]/swagger
   - Notification Service: https://localhost:[port]/swagger

## 📐 Project Structure

```
RecipePlatform/
├── Gateway/
│   └── RecipePlatform.Gateway.Api/
├── AdminManagementService/
│   └── RecipePlatform.AdminManagementService.Api/
└── NotificationDashboardService/
    └── RecipePlatform.NotificationDashboardService.Api/
```

## 🔧 Development

Each microservice includes:
- Swagger/OpenAPI integration for API documentation
- Development environment configurations
- Proper dependency injection setup
- Error handling middleware

## 📧 Email Service Features

The Notification Dashboard Service includes:
- Custom email template support
- Configurable SMTP settings
- Secure credential management through Azure Key Vault
- Email service abstraction for easy maintenance

## 🛡️ Security Considerations

1. All sensitive configuration is stored in Azure Key Vault
2. HTTPS is enforced across all services
3. Proper authentication and authorization middleware
4. Secure handling of SMTP credentials

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## ✨ Future Enhancements

- [ ] Add user authentication service
- [ ] Implement recipe management features
- [ ] Add search functionality
- [ ] Implement caching layer
- [ ] Add monitoring and logging
- [ ] Implement CI/CD pipelines

## 📞 Support

For support, please open an issue in the GitHub repository or contact the maintainers.

---
⚡️ Developed with ❤️ by [Kushalsathe123](https://github.com/Kushalsathe123)
