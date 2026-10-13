using AutoMapper;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace CseLibrary.Services.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UserService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserDto>();
        foreach (var u in users)
        {
            var dto = _mapper.Map<UserDto>(u);
            dto.Roles = await _userManager.GetRolesAsync(u);
            result.Add(dto);
        }
        return result;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _userManager.GetRolesAsync(user);
        return dto;
    }

    public async Task<(bool Success, string? Error, UserDto? User)> CreateAsync(CreateUserDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var create = await _userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
            return (false, string.Join("; ", create.Errors.Select(e => e.Description)), null);

        if (dto.Roles.Any())
        {
            var addRoles = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!addRoles.Succeeded)
                return (false, string.Join("; ", addRoles.Errors.Select(e => e.Description)), null);
        }

        var result = _mapper.Map<UserDto>(user);
        result.Roles = await _userManager.GetRolesAsync(user);
        return (true, null, result);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null) return (false, "User not found");

        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(dto.Roles).ToList();
        var toAdd = dto.Roles.Except(currentRoles).ToList();

        if (toRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removeResult.Succeeded)
                return (false, string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        }
        if (toAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded)
                return (false, string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found");
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, string? Error)> SetActiveAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found");
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
