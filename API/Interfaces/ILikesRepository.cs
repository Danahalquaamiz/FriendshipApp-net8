using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId); //Retrieves a specific like between two users.
    Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams); //Retrieves a list of users who liked or were liked by a user.
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId); //Gets the IDs of users the current user has liked.
    void DeleteLike(UserLike like); //Adds a like to the database.
    void AddLike(UserLike like); //Removes a like from the database.
    Task<bool> SaveChanges(); //Saves changes to the database.
    
    
}
