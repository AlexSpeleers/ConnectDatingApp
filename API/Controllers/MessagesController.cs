using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IMessageRepository messageRepository, IMemberRepository memberRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        Member? sender = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
        Member? recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
        if (recipient is null || sender is null || sender.Id == createMessageDto.RecipientId)
            return BadRequest("Cannot send this message");

        Message message = new()
        {
            Sender = sender,
            Recipient = recipient,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };
        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync())
            return message.ToDto();
        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByConteiner([FromQuery] MessageParams messageParams)
    {
        messageParams.MemberId = User.GetMemberId();
        return await messageRepository.GetMessagesForMember(messageParams);
    }

    [HttpGet("thread/{recepientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recepientId)
    {
        return Ok(await messageRepository.GetMessageThread(User.GetMemberId(), recepientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        string memberId = User.GetMemberId();
        var message = await messageRepository.GetMessage(id);
        if (message == null)
            return BadRequest("Cannot delete this message.");
        if (message.SenderId != memberId && message.RecipientId != memberId)
            return BadRequest("You cannot delete this message.");
        if (message.SenderId == memberId) message.SenderDeleted = true;
        if (message.RecipientId == memberId) message.RecipientDeleted = true;
        if (message is { SenderDeleted: true, RecipientDeleted: true })
            messageRepository.DeleteMessage(message);
        if (await messageRepository.SaveAllAsync())
            return Ok();
        return BadRequest("Problem deleting the message");
    }
}