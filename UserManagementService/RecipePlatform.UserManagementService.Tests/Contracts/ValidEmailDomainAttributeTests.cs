using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using RecipePlatform.UserManagementService.Contracts.CustomAttributes;

namespace RecipePlatform.UserManagementService.Tests.CustomAttributes
{
    public class ValidEmailDomainAttributeTests
    {
        private readonly ValidEmailDomainAttribute _attribute;

        public ValidEmailDomainAttributeTests()
        {
            _attribute = new ValidEmailDomainAttribute();
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user@domain.org")]
        [InlineData("name@subdomain.co.uk")]
        public void IsValid_ValidEmails_ReturnsSuccess(string email)
        {
            // Arrange
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(email, validationContext);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@invalid")]
        [InlineData("@domain.com")]
        public void IsValid_InvalidEmailFormat_ReturnsValidationError(string email)
        {
            // Arrange
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(email, validationContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid email format.", result.ErrorMessage);
        }

        [Theory]
        [InlineData("test@example.invalid")]
        [InlineData("user@domain.xyz")]
        public void IsValid_InvalidTopLevelDomain_ReturnsValidationError(string email)
        {
            // Arrange
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(email, validationContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid top-level domain in email.", result.ErrorMessage);
        }

        [Theory]
        [InlineData("test@tempmail.com")]
        [InlineData("user@mailinator.com")]
        [InlineData("name@10minutemail.com")]
        public void IsValid_BlockedDomain_ReturnsValidationError(string email)
        {
            // Arrange
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(email, validationContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Email domain is not allowed.", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_NullValue_ReturnsSuccess()
        {
            // Arrange
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(null, validationContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IsValid_NonStringValue_ReturnsSuccess()
        {
            // Arrange
            var nonStringValue = 123;
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(nonStringValue, validationContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IsValid_ValidDomainWithoutMailServer_ReturnsSuccess()
        {
            // Arrange
            var email = "test@nonexistentdomain.com";
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(email, validationContext);

            // Assert
            // Note: This test assumes that the attribute doesn't actually check for mail server existence
            // If it does, this test might fail and you'd need to mock the DNS lookup
            Assert.Null(result);
        }
    }
}