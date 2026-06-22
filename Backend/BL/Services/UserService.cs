using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using DAL.Database;
using BL.DTOs;
using AutoMapper;

namespace BL.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(UserRegisterDto dto, string creatorUserId);
        Task<UserResponseDto> GetUserByIdAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(string userId, UserResponseDto dto);
        Task<IdentityResult> DeleteUserAsync(string userId, string deleterUserId);
        Task<User> CreateExternalUserAsync(string email, string firstName, string lastName);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, AppDbContext context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IdentityResult> RegisterUserAsync(UserRegisterDto dto, string creatorUserId)
        {
            var user = _mapper.Map<User>(dto);
            user.CreatorUserId = creatorUserId;
            user.CreatedOn = DateTime.UtcNow;
            user.SessionMinutes = 300; // Give 300 free minutes
            user.LastMinutesResetDate = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (result.Succeeded)
            {
                // Assign "User" role to new registered users
                await _userManager.AddToRoleAsync(user, "User");
            }

            return result;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Country)
                .Include(u => u.Governorate)
                .Include(u => u.City)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var userDto = _mapper.Map<UserResponseDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToArray();

            return userDto;
        }

        public async Task<IdentityResult> UpdateUserAsync(string userId, UserResponseDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.BirthDate = dto.BirthDate;
            user.Mobile = dto.Mobile;
            user.CountryId = dto.CountryId;
            user.GovernorateId = dto.GovernorateId;
            user.CityId = dto.CityId;
            user.PostalCode = dto.PostalCode;
            user.InterestsToLearn = dto.InterestsToLearn;
            user.ModifierUserId = userId;
            user.ModifiedOn = DateTime.UtcNow;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId, string deleterUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.IsDeleted = true;
            user.DeleterUserId = deleterUserId;
            user.DeletedOn = DateTime.UtcNow;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<User> CreateExternalUserAsync(string email, string firstName, string lastName)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedOn = DateTime.UtcNow,
                CreatorUserId = "ExternalLogin",
                SessionMinutes = 300, // Give 300 free minutes
                LastMinutesResetDate = DateTime.UtcNow
            };

            await _userManager.CreateAsync(user);
            // Assign "User" role to external login users
            await _userManager.AddToRoleAsync(user, "User");
            
            return user;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var normalizedEmail = email?.Trim().ToLower();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var normalizedEmail = email?.Trim().ToLower();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }
    }
}
