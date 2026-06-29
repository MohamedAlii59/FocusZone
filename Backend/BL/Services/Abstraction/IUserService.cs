using BL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Abstraction
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(UserRegisterDto dto, string creatorUserId);
        Task<UserResponseDto> GetUserByIdAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(string userId, UserResponseDto dto);
        Task<IdentityResult> DeleteUserAsync(string userId, string deleterUserId);
        Task<User> CreateExternalUserAsync(string email, string firstName, string lastName);
        Task<string> GeneratePasswordResetTokenAsync(string email);
    }
}