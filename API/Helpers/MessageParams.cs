using System;

namespace API.Helpers;

//Provides filtering and pagination options for retrieving messages.
public class MessageParams : PaginationParams
{
    public string? Username { get; set; } // The username of the logged-in user.

    public string Container { get; set; } = "Unread"; // Specifies the type of messages to retrieve (e.g., Inbox, Outbox, Unread)
}
