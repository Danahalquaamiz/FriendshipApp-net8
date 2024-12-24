using System;

namespace API.Errors;

public class ApiException (int statusCode, string message, string? details) // string? --> Optional String. because not all the exceptions come with stack trace.
{
    public int statusCode { get; set; } = statusCode;
    public string message { get; set; } = message;
    public string? details { get; set; } = details;
}
