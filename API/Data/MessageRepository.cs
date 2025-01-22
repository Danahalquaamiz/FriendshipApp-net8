using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// Handles database interactions for messages.

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message) // removes a message entity from the database.
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message) // removes a message entity from the database.
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id) //Retrieves a single message by its ID.
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams) //Retrieves paginated messages for a user.
    {
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch 
        {
            "Inbox" => query.Where( x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false), // Messages where the logged-in user is the recipient.
            "Outbox" => query.Where( x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false), // Messages where the logged-in user is the sender.
            _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false) // (default): Messages where the logged-in user is the recipient and the message is unread.
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider); //Maps the messages to MessageDto using AutoMapper.

        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize); //Returns a paginated list of MessageDto.
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername) // Retrieves all messages exchanged between the logged-in user and a specific recipient.
    {
        var messgaes = await context.Messages
            // Includes sender and recipient details (e.g., usernames and photos) via Include and ThenInclude.
            .Include(x => x.Sender).ThenInclude(x => x.Photos)
            .Include(x => x.Recipient).ThenInclude(x => x.Photos)
            .Where(x => 
            x.RecipientUsername == currentUsername && x.RecipientDeleted == false && x.SenderUsername == recipientUsername ||
            x.SenderUsername == currentUsername && x.SenderDeleted == false && x.RecipientUsername == recipientUsername )
            .OrderBy(x => x.MessageSent)
            .ToListAsync();

            var unreadMessages= messgaes.Where(x => x.DateRead == null && x.RecipientUsername == currentUsername).ToList(); // Marks unread messages as read and saves changes.

            // Marks unread messages as read and saves changes.
            if(unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messgaes); // aps the thread to MessageDto.
    }
    public async Task<bool> SaveAllAsync() // Saves changes to the database and returns true if successful.
    {
        return await context.SaveChangesAsync() > 0;
    }
}
