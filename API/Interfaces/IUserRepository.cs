using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMemberAsync(UserParams userParams);
    Task<MemberDto?> GetMemberAsync(string username);
    Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser);
    Task<AppUser?> GetUserByPhotoId(int photoId);
}
