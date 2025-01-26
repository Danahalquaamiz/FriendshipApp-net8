using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public void AddLike(UserLike like)
    {
        context.Likes.Add(like); // adds a like entity to the Likes table.
    }

    public void DeleteLike(UserLike like)
    {
        context.Likes.Remove(like); //removes a like entity from the table.
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId) //Retrieves a list of user IDs that the current user has liked.
    {
        return await context.Likes
            .Where(x => x.SourceUserId == currentUserId)
            .Select(x => x.TargetUserId)
            .ToListAsync();
    }

    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId); //Uses FindAsync to quickly retrieve a like by the composite key of SourceUserId and TargetUserId.
    }

    public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams) // Retrieves a list of users based on the predicate:
    {
        var likes = context.Likes.AsQueryable();
        IQueryable<MemberDto> query;
        switch (likesParams.Predicate)
        {
            case "liked": //"liked": Users liked by the given user.
                query = likes 
                    .Where(x => x.SourceUserId == likesParams.UserId)
                    .Select(x =>x.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

            case "likedBy": //"likedBy": Users who liked the given user.
                query = likes 
                    .Where(x => x.TargetUserId == likesParams.UserId)
                    .Select(x =>x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

            default: // Default: Intersection of users liked and liked by the given user.
                var likeIds = await GetCurrentUserLikeIds(likesParams.UserId);
                query = likes 
                    .Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId))
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

        }
        return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
    }
}
