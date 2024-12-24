using System;
using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

// RequestDelegate next: Represents the next middleware in the pipeline. If no exceptions occur, this is invoked to continue the request processing.
//ILogger<ExceptionMiddleware> logger: Used to log the exception details.
//IHostEnvironment env: Determines if the application is running in development or production mode.

public class ExceptionMiddleware (RequestDelegate next, ILogger<ExceptionMiddleware> logger, 
IHostEnvironment env)
{
     public async Task InvokeAsync(HttpContext context)
     {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = env.IsDevelopment()
            ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace)
            : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

            //If in development mode, the response includes the full stack trace.
            //If in production mode, the response includes only the status code and a generic error message.

            // Serialization is the process of converting an object in memory (like the ApiException object) into a format that can be easily transmitted or stored
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json); //The JSON error response is sent back to the client.
        }
     }
}
