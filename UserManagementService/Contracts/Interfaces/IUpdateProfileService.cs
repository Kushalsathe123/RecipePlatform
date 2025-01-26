using RecipePlatform.UserManagementService.Contracts.DTOs;

namespace RecipePlatform.UserManagementService.Contracts.Interfaces
{
    public interface IUpdateProfileService
    {
        Task UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto passwordDto);
    }
}
