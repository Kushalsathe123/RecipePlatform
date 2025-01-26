using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace RecipePlatform.UserManagementService.Contracts.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed partial class ValidEmailDomainAttribute : ValidationAttribute
    {
        private static readonly string[] AllowedTopLevelDomains = { ".com", ".net", ".org", ".edu", ".gov", ".co.uk", ".io" };
        private static readonly string[] BlockedDomains = { "tempmail.com", "mailinator.com", "10minutemail.com" };

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
        private static partial Regex EmailRegex();

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string email)
            {
                return ValidationResult.Success; // Assuming null or non-string values are handled elsewhere
            }

            // Check basic email format
            if (!EmailRegex().IsMatch(email))
            {
                return new ValidationResult("Invalid email format.");
            }

            // Extract domain from email
            string[] parts = email.Split('@');
            if (parts.Length != 2)
            {
                return new ValidationResult("Invalid email format.");
            }
            string domain = parts[1];

            // Check TLD
            if (!IsValidTopLevelDomain(domain))
            {
                return new ValidationResult("Invalid top-level domain in email.");
            }

            // Check if domain is in blocked list
            if (IsBlockedDomain(domain))
            {
                return new ValidationResult("Email domain is not allowed.");
            }

            // Check DNS records (for MX or A records)
            if (!DomainHasMailServer(domain))
            {
                return new ValidationResult("Email domain does not have a valid mail server.");
            }

            return ValidationResult.Success;
        }

        private static bool IsValidTopLevelDomain(string domain)
        {
            return Array.Exists(AllowedTopLevelDomains, tld => domain.EndsWith(tld, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBlockedDomain(string domain)
        {
            return Array.Exists(BlockedDomains, blockedDomain => string.Equals(domain, blockedDomain, StringComparison.OrdinalIgnoreCase));
        }

        private static bool DomainHasMailServer(string domain)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(domain);
                return Array.Exists(hostEntry.AddressList, ip => ip.AddressFamily == AddressFamily.InterNetwork ||
                                                                 ip.AddressFamily == AddressFamily.InterNetworkV6);
            }
            catch
            {
                return false; // Domain does not resolve or has no mail server
            }
        }
    }
}