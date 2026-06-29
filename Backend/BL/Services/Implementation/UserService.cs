
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using DAL.Database;
using BL.DTOs;
using AutoMapper;
using BL.Services.Abstraction;

namespace BL.Services.Implementation
{
    

    public class UserService : IUserService
    {
        private readonly UserManager<User> _user_manager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, AppDbContext context, IMapper mapper)
        {
            _user_manager = userManager;
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

            var result = await _user_manager.CreateAsync(user, dto.Password);
            
            if (result.Succeeded)
            {
                // Assign "User" role to new registered users
                await _user_manager.AddToRoleAsync(user, "User");

                // Store goals if provided
                if (dto.Goals != null && dto.Goals.Length > 0)
                {
                    foreach (var title in dto.Goals)
                    {
                        if (string.IsNullOrWhiteSpace(title))
                            continue;

                        var goal = new Goal
                        {
                            UserId = user.Id,
                            Title = title.Trim(),
                            Category = "registration",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Goals.Add(goal);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return result;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string userId)
        {
            var user = await _user_manager.Users
                .Include(u => u.Goals)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var userDto = _mapper.Map<UserResponseDto>(user);
            var roles = await _user_manager.GetRolesAsync(user);
        //    userDto.Roles = roles.ToArray();

            return userDto;
        }

        public async Task<IdentityResult> UpdateUserAsync(string userId, UserResponseDto dto)
        {
            var user = await _user_manager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.BirthDate = dto.BirthDate;
            user.Mobile = dto.Mobile;
            // Map string location fields (if present)
            if (dto.Country != null) user.Country = dto.Country;
            if (dto.State != null) user.State = dto.State;
            if (dto.City != null) user.City = dto.City;
            if (dto.LinkedIn != null) user.LinkedIn = dto.LinkedIn;
            if (dto.GitHub != null) user.GitHub = dto.GitHub;
            if(dto.PostalCode!=null)user.PostalCode = dto.PostalCode;
            user.ModifierUserId = userId;
            user.ModifiedOn = DateTime.UtcNow;

            return await _user_manager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId, string deleterUserId)
        {
            var user = await _user_manager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.IsDeleted = true;
            user.DeleterUserId = deleterUserId;
            user.DeletedOn = DateTime.UtcNow;

            return await _user_manager.UpdateAsync(user);
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

            await _user_manager.CreateAsync(user);
            // Assign "User" role to external login users
            await _user_manager.AddToRoleAsync(user, "User");
            
            return user;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var normalizedEmail = email?.Trim().ToLower();
            var user = await _user_manager.FindByEmailAsync(normalizedEmail);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var token = await _user_manager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var normalizedEmail = email?.Trim().ToLower();
            var user = await _user_manager.FindByEmailAsync(normalizedEmail);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var result = await _user_manager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }
    }
}

