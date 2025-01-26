using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// Exposes endpoints to manage messages.
[Authorize]
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto) //Purpose: Allows a user to send a message to another user.
    {
        var username = User.GetUsername(); //Retrieves the logged-in user's username

        if (username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot message yourself!"); 

        // Retrieves both the sender and recipient from the unitOfWork.UserRepository.
        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) 
            return BadRequest("Cannot send message at this time");

        //Creates a new Message object.
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient, 
            SenderUsername = sender.UserName,
            RecipientUsername =  recipient.UserName,
            Content = createMessageDto.Content
        };
        // Adds the message to the database via unitOfWork.MessageRepository.AddMessage(message).
        unitOfWork.MessageRepository.AddMessage(message);

        // Saves changes (unitOfWork.MessageRepository.SaveAllAsync()) , Returns the created message as a MessageDto on success or an error message on failure.
        if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Failed to save message");
    }

    [HttpGet]

    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams) //Retrieves paginated messages for the logged-in user.
    {
        messageParams.Username = User.GetUsername(); // Gets the logged-in user's username and assigns it to messageParams.Username.
        var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams); //Uses unitOfWork.MessageRepository.GetMessagesForUser(messageParams) to retrieve messages, filtering them based on the Container property (e.g., Inbox, Outbox, Unread).
        Response.AddPaginationHeader(messages); //Adds a pagination header to the response
        return messages; //Returns the paginated list of messages as MessageDto.
    }

    [HttpGet("thread/{username}")]
    
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username) //Retrieves a conversation thread between the logged-in user and another user.
    {
        var currentUsername = User.GetUsername(); //Gets the logged-in user's username

        return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUsername, username)); //Uses unitOfWork.MessageRepository.GetMessageThread(currentUsername, username) to fetch all messages between the two users.
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await unitOfWork.MessageRepository.GetMessage(id); 

        if(message == null) return BadRequest("Cannot delete this message");

        if(message.SenderUsername != username && message.RecipientUsername != username) return Forbid();

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if(message.SenderDeleted == true && message.RecipientDeleted == true)

        if (message is {SenderDeleted: true, RecipientDeleted: true}) {
            unitOfWork.MessageRepository.DeleteMessage(message);
        }

        if(await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
