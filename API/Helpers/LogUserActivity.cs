using System;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

//This filter updates a user's LastActive timestamp in the database whenever they make an authenticated API request.

public class LogUserActivity : IAsyncActionFilter // IAsyncActionFilter to define asynchronous logic to run before and/or after an action method executes.
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if(context.HttpContext.User.Identity?.IsAuthenticated != true) return; //Check if User is Authenticated: If the request is not authenticated, the filter exits without making any changes.

        var userId = resultContext.HttpContext.User.GetUserId(); //Retrieve the Username

        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>(); //Access the User Repository:

        var user = await repo.GetUserByIdAsync(userId); //Fetch the User:
        if(user == null) return;

        //Update the LastActive Timestamp:
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
