using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// The LikesController class defines API endpoints for managing likes. It relies on IunitOfWork.LikesRepository for data operations 
//and ensures the correct handling of business rules.
public class LikesController(IUnitOfWork unitOfWork): BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId) //Toggles the "like" state between the current user (sourceUserId) and the target user (targetUserId).
    {

        // If the like exists, it deletes it; otherwise, it adds a new like.
        // Returns:
        //Ok if the operation succeeds.
        //BadRequest for invalid requests, e.g., liking oneself or failed updates.

        var sourceUserId = User.GetUserId();
        if (sourceUserId == targetUserId) return BadRequest("You cannot like yourself");

        var existingLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId,targetUserId); 

        if(existingLike == null)
        {
            var like = new UserLike 
            {
                SourceUserId = sourceUserId, 
                TargetUserId = targetUserId
            };
            unitOfWork.LikesRepository.AddLike(like);
        }
        else
        {
            unitOfWork.LikesRepository.DeleteLike(existingLike);
        }
        if (await unitOfWork.Complete()) return Ok();
        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds() // Returns a list of IDs of users the current user has liked.
    {
        return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet] // Retrieves users based on the predicate ("liked" or "likedBy") and returns a list of MemberDto objects.
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);


        Response.AddPaginationHeader(users);
        return Ok(users);
    }
}

//The AutoMapper library maps User entities to MemberDto objects for API responses, 
//keeping database models separate from the API layer.
